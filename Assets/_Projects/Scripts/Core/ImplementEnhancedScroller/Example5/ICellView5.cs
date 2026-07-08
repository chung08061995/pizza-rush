/// <summary>
/// Interface cho cell view trong EnhancedScroller5 (Lazy Load).
/// Cell load nội dung khi visible, clear khi bị recycle.
/// </summary>
public interface ICellView5<TData>
{
    /// <summary>
    /// Gán dữ liệu và bắt đầu load resource (gọi khi cell trở nên visible).
    /// </summary>
    void SetData(TData data, int index);

    /// <summary>
    /// Clear/hủy resource đang load (gọi khi cell bị recycle hoặc ẩn đi).
    /// </summary>
    void ClearData();
}
