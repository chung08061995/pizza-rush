using System;
using UnityEngine;
using System.Collections.Generic;

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
            var prefab = DraftUtils.Utils.ListUtils.GetRandomElement(productionLines);

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
}
