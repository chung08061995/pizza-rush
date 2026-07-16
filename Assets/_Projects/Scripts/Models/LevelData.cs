using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class ProductionCollectionSaveData
{
    public ColorType colorType;
    public int Amount;

}

[System.Serializable]
public class ContainerSaveData
{
    public SerializableVector2Int position;
    public RotationType rotationType;
    public bool flipX;
    public ContainerData containerData = new();
}
public static class ContainerSaveDataExtensions
{
    public static List<Vector2Int> GetPartPositions(ContainerSaveData containerSaveData)
    {
        var partPosition = ContainerShapeTypeExtensions.GetPartPositions(containerSaveData.containerData.containerShapeType);

        List<Vector2Int> result = new List<Vector2Int>();
        float angle = RotationTypeExtensions.ConvertToAngle(containerSaveData.rotationType);
        Quaternion rotation = Quaternion.Euler(0, angle, 0);

        foreach (var part in partPosition)
        {
            var localPart = containerSaveData.flipX
                ? new Vector2Int(-part.x, part.y)
                : part;
            Vector3 rotated = rotation * new Vector3(localPart.x, 0, localPart.y);
            result.Add(new Vector2Int(Mathf.RoundToInt(rotated.x), Mathf.RoundToInt(rotated.z)));
        }

        return result;
    }

    public static Vector2Int TransformLocalPosition(ContainerSaveData containerSaveData, Vector2Int localPosition)
    {
        var flippedPosition = containerSaveData.flipX
            ? new Vector2Int(-localPosition.x, localPosition.y)
            : localPosition;

        float angle = RotationTypeExtensions.ConvertToAngle(containerSaveData.rotationType);
        Quaternion rotation = Quaternion.Euler(0, angle, 0);
        Vector3 rotated = rotation * new Vector3(flippedPosition.x, 0, flippedPosition.y);
        return new Vector2Int(Mathf.RoundToInt(rotated.x), Mathf.RoundToInt(rotated.z));
    }
}
[System.Serializable]
public class ProductionLineSaveData
{
    public ProductionLineMode productionLineMode;
    public ProductionLineVisualType productionLineVisualType;
    public SerializableVector2Int position;
    public RotationType rotationType;
    public List<ProductionCollectionSaveData> productionCollections = new();
}

public enum ProductionLineVisualType
{
    LegacyRandom = 0,
    SafeStraight = 1,
    SafeCurvedRight = 2,
    SafeCurvedLeft = 3,
}

public enum ProductionLineMode
{
    None = 0,
    Normal = 1,

}


[System.Serializable]
public class LevelData
{
    public int levelIndex;
    public List<SerializableVector2Int> gridPositions = new();
    public List<ContainerSaveData> containers = new();
    public List<ProductionLineSaveData> productionLines = new();
    public float duration = 0f;
    public int goldReward = 0;

    /// <summary>
    /// Tải level data từ file JSON
    /// </summary>
    public static LevelData Load(string path)
    {
        // Kiểm tra và tạo file nếu chưa tồn tại
        if (!File.Exists(path))
        {
            CreateEmptyFile(path);
        }

        LevelData levelData;
        if (!DraftUtils.Utils.JsonFileUtils.TryLoadJsonFromFile(path, out levelData))
        {
            CreateEmptyFile(path);
            levelData = new LevelData();
        }

        levelData.SetLevelIndexIfValid(ExtractLevelIndexFromPath(path));
        return levelData;
    }

    /// <summary>
    /// Lưu level data vào file JSON
    /// </summary>
    public void Save(string path)
    {
        SetLevelIndexIfValid(ExtractLevelIndexFromPath(path));
        DraftUtils.Utils.JsonFileUtils.SaveToFile(path, this);
    }

    /// <summary>
    /// Tạo file JSON rỗng
    /// </summary>
    private static void CreateEmptyFile(string filePath)
    {
        var levelData = new LevelData
        {
            levelIndex = ExtractLevelIndexFromPath(filePath),
            gridPositions = new()
        };

        DraftUtils.Utils.JsonFileUtils.SaveToFile(filePath, levelData);
    }

    public void SetLevelIndex(int index)
    {
        SetLevelIndexIfValid(index);
    }

    private void SetLevelIndexIfValid(int index)
    {
        if (index > 0)
        {
            levelIndex = index;
        }
    }

    private static int ExtractLevelIndexFromPath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return 0;
        }

        var fileName = Path.GetFileNameWithoutExtension(path);
        return int.TryParse(fileName, out var index) ? index : 0;
    }

    /// <summary>
    /// Thêm vị trí vào levelData
    /// </summary>
    public void AddPosition(SerializableVector2Int position)
    {
        DraftUtils.Utils.ListUtils.AddIfNotExists(gridPositions, position);
    }

    public void ShuffleColors()
    {
        var sourceColors = GetColorsFromLevelData();
        if (sourceColors.Count <= 1)
        {
            return;
        }

        var colorMap = new Dictionary<ColorType, ColorType>();
        var replacementColors = GetReplacementColors(sourceColors);

        if (replacementColors.Count == sourceColors.Count)
        {
            for (int i = 0; i < sourceColors.Count; i++)
            {
                colorMap[sourceColors[i]] = replacementColors[i];
            }
        }
        else
        {
            var mappedColors = new List<ColorType>(sourceColors);
            int attempt = 0;
            bool hasFixedPoint;
            do
            {
                for (int i = mappedColors.Count - 1; i > 0; i--)
                {
                    int j = UnityEngine.Random.Range(0, i + 1);
                    var temp = mappedColors[i];
                    mappedColors[i] = mappedColors[j];
                    mappedColors[j] = temp;
                }

                hasFixedPoint = false;
                for (int i = 0; i < sourceColors.Count; i++)
                {
                    if (sourceColors[i] == mappedColors[i])
                    {
                        hasFixedPoint = true;
                        break;
                    }
                }

                attempt++;
            }
            while (hasFixedPoint && attempt < 20);

            if (hasFixedPoint)
            {
                if (sourceColors.Count == 2)
                {
                    var temp = mappedColors[0];
                    mappedColors[0] = mappedColors[1];
                    mappedColors[1] = temp;
                }
                else
                {
                    for (int i = 0; i < sourceColors.Count; i++)
                    {
                        if (sourceColors[i] == mappedColors[i])
                        {
                            int swapIndex = (i + 1) % sourceColors.Count;
                            var temp = mappedColors[i];
                            mappedColors[i] = mappedColors[swapIndex];
                            mappedColors[swapIndex] = temp;
                        }
                    }
                }
            }

            for (int i = 0; i < sourceColors.Count; i++)
            {
                colorMap[sourceColors[i]] = mappedColors[i];
            }
        }

        ReplaceColors(colorMap);
    }

    private List<ColorType> GetReplacementColors(List<ColorType> sourceColors)
    {
        var availableColors = new List<ColorType>();
        foreach (ColorType color in System.Enum.GetValues(typeof(ColorType)))
        {
            if (color == ColorType.None || color == ColorType.White || sourceColors.Contains(color))
            {
                continue;
            }

            availableColors.Add(color);
        }

        if (availableColors.Count < sourceColors.Count)
        {
            return new List<ColorType>();
        }

        var replacementColors = new List<ColorType>();
        var shuffledPool = new List<ColorType>(availableColors);
        while (replacementColors.Count < sourceColors.Count && shuffledPool.Count > 0)
        {
            int index = UnityEngine.Random.Range(0, shuffledPool.Count);
            replacementColors.Add(shuffledPool[index]);
            shuffledPool.RemoveAt(index);
        }

        return replacementColors;
    }

    private List<ColorType> GetColorsFromLevelData()
    {
        var colors = new List<ColorType>();

        if (containers != null)
        {
            foreach (var container in containers)
            {
                var colorData = container?.containerData?.containerColorData;
                if (colorData == null) continue;
                var containerColors = colorData.colors != null && colorData.colors.Count > 0
                    ? colorData.colors
                    : new List<ColorType> { colorData.colorType };
                foreach (var color in containerColors)
                {
                    if (color != ColorType.None && !colors.Contains(color))
                    {
                        colors.Add(color);
                    }
                }
            }
        }

        if (productionLines != null)
        {
            foreach (var productionLine in productionLines)
            {
                if (productionLine?.productionCollections == null) continue;
                foreach (var collection in productionLine.productionCollections)
                {
                    if (collection == null) continue;
                    var color = collection.colorType;
                    if (color != ColorType.None && !colors.Contains(color))
                    {
                        colors.Add(color);
                    }
                }
            }
        }

        return colors;
    }

    private void ReplaceColors(Dictionary<ColorType, ColorType> colorMap)
    {
        if (containers != null)
        {
            foreach (var container in containers)
            {
                if (container?.containerData == null) continue;
                ReplaceContainerDataColors(container.containerData, colorMap);
            }
        }

        if (productionLines != null)
        {
            foreach (var productionLine in productionLines)
            {
                if (productionLine?.productionCollections == null) continue;
                foreach (var collection in productionLine.productionCollections)
                {
                    if (collection == null) continue;
                    var oldColor = collection.colorType;
                    if (colorMap.TryGetValue(oldColor, out var newColor))
                    {
                        collection.colorType = newColor;
                    }
                }
            }
        }
    }

    private static void ReplaceContainerDataColors(ContainerData data, Dictionary<ColorType, ColorType> colorMap)
    {
        if (data?.containerColorData != null)
        {
            var colorData = data.containerColorData;
            if (colorMap.TryGetValue(colorData.colorType, out var newColor))
            {
                colorData.colorType = newColor;
            }
            if (colorData.colors != null)
            {
                for (var colorIndex = 0; colorIndex < colorData.colors.Count; colorIndex++)
                {
                    if (colorMap.TryGetValue(colorData.colors[colorIndex], out var newLayerColor))
                    {
                        colorData.colors[colorIndex] = newLayerColor;
                    }
                }
            }
        }

        var inner = data?.containerIceData?.innerContainerData;
        if (inner != null && !ReferenceEquals(inner, data))
        {
            ReplaceContainerDataColors(inner, colorMap);
        }
    }
}
