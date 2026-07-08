using System.Collections.Generic;

/// <summary>
/// Interface cho row cell view trong grid scroller (EnhancedScroller3).
/// Row cell chứa nhiều sub-cell (cột), nhận list data + offset để render 1 hàng.
/// </summary>
public interface ICellView3<TData>
{
    /// <summary>
    /// Gán dữ liệu cho 1 hàng trong grid.
    /// </summary>
    /// <param name="data">Toàn bộ danh sách dữ liệu.</param>
    /// <param name="startIndex">Index đầu tiên trong data cho hàng này.</param>
    /// <param name="cellsPerRow">Số cột mỗi hàng.</param>
    void SetData(IList<TData> data, int startIndex, int cellsPerRow);
}
