/// <summary>
/// Interface cho cell view có kích thước dynamic (view-driven).
/// Cell tự tính height dựa trên nội dung (ví dụ: text dài ngắn khác nhau).
/// </summary>
/// <typeparam name="TData">Loại dữ liệu, phải implement IViewDrivenCellData để lưu cellSize.</typeparam>
public interface ICellView8<TData> where TData : IViewDrivenCellData
{
    /// <summary>
    /// Gán dữ liệu vào cell.
    /// </summary>
    /// <param name="data">Dữ liệu cần hiển thị.</param>
    /// <param name="index">Vị trí trong danh sách.</param>
    /// <param name="calculateLayout">
    /// true = pass đầu tiên, cell cần tính toán layout rồi ghi cellSize vào data.
    /// false = pass thứ 2, chỉ hiển thị bình thường.
    /// </param>
    void SetData(TData data, int index, bool calculateLayout);
}
