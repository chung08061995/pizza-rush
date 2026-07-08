using System;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DraftUtils
{
    /// <summary>
    /// Generic wrapper cho EnhancedScroller hỗ trợ view-driven cell sizes.
    /// Mỗi cell có thể có kích thước khác nhau, được tính tự động dựa trên nội dung (ContentSizeFitter).
    /// 
    /// Cơ chế 2-pass:
    /// - Pass 1: Mở rộng scroller ra max, render tất cả cell để ContentSizeFitter tính height,
    ///           cell ghi kết quả vào data.CellSize.
    /// - Pass 2: Reset scroller về kích thước gốc, reload lại với cellSize đã tính.
    /// 
    /// Ví dụ sử dụng:
    /// [SerializeField] private EnhancedScroller8&lt;MyCellView, MyData&gt; scroller = new();
    /// 
    /// TItem: Cell view (kế thừa EnhancedScrollerCellView, implement ICellView8&lt;TData&gt;)
    /// TData: Data model (implement IViewDrivenCellData để lưu cellSize)
    /// </summary>
    [Serializable]
    public class EnhancedScroller8<TItem, TData> : IEnhancedScrollerDelegate
        where TItem : EnhancedScrollerCellView, ICellView8<TData>
        where TData : IViewDrivenCellData
    {
        /// <summary>
        /// Tham chiếu đến EnhancedScroller component trên scene.
        /// </summary>
        [SerializeField] private EnhancedScroller scroller;

        /// <summary>
        /// Prefab cell view dùng để tạo/recycle các cell trong scroller.
        /// </summary>
        [SerializeField] private TItem cellPrefab;

        /// <summary>
        /// Danh sách dữ liệu hiện tại đang hiển thị trong scroller.
        /// </summary>
        private IList<TData> _data = new List<TData>();

        /// <summary>
        /// Cờ đánh dấu đang ở pass tính layout (pass 1) hay pass hiển thị (pass 2).
        /// </summary>
        private bool _calculateLayout;

        /// <summary>
        /// Truy cập danh sách dữ liệu hiện tại.
        /// </summary>
        public IList<TData> Data => _data;

        /// <summary>
        /// Truy cập trực tiếp EnhancedScroller component nếu cần thao tác nâng cao.
        /// </summary>
        public EnhancedScroller Scroller => scroller;

        /// <summary>
        /// Sự kiện được gọi khi một cell được click.
        /// Tham số: index của cell trong danh sách, và dữ liệu tương ứng.
        /// </summary>
        public event Action<int, TData> OnCellClicked;

        /// <summary>
        /// Khởi tạo scroller. Gọi hàm này trong Start() hoặc sau khi đã có reference đến scroller.
        /// </summary>
        public void Initialize()
        {
            scroller.Delegate = this;
        }

        /// <summary>
        /// Cập nhật danh sách dữ liệu, tính toán kích thước từng cell rồi hiển thị.
        /// Tự động chạy cơ chế 2-pass để tính view-driven cell sizes.
        /// </summary>
        /// <param name="data">Danh sách dữ liệu mới. Nếu null sẽ dùng list rỗng.</param>
        public void UpdateData(IList<TData> data)
        {
            _data = data ?? new List<TData>();
            ResizeScroller();
        }

        /// <summary>
        /// Cơ chế 2-pass để tính kích thước cell dựa trên nội dung thực tế.
        /// Pass 1: Mở rộng scroller tối đa, render cell, ContentSizeFitter tính height → ghi vào data.CellSize.
        /// Pass 2: Reset kích thước scroller, reload lại với cellSize đã có.
        /// </summary>
        private void ResizeScroller()
        {
            // Lưu kích thước gốc
            var rectTransform = scroller.GetComponent<RectTransform>();
            var size = rectTransform.sizeDelta;

            // Mở rộng scroller để tất cả cell được render (không bị recycle)
            rectTransform.sizeDelta = new Vector2(size.x, float.MaxValue);

            // Pass 1: Tính layout
            _calculateLayout = true;
            scroller.ReloadData();

            // Khôi phục kích thước gốc
            rectTransform.sizeDelta = size;

            // Pass 2: Hiển thị với kích thước đã tính
            _calculateLayout = false;
            scroller.ReloadData();
        }

        #region IEnhancedScrollerDelegate

        /// <summary>
        /// Trả về tổng số cell cần hiển thị.
        /// </summary>
        /// <param name="scroller">Scroller đang yêu cầu.</param>
        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return _data.Count;
        }

        /// <summary>
        /// Trả về kích thước cell tại dataIndex.
        /// Pass 1: trả 0 (chưa tính xong, scroller sẽ dùng min size).
        /// Pass 2: trả giá trị đã được cell view tính ở pass 1.
        /// </summary>
        /// <param name="scroller">Scroller đang yêu cầu.</param>
        /// <param name="dataIndex">Vị trí dữ liệu.</param>
        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return _data[dataIndex].CellSize;
        }

        /// <summary>
        /// Tạo hoặc tái sử dụng cell view, gán dữ liệu.
        /// Truyền cờ _calculateLayout để cell biết cần tính size hay chỉ hiển thị.
        /// </summary>
        /// <param name="scroller">Scroller đang yêu cầu cell.</param>
        /// <param name="dataIndex">Vị trí dữ liệu.</param>
        /// <param name="cellIndex">Vị trí cell hiển thị.</param>
        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = scroller.GetCellView(cellPrefab) as TItem;
            cellView.SetData(_data[dataIndex], dataIndex, _calculateLayout);
            return cellView;
        }

        #endregion

        #region Navigation

        /// <summary>
        /// Nhảy đến đầu danh sách (cell đầu tiên).
        /// </summary>
        /// <param name="tweenTime">Thời gian animation (giây). Mặc định 0 = nhảy ngay lập tức.</param>
        [Button]
        public void JumpToTop(float tweenTime = 0f)
        {
            if (_data.Count == 0) return;
            scroller.JumpToDataIndex(0, tweenTime: tweenTime);
        }

        /// <summary>
        /// Nhảy đến cuối danh sách (cell cuối cùng).
        /// </summary>
        /// <param name="tweenTime">Thời gian animation (giây). Mặc định 0 = nhảy ngay lập tức.</param>
        [Button]
        public void JumpToBottom(float tweenTime = 0f)
        {
            if (_data.Count == 0) return;
            scroller.JumpToDataIndex(_data.Count - 1, scrollerOffset: 1f, cellOffset: 1f, tweenTime: tweenTime);
        }

        /// <summary>
        /// Nhảy đến cell tại vị trí index chỉ định (cell sẽ ở giữa viewport).
        /// </summary>
        /// <param name="index">Vị trí cell muốn nhảy đến (0-based).</param>
        /// <param name="tweenTime">Thời gian animation (giây). Mặc định 0 = nhảy ngay lập tức.</param>
        [Button]
        public void JumpToIndex(int index, float tweenTime = 0f)
        {
            if (index < 0 || index >= _data.Count) return;
            scroller.JumpToDataIndex(index, scrollerOffset: 0.5f, cellOffset: 0.5f, tweenTime: tweenTime);
        }

        #endregion
    }
}
