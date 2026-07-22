using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace CoffeeRunMigration
{
    public static class CoffeeRunRuntimeAudit
    {
        private const string LevelDirectory = "Assets/_Projects/Resources/LevelData";

        [MenuItem("MyMenu/Coffee Run/Audit existing Pizza Rush levels 1-6")]
        public static void AuditFirstSix()
        {
            AuditRange(1, 6, null);
        }

        [MenuItem("MyMenu/Coffee Run/Audit all converted Pizza Rush levels")]
        public static void AuditAll()
        {
            var last = Directory.GetFiles(LevelDirectory, "*.json")
                .Select(Path.GetFileNameWithoutExtension)
                .Where(name => int.TryParse(name, out _))
                .Select(int.Parse)
                .DefaultIfEmpty(1)
                .Max();
            AuditRange(1, last, "CoffeeRunMigration/Reports/runtime-audit-all.md");
        }

        private static void AuditRange(int first, int last, string outputPath)
        {
            var report = new List<string>();
            for (var index = first; index <= last; index++)
            {
                var path = Path.Combine(LevelDirectory, $"{index:0000}.json");
                if (!File.Exists(path))
                {
                    report.Add($"{index:0000}: Mismatch - file missing");
                    continue;
                }

                var level = JsonConvert.DeserializeObject<LevelData>(File.ReadAllText(path));
                var errors = Validate(index, level);
                report.Add(errors.Count == 0
                    ? $"{index:0000}: Exact runtime integrity"
                    : $"{index:0000}: Mismatch - {string.Join("; ", errors)}");
            }
            Debug.Log("Coffee Run existing-level audit:\n" + string.Join("\n", report));
            if (!string.IsNullOrWhiteSpace(outputPath))
            {
                File.WriteAllText(outputPath,
                    "# Runtime integrity audit\n\n" +
                    $"Levels: {first:0000}–{last:0000}\n\n```text\n" +
                    string.Join("\n", report) + "\n```\n");
                AssetDatabase.Refresh();
            }
        }

        [MenuItem("MyMenu/Coffee Run/Repair deterministic line keys for levels 2-6")]
        public static void RepairKnownProductionLineVisuals()
        {
            for (var index = 2; index <= 6; index++)
            {
                var path = Path.Combine(LevelDirectory, $"{index:0000}.json");
                if (!File.Exists(path))
                {
                    continue;
                }
                var level = JsonConvert.DeserializeObject<LevelData>(File.ReadAllText(path));
                if (level?.productionLines == null)
                {
                    continue;
                }
                foreach (var line in level.productionLines)
                {
                    line.productionLineVisualType = line.rotationType == RotationType.Rotate_0
                        ? ProductionLineVisualType.SafeStraight
                        : ProductionLineVisualType.SafeCurvedRight;
                }
                File.WriteAllText(path, JsonConvert.SerializeObject(level, Formatting.None));
            }
            AssetDatabase.Refresh();
            Debug.Log("Repaired deterministic production-line visual keys for levels 2-6. Timer and grid findings remain unchanged.");
        }

        [MenuItem("MyMenu/Coffee Run/Repair missing early-level timers and duplicate grid")]
        public static void RepairMissingEarlyTimersAndDuplicateGrid()
        {
            for (var index = 4; index <= 6; index++)
            {
                var path = Path.Combine(LevelDirectory, $"{index:0000}.json");
                if (!File.Exists(path))
                {
                    continue;
                }
                var level = JsonConvert.DeserializeObject<LevelData>(File.ReadAllText(path));
                if (level == null)
                {
                    continue;
                }
                level.duration = 180f;
                level.gridPositions = level.gridPositions
                    .GroupBy(position => $"{position.x},{position.y}")
                    .Select(group => group.First())
                    .ToList();
                File.WriteAllText(path, JsonConvert.SerializeObject(level, Formatting.None));
            }
            AssetDatabase.Refresh();
            Debug.Log("Repaired Levels 4-6 to 180 seconds and removed duplicate grid cells.");
        }

        private static List<string> Validate(int expectedIndex, LevelData level)
        {
            var errors = new List<string>();
            if (level == null)
            {
                errors.Add("invalid JSON");
                return errors;
            }
            if (level.levelIndex != expectedIndex)
            {
                errors.Add($"levelIndex={level.levelIndex}");
            }
            if (level.duration <= 0)
            {
                errors.Add($"duration={level.duration}");
            }

            var grid = new HashSet<string>(level.gridPositions.Select(position => $"{position.x},{position.y}"));
            if (grid.Count != level.gridPositions.Count)
            {
                errors.Add("duplicate grid cell");
            }
            var occupied = new HashSet<string>();
            var capacity = new Dictionary<ColorType, int>();
            foreach (var container in level.containers ?? new List<ContainerSaveData>())
            {
                var anchor = container.position.ToVector2Int();
                foreach (var local in ContainerSaveDataExtensions.GetPartPositions(container))
                {
                    var cell = anchor + local;
                    var key = $"{cell.x},{cell.y}";
                    if (!grid.Contains(key)) errors.Add($"container outside grid at {key}");
                    if (!occupied.Add(key)) errors.Add($"container overlap at {key}");
                }
                if (container.containerData.isStone) continue;
                var colorData = container.containerData.containerColorData;
                var colors = colorData.colors != null && colorData.colors.Count > 0
                    ? colorData.colors
                    : new List<ColorType> { colorData.colorType };
                var amounts = colorData.colorAmounts;
                if (amounts == null || amounts.Count != colors.Count)
                {
                    errors.Add("container color amount metadata mismatch");
                    continue;
                }
                for (var colorIndex = 0; colorIndex < colors.Count; colorIndex++)
                {
                    var color = colors[colorIndex];
                    capacity[color] = capacity.TryGetValue(color, out var current)
                        ? current + amounts[colorIndex]
                        : amounts[colorIndex];
                }
            }
            var demand = new Dictionary<ColorType, int>();
            foreach (var line in level.productionLines ?? new List<ProductionLineSaveData>())
            {
                if (line.productionLineVisualType == ProductionLineVisualType.LegacyRandom)
                {
                    errors.Add("production line uses LegacyRandom");
                }
                var linePosition = line.position.ToVector2Int();
                var intakeDirection = line.rotationType switch
                {
                    RotationType.Rotate_0 => new Vector2Int(0, -1),
                    RotationType.Rotate_90 => new Vector2Int(1, 0),
                    RotationType.Rotate_180 => new Vector2Int(0, 1),
                    RotationType.Rotate_270 => new Vector2Int(-1, 0),
                    _ => Vector2Int.zero,
                };
                var intakeCell = linePosition + intakeDirection;
                if (!grid.Contains($"{intakeCell.x},{intakeCell.y}"))
                {
                    errors.Add($"production line points outside grid at {intakeCell.x},{intakeCell.y}");
                }
                foreach (var item in line.productionCollections ?? new List<ProductionCollectionSaveData>())
                {
                    demand[item.colorType] = demand.TryGetValue(item.colorType, out var current)
                        ? current + item.Amount
                        : item.Amount;
                }
            }
            foreach (var color in capacity.Keys.Union(demand.Keys))
            {
                capacity.TryGetValue(color, out var available);
                demand.TryGetValue(color, out var required);
                if (available != required) errors.Add($"{color} capacity={available}, demand={required}");
            }
            return errors.Distinct().ToList();
        }
    }
}
