/// <summary>
/// Interface cho data model hỗ trợ view-driven cell size.
/// Data phải có field cellSize để cell view ghi kích thước đã tính vào.
/// </summary>
public interface IViewDrivenCellData
{
    /// <summary>
    /// Kích thước của cell (được cell view tính toán và ghi vào ở pass đầu).
    /// </summary>
    float CellSize { get; set; }
}
