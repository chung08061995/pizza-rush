using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CoffeeRunMigration
{
    public static class CoffeeRunLevelValidator
    {
        public static CoffeeRunValidationReport ValidateSource(CoffeeRunLevelRecord source)
        {
            var report = new CoffeeRunValidationReport { level = source?.level ?? 0 };
            if (source == null)
            {
                report.Mismatch("Source record is null.");
                return report;
            }
            if (source.level < 1 || source.level > 883)
            {
                report.Mismatch($"Level index {source.level} is outside 1-883.");
            }
            if (source.sourceLevel < 0 || source.sourceLevel > 883)
            {
                report.Mismatch($"Source level index {source.sourceLevel} is outside 0-883.");
            }
            if (source.sourceVersion != "3.20.0" || source.sourceVersionCode != 790)
            {
                report.Mismatch("Source fingerprint is not Coffee Run 3.20.0 (790).");
            }
            if (source.configVariant != "Default")
            {
                report.Mismatch($"Runtime config must be Default, found '{source.configVariant}'.");
            }
            if (source.extractionMethod != "Il2CppAsset" && source.extractionMethod != "AdbVisualFallback")
            {
                report.Mismatch($"Unknown extraction method '{source.extractionMethod}'.");
            }
            if (source.timerSeconds <= 0)
            {
                report.Mismatch("Timer must be greater than zero.");
            }
            if (source.grid == null || source.grid.cells == null || source.grid.cells.Count == 0)
            {
                report.Mismatch("Grid has no cells.");
                return report;
            }
            if (source.grid.rows <= 0 || source.grid.columns <= 0)
            {
                report.Mismatch("Grid row and column counts must be positive.");
            }

            var uniqueCells = new HashSet<string>();
            foreach (var cell in source.grid.cells)
            {
                if (cell == null || !uniqueCells.Add(Key(cell.x, cell.y)))
                {
                    report.Mismatch("Grid contains a null or duplicate cell.");
                }
            }

            if (source.containers == null || source.containers.Count == 0)
            {
                report.Mismatch("Level has no containers.");
            }
            if (source.productionLines == null || source.productionLines.Count == 0)
            {
                report.Mismatch("Level has no production lines.");
            }

            foreach (var container in source.containers ?? new List<CoffeeRunContainerRecord>())
            {
                if (container == null || container.position == null)
                {
                    report.Mismatch("Container or its position is null.");
                    continue;
                }
                if (string.IsNullOrWhiteSpace(container.colorKey))
                {
                    report.Mismatch($"Container '{container.id}' has no color key.");
                }
                if (container.rotationQuarterTurns < 0 || container.rotationQuarterTurns > 3)
                {
                    report.Mismatch($"Container '{container.id}' rotation must be 0-3 quarter turns.");
                }
                var colorKeys = container.colorKeys != null && container.colorKeys.Count > 0
                    ? container.colorKeys
                    : new List<string> { container.colorKey };
                if (container.colorAmounts == null || container.colorAmounts.Count != colorKeys.Count ||
                    container.colorAmounts.Any(amount => amount <= 0))
                {
                    report.Mismatch($"Container '{container.id}' has invalid per-color capacity metadata.");
                }
            }

            foreach (var line in source.productionLines ?? new List<CoffeeRunProductionLineRecord>())
            {
                if (line == null || line.position == null)
                {
                    report.Mismatch("Production line or its position is null.");
                    continue;
                }
                if (line.productionOrder == null || line.productionOrder.Count == 0)
                {
                    report.Mismatch($"Production line '{line.id}' has an empty order.");
                    continue;
                }
                if (line.productionOrder.Any(item => item == null || item.amount <= 0 || string.IsNullOrWhiteSpace(item.colorKey)))
                {
                    report.Mismatch($"Production line '{line.id}' contains an invalid production item.");
                }
            }
            return report;
        }

        public static void ValidateOutput(
            CoffeeRunLevelRecord source,
            LevelData output,
            IReadOnlyDictionary<string, ColorType> colorMap,
            CoffeeRunValidationReport report)
        {
            if (output == null)
            {
                report.Mismatch("Converted LevelData is null.");
                return;
            }
            if (output.levelIndex != source.level)
            {
                report.Mismatch($"Output index {output.levelIndex} differs from source {source.level}.");
            }
            if (Mathf.Abs(output.duration - source.timerSeconds) > 0.001f)
            {
                report.Mismatch($"Output timer {output.duration} differs from source {source.timerSeconds}.");
            }

            var grid = new HashSet<string>(output.gridPositions.Select(item =>
            {
                var value = item.ToVector2Int();
                return Key(value.x, value.y);
            }));
            if (grid.Count != source.grid.cells.Count || source.grid.cells.Any(cell => !grid.Contains(Key(cell.x, cell.y))))
            {
                report.Mismatch("Normalized source grid differs from output grid.");
            }
            if (output.containers.Count != source.containers.Count)
            {
                report.Mismatch("Container count differs from normalized source.");
            }
            if (output.productionLines.Count != source.productionLines.Count)
            {
                report.Mismatch("Production-line count differs from normalized source.");
            }

            for (var containerIndex = 0;
                 containerIndex < Math.Min(output.containers.Count, source.containers.Count);
                 containerIndex++)
            {
                ValidateContainerMapping(source.containers[containerIndex], output.containers[containerIndex],
                    colorMap, containerIndex, report);
            }

            var occupied = new HashSet<string>();
            var capacities = new Dictionary<ColorType, int>();
            foreach (var container in output.containers)
            {
                var anchor = container.position.ToVector2Int();
                var parts = ContainerSaveDataExtensions.GetPartPositions(container);
                if (parts.Count == 0)
                {
                    report.Mismatch($"Container at {anchor} has an invalid shape.");
                    continue;
                }
                foreach (var local in parts)
                {
                    var position = anchor + local;
                    var key = Key(position.x, position.y);
                    if (!grid.Contains(key))
                    {
                        report.Mismatch($"Container cell {key} is outside the grid.");
                    }
                    if (!occupied.Add(key))
                    {
                        report.Mismatch($"Containers overlap at {key}.");
                    }
                }

                if (container.containerData.isStone)
                {
                    continue;
                }

                var colorData = container.containerData.containerColorData;
                var colors = colorData.colors != null && colorData.colors.Count > 0
                    ? colorData.colors
                    : new List<ColorType> { colorData.colorType };
                var totalCapacity = parts.Count * 4;
                var amounts = colorData.colorAmounts;
                if (amounts == null || amounts.Count != colors.Count)
                {
                    report.Mismatch("Container color amount metadata does not match its colors.");
                    continue;
                }
                if (colorData.isMultiColor && amounts.Sum() != totalCapacity)
                {
                    report.Mismatch($"Multi-color container amounts total {amounts.Sum()}, expected {totalCapacity}.");
                    continue;
                }
                for (var colorIndex = 0; colorIndex < colors.Count; colorIndex++)
                {
                    var color = colors[colorIndex];
                    var capacityPerColor = amounts[colorIndex];
                    capacities[color] = capacities.TryGetValue(color, out var current)
                        ? current + capacityPerColor
                        : capacityPerColor;
                }
            }

            var demand = new Dictionary<ColorType, int>();
            for (var lineIndex = 0; lineIndex < output.productionLines.Count; lineIndex++)
            {
                var line = output.productionLines[lineIndex];
                if (line.productionLineVisualType == ProductionLineVisualType.LegacyRandom)
                {
                    report.Unsupported($"Converted production line {lineIndex} uses LegacyRandom.");
                }
                var linePosition = line.position.ToVector2Int();
                var intakeCell = linePosition + IntakeDirection(line.rotationType);
                if (!grid.Contains(Key(intakeCell.x, intakeCell.y)))
                {
                    report.Mismatch($"Production line {lineIndex} points outside the board at {Key(intakeCell.x, intakeCell.y)}.");
                }
                foreach (var item in line.productionCollections)
                {
                    demand[item.colorType] = demand.TryGetValue(item.colorType, out var current)
                        ? current + item.Amount
                        : item.Amount;
                }

                if (lineIndex >= source.productionLines.Count)
                {
                    continue;
                }
                var sourceLine = source.productionLines[lineIndex];
                if (line.productionLineMode != ProductionLineMode.Normal ||
                    line.position.ToVector2Int() != new Vector2Int(sourceLine.position.x, sourceLine.position.y) ||
                    line.rotationType != ExpectedRotation(sourceLine.rotationQuarterTurns) ||
                    line.productionLineVisualType != ExpectedVisual(sourceLine.visualKey))
                {
                    report.Mismatch($"Production-line mapping differs at line {lineIndex}.");
                }
                var sourceOrder = sourceLine.productionOrder;
                if (sourceOrder.Count != line.productionCollections.Count)
                {
                    report.Mismatch($"Production order length differs for line {lineIndex}.");
                    continue;
                }
                for (var itemIndex = 0; itemIndex < sourceOrder.Count; itemIndex++)
                {
                    var expected = sourceOrder[itemIndex];
                    var actual = line.productionCollections[itemIndex];
                    if (!colorMap.TryGetValue(expected.colorKey, out var expectedColor) ||
                        expectedColor != actual.colorType || expected.amount != actual.Amount)
                    {
                        report.Mismatch($"Production order differs at line {lineIndex}, item {itemIndex}.");
                    }
                }
            }

            foreach (var color in capacities.Keys.Union(demand.Keys))
            {
                capacities.TryGetValue(color, out var capacity);
                demand.TryGetValue(color, out var required);
                if (capacity != required)
                {
                    report.Mismatch($"Color {color}: container capacity {capacity}, production amount {required}.");
                }
            }
        }

        private static void ValidateContainerMapping(
            CoffeeRunContainerRecord source,
            ContainerSaveData output,
            IReadOnlyDictionary<string, ColorType> colorMap,
            int index,
            CoffeeRunValidationReport report)
        {
            var modifiers = source.modifiers ?? new CoffeeRunModifierRecord();
            var expectedPosition = new Vector2Int(source.position.x, source.position.y);
            var expectedMaterial = modifiers.iceLayers > 0
                ? ContainerMaterialType.Ice
                : source.material switch
                {
                    "Color" => ContainerMaterialType.Color,
                    "Unassigned" => ContainerMaterialType.NoAsign,
                    "Ice" => ContainerMaterialType.Ice,
                    _ => ContainerMaterialType.None,
                };
            var expectedShape = source.shape switch
            {
                "One" => ContainerShapeType.Rectangle_1x1,
                "Two" => ContainerShapeType.Rectangle_1x2,
                "Three" => ContainerShapeType.Rectangle_1x3,
                "Square" => ContainerShapeType.Rectangle_2x2,
                "LShort" => ContainerShapeType.L_1x1,
                "L" or "LLeft" => ContainerShapeType.L_1x2,
                "T" => ContainerShapeType.T,
                "Cross" => ContainerShapeType.Plus,
                _ => ContainerShapeType.None,
            };
            var expectedMovement = source.movement switch
            {
                "Free" => ContainerMovementType.Walkable,
                "AxisX" => ContainerMovementType.Vertical,
                "AxisY" => ContainerMovementType.Horizontal,
                "Blocked" => ContainerMovementType.Blocked,
                _ => ContainerMovementType.None,
            };
            var colorKeys = source.colorKeys != null && source.colorKeys.Count > 0
                ? source.colorKeys
                : new List<string> { source.colorKey };
            var mappedColors = colorKeys
                .Where(colorMap.ContainsKey)
                .Select(key => colorMap[key])
                .ToList();
            if (modifiers.layerBox)
            {
                mappedColors.Reverse();
            }
            var colorData = output.containerData.containerColorData;
            var inner = output.containerData.containerIceData.innerContainerData;
            var iceInnerMismatch = modifiers.iceLayers > 0 &&
                (inner == null ||
                 inner.containerShapeType != expectedShape ||
                 inner.containerMovementType != expectedMovement ||
                 inner.containerMaterialType == ContainerMaterialType.Ice ||
                 inner.containerColorData == null ||
                 inner.containerColorData.colorType != mappedColors.FirstOrDefault() ||
                 inner.containerColorData.colors == null ||
                 !inner.containerColorData.colors.SequenceEqual(mappedColors) ||
                 inner.containerColorData.colorAmounts == null ||
                 !inner.containerColorData.colorAmounts.SequenceEqual(source.colorAmounts));
            if (output.position.ToVector2Int() != expectedPosition ||
                output.rotationType != ExpectedRotation(source.rotationQuarterTurns) ||
                output.flipX != source.flipX ||
                output.containerData.containerShapeType != expectedShape ||
                output.containerData.containerMovementType != expectedMovement ||
                output.containerData.containerMaterialType != expectedMaterial ||
                output.containerData.isStone != (modifiers.stoneLayers > 0) ||
                output.containerData.containerIceData.iceAmount != modifiers.iceLayers ||
                output.containerData.containerBoombData.boombAmount != modifiers.bombCount ||
                output.containerData.containerKeyData.keyAmount != modifiers.keyCount ||
                colorData.colorType != mappedColors.FirstOrDefault() ||
                colorData.colors == null || !colorData.colors.SequenceEqual(mappedColors) ||
                colorData.colorAmounts == null || !colorData.colorAmounts.SequenceEqual(source.colorAmounts) ||
                colorData.isLayerBox != modifiers.layerBox ||
                colorData.isMultiColor != (mappedColors.Count > 1 && !modifiers.layerBox) ||
                iceInnerMismatch)
            {
                report.Mismatch($"Container mapping differs at index {index} ('{source.id}').");
            }
        }

        private static RotationType ExpectedRotation(int quarterTurns) => quarterTurns switch
        {
            0 => RotationType.Rotate_0,
            1 => RotationType.Rotate_90,
            2 => RotationType.Rotate_180,
            _ => RotationType.Rotate_270,
        };

        private static ProductionLineVisualType ExpectedVisual(string visualKey) => visualKey switch
        {
            "Straight" => ProductionLineVisualType.SafeStraight,
            "CurvedRight" => ProductionLineVisualType.SafeCurvedRight,
            "CurvedLeft" => ProductionLineVisualType.SafeCurvedLeft,
            _ => ProductionLineVisualType.LegacyRandom,
        };

        private static Vector2Int IntakeDirection(RotationType rotationType) => rotationType switch
        {
            RotationType.Rotate_0 => new Vector2Int(0, -1),
            RotationType.Rotate_90 => new Vector2Int(1, 0),
            RotationType.Rotate_180 => new Vector2Int(0, 1),
            RotationType.Rotate_270 => new Vector2Int(-1, 0),
            _ => Vector2Int.zero,
        };

        private static string Key(int x, int y) => $"{x},{y}";
    }
}
