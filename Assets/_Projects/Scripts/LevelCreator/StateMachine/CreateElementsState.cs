
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class CreateElementsState : DraftUtils.IState
{
    private LevelCreator _levelCreator;

    [ShowInInspector][ReadOnly] private List<Container> snapContainers = new();
    [ShowInInspector][ReadOnly] private List<ProductionLine> snapProductionLines = new();
    public void SetLevelCreator(LevelCreator levelCreator)
    {
        _levelCreator = levelCreator;
    }
    public void FixedUpdate()
    {

    }

    public void OnEnter()
    {
    }

    public void OnExit()
    {

    }

    public void Update()
    {
        snapContainers = MonoBehaviour.FindObjectsOfType<Container>().ToList();
        foreach (var container in snapContainers)
        {
            if (container.Data == null)
            {
                ContainerSaveData containerData = new();
                containerData.rotationType = RotationType.Rotate_0;
                containerData.containerData.containerMaterialType = ContainerMaterialType.Color;
                container.SetData(containerData);
            }

            if (container.Data.containerData.containerShapeType == ContainerShapeType.None)
            {
                container.Data.containerData.containerShapeType = container.ShapeType;
            }


            Vector2Int cellPosition = _levelCreator.LevelObjectSpawner.Grid.WorldToCell(container.transform.position);
            Vector3 position = _levelCreator.LevelObjectSpawner.Grid.CellToWorld(cellPosition);
            position.y = container.transform.position.y;
            container.transform.position = position;

            container.Data.position = cellPosition;
            container.Data.flipX = container.ShapeRoot != null && container.ShapeRoot.localScale.x < 0f;
            container.Reload();
        }
        AddContainersToLevelData(snapContainers);


        snapProductionLines = MonoBehaviour.FindObjectsOfType<ProductionLine>().ToList();
        foreach (var productionLine in snapProductionLines)
        {
            if (productionLine.Data == null || productionLine.Data.productionLineSaveData == null)
            {
                ProductionLineSaveData plData = new();
                plData.rotationType = RotationType.Rotate_0;
                productionLine.SetData(plData);
            }

            Vector2Int cellPosition = _levelCreator.LevelObjectSpawner.Grid.WorldToCell(productionLine.transform.position);
            Vector3 position = _levelCreator.LevelObjectSpawner.Grid.CellToWorld(cellPosition);
            position.y = productionLine.transform.position.y;
            productionLine.transform.position = position;

            productionLine.Data.productionLineSaveData.position = new SerializableVector2Int(cellPosition);
        }
        AddProductionLinesToLevelData(snapProductionLines);

    }
    private void AddContainersToLevelData(List<Container> containers)
    {
        _levelCreator.LevelData.containers = containers.Select(x => x.Data).ToList();
        _levelCreator.LevelData.Save(_levelCreator.LevelDataPath);
    }

    private void AddProductionLinesToLevelData(List<ProductionLine> productionLines)
    {
        _levelCreator.LevelData.productionLines = productionLines.Select(x => x.Data.productionLineSaveData).ToList();
        _levelCreator.LevelData.Save(_levelCreator.LevelDataPath);
    }

}
