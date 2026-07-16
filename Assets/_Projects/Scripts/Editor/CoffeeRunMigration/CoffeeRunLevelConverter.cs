using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CoffeeRunMigration
{
    public static class CoffeeRunLevelConverter
    {
        private static readonly ColorType[] TargetPalette =
        {
            ColorType.Red, ColorType.Green, ColorType.Blue, ColorType.White,
            ColorType.Orange, ColorType.Yellow, ColorType.Brown, ColorType.Cyan,
            ColorType.DarkPurple, ColorType.Pink, ColorType.Violet, ColorType.Lime,
            ColorType.Navy, ColorType.Gray,
        };

        public static bool TryConvert(
            CoffeeRunLevelRecord source,
            int pizzaRushGoldReward,
            out LevelData output,
            out CoffeeRunValidationReport report)
        {
            output = null;
            report = CoffeeRunLevelValidator.ValidateSource(source);
            if (report.status != CoffeeRunComparisonStatus.Exact)
            {
                return false;
            }

            var colorMap = BuildColorMap(source, report);
            if (report.status != CoffeeRunComparisonStatus.Exact)
            {
                return false;
            }

            output = new LevelData
            {
                levelIndex = source.level,
                duration = source.timerSeconds,
                goldReward = pizzaRushGoldReward,
                gridPositions = source.grid.cells
                    .Select(cell => new SerializableVector2Int(new Vector2Int(cell.x, cell.y)))
                    .ToList(),
            };

            foreach (var sourceContainer in source.containers)
            {
                if (!TryMapContainer(sourceContainer, colorMap, report, out var container))
                {
                    output = null;
                    return false;
                }
                output.containers.Add(container);
            }

            foreach (var sourceLine in source.productionLines)
            {
                if (!TryMapProductionLine(sourceLine, colorMap, report, out var line))
                {
                    output = null;
                    return false;
                }
                output.productionLines.Add(line);
            }

            CoffeeRunLevelValidator.ValidateOutput(source, output, colorMap, report);
            if (report.status != CoffeeRunComparisonStatus.Exact)
            {
                output = null;
                return false;
            }

            return true;
        }

        private static Dictionary<string, ColorType> BuildColorMap(
            CoffeeRunLevelRecord source,
            CoffeeRunValidationReport report)
        {
            var orderedKeys = new List<string>();
            foreach (var key in source.containers.SelectMany(item =>
                             item.colorKeys != null && item.colorKeys.Count > 0
                                 ? item.colorKeys
                                 : new List<string> { item.colorKey })
                         .Concat(source.productionLines.SelectMany(line =>
                             line.productionOrder.Select(item => item.colorKey))))
            {
                if (!string.IsNullOrWhiteSpace(key) && !orderedKeys.Contains(key))
                {
                    orderedKeys.Add(key);
                }
            }

            if (orderedKeys.Count > TargetPalette.Length)
            {
                report.Unsupported($"Level uses {orderedKeys.Count} colors; Pizza Rush supports {TargetPalette.Length}.");
                return new Dictionary<string, ColorType>();
            }

            return orderedKeys.Select((key, index) => new { key, index })
                .ToDictionary(item => item.key, item => TargetPalette[item.index]);
        }

        private static bool TryMapContainer(
            CoffeeRunContainerRecord source,
            IReadOnlyDictionary<string, ColorType> colorMap,
            CoffeeRunValidationReport report,
            out ContainerSaveData output)
        {
            output = null;
            if (!TryMapShape(source.shape, out var shape))
            {
                report.Unsupported($"Container '{source.id}' uses unsupported shape '{source.shape}'.");
                return false;
            }
            if (!TryMapMovement(source.movement, out var movement))
            {
                report.Unsupported($"Container '{source.id}' uses unsupported movement '{source.movement}'.");
                return false;
            }
            if (!TryMapMaterial(source.material, out var material))
            {
                report.Unsupported($"Container '{source.id}' uses unsupported material '{source.material}'.");
                return false;
            }
            if (!colorMap.TryGetValue(source.colorKey, out var color))
            {
                report.Mismatch($"Container '{source.id}' has no mapped color '{source.colorKey}'.");
                return false;
            }

            var modifiers = source.modifiers ?? new CoffeeRunModifierRecord();
            if (modifiers.cap || modifiers.linked || modifiers.barrier || modifiers.ropes > 0)
            {
                report.Unsupported($"Container '{source.id}' uses a modifier without Pizza Rush runtime support.");
                return false;
            }

            var sourceColorKeys = source.colorKeys != null && source.colorKeys.Count > 0
                ? source.colorKeys
                : new List<string> { source.colorKey };
            var mappedColors = new List<ColorType>();
            foreach (var key in sourceColorKeys)
            {
                if (!colorMap.TryGetValue(key, out var mappedColor))
                {
                    report.Mismatch($"Container '{source.id}' has no mapped layer color '{key}'.");
                    return false;
                }
                mappedColors.Add(mappedColor);
            }
            if (mappedColors.Count > 1 && mappedColors.Count != 2)
            {
                report.Unsupported($"Container '{source.id}' has {mappedColors.Count} colors; only two-color source blocks are supported.");
                return false;
            }
            // Coffee Run serializes LayerBox colors from the inner/base color to
            // the outer/current cover. Pizza Rush consumes index 0 first.
            if (modifiers.layerBox)
            {
                mappedColors.Reverse();
            }

            ContainerColorData CreateColorData()
            {
                return new ContainerColorData
                {
                    colorType = mappedColors.Count > 0 ? mappedColors[0] : color,
                    colors = new List<ColorType>(mappedColors),
                    colorAmounts = source.colorAmounts != null
                        ? new List<int>(source.colorAmounts)
                        : new List<int>(),
                    isLayerBox = modifiers.layerBox,
                    isMultiColor = mappedColors.Count > 1 && !modifiers.layerBox,
                };
            }

            ContainerData CreateResolvedData()
            {
                return new ContainerData
                {
                    containerShapeType = shape,
                    containerMovementType = movement,
                    containerMaterialType = material,
                    containerColorData = CreateColorData(),
                    containerIceData = new ContainerIceData(),
                    containerBoombData = new ContainerBoombData { boombAmount = modifiers.bombCount },
                    containerKeyData = new ContainerKeyData { keyAmount = modifiers.keyCount },
                    isStone = modifiers.stoneLayers > 0,
                };
            }

            var resolvedData = CreateResolvedData();
            var runtimeData = resolvedData;
            if (modifiers.iceLayers > 0)
            {
                runtimeData = new ContainerData
                {
                    containerShapeType = shape,
                    containerMovementType = movement,
                    containerMaterialType = ContainerMaterialType.Ice,
                    containerColorData = CreateColorData(),
                    containerIceData = new ContainerIceData
                    {
                        iceAmount = modifiers.iceLayers,
                        innerContainerData = resolvedData,
                    },
                    containerBoombData = new ContainerBoombData { boombAmount = modifiers.bombCount },
                    containerKeyData = new ContainerKeyData { keyAmount = modifiers.keyCount },
                    isStone = modifiers.stoneLayers > 0,
                };
            }

            output = new ContainerSaveData
            {
                position = ToSerializable(source.position),
                rotationType = MapRotation(source.rotationQuarterTurns),
                flipX = source.flipX,
                containerData = runtimeData,
            };
            return true;
        }

        private static bool TryMapProductionLine(
            CoffeeRunProductionLineRecord source,
            IReadOnlyDictionary<string, ColorType> colorMap,
            CoffeeRunValidationReport report,
            out ProductionLineSaveData output)
        {
            output = null;
            if (!TryMapVisual(source.visualKey, out var visual))
            {
                report.Unsupported($"Production line '{source.id}' uses unsupported visual key '{source.visualKey}'.");
                return false;
            }
            output = new ProductionLineSaveData
            {
                productionLineMode = ProductionLineMode.Normal,
                productionLineVisualType = visual,
                position = ToSerializable(source.position),
                rotationType = MapRotation(source.rotationQuarterTurns),
            };
            foreach (var item in source.productionOrder)
            {
                if (!colorMap.TryGetValue(item.colorKey, out var color))
                {
                    report.Mismatch($"Production line '{source.id}' has no mapped color '{item.colorKey}'.");
                    return false;
                }
                output.productionCollections.Add(new ProductionCollectionSaveData
                {
                    colorType = color,
                    Amount = item.amount,
                });
            }
            return true;
        }

        private static bool TryMapShape(string value, out ContainerShapeType result)
        {
            result = value switch
            {
                "One" => ContainerShapeType.Rectangle_1x1,
                "Two" => ContainerShapeType.Rectangle_1x2,
                "Three" => ContainerShapeType.Rectangle_1x3,
                "Square" => ContainerShapeType.Rectangle_2x2,
                "LShort" => ContainerShapeType.L_1x1,
                "L" => ContainerShapeType.L_1x2,
                "LLeft" => ContainerShapeType.L_1x2,
                "T" => ContainerShapeType.T,
                "Cross" => ContainerShapeType.Plus,
                _ => ContainerShapeType.None,
            };
            return result != ContainerShapeType.None;
        }

        private static bool TryMapMovement(string value, out ContainerMovementType result)
        {
            result = value switch
            {
                "Free" => ContainerMovementType.Walkable,
                "AxisX" => ContainerMovementType.Vertical,
                "AxisY" => ContainerMovementType.Horizontal,
                "Blocked" => ContainerMovementType.Blocked,
                _ => ContainerMovementType.None,
            };
            return result != ContainerMovementType.None;
        }

        private static bool TryMapMaterial(string value, out ContainerMaterialType result)
        {
            result = value switch
            {
                "Color" => ContainerMaterialType.Color,
                "Unassigned" => ContainerMaterialType.NoAsign,
                "Ice" => ContainerMaterialType.Ice,
                _ => ContainerMaterialType.None,
            };
            return result != ContainerMaterialType.None;
        }

        private static bool TryMapVisual(string value, out ProductionLineVisualType result)
        {
            result = value switch
            {
                "Straight" => ProductionLineVisualType.SafeStraight,
                "CurvedRight" => ProductionLineVisualType.SafeCurvedRight,
                "CurvedLeft" => ProductionLineVisualType.SafeCurvedLeft,
                _ => ProductionLineVisualType.LegacyRandom,
            };
            return result != ProductionLineVisualType.LegacyRandom;
        }

        private static RotationType MapRotation(int quarterTurns)
        {
            return (((quarterTurns % 4) + 4) % 4) switch
            {
                0 => RotationType.Rotate_0,
                1 => RotationType.Rotate_90,
                2 => RotationType.Rotate_180,
                _ => RotationType.Rotate_270,
            };
        }

        private static SerializableVector2Int ToSerializable(CoffeeRunPosition position)
        {
            return new SerializableVector2Int(new Vector2Int(position.x, position.y));
        }
    }
}
