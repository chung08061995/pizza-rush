using System;
using System.Collections.Generic;

/// <summary>
/// Interface cho row cell view trong EnhancedScroller10 (Grid + Selection).
/// Row cell chứa nhiều sub-cell, nhận data + offset + callback click + selectedIndex.
/// </summary>
public interface ICellView10<TData>
{
    /// <summary>
    /// Gán dữ liệu cho 1 hàng trong grid.
    /// </summary>
    /// <param name="data">Toàn bộ danh sách dữ liệu.</param>
    /// <param name="startIndex">Index đầu tiên trong data cho hàng này.</param>
    /// <param name="cellsPerRow">Số cột mỗi hàng.</param>
    /// <param name="selectedIndex">Flat index đang được chọn (-1 = chưa chọn).</param>
    /// <param name="onItemClicked">Callback khi sub-cell được click (truyền flat index).</param>
    void SetData(IList<TData> data, int startIndex, int cellsPerRow, int selectedIndex, Action<int> onItemClicked);
}
