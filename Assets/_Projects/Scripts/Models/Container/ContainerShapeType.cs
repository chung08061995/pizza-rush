using System.Collections.Generic;
using UnityEngine;



// Chỉ quy định hình dạng
public enum ContainerShapeType
{
    None = 0,
    Rectangle_1x1 = 100,
    Rectangle_1x2 = 101,
    Rectangle_1x3 = 102,


    L_1x1 = 200,
    L_1x2 = 201,


    Rectangle_2x2 = 300,


    T = 400,

    Plus = 500,
}

public static class ContainerShapeTypeExtensions
{
    public static List<Vector2Int> GetPartPositions(ContainerShapeType containerShapeType)
    {
        if (containerShapeType == ContainerShapeType.L_1x1)
        {
            return new()
            {
                Vector2Int.zero,
                new Vector2Int(0, 1),
                new Vector2Int(1, 1)
            };
        }
        if (containerShapeType == ContainerShapeType.L_1x2)
        {
            return new()
            {
                Vector2Int.zero,
                new Vector2Int(0, 1),
                new Vector2Int(0, 2),
                new Vector2Int(1, 2)
            };
        }
        if (containerShapeType == ContainerShapeType.Rectangle_1x1)
        {
            return new()
            {
                Vector2Int.zero
            };
        }
        if (containerShapeType == ContainerShapeType.Rectangle_1x2)
        {
            return new()
            {
                Vector2Int.zero,
                new Vector2Int(0, 1)
            };
        }
        if (containerShapeType == ContainerShapeType.Rectangle_1x3)
        {
            return new()
            {
                Vector2Int.zero,
                new Vector2Int(0, 1),
                new Vector2Int(0, 2)
            };
        }
        if (containerShapeType == ContainerShapeType.Rectangle_2x2)
        {
            return new()
            {
                Vector2Int.zero,
                new Vector2Int(1, 0),
                new Vector2Int(0, 1),
                new Vector2Int(1, 1)
            };
        }
        if (containerShapeType == ContainerShapeType.T)
        {
            return new()
            {
                Vector2Int.zero,
                new Vector2Int(0, 1),
                new Vector2Int(-1, 1),
                new Vector2Int(1, 1)
            };
        }
        if (containerShapeType == ContainerShapeType.Plus)
        {
            return new()
            {
                Vector2Int.zero,
                new Vector2Int(0, 1),
                new Vector2Int(-1, 1),
                new Vector2Int(1, 1),
                new Vector2Int(0, 2),
            };
        }
        return new();
    }
}