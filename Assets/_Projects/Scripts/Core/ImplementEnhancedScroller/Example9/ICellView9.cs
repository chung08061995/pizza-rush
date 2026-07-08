/// <summary>
/// Interface cho master cell view trong EnhancedScroller9 (Nested Scrollers).
/// Mỗi master cell chứa 1 child scroller bên trong.
/// </summary>
public interface ICellView9<TData>
{
    /// <summary>
    /// Gán dữ liệu cho master cell (bao gồm child data cho nested scroller).
    /// </summary>
    /// <param name="data">Dữ liệu master cell.</param>
    /// <param name="index">Vị trí trong danh sách master.</param>
    void SetData(TData data, int index);
}
