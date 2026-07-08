using System.Collections.Generic;
using UnityEngine;

public enum ContainerMovementType
{
    None = 0,
    Walkable = 100,
    Horizontal = 200,   // sửa typo "Horizomtal"
    Vertical = 300,
    Blocked = 400,   // "NoWalkable" → Blocked / Impassable / Restricted
}

public static class ContainerMovementTypeExtensions
{
    /// <summary>
    /// Lấy danh sách các hướng di chuyển được phép dựa trên loại di chuyển của container.
    /// </summary>
    /// <param name="movementType">Loại di chuyển (ngang, dọc, tự do, bị khóa)</param>
    /// <returns>Danh sách các vector hướng (grid X/Y)</returns>
    public static List<Vector2Int> GetAllowedDirections(ContainerMovementType movementType)
    {
        List<Vector2Int> directions = new List<Vector2Int>();
        if (movementType == ContainerMovementType.Horizontal)
        {
            // Horizontal movement type in CommonFunction locks X, so we can only move along Z (grid Y)
            directions.Add(Vector2Int.up);
            directions.Add(Vector2Int.down);
        }
        else if (movementType == ContainerMovementType.Vertical)
        {
            // Vertical movement type in CommonFunction locks Z, so we can only move along X (grid X)
            directions.Add(Vector2Int.left);
            directions.Add(Vector2Int.right);
        }
        else if (movementType != ContainerMovementType.Blocked)
        {
            // Free movement
            directions.Add(Vector2Int.up);
            directions.Add(Vector2Int.down);
            directions.Add(Vector2Int.left);
            directions.Add(Vector2Int.right);
        }
        return directions;
    }
}