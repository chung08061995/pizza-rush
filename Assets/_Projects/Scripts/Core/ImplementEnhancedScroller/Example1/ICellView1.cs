/// <summary>
/// Interface cho cell view generic. Implement cái này trên EnhancedScrollerCellView của bạn.
/// </summary>
public interface ICellView1<TData>
{
    void SetData(TData data, int index);
}

/// <summary>
/// Interface tùy chọn: nếu data implement cái này thì scroller sẽ lấy cellSize từ data
/// thay vì dùng cellSize cố định chung. Hữu ích khi có nhiều loại cell kích thước khác nhau.
/// </summary>
public interface ICellSize
{
    float CellSize { get; }
}
