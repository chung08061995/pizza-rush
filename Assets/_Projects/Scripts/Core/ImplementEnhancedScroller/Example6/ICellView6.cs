/// <summary>
/// Interface cho cell view trong EnhancedScroller6 (Snapping).
/// Cell nhận data + index + trạng thái snapped (đang được snap vào hay không).
/// </summary>
public interface ICellView6<TData>
{
    /// <summary>
    /// Gán dữ liệu vào cell.
    /// </summary>
    /// <param name="data">Dữ liệu cell.</param>
    /// <param name="index">Vị trí trong danh sách.</param>
    /// <param name="isSnapped">Cell này có đang được snap (focus) hay không.</param>
    void SetData(TData data, int index, bool isSnapped);
}
