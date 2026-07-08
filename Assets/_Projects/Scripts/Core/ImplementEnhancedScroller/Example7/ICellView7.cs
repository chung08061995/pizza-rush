/// <summary>
/// Interface cho cell view trong EnhancedScroller7 (Pull Down Refresh).
/// Giống ICellView1 — cell nhận data + index.
/// </summary>
public interface ICellView7<TData>
{
    void SetData(TData data, int index);
}
