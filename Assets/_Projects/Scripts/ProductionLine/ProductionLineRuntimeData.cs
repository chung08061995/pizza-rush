using System.Collections.Generic;

[System.Serializable]
public class ProductionLineRuntimeData
{
    public ProductionLineSaveData productionLineSaveData;

    public List<ColorType> productionColors = new();
}

public static class ProductionLineRuntimeDataExensions
{
    public static void SetData(ProductionLineRuntimeData runtimeData, ProductionLineSaveData data)
    {
        runtimeData.productionLineSaveData = data;
        runtimeData.productionColors.Clear();
        foreach (var colorConfig in data.productionCollections)
        {
            for (int i = 0; i < colorConfig.Amount; i++)
            {
                runtimeData.productionColors.Add(colorConfig.colorType);
            }
        }
    }
    public static IEnumerable<ColorType> GetFirstColors(ProductionLineRuntimeData runtimeData, ColorType colorType)
    {
        foreach (var productionColor in runtimeData.productionColors)
        {
            if (colorType != productionColor)
            {
                break;
            }
            yield return productionColor;
        }
    }

    /// <summary>
    /// Kiểm tra màu của production đầu tiên trong line có khớp không.
    /// </summary>
    public static bool HasFirstProductionColor(ProductionLineRuntimeData runtimeData, ColorType colorType)
    {
        if (runtimeData.productionColors.Count == 0)
        {
            return false;
        }

        return runtimeData.productionColors[0] == colorType;
    }
}