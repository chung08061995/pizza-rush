/// <summary>
/// Interface cho cell view trong EnhancedScroller4 (Pagination).
/// Giống ICellView1 — cell nhận data + index.
/// </summary>
public interface ICellView4<TData>
{
    void SetData(TData data, int index);
}
