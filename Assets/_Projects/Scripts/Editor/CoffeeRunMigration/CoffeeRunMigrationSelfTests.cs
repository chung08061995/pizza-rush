using System;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace CoffeeRunMigration
{
    public static class CoffeeRunMigrationSelfTests
    {
        private const string LevelOnePath = "CoffeeRunMigration/Normalized/0001.json";

        [MenuItem("MyMenu/Coffee Run/Run pipeline self-tests")]
        public static void Run()
        {
            var sourceJson = File.ReadAllText(LevelOnePath);
            var source = JsonConvert.DeserializeObject<CoffeeRunLevelRecord>(sourceJson);

            Assert(CoffeeRunLevelConverter.TryConvert(source, 50, out var output, out var exactReport),
                $"Level 1 should convert: {string.Join("; ", exactReport.messages)}");
            Assert(exactReport.status == CoffeeRunComparisonStatus.Exact, "Level 1 should be Exact.");
            Assert(output.duration == 180f, "Level 1 timer should be 180 seconds.");
            Assert(output.gridPositions.Count == 20, "Level 1 should contain 20 grid cells.");
            Assert(output.containers.Count == 2 && output.productionLines.Count == 2,
                "Level 1 should contain two containers and two production lines.");
            Assert(output.productionLines[1].productionLineVisualType == ProductionLineVisualType.SafeCurvedLeft,
                "Level 1 bottom line should preserve its leftward path offset.");

            AssertConvertedSpecial(21, data =>
            {
                var frozen = data.containers.FindAll(container =>
                    container.containerData.containerMaterialType == ContainerMaterialType.Ice);
                Assert(frozen.Count == 2, "Level 21 should preserve two frozen containers.");
                Assert(frozen.TrueForAll(container =>
                        container.containerData.containerIceData.innerContainerData != null &&
                        container.containerData.containerIceData.innerContainerData.containerMaterialType != ContainerMaterialType.Ice),
                    "Every frozen container must contain a movable post-thaw payload.");
            });
            AssertConvertedSpecial(31, data =>
            {
                var layered = data.containers.Find(container => container.containerData.containerColorData.isLayerBox);
                Assert(layered != null && layered.containerData.containerColorData.colors.Count == 2,
                    "Level 31 should preserve a two-color LayerBox.");
                Assert(layered.containerData.containerColorData.colorAmounts.TrueForAll(amount => amount == 12),
                    "Level 31 LayerBox should preserve full capacity for both layers.");
                Assert(layered.containerData.containerColorData.colorType ==
                       layered.containerData.containerColorData.colors[0],
                    "Level 31 LayerBox should expose its outer color first.");
            });
            AssertConvertedSpecial(46, data =>
            {
                var stones = data.containers.FindAll(container => container.containerData.isStone);
                Assert(stones.Count == 2,
                    "Level 46 should preserve two stone obstacles.");
                Assert(stones.TrueForAll(container => ContainerDataUtils.CanMoving(container.containerData)),
                    "Level 46 stone obstacles should remain movable blockers.");
            });
            AssertConvertedSpecial(61, data =>
            {
                var multiColor = data.containers.Find(container => container.containerData.containerColorData.isMultiColor);
                Assert(multiColor != null &&
                       multiColor.containerData.containerColorData.colorAmounts[0] == 6 &&
                       multiColor.containerData.containerColorData.colorAmounts[1] == 6,
                    "Level 61 should preserve the 6/6 multi-color slot distribution.");
            });

            var capacityMismatch = JsonConvert.DeserializeObject<CoffeeRunLevelRecord>(sourceJson);
            capacityMismatch.productionLines[0].productionOrder[0].amount = 11;
            Assert(!CoffeeRunLevelConverter.TryConvert(capacityMismatch, 50, out _, out var mismatchReport),
                "A capacity mismatch must fail conversion.");
            Assert(mismatchReport.status == CoffeeRunComparisonStatus.Mismatch,
                "A capacity mismatch must report Mismatch.");

            var unsupportedVisual = JsonConvert.DeserializeObject<CoffeeRunLevelRecord>(sourceJson);
            unsupportedVisual.productionLines[0].visualKey = "ClosestLookingPrefab";
            Assert(!CoffeeRunLevelConverter.TryConvert(unsupportedVisual, 50, out _, out var unsupportedReport),
                "An unknown production-line visual must fail conversion.");
            Assert(unsupportedReport.status == CoffeeRunComparisonStatus.Unsupported,
                "An unknown visual must report Unsupported.");

            Debug.Log("Coffee Run migration self-tests passed (Exact, Mismatch and Unsupported gates).");
        }

        private static void AssertConvertedSpecial(int level, Action<LevelData> assertion)
        {
            var path = $"CoffeeRunMigration/Normalized/{level:0000}.json";
            var source = JsonConvert.DeserializeObject<CoffeeRunLevelRecord>(File.ReadAllText(path));
            Assert(CoffeeRunLevelConverter.TryConvert(source, 50, out var output, out var report),
                $"Level {level} should convert: {string.Join("; ", report.messages)}");
            assertion(output);
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
