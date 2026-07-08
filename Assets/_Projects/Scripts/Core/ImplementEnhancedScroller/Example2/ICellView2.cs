/// <summary>
/// Interface cho cell view trong EnhancedScroller2.
/// Mỗi prefab cell implement cái này để nhận dữ liệu.
/// </summary>
public interface ICellView2<TData>
{
    void SetData(TData data, int index);
}
