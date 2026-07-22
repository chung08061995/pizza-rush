using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace CoffeeRunMigration
{
    /// <summary>
    /// Imports AssetRipper's text export of Coffee Run's encrypted Unity bundle.
    /// Source assets remain outside the repository; only normalized records are written here.
    /// </summary>
    public static class CoffeeRunUnityAssetImporter
    {
        private const string SourceDirectoryVariable = "COFFEE_RUN_LEVEL_ASSET_DIR";
        private const string NormalizedDirectory = "CoffeeRunMigration/Normalized";

        private static readonly string[] ShapeNames =
        {
            "One", "Two", "Three", "Square", "LShort", "T", "L", "LLeft", "Cross",
        };

        // Values recovered from Coffee Run 3.20.0's IL2CPP
        // LevelEditorController.GetFilledTilePos{Right,Left,Up,Down}.
        // Coordinates are (source row, source column) deltas.
        private static readonly Vector2Int[][][] SourceShapeOffsets =
        {
            Directions(new[] { V(0, 0) }, new[] { V(0, 0) }, new[] { V(0, 0) }, new[] { V(0, 0) }),
            Directions(new[] { V(0, 0), V(0, 1) }, new[] { V(0, 0), V(1, 0) }, new[] { V(0, 0), V(0, -1) }, new[] { V(0, 0), V(-1, 0) }),
            Directions(new[] { V(0, 0), V(0, 1), V(0, 2) }, new[] { V(0, 0), V(1, 0), V(2, 0) }, new[] { V(0, 0), V(0, -1), V(0, -2) }, new[] { V(0, 0), V(-1, 0), V(-2, 0) }),
            Directions(new[] { V(0, 0), V(0, 1), V(1, 0), V(1, 1) }, new[] { V(0, 0), V(0, -1), V(1, 0), V(1, -1) }, new[] { V(0, 0), V(0, 1), V(1, 0), V(1, 1) }, new[] { V(0, 0), V(0, 1), V(-1, 0), V(-1, 1) }),
            Directions(new[] { V(0, 0), V(1, 0), V(1, 1) }, new[] { V(0, 0), V(0, -1), V(1, -1) }, new[] { V(0, 0), V(-1, 0), V(-1, -1) }, new[] { V(0, 0), V(0, 1), V(-1, 1) }),
            Directions(new[] { V(0, 0), V(0, 1), V(0, 2), V(1, 1) }, new[] { V(0, 0), V(1, 0), V(2, 0), V(1, -1) }, new[] { V(0, 0), V(0, -1), V(0, -2), V(-1, -1) }, new[] { V(0, 0), V(-1, 0), V(-2, 0), V(-1, 1) }),
            Directions(new[] { V(0, 0), V(1, 0), V(2, 0), V(2, 1) }, new[] { V(0, 0), V(0, -1), V(0, -2), V(1, -2) }, new[] { V(0, 0), V(-1, 0), V(-2, 0), V(-2, -1) }, new[] { V(0, 0), V(0, 1), V(0, 2), V(-1, 2) }),
            Directions(new[] { V(0, 0), V(1, 0), V(2, 0), V(2, -1) }, new[] { V(0, 0), V(0, -1), V(0, -2), V(-1, -2) }, new[] { V(0, 0), V(-1, 0), V(-2, 0), V(-2, 1) }, new[] { V(0, 0), V(0, 1), V(0, 2), V(1, 2) }),
            Directions(new[] { V(0, 0), V(1, -1), V(1, 0), V(1, 1), V(2, 0) }, new[] { V(0, 0), V(0, -1), V(0, -2), V(-1, -1), V(1, -1) }, new[] { V(0, 0), V(-1, -1), V(-1, 0), V(-1, 1), V(-2, 0) }, new[] { V(0, 0), V(0, 1), V(0, 2), V(1, 1), V(-1, 1) }),
        };

        [MenuItem("MyMenu/Coffee Run/Import AssetRipper level assets")]
        public static void ImportAllFromEnvironment()
        {
            var sourceDirectory = Environment.GetEnvironmentVariable(SourceDirectoryVariable);
            if (string.IsNullOrWhiteSpace(sourceDirectory))
            {
                throw new InvalidOperationException($"Set {SourceDirectoryVariable} to AssetRipper's encryptedasset/levels directory.");
            }

            ImportRange(sourceDirectory, 1, 100);
            AssetDatabase.Refresh();
        }

        public static void ImportRange(string sourceDirectory, int firstLevel, int lastLevel)
        {
            Directory.CreateDirectory(NormalizedDirectory);
            for (var level = firstLevel; level <= lastLevel; level++)
            {
                var sourcePath = Path.Combine(sourceDirectory, $"Level_{level}.asset");
                if (!File.Exists(sourcePath))
                {
                    throw new FileNotFoundException($"Missing Coffee Run level asset {level}.", sourcePath);
                }

                var record = ParseLevel(File.ReadAllText(sourcePath), sourcePath);
                if (record.level != level)
                {
                    throw new InvalidDataException($"{sourcePath} contains level {record.level}, expected {level}.");
                }

                var outputPath = Path.Combine(NormalizedDirectory, $"{level:0000}.json");
                File.WriteAllText(outputPath, JsonConvert.SerializeObject(record, Formatting.Indented));
            }
        }

        private static CoffeeRunLevelRecord ParseLevel(string yaml, string sourcePath)
        {
            var level = ReadInt(yaml, @"(?m)^  level: (-?\d+)");
            var rows = ReadInt(yaml, @"(?m)^    row: (-?\d+)");
            var columns = ReadInt(yaml, @"(?m)^    col: (-?\d+)");
            var horizontalOffset = (10 - columns) / 2;
            var record = new CoffeeRunLevelRecord
            {
                extractionMethod = "Il2CppAsset",
                level = level,
                timerSeconds = ReadInt(yaml, @"(?m)^  time: (-?\d+)"),
                grid = new CoffeeRunGridRecord { rows = rows, columns = columns },
            };

            var gridBody = ReadGroup(yaml, @"(?ms)^    strGrid:\s*\n(.*?)^  blocks:");
            var gridRows = Regex.Matches(gridBody, @"(?m)^    - ([0-9,]+)$")
                .Cast<Match>()
                .Select(match => match.Groups[1].Value.Split(',').Select(int.Parse).ToArray())
                .ToList();
            if (gridRows.Count != rows || gridRows.Any(row => row.Length != columns))
            {
                throw new InvalidDataException($"Level {level} has an invalid strGrid size.");
            }
            for (var sourceRow = 0; sourceRow < rows; sourceRow++)
            {
                for (var sourceColumn = 0; sourceColumn < columns; sourceColumn++)
                {
                    if (gridRows[sourceRow][sourceColumn] != 0)
                    {
                        record.grid.cells.Add(ToTarget(sourceRow, sourceColumn, rows, horizontalOffset));
                    }
                }
            }

            var blocksBody = ReadGroup(yaml, @"(?ms)^  blocks:\s*\n(.*?)^  doors:");
            var blockMatches = Regex.Matches(blocksBody, @"(?ms)^  - tileCount:.*?(?=^  - tileCount:|\z)");
            for (var index = 0; index < blockMatches.Count; index++)
            {
                record.containers.Add(ParseBlock(blockMatches[index].Value, index, rows, horizontalOffset));
            }

            var doorsBody = ReadGroup(yaml, @"(?ms)^  doors:\s*\n(.*?)^  time:");
            var doorMatches = Regex.Matches(doorsBody, @"(?ms)^  - position:.*?(?=^  - position:|\z)");
            for (var index = 0; index < doorMatches.Count; index++)
            {
                record.productionLines.Add(ParseDoor(doorMatches[index].Value, index, rows, columns, horizontalOffset));
            }

            record.evidence.Add($"AssetRipper Unity YAML: {Path.GetFileName(sourcePath)}");
            record.evidence.Add("Runtime bundle/config variant: Default");
            return record;
        }

        private static CoffeeRunContainerRecord ParseBlock(string yaml, int index, int rows, int horizontalOffset)
        {
            var sourcePosition = ReadPosition(yaml);
            var shapeType = ReadInt(yaml, @"(?m)^    shapeType: (-?\d+)");
            var direction = ReadInt(yaml, @"(?m)^    direction: (-?\d+)");
            if (shapeType < 0 || shapeType >= ShapeNames.Length || direction < 0 || direction > 3)
            {
                throw new NotSupportedException($"Unsupported source shape/direction {shapeType}/{direction}.");
            }

            var targetAnchor = ToTarget(sourcePosition.x, sourcePosition.y, rows, horizontalOffset);
            var shapeMapping = FindTargetShapeMapping(shapeType, direction);
            targetAnchor.x += shapeMapping.anchorOffset.x;
            targetAnchor.y += shapeMapping.anchorOffset.y;

            var colorsHex = ReadString(yaml, @"(?m)^    colors: ([0-9a-fA-F]+)");
            var colors = DecodeColors(colorsHex);
            var tileCount = ReadInt(yaml, @"(?m)^  - tileCount: (-?\d+)");
            var blockAxis = ReadInt(yaml, @"(?m)^      blockAxis: (-?\d+)");
            var isLayerBox = ReadInt(yaml, @"(?m)^      isLayerBox: (-?\d+)") != 0;
            return new CoffeeRunContainerRecord
            {
                id = $"block-{index + 1}",
                position = targetAnchor,
                shape = ShapeNames[shapeType],
                rotationQuarterTurns = shapeMapping.quarterTurns,
                flipX = shapeMapping.flipX,
                movement = blockAxis switch { 0 => "Free", 1 => "AxisX", 2 => "AxisY", _ => "Blocked" },
                material = "Color",
                colorKey = ColorKey(colors[0]),
                colorKeys = colors.Select(ColorKey).ToList(),
                colorAmounts = GetColorAmounts(shapeType, tileCount, colors.Count, isLayerBox),
                modifiers = new CoffeeRunModifierRecord
                {
                    iceLayers = ReadInt(yaml, @"(?m)^      iceCount: (-?\d+)"),
                    bombCount = ReadInt(yaml, @"(?m)^      boomCount: (-?\d+)"),
                    keyCount = ReadInt(yaml, @"(?m)^        isActive: (-?\d+)", false),
                    stoneLayers = ReadInt(yaml, @"(?m)^      isStone: (-?\d+)"),
                    colorLayers = colors.Count - 1,
                    layerBox = isLayerBox,
                    cap = ReadInt(yaml, @"(?m)^      isCapActive: (-?\d+)") != 0,
                    linked = ReadInt(yaml, @"(?m)^      groupConnectId: (-?\d+)") >= 0,
                    barrier = ReadInt(yaml, @"(?m)^      isBarrier: (-?\d+)") != 0,
                    timedExplodeSeconds = ReadInt(yaml, @"(?m)^      timeExplode: (-?\d+)"),
                    scissor = ReadInt(yaml, @"(?m)^        isActive: (-?\d+)", false) != 0,
                    ropes = Regex.Matches(yaml, @"(?m)^      - direction:").Count,
                },
            };
        }

        private static List<int> GetColorAmounts(int shapeType, int tileCount, int colorCount, bool isLayerBox)
        {
            var capacity = tileCount * 4;
            if (colorCount <= 1) return new List<int> { capacity };
            if (isLayerBox) return Enumerable.Repeat(capacity, colorCount).ToList();
            if (colorCount != 2)
            {
                throw new NotSupportedException($"Multi-color source block has {colorCount} colors.");
            }

            // Exact slot distribution used by Coffee Run's InitColorTiles/CountColors.
            var firstColorAmount = shapeType switch
            {
                0 => 2,
                1 => 4,
                2 => 6,
                3 => 8,
                4 => 8,
                5 => 12,
                6 or 7 => 8,
                8 => 10,
                _ => throw new NotSupportedException($"Multi-color source shape {shapeType} is unsupported."),
            };
            return new List<int> { firstColorAmount, capacity - firstColorAmount };
        }

        private static CoffeeRunProductionLineRecord ParseDoor(
            string yaml, int index, int rows, int columns, int horizontalOffset)
        {
            var sourcePosition = ReadPosition(yaml);
            var direction = ReadInt(yaml, @"(?m)^    direction: (-?\d+)");
            var intakeCell = ToTarget(sourcePosition.x, sourcePosition.y, rows, horizontalOffset);
            var outwardDirection = direction switch
            {
                0 => new CoffeeRunPosition { x = 1, y = 0 },
                1 => new CoffeeRunPosition { x = -1, y = 0 },
                2 => new CoffeeRunPosition { x = 0, y = 1 },
                3 => new CoffeeRunPosition { x = 0, y = -1 },
                _ => throw new NotSupportedException($"Unsupported door direction {direction}."),
            };
            var line = new CoffeeRunProductionLineRecord
            {
                id = $"door-{index + 1}",
                position = new CoffeeRunPosition
                {
                    x = intakeCell.x + outwardDirection.x,
                    y = intakeCell.y + outwardDirection.y,
                },
                // ProductionLine probes local (0,-1); rotate it toward the board interior.
                rotationQuarterTurns = direction switch { 0 => 3, 1 => 1, 2 => 0, 3 => 2, _ => 0 },
                modifiers = new CoffeeRunProductionLineModifierRecord
                {
                    keyLock = ReadInt(yaml, @"(?m)^      isActive: (-?\d+)", false) != 0,
                    iceLayers = ReadInt(yaml, @"(?m)^    iceCount: (-?\d+)", false),
                    cap = ReadInt(yaml, @"(?m)^    isCapActive: (-?\d+)", false) != 0,
                    directionalLock = ReadInt(yaml, @"(?m)^    isDirectionalLock: (-?\d+)", false) != 0,
                },
            };

            foreach (Match item in Regex.Matches(yaml, @"(?m)^    - color: (-?\d+)\s*\n      quantity: (-?\d+)$"))
            {
                line.productionOrder.Add(new CoffeeRunProductionItemRecord
                {
                    colorKey = ColorKey(int.Parse(item.Groups[1].Value)),
                    amount = int.Parse(item.Groups[2].Value),
                });
            }

            var directionsBody = ReadGroup(yaml, @"(?ms)^    listDirections:\s*\n(.*?)^    keyLockInfo:");
            foreach (Match segment in Regex.Matches(directionsBody, @"(?m)^    - length: (-?\d+)\s*\n      direction: (-?\d+)$"))
            {
                var segmentDirection = int.Parse(segment.Groups[2].Value);
                line.path.Add(new CoffeeRunPathSegmentRecord
                {
                    length = int.Parse(segment.Groups[1].Value),
                    direction = DirectionName(segmentDirection),
                });
            }
            line.visualKey = VisualKey(direction, line.path);
            return line;
        }

        private static string VisualKey(int initialDirection, List<CoffeeRunPathSegmentRecord> path)
        {
            var initial = DirectionVector(DirectionName(initialDirection));
            var turn = path.FirstOrDefault(segment => segment.direction != DirectionName(initialDirection));
            if (turn == null)
            {
                return "Straight";
            }

            var next = DirectionVector(turn.direction);
            var cross = initial.x * next.y - initial.y * next.x;
            return cross > 0 ? "CurvedLeft" : "CurvedRight";
        }

        private static Vector2Int DirectionVector(string direction) => direction switch
        {
            "Right" => Vector2Int.right,
            "Left" => Vector2Int.left,
            "Up" => Vector2Int.up,
            "Down" => Vector2Int.down,
            _ => Vector2Int.zero,
        };

        private static ShapeMapping FindTargetShapeMapping(int shapeType, int direction)
        {
            var targetShape = shapeType switch
            {
                0 => ContainerShapeType.Rectangle_1x1,
                1 => ContainerShapeType.Rectangle_1x2,
                2 => ContainerShapeType.Rectangle_1x3,
                3 => ContainerShapeType.Rectangle_2x2,
                4 => ContainerShapeType.L_1x1,
                5 => ContainerShapeType.T,
                6 or 7 => ContainerShapeType.L_1x2,
                8 => ContainerShapeType.Plus,
                _ => ContainerShapeType.None,
            };
            var desired = SourceShapeOffsets[shapeType][direction]
                .Select(offset => new Vector2Int(offset.y, -offset.x))
                .ToList();
            var targetParts = ContainerShapeTypeExtensions.GetPartPositions(targetShape);

            for (var quarterTurns = 0; quarterTurns < 4; quarterTurns++)
            {
                for (var flipIndex = 0; flipIndex < 2; flipIndex++)
                {
                    var flipX = flipIndex == 1;
                    var transformed = targetParts.Select(part => Transform(part, quarterTurns, flipX)).ToList();
                    foreach (var desiredPoint in desired)
                    {
                        var shift = desiredPoint - transformed[0];
                        if (transformed.Select(point => point + shift).OrderBy(Key)
                            .SequenceEqual(desired.OrderBy(Key)))
                        {
                            return new ShapeMapping(quarterTurns, flipX, shift);
                        }
                    }
                }
            }
            throw new NotSupportedException($"Cannot map Coffee Run shape/direction {shapeType}/{direction}.");
        }

        private static Vector2Int Transform(Vector2Int value, int quarterTurns, bool flipX)
        {
            var result = flipX ? new Vector2Int(-value.x, value.y) : value;
            for (var index = 0; index < quarterTurns; index++)
            {
                result = new Vector2Int(-result.y, result.x);
            }
            return result;
        }

        private static List<int> DecodeColors(string hex)
        {
            if (hex.Length == 0 || hex.Length % 8 != 0)
            {
                throw new InvalidDataException($"Invalid Coffee Run color byte string '{hex}'.");
            }
            var result = new List<int>();
            for (var index = 0; index < hex.Length; index += 8)
            {
                var bytes = Enumerable.Range(0, 4)
                    .Select(offset => byte.Parse(hex.Substring(index + offset * 2, 2), NumberStyles.HexNumber))
                    .ToArray();
                result.Add(BitConverter.ToInt32(bytes, 0));
            }
            return result;
        }

        private static CoffeeRunPosition ToTarget(int sourceRow, int sourceColumn, int rows, int horizontalOffset) =>
            new() { x = horizontalOffset + sourceColumn, y = rows - sourceRow };

        private static CoffeeRunPosition ReadPosition(string text)
        {
            var match = Regex.Match(text, @"(?m)^  (?:- |  )position: \{x: (-?\d+), y: (-?\d+)\}$");
            if (!match.Success)
            {
                throw new InvalidDataException("Missing source position.");
            }
            return new CoffeeRunPosition { x = int.Parse(match.Groups[1].Value), y = int.Parse(match.Groups[2].Value) };
        }

        private static int ReadInt(string text, string pattern, bool required = true)
        {
            var match = Regex.Match(text, pattern);
            if (!match.Success)
            {
                if (!required) return 0;
                throw new InvalidDataException($"Missing integer matching {pattern}.");
            }
            return int.Parse(match.Groups[1].Value);
        }

        private static string ReadString(string text, string pattern) => ReadGroup(text, pattern);

        private static string ReadGroup(string text, string pattern)
        {
            var match = Regex.Match(text, pattern);
            if (!match.Success)
            {
                throw new InvalidDataException($"Missing source field matching {pattern}.");
            }
            return match.Groups[1].Value;
        }

        private static string ColorKey(int value) => $"source-color-{value}";
        private static string DirectionName(int value) => value switch
        {
            0 => "Right", 1 => "Left", 2 => "Up", 3 => "Down", _ => $"Unknown-{value}",
        };
        private static string Key(Vector2Int value) => $"{value.x:0000},{value.y:0000}";
        private static Vector2Int V(int x, int y) => new(x, y);
        private static Vector2Int[][] Directions(params Vector2Int[][] values) => values;

        private readonly struct ShapeMapping
        {
            public readonly int quarterTurns;
            public readonly bool flipX;
            public readonly Vector2Int anchorOffset;

            public ShapeMapping(int quarterTurns, bool flipX, Vector2Int anchorOffset)
            {
                this.quarterTurns = quarterTurns;
                this.flipX = flipX;
                this.anchorOffset = anchorOffset;
            }
        }
    }
}
