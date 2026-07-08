using System;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DraftUtils
{
    /// <summary>
    /// Generic wrapper cho EnhancedScroller hỗ trợ Snapping.
    /// Khi user thả tay, scroller tự snap (dính) vào cell gần nhất.
    /// 
    /// Yêu cầu: Bật snapping trên EnhancedScroller component trong Inspector.
    /// 
    /// TItem: Cell view (kế thừa EnhancedScrollerCellView, implement ICellView6&lt;TData&gt;)
    /// TData: Loại dữ liệu mỗi cell.
    /// 
    /// Ví dụ sử dụng:
    /// [SerializeField] private EnhancedScroller6&lt;MyCell, MyData&gt; scroller = new();
    /// scroller.OnSnapped += (index, data) => { ... };
    /// </summary>
    [Serializable]
    public class EnhancedScroller6<TItem, TData> : IEnhancedScrollerDelegate
        where TItem : EnhancedScrollerCellView, ICellView6<TData>
    {
        /// <summary>
        /// Tham chiếu đến EnhancedScroller component trên scene.
        /// Lưu ý: phải bật Snapping trong Inspector của EnhancedScroller.
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
        /// Bật loop (scroller lặp vô hạn).
        /// </summary>
        [SerializeField] private bool loop;

        /// <summary>
        /// Danh sách dữ liệu.
        /// </summary>
        private IList<TData> _data = new List<TData>();

        /// <summary>
        /// Data index hiện tại đang được snap.
        /// </summary>
        [ShowInInspector][ReadOnly] private int _snappedIndex = -1;

        public IList<TData> Data => _data;
        public EnhancedScroller Scroller => scroller;

        /// <summary>
        /// Index cell đang được snap. -1 nếu chưa snap.
        /// </summary>
        public int SnappedIndex => _snappedIndex;

        /// <summary>
        /// Sự kiện khi scroller snap vào 1 cell.
        /// Tham số: dataIndex, data tại index đó.
        /// </summary>
        public event Action<int, TData> OnSnapped;

        /// <summary>
        /// Khởi tạo scroller. Gọi trong Start().
        /// </summary>
        public void Initialize()
        {
            scroller.Loop = loop;
            scroller.Delegate = this;
            scroller.scrollerSnapped = OnScrollerSnapped;
        }

        /// <summary>
        /// Cập nhật dữ liệu và reload.
        /// </summary>
        /// <param name="data">Danh sách dữ liệu.</param>
        public void UpdateData(IList<TData> data)
        {
            _data = data ?? new List<TData>();
            _snappedIndex = -1;
            scroller.ReloadData();
        }

        /// <summary>
        /// Thêm velocity vào scroller (giống kéo/fling).
        /// Dùng cho slot machine hoặc carousel.
        /// </summary>
        /// <param name="velocity">Tốc độ (dương = xuống/phải, âm = lên/trái).</param>
        public void AddVelocity(float velocity)
        {
            scroller.Velocity = new Vector2(velocity, velocity);
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
        /// Callback khi scroller snap vào cell.
        /// </summary>
        private void OnScrollerSnapped(EnhancedScroller scroller, int cellIndex, int dataIndex, EnhancedScrollerCellView cellView)
        {
            _snappedIndex = dataIndex;

            if (dataIndex >= 0 && dataIndex < _data.Count)
            {
                OnSnapped?.Invoke(dataIndex, _data[dataIndex]);
            }

            // Refresh để cell biết trạng thái snapped
            RefreshActiveCells();
        }

        #region IEnhancedScrollerDelegate

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return _data.Count;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return cellSize;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = scroller.GetCellView(cellPrefab) as TItem;
            cellView.SetData(_data[dataIndex], dataIndex, dataIndex == _snappedIndex);
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
