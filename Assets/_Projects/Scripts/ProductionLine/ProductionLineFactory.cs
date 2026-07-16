using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ProductionLineFactory : DraftUtils.DraftMonoBehaviour
{

    [SerializeField] private List<ProductionLine> productionLines = new();

    public void SpawnFromLevelData(
        LevelData levelData,
        DraftUtils.Pooler<ProductionLine> pooler,
        DraftUtils.GridXZ gridSystem)
    {
        if (levelData == null)
        {
            return;
        }
        if (levelData.containers == null)
        {
            return;
        }

        pooler.Factory = new DraftUtils.ComponentInstantiatePoolFactory<ProductionLine>();

        pooler.DespawnAll();

        foreach (var savedContainer in levelData.productionLines)
        {
            if (savedContainer == null)
            {
                continue;
            }
            var prefab = GetPrefab(savedContainer);
            if (prefab == null)
            {
                continue;
            }

            pooler.SetItem(prefab);
            var container = pooler.Spawn();
            container.transform.position = gridSystem.CellToWorld(savedContainer.position.ToVector2Int());
            container.SetData(savedContainer);


            // Check cho trường hợp creator
            if(savedContainer.productionCollections != null && savedContainer.productionCollections.Count > 0)
            {
                container.SetColor(ColorTypeUtils.ToColor(savedContainer.productionCollections[0].colorType));
            }
        }
    }

    private ProductionLine GetPrefab(ProductionLineSaveData savedLine)
    {
        if (productionLines == null || productionLines.Count == 0)
        {
            return null;
        }

        if (savedLine.productionLineVisualType == ProductionLineVisualType.LegacyRandom)
        {
            return DraftUtils.Utils.ListUtils.GetRandomElement(productionLines);
        }

        string requiredPrefabName = savedLine.productionLineVisualType switch
        {
            ProductionLineVisualType.SafeStraight => "ProductionLine_Straing",
            ProductionLineVisualType.SafeCurvedRight => "ProductionLine_Belt",
            ProductionLineVisualType.SafeCurvedLeft => "ProductionLine_Belt",
            _ => null,
        };

        if (string.IsNullOrEmpty(requiredPrefabName))
        {
            Debug.LogError($"Unsupported production-line visual: {savedLine.productionLineVisualType}");
            return null;
        }

        var prefab = productionLines.FirstOrDefault(line =>
            line != null &&
            string.Equals(line.name, requiredPrefabName, System.StringComparison.Ordinal));

        if (prefab == null)
        {
            Debug.LogError($"Production-line prefab '{requiredPrefabName}' is not configured on {name}.");
        }

        return prefab;
    }
}
