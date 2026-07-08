using System;
using UnityEngine;
using System.Collections.Generic;

public class ContainerFactory : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] private Container rectangle1x1Container;
    [SerializeField] private Container rectangle1x2Container;
    [SerializeField] private Container rectangle1x3Container;
    [SerializeField] private Container l1x1Container;
    [SerializeField] private Container l1x2Container;
    [SerializeField] private Container rectangle2x2Container;
    [SerializeField] private Container tContainer;
    [SerializeField] private Container plusContainer;
    public void SpawnFromLevelData(
        LevelData levelData,
        DraftUtils.ObjectCreator<Container> pooler,
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

        pooler.DespawnAll();
        foreach (var savedContainer in levelData.containers)
        {
            if (savedContainer == null)
            {
                continue;
            }
            SpawnSingleContainer(savedContainer, pooler, gridSystem);
        }
    }

    public Container SpawnSingleContainer(
        ContainerSaveData savedContainer,
        DraftUtils.ObjectCreator<Container> pooler,
        DraftUtils.GridXZ gridSystem)
    {
        Container prefab = null;
        if (savedContainer.containerData.containerShapeType == ContainerShapeType.L_1x1)
        {
            prefab = l1x1Container;
        }
        if (savedContainer.containerData.containerShapeType == ContainerShapeType.L_1x2)
        {
            prefab = l1x2Container;
        }
        if (savedContainer.containerData.containerShapeType == ContainerShapeType.Rectangle_1x1)
        {
            prefab = rectangle1x1Container;
        }
        if (savedContainer.containerData.containerShapeType == ContainerShapeType.Rectangle_1x2)
        {
            prefab = rectangle1x2Container;
        }
        if (savedContainer.containerData.containerShapeType == ContainerShapeType.Rectangle_1x3)
        {
            prefab = rectangle1x3Container;
        }
        if (savedContainer.containerData.containerShapeType == ContainerShapeType.Rectangle_2x2)
        {
            prefab = rectangle2x2Container;
        }
        if (savedContainer.containerData.containerShapeType == ContainerShapeType.T)
        {
            prefab = tContainer;
        }
        if (savedContainer.containerData.containerShapeType == ContainerShapeType.Plus)
        {
            prefab = plusContainer;
        }
        if (prefab == null) return null;

        pooler.SetItem(prefab);
        var container = pooler.Spawn();
        var postionSpawn = gridSystem.CellToWorld(savedContainer.position.ToVector2Int());
        container.transform.position = postionSpawn;

        container.SetData(savedContainer);

        // Configure SmoothMover for smooth snapping movement
        var mover = container.StateMachine.MoveToPositionState.SmoothMover;
        mover.SetModel(container.transform);
        mover.SetImmediateSnapDistance(.1f);
        mover.SetTargetPosition(postionSpawn);
        mover.SetMoveSpeed(DataManager.Instance.ParametterGameConfigSO.ContainerMoveSpeed);
        mover.SetOnMoveComplete(mover.Pause);
        mover.StartMoving();

        container.StateMachine.SetData(container);
        return container;
    }
}
