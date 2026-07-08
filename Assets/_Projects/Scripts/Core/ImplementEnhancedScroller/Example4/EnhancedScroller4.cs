using System;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DraftUtils
{
    /// <summary>
    /// Generic wrapper cho EnhancedScroller hỗ trợ Pagination (infinite scroll).
    /// Khi scroll đến cuối danh sách, tự động fire event để load thêm data.
    /// 
    /// TItem: Cell view (kế thừa EnhancedScrollerCellView, implement ICellView4&lt;TData&gt;)
    /// TData: Loại dữ liệu mỗi cell hiển thị.
    /// 
    /// Ví dụ sử dụng:
    /// [SerializeField] private EnhancedScroller4&lt;MyCell, MyData&gt; scroller = new();
    /// scroller.OnLoadMore += (currentCount) => { LoadNextPage(); };
    /// </summary>
    [Serializable]
    public class EnhancedScroller4<TItem, TData> : IEnhancedScrollerDelegate
        where TItem : EnhancedScrollerCellView, ICellView4<TData>
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
        /// Ngưỡng NormalizedScrollPosition để trigger load more (0~1).
        /// Mặc định 1 = scroll đến tận cùng cuối mới load.
        /// Đặt 0.9 = load sớm hơn khi gần cuối.
        /// </summary>
        [SerializeField] private float loadMoreThreshold = 1f;

        /// <summary>
        /// Danh sách dữ liệu hiện tại.
        /// </summary>
        private List<TData> _data = new();

        /// <summary>
        /// Cờ chặn load nhiều lần liên tục.
        /// </summary>
        private bool _isLoading;

        public IList<TData> Data => _data;
        public EnhancedScroller Scroller => scroller;

        /// <summary>
        /// Đang loading thêm data hay không. Set false sau khi AppendData xong.
        /// </summary>
        public bool IsLoading => _isLoading;

        /// <summary>
        /// Sự kiện khi scroll đến cuối — cần load thêm data.
        /// Tham số: số item hiện tại (dùng làm offset cho page tiếp).
        /// </summary>
        public event Action<int> OnLoadMore;

        /// <summary>
        /// Sự kiện khi cell được click.
        /// </summary>
        public event Action<int, TData> OnCellClicked;

        /// <summary>
        /// Khởi tạo scroller. Gọi trong Start().
        /// </summary>
        public void Initialize()
        {
            scroller.Delegate = this;
            scroller.scrollerScrolled = OnScrollerScrolled;
        }

        /// <summary>
        /// Set dữ liệu ban đầu (page đầu tiên). Reset toàn bộ.
        /// </summary>
        /// <param name="data">Dữ liệu page đầu.</param>
        public void SetData(IList<TData> data)
        {
            _data = new List<TData>(data ?? new List<TData>());
            _isLoading = false;
            scroller.ReloadData();
        }

        /// <summary>
        /// Thêm data vào cuối (page tiếp theo). Giữ nguyên scroll position.
        /// Gọi sau khi load xong page mới.
        /// </summary>
        /// <param name="newData">Dữ liệu page mới cần append.</param>
        public void AppendData(IList<TData> newData)
        {
            if (newData == null || newData.Count == 0)
            {
                _isLoading = false;
                return;
            }

            // Lưu vị trí scroll hiện tại
            var previousCount = _data.Count;

            _data.AddRange(newData);
            scroller.ReloadData();

            // Nhảy về vị trí cũ để user không bị giật
            scroller.JumpToDataIndex(previousCount, 1f, 1f);

            _isLoading = false;
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
        /// Callback khi scroller scroll — kiểm tra nếu đến cuối thì fire OnLoadMore.
        /// </summary>
        private void OnScrollerScrolled(EnhancedScroller scroller, Vector2 val, float scrollPosition)
        {
            if (_isLoading) return;
            if (_data.Count == 0) return;

            if (scroller.NormalizedScrollPosition >= loadMoreThreshold)
            {
                _isLoading = true;
                OnLoadMore?.Invoke(_data.Count);
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

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = scroller.GetCellView(cellPrefab) as TItem;
            cellView.SetData(_data[dataIndex], dataIndex);
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
