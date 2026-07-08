using System.Collections.Generic;
using UnityEngine;

public static class CommonFunction
{
    /// <summary>
    /// Điều chỉnh vị trí mục tiêu của container dựa trên loại di chuyển được phép.
    /// Hàm này giới hạn chuyển động của container theo các hạn chế cụ thể:
    /// - Nếu container bị khóa (Blocked), trả về vị trí ban đầu (không di chuyển)
    /// - Nếu container chỉ di chuyển ngang (Horizontal), giữ nguyên tọa độ X từ vị trí ban đầu
    /// - Nếu container chỉ di chuyển dọc (Vertical), giữ nguyên tọa độ Z từ vị trí ban đầu
    /// </summary>
    /// <param name="container">Container cần điều chỉnh vị trí</param>
    /// <param name="targetPosition">Vị trí mục tiêu mong muốn</param>
    /// <param name="grid">Grid để chuyển đổi giữa tọa độ grid và tọa độ thế giới</param>
    /// <returns>Vị trí mục tiêu đã được điều chỉnh phù hợp với hạn chế di chuyển</returns>
    public static Vector3 AdjustTargetPositionByMovementType(Container container, Vector3 targetPosition, DraftUtils.GridXZ grid)
    {
        var beginPosition = grid.CellToWorld(container.Data.position);
        if (container.Data.containerData.containerMovementType == ContainerMovementType.Blocked)
        {
            return beginPosition;
        }
        if (container.Data.containerData.containerMovementType == ContainerMovementType.Horizontal)
        {
            targetPosition.x = beginPosition.x;
        }
        if (container.Data.containerData.containerMovementType == ContainerMovementType.Vertical)
        {
            targetPosition.z = beginPosition.z;
        }

        return targetPosition;
    }


}