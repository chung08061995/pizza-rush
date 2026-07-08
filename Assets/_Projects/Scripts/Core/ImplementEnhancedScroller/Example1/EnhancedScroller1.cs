using System;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DraftUtils
{
    /// <summary>
    /// Wrapper generic cho EnhancedScroller, dùng composition thay vì kế thừa.
    /// Chỉ cần khai báo field rồi gọi Initialize() và UpdateData() là xong.
    /// 
    /// Ví dụ sử dụng:
    /// [SerializeField] private EnhancedScroller1&lt;MyCell, MyData&gt; scroller = new();
    /// 
    /// TItem: Loại cell view (kế thừa EnhancedScrollerCellView, implement ICellView1&lt;TData&gt;)
    /// TData: Loại dữ liệu mỗi cell hiển thị
    /// </summary>
    [Serializable]
    public class EnhancedScroller1<TItem, TData> : IEnhancedScrollerDelegate
        where TItem : EnhancedScrollerCellView, ICellView1<TData>
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
        /// Kích thước (chiều cao hoặc chiều rộng tùy hướng scroll) của mỗi cell.
        /// </summary>
        [SerializeField] private float cellSize;

        /// <summary>
        /// Danh sách dữ liệu hiện tại đang hiển thị trong scroller.
        /// </summary>
        private IList<TData> _data = new List<TData>();

        /// <summary>
        /// Index của cell đang được chọn. -1 = không có cell nào được chọn.
        /// </summary>
        [ShowInInspector][ReadOnly] private int _selectedIndex = -1;

        /// <summary>
        /// Truy cập danh sách dữ liệu hiện tại.
        /// </summary>
        public IList<TData> Data => _data;

        /// <summary>
        /// Truy cập trực tiếp EnhancedScroller component nếu cần thao tác nâng cao.
        /// </summary>
        public EnhancedScroller Scroller => scroller;

        /// <summary>
        /// Index cell đang được chọn. -1 nếu chưa chọn.
        /// </summary>
        public int SelectedIndex => _selectedIndex;

        /// <summary>
        /// Sự kiện được gọi khi một cell được click.
        /// Tham số: index của cell trong danh sách, và dữ liệu tương ứng.
        /// </summary>
        public event Action<int, TData> OnCellClicked;

        /// <summary>
        /// Sự kiện khi selected index thay đổi.
        /// Tham số: index mới được chọn.
        /// </summary>
        public event Action<int> OnSelectionChanged;

        /// <summary>
        /// Khởi tạo scroller. Gọi hàm này trong Start() hoặc sau khi đã có reference đến scroller.
        /// </summary>
        public void Initialize()
        {
            scroller.Delegate = this;
        }

        /// <summary>
        /// Cập nhật danh sách dữ liệu và reload scroller để hiển thị lại.
        /// </summary>
        /// <param name="data">Danh sách dữ liệu mới cần hiển thị. Nếu null sẽ dùng list rỗng.</param>
        public void UpdateData(IList<TData> data)
        {
            _data = data ?? new List<TData>();
            _selectedIndex = -1;
            scroller.ReloadData();
        }

        /// <summary>
        /// Chọn cell tại index. Gọi RefreshActiveCells để cập nhật UI highlight.
        /// </summary>
        /// <param name="index">Index cell muốn chọn. -1 để bỏ chọn.</param>
        [Button]
        public void Select(int index)
        {
            if (index == _selectedIndex) return;
            _selectedIndex = index;
            OnSelectionChanged?.Invoke(_selectedIndex);
            RefreshActiveCells();
        }

        /// <summary>
        /// Refresh lại các cell đang hiển thị (không reload toàn bộ data).
        /// Hữu ích khi thay đổi trạng thái (selected, highlight) mà không đổi data.
        /// </summary>
        [Button]
        public void RefreshActiveCells()
        {
            scroller.RefreshActiveCellViews();
        }

        /// <summary>
        /// Lấy chiều cao của cellPrefab từ RectTransform và gán vào cellSize.
        /// Bấm button này trong Inspector (Odin) để tự động tính cellSize.
        /// </summary>
        [Button]
        public void GetCellSizeAsHeightCellPrefab()
        {
            if (cellPrefab == null) return;
            var rt = cellPrefab.GetComponent<RectTransform>();
            if (rt != null) cellSize = rt.rect.height;
        }

        #region IEnhancedScrollerDelegate

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return _data.Count;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            if (_data[dataIndex] is ICellSize sized)
                return sized.CellSize;
            return cellSize;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = scroller.GetCellView(cellPrefab) as TItem;
            cellView.SetData(_data[dataIndex], dataIndex);
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
        /// <param name="index">Vị trí của cell muốn nhảy đến (0-based).</param>
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
