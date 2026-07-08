using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class WritePositionState : DraftUtils.IState
{
    private LevelCreator _levelCreator;

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



        // AddContainersToLevelData();
        // AddProductionLinesToLevelData();
    }


    private void AddContainerToLevelData(Container container)
    {
        var containerSaveData = new ContainerSaveData
        {
            //containerType = container.ContainerType,
            position = new SerializableVector2Int(_levelCreator.LevelObjectSpawner.Grid.WorldToCell(container.transform.position))
        };
        _levelCreator.LevelData.containers.Add(containerSaveData);
        _levelCreator.LevelData.Save(_levelCreator.LevelDataPath);
    }

    private void AddProductionLineToLevelData(ProductionLine productionLine)
    {
        var plSave = new ProductionLineSaveData
        {
            position = new SerializableVector2Int(_levelCreator.LevelObjectSpawner.Grid.WorldToCell(productionLine.transform.position)),
            rotationType = productionLine.Data?.productionLineSaveData?.rotationType ?? RotationType.Rotate_0
        };
        _levelCreator.LevelData.productionLines.Add(plSave);
        _levelCreator.LevelData.Save(_levelCreator.LevelDataPath);
    }


    // [Button]
    // private void AddContainersToLevelData()
    // {
    //     _levelCreator.LevelData.containers.Clear();
    //     foreach (var container in snapContainers)
    //     {
    //         AddContainerToLevelData(container);
    //     }
    //     _levelCreator.LevelData.Save(_levelCreator.LevelDataPath);
    // }
    // [Button]
    // private void AddProductionLinesToLevelData()
    // {
    //     _levelCreator.LevelData.productionLines.Clear();
    //     foreach (var pl in snapProductionLines)
    //     {
    //         AddProductionLineToLevelData(pl);
    //     }
    //     _levelCreator.LevelData.Save(_levelCreator.LevelDataPath);
    // }
}