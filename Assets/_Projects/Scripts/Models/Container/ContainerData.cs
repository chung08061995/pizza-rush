
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ContainerColorData
{
    public ColorType colorType;
}
public class ContainerIceData
{
    public int iceAmount;
    // Data của container sẽ xuất hiện sau khi băng tan hết (quy định là cùng Shape với container hiện tại)
    public ContainerData innerContainerData;
}
public class ContainerBoombData
{
    public int boombAmount;
}
public class ContainerKeyData
{
    public int keyAmount;
}

[System.Serializable]
public class ContainerData
{
    public ContainerShapeType containerShapeType;
    public ContainerMaterialType containerMaterialType;
    public ContainerMovementType containerMovementType;

    public ContainerColorData containerColorData = new();
    public ContainerIceData containerIceData = new();
    public ContainerBoombData containerBoombData = new();
    public ContainerKeyData containerKeyData = new();
}

public class SplitContainerData
{
    public Vector2Int Position;
    public ContainerShapeType containerShapeType;
    public RotationType rotationType;
}

public static class ContainerDataUtils
{
    public static bool CanMoving(ContainerData data)
    {
        if (data.containerMaterialType == ContainerMaterialType.Ice)
        {
            return false;
        }
        if (data.containerMovementType == ContainerMovementType.Blocked)
        {
            return false;
        }
        return true;
    }

    public static List<SplitContainerData> Split(ContainerData data)
    {
        if (data.containerShapeType == ContainerShapeType.L_1x1)
        {
            return new()
            {
                new SplitContainerData()
                {
                    containerShapeType = ContainerShapeType.Rectangle_1x2,
                    rotationType = RotationType.Rotate_0,
                    Position = Vector2Int.zero
                },
                new SplitContainerData()
                {
                    containerShapeType = ContainerShapeType.Rectangle_1x1,
                    rotationType = RotationType.Rotate_0,
                    Position = new Vector2Int(1, 1)
                },
            };
        }

        if (data.containerShapeType == ContainerShapeType.L_1x2)
        {
            return new()
            {
                new SplitContainerData()
                {
                    containerShapeType = ContainerShapeType.Rectangle_1x3,
                    rotationType = RotationType.Rotate_0,
                    Position = Vector2Int.zero
                },
                new SplitContainerData()
                {
                    containerShapeType = ContainerShapeType.Rectangle_1x1,
                    rotationType = RotationType.Rotate_0,
                    Position = new Vector2Int(1, 2)
                },
            };
        }

        if (data.containerShapeType == ContainerShapeType.Rectangle_1x3)
        {
            return new()
            {
                new SplitContainerData()
                {
                    containerShapeType = ContainerShapeType.Rectangle_1x1,
                    rotationType = RotationType.Rotate_0,
                    Position = Vector2Int.zero
                },
                new SplitContainerData()
                {
                    containerShapeType = ContainerShapeType.Rectangle_1x2,
                    rotationType = RotationType.Rotate_0,
                    Position = new Vector2Int(0, 1)
                },
            };
        }

        if (data.containerShapeType == ContainerShapeType.Rectangle_2x2)
        {
            return new()
            {
                new SplitContainerData()
                {
                    containerShapeType = ContainerShapeType.Rectangle_1x2,
                    rotationType = RotationType.Rotate_0,
                    Position = Vector2Int.zero
                },
                new SplitContainerData()
                {
                    containerShapeType = ContainerShapeType.Rectangle_1x2,
                    rotationType = RotationType.Rotate_0,
                    Position = new Vector2Int(1, 0)
                },
            };
        }

        if (data.containerShapeType == ContainerShapeType.Rectangle_1x2)
        {
            return new()
            {
                new SplitContainerData()
                {
                    containerShapeType = ContainerShapeType.Rectangle_1x1,
                    rotationType = RotationType.Rotate_0,
                    Position = Vector2Int.zero
                },
                new SplitContainerData()
                {
                    containerShapeType = ContainerShapeType.Rectangle_1x1,
                    rotationType = RotationType.Rotate_0,
                    Position = new Vector2Int(0, 1)
                },
            };
        }

        return new();
    }

}