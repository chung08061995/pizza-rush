using System;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DraftUtils
{
    /// <summary>
    /// Generic wrapper cho EnhancedScroller hỗ trợ Pull Down to Refresh.
    /// Khi user kéo scroller vượt quá ngưỡng ở đầu danh sách rồi thả tay,
    /// sẽ fire event OnPullDownRefresh để load/refresh data.
    /// 
    /// Lưu ý: Script này cần được đặt trên cùng GameObject với EnhancedScroller
    /// để nhận được OnBeginDrag/OnEndDrag events.
    /// Hoặc dùng phương thức NotifyBeginDrag/NotifyEndDrag từ bên ngoài.
    /// 
    /// TItem: Cell view (kế thừa EnhancedScrollerCellView, implement ICellView7&lt;TData&gt;)
    /// TData: Loại dữ liệu mỗi cell.
    /// 
    /// Ví dụ sử dụng:
    /// [SerializeField] private EnhancedScroller7&lt;MyCell, MyData&gt; scroller = new();
    /// scroller.OnPullDownRefresh += () => { RefreshData(); };
    /// </summary>
    [Serializable]
    public class EnhancedScroller7<TItem, TData> : IEnhancedScrollerDelegate
        where TItem : EnhancedScrollerCellView, ICellView7<TData>
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
        /// Ngưỡng kéo (pixel) để trigger refresh.
        /// Kéo vượt quá giá trị này rồi thả = refresh.
        /// </summary>
        [SerializeField] private float pullDownThreshold = 50f;

        /// <summary>
        /// Danh sách dữ liệu.
        /// </summary>
        private List<TData> _data = new();

        /// <summary>
        /// Cờ đánh dấu đang drag.
        /// </summary>
        private bool _dragging;

        /// <summary>
        /// Cờ đánh dấu đã kéo vượt ngưỡng, sẵn sàng refresh khi thả.
        /// </summary>
        private bool _readyToRefresh;

        public IList<TData> Data => _data;
        public EnhancedScroller Scroller => scroller;

        /// <summary>
        /// Đang ở trạng thái sẵn sàng refresh (đã kéo vượt ngưỡng, chưa thả).
        /// Dùng để hiển thị UI "Thả để refresh".
        /// </summary>
        public bool ReadyToRefresh => _readyToRefresh;

        /// <summary>
        /// Sự kiện khi user kéo xuống vượt ngưỡng rồi thả — cần refresh data.
        /// </summary>
        public event Action OnPullDownRefresh;

        /// <summary>
        /// Sự kiện khi trạng thái readyToRefresh thay đổi.
        /// Dùng để bật/tắt UI hint ("Kéo xuống để refresh" / "Thả để refresh").
        /// </summary>
        public event Action<bool> OnReadyToRefreshChanged;

        /// <summary>
        /// Sự kiện khi cell clicked.
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
        /// Set dữ liệu mới (dùng sau refresh hoặc lần đầu).
        /// </summary>
        /// <param name="data">Danh sách dữ liệu.</param>
        public void SetData(IList<TData> data)
        {
            _data = new List<TData>(data ?? new List<TData>());
            _readyToRefresh = false;
            scroller.ReloadData();
        }

        /// <summary>
        /// Chèn data mới vào đầu danh sách (kiểu pull-to-refresh thêm item mới).
        /// </summary>
        /// <param name="newData">Data mới chèn vào đầu.</param>
        public void PrependData(IList<TData> newData)
        {
            if (newData == null || newData.Count == 0) return;

            _data.InsertRange(0, newData);
            _readyToRefresh = false;
            scroller.ReloadData();
        }

        /// <summary>
        /// Gọi khi bắt đầu drag (từ bên ngoài hoặc MonoBehaviour OnBeginDrag).
        /// </summary>
        public void NotifyBeginDrag()
        {
            _dragging = true;
        }

        /// <summary>
        /// Gọi khi kết thúc drag (từ bên ngoài hoặc MonoBehaviour OnEndDrag).
        /// Nếu đã sẵn sàng refresh → fire event.
        /// </summary>
        public void NotifyEndDrag()
        {
            _dragging = false;

            if (_readyToRefresh)
            {
                _readyToRefresh = false;
                OnReadyToRefreshChanged?.Invoke(false);
                OnPullDownRefresh?.Invoke();
            }
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
        /// Callback khi scroller scroll — kiểm tra nếu kéo vượt ngưỡng.
        /// </summary>
        private void OnScrollerScrolled(EnhancedScroller scroller, Vector2 val, float scrollPosition)
        {
            if (!_dragging) return;

            // scrollPosition <= -threshold = đã kéo vượt quá đầu danh sách
            bool shouldRefresh = scrollPosition <= -pullDownThreshold;

            if (shouldRefresh != _readyToRefresh)
            {
                _readyToRefresh = shouldRefresh;
                OnReadyToRefreshChanged?.Invoke(_readyToRefresh);
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
