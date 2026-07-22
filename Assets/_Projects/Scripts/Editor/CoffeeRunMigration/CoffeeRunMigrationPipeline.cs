using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace CoffeeRunMigration
{
    public static class CoffeeRunMigrationPipeline
    {
        private const string NormalizedDirectory = "CoffeeRunMigration/Normalized";
        private const string OutputDirectory = "Assets/_Projects/Resources/LevelData";
        private const string ReportPath = "CoffeeRunMigration/Reports/conversion-manifest.json";

        [MenuItem("MyMenu/Coffee Run/Convert normalized levels")]
        public static void ConvertAll()
        {
            var manifest = new CoffeeRunManifest { generatedUtc = DateTime.UtcNow.ToString("O") };
            Directory.CreateDirectory(NormalizedDirectory);
            Directory.CreateDirectory(OutputDirectory);
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "CoffeeRunMigration/Reports");

            foreach (var sourcePath in Directory.GetFiles(NormalizedDirectory, "*.json").OrderBy(path => path))
            {
                var entry = ConvertOne(sourcePath);
                manifest.levels.Add(entry);
            }

            File.WriteAllText(ReportPath, JsonConvert.SerializeObject(manifest, Formatting.Indented));
            AssetDatabase.Refresh();

            var exact = manifest.levels.Count(entry => entry.status == CoffeeRunComparisonStatus.Exact);
            var unsupported = manifest.levels.Count(entry => entry.status == CoffeeRunComparisonStatus.Unsupported);
            var mismatch = manifest.levels.Count - exact - unsupported;
            Debug.Log($"Coffee Run conversion complete: Exact={exact}, Mismatch={mismatch}, Unsupported={unsupported}. See {ReportPath}");
        }

        [MenuItem("MyMenu/Coffee Run/Validate converted levels")]
        public static void ValidateAll()
        {
            ConvertAll();
        }

        private static CoffeeRunManifestEntry ConvertOne(string sourcePath)
        {
            var entry = new CoffeeRunManifestEntry { sourceFile = sourcePath.Replace('\\', '/') };
            try
            {
                var source = JsonConvert.DeserializeObject<CoffeeRunLevelRecord>(File.ReadAllText(sourcePath));
                entry.level = source?.level ?? 0;
                entry.sourceLevel = source?.sourceLevel > 0 ? source.sourceLevel : entry.level;
                var expectedName = $"{entry.level:0000}.json";
                if (!string.Equals(Path.GetFileName(sourcePath), expectedName, StringComparison.Ordinal))
                {
                    entry.status = CoffeeRunComparisonStatus.Mismatch;
                    entry.messages.Add($"Source filename must be {expectedName}.");
                    return entry;
                }

                var outputPath = Path.Combine(OutputDirectory, expectedName).Replace('\\', '/');
                entry.outputFile = outputPath;
                var reward = ReadPizzaRushReward(outputPath);
                if (!CoffeeRunLevelConverter.TryConvert(source, reward, out var output, out var report))
                {
                    entry.status = report.status;
                    entry.messages.AddRange(report.messages);
                    return entry;
                }

                // The destination is touched only after every mapping and validation succeeds.
                File.WriteAllText(outputPath, JsonConvert.SerializeObject(output, Formatting.None));
                entry.status = CoffeeRunComparisonStatus.Exact;
                entry.messages.Add("Normalized source and output are exact.");
                return entry;
            }
            catch (Exception exception)
            {
                entry.status = CoffeeRunComparisonStatus.Mismatch;
                entry.messages.Add(exception.Message);
                return entry;
            }
        }

        private static int ReadPizzaRushReward(string outputPath)
        {
            if (!File.Exists(outputPath))
            {
                return 50;
            }
            try
            {
                return JsonConvert.DeserializeObject<LevelData>(File.ReadAllText(outputPath))?.goldReward ?? 50;
            }
            catch
            {
                return 50;
            }
        }
    }
}
