using System;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DraftUtils
{
    /// <summary>
    /// Generic wrapper cho EnhancedScroller hỗ trợ Lazy Load.
    /// Cell chỉ load resource (ảnh, async data) khi visible,
    /// và tự clear khi bị recycle/ẩn đi.
    /// 
    /// TItem: Cell view (kế thừa EnhancedScrollerCellView, implement ICellView5&lt;TData&gt;)
    /// TData: Loại dữ liệu mỗi cell.
    /// 
    /// Ví dụ sử dụng:
    /// [SerializeField] private EnhancedScroller5&lt;ImageCell, ImageData&gt; scroller = new();
    /// </summary>
    [Serializable]
    public class EnhancedScroller5<TItem, TData> : IEnhancedScrollerDelegate
        where TItem : EnhancedScrollerCellView, ICellView5<TData>
    {
        /// <summary>
        /// Tham chiếu đến EnhancedScroller component trên scene.
        /// </summary>
        [SerializeField] private EnhancedScroller scroller;

        /// <summary>
        /// Prefab cell view.
        /// </summary>
        [SerializeField] private TItem cellPrefab;

        /// <summary>
        /// Kích thước mỗi cell.
        /// </summary>
        [SerializeField] private float cellSize = 100f;

        /// <summary>
        /// Danh sách dữ liệu hiện tại.
        /// </summary>
        private IList<TData> _data = new List<TData>();

        public IList<TData> Data => _data;
        public EnhancedScroller Scroller => scroller;

        /// <summary>
        /// Sự kiện khi cell được click.
        /// </summary>
        public event Action<int, TData> OnCellClicked;

        /// <summary>
        /// Khởi tạo scroller. Gọi trong Start().
        /// Đăng ký callback visibility changed và will recycle.
        /// </summary>
        public void Initialize()
        {
            scroller.Delegate = this;
            scroller.cellViewVisibilityChanged = OnCellViewVisibilityChanged;
            scroller.cellViewWillRecycle = OnCellViewWillRecycle;
        }

        /// <summary>
        /// Cập nhật dữ liệu và reload scroller.
        /// </summary>
        /// <param name="data">Danh sách dữ liệu mới.</param>
        public void UpdateData(IList<TData> data)
        {
            _data = data ?? new List<TData>();
            scroller.ReloadData();
        }

        /// <summary>
        /// Refresh các cell đang hiển thị.
        /// </summary>
        [Button]
        public void RefreshActiveCells()
        {
            scroller.RefreshActiveCellViews();
        }

        /// <summary>
        /// Lấy chiều cao cellPrefab từ RectTransform.
        /// </summary>
        [Button]
        public void GetCellSizeAsHeightCellPrefab()
        {
            if (cellPrefab == null) return;
            var rt = cellPrefab.GetComponent<RectTransform>();
            if (rt != null) cellSize = rt.rect.height;
        }

        /// <summary>
        /// Callback khi cell trở nên visible hoặc hidden.
        /// Visible = gọi SetData (bắt đầu load resource).
        /// Hidden = gọi ClearData (hủy/clear resource).
        /// </summary>
        private void OnCellViewVisibilityChanged(EnhancedScrollerCellView cellView)
        {
            if (cellView is not TItem typedCell) return;

            if (cellView.active)
            {
                // Cell vừa visible → load data
                var dataIndex = cellView.dataIndex;
                if (dataIndex >= 0 && dataIndex < _data.Count)
                {
                    typedCell.SetData(_data[dataIndex], dataIndex);
                }
            }
            else
            {
                // Cell vừa bị ẩn → clear
                typedCell.ClearData();
            }
        }

        /// <summary>
        /// Callback khi cell sắp bị recycle → clear resource.
        /// </summary>
        private void OnCellViewWillRecycle(EnhancedScrollerCellView cellView)
        {
            if (cellView is TItem typedCell)
            {
                typedCell.ClearData();
            }
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

        /// <summary>
        /// Tạo/recycle cell. KHÔNG gọi SetData ở đây — chờ visibility callback.
        /// </summary>
        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = scroller.GetCellView(cellPrefab) as TItem;
            // Không gọi SetData ở đây. Chờ OnCellViewVisibilityChanged gọi khi cell active.
            return cellView;
        }

        #endregion

        #region Navigation

        [Button]
        public void JumpToTop(float tweenTime = 0f)
        {
            if (_data.Count == 0) return;
            scroller.JumpToDataIndex(0, tweenTime: tweenTime);
        }

        [Button]
        public void JumpToBottom(float tweenTime = 0f)
        {
            if (_data.Count == 0) return;
            scroller.JumpToDataIndex(_data.Count - 1, scrollerOffset: 1f, cellOffset: 1f, tweenTime: tweenTime);
        }

        [Button]
        public void JumpToIndex(int index, float tweenTime = 0f)
        {
            if (index < 0 || index >= _data.Count) return;
            scroller.JumpToDataIndex(index, scrollerOffset: 0.5f, cellOffset: 0.5f, tweenTime: tweenTime);
        }

        #endregion
    }
}
