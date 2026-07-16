using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CoffeeRunMigration
{
    [Serializable]
    public sealed class CoffeeRunLevelRecord
    {
        public string sourcePackage = "com.coffee.run.puzzle";
        public string sourceVersion = "3.20.0";
        public int sourceVersionCode = 790;
        public string configVariant = "Default";
        public string extractionMethod;
        public int level;
        public int timerSeconds;
        public CoffeeRunGridRecord grid = new();
        public List<CoffeeRunContainerRecord> containers = new();
        public List<CoffeeRunProductionLineRecord> productionLines = new();
        public List<string> evidence = new();
    }

    [Serializable]
    public sealed class CoffeeRunGridRecord
    {
        public int rows;
        public int columns;
        public List<CoffeeRunPosition> cells = new();
    }

    [Serializable]
    public sealed class CoffeeRunPosition
    {
        public int x;
        public int y;
    }

    [Serializable]
    public sealed class CoffeeRunContainerRecord
    {
        public string id;
        public CoffeeRunPosition position = new();
        public string shape;
        public int rotationQuarterTurns;
        public bool flipX;
        public string movement = "Free";
        public string material = "Color";
        public string colorKey;
        public List<string> colorKeys = new();
        public List<int> colorAmounts = new();
        public CoffeeRunModifierRecord modifiers = new();
    }

    [Serializable]
    public sealed class CoffeeRunModifierRecord
    {
        public int iceLayers;
        public int bombCount;
        public int keyCount;
        public int stoneLayers;
        public int colorLayers;
        public bool layerBox;
        public bool cap;
        public bool linked;
        public bool barrier;
        public int ropes;
    }

    [Serializable]
    public sealed class CoffeeRunProductionLineRecord
    {
        public string id;
        public CoffeeRunPosition position = new();
        public int rotationQuarterTurns;
        public string visualKey;
        public List<CoffeeRunProductionItemRecord> productionOrder = new();
        public List<CoffeeRunPathSegmentRecord> path = new();
    }

    [Serializable]
    public sealed class CoffeeRunProductionItemRecord
    {
        public string colorKey;
        public int amount;
    }

    [Serializable]
    public sealed class CoffeeRunPathSegmentRecord
    {
        public string direction;
        public int length;
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum CoffeeRunComparisonStatus
    {
        Exact,
        Mismatch,
        Unsupported,
    }

    [Serializable]
    public sealed class CoffeeRunValidationReport
    {
        public int level;
        public CoffeeRunComparisonStatus status = CoffeeRunComparisonStatus.Exact;
        public List<string> messages = new();

        public void Mismatch(string message)
        {
            if (status != CoffeeRunComparisonStatus.Unsupported)
            {
                status = CoffeeRunComparisonStatus.Mismatch;
            }
            messages.Add(message);
        }

        public void Unsupported(string message)
        {
            status = CoffeeRunComparisonStatus.Unsupported;
            messages.Add(message);
        }
    }

    [Serializable]
    public sealed class CoffeeRunManifest
    {
        public string generatedUtc;
        public string sourcePackage = "com.coffee.run.puzzle";
        public string sourceVersion = "3.20.0";
        public int sourceVersionCode = 790;
        public string configVariant = "Default";
        public List<CoffeeRunManifestEntry> levels = new();
    }

    [Serializable]
    public sealed class CoffeeRunManifestEntry
    {
        public int level;
        public CoffeeRunComparisonStatus status;
        public string sourceFile;
        public string outputFile;
        public List<string> messages = new();
    }
}
