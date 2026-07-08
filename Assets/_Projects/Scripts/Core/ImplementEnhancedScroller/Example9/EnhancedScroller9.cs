using System;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DraftUtils
{
    /// <summary>
    /// Generic wrapper cho EnhancedScroller dạng Nested (master-detail).
    /// Master scroller (vertical) chứa các row, mỗi row bên trong có 1 child scroller (horizontal).
    /// 
    /// TItem: Master cell view (kế thừa EnhancedScrollerCellView, implement ICellView9&lt;TData&gt;).
    ///        Bên trong master cell tự quản lý child EnhancedScroller.
    /// TData: Dữ liệu master (thường chứa list child data bên trong).
    /// 
    /// Lưu ý: Child scroller nên dùng ScrollRectEx (có sẵn trong plugin) để truyền
    /// drag events ngược lên parent scroller khi kéo dọc.
    /// 
    /// Ví dụ sử dụng:
    /// [SerializeField] private EnhancedScroller9&lt;CategoryRow, CategoryData&gt; scroller = new();
    /// </summary>
    [Serializable]
    public class EnhancedScroller9<TItem, TData> : IEnhancedScrollerDelegate
        where TItem : EnhancedScrollerCellView, ICellView9<TData>
    {
        /// <summary>
        /// Tham chiếu đến master EnhancedScroller component trên scene.
        /// </summary>
        [SerializeField] private EnhancedScroller scroller;

        /// <summary>
        /// Prefab master cell view (chứa child scroller bên trong).
        /// </summary>
        [SerializeField] private TItem masterCellPrefab;

        /// <summary>
        /// Chiều cao mỗi master cell (= chiều cao row bao gồm child scroller).
        /// </summary>
        [SerializeField] private float masterCellSize = 150f;

        /// <summary>
        /// Danh sách dữ liệu master.
        /// </summary>
        private IList<TData> _data = new List<TData>();

        public IList<TData> Data => _data;
        public EnhancedScroller Scroller => scroller;

        /// <summary>
        /// Sự kiện khi master cell clicked.
        /// </summary>
        public event Action<int, TData> OnCellClicked;

        /// <summary>
        /// Khởi tạo scroller. Gọi trong Start().
        /// </summary>
        public void Initialize()
        {
            scroller.Delegate = this;
        }

        /// <summary>
        /// Cập nhật dữ liệu master và reload.
        /// </summary>
        /// <param name="data">Danh sách dữ liệu master.</param>
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
        /// Lấy chiều cao master cell từ prefab RectTransform.
        /// </summary>
        [Button]
        public void GetCellSizeFromPrefab()
        {
            if (masterCellPrefab == null) return;
            var rt = masterCellPrefab.GetComponent<RectTransform>();
            if (rt != null) masterCellSize = rt.rect.height;
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
            return masterCellSize;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = scroller.GetCellView(masterCellPrefab) as TItem;
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
