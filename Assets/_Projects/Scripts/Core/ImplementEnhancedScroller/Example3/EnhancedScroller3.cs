using System;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DraftUtils
{
    /// <summary>
    /// Generic wrapper cho EnhancedScroller dạng Grid (nhiều cột).
    /// Scroller hiển thị theo hàng, mỗi hàng chứa N sub-cell.
    /// 
    /// TItem: Row cell view (kế thừa EnhancedScrollerCellView, implement ICellView3&lt;TData&gt;)
    /// TData: Loại dữ liệu của mỗi item trong grid.
    /// 
    /// Ví dụ sử dụng:
    /// [SerializeField] private EnhancedScroller3&lt;MyRowCell, MyData&gt; gridScroller = new();
    /// </summary>
    [Serializable]
    public class EnhancedScroller3<TItem, TData> : IEnhancedScrollerDelegate
        where TItem : EnhancedScrollerCellView, ICellView3<TData>
    {
        /// <summary>
        /// Tham chiếu đến EnhancedScroller component trên scene.
        /// </summary>
        [SerializeField] private EnhancedScroller scroller;

        /// <summary>
        /// Prefab row cell view (chứa N sub-cell bên trong).
        /// </summary>
        [SerializeField] private TItem rowCellPrefab;

        /// <summary>
        /// Chiều cao mỗi hàng.
        /// </summary>
        [SerializeField] private float rowHeight = 100f;

        /// <summary>
        /// Số cột (item) mỗi hàng.
        /// </summary>
        [SerializeField] private int cellsPerRow = 3;

        /// <summary>
        /// Danh sách dữ liệu (flat list, mỗi item = 1 ô trong grid).
        /// </summary>
        private IList<TData> _data = new List<TData>();

        /// <summary>
        /// Index item đang được chọn trong grid. -1 = chưa chọn.
        /// </summary>
        [ShowInInspector][ReadOnly] private int _selectedIndex = -1;

        public IList<TData> Data => _data;
        public EnhancedScroller Scroller => scroller;
        public int CellsPerRow => cellsPerRow;
        public int SelectedIndex => _selectedIndex;

        /// <summary>
        /// Sự kiện khi 1 item trong grid được click.
        /// Tham số: index (flat) trong data list.
        /// </summary>
        public event Action<int, TData> OnItemClicked;

        /// <summary>
        /// Sự kiện khi selection thay đổi.
        /// </summary>
        public event Action<int> OnSelectionChanged;

        /// <summary>
        /// Khởi tạo scroller.
        /// </summary>
        public void Initialize()
        {
            scroller.Delegate = this;
        }

        /// <summary>
        /// Cập nhật dữ liệu và reload grid.
        /// </summary>
        /// <param name="data">Flat list dữ liệu mới.</param>
        public void UpdateData(IList<TData> data)
        {
            _data = data ?? new List<TData>();
            _selectedIndex = -1;
            scroller.ReloadData();
        }

        /// <summary>
        /// Chọn item tại flat index.
        /// </summary>
        [Button]
        public void Select(int index)
        {
            if (index == _selectedIndex) return;
            _selectedIndex = index;
            OnSelectionChanged?.Invoke(_selectedIndex);
            scroller.RefreshActiveCellViews();
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
        /// Lấy chiều cao row từ prefab RectTransform.
        /// </summary>
        [Button]
        public void GetRowHeightFromPrefab()
        {
            if (rowCellPrefab == null) return;
            var rt = rowCellPrefab.GetComponent<RectTransform>();
            if (rt != null) rowHeight = rt.rect.height;
        }

        #region IEnhancedScrollerDelegate

        /// <summary>
        /// Số hàng = ceil(totalItems / cellsPerRow).
        /// </summary>
        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return Mathf.CeilToInt((float)_data.Count / cellsPerRow);
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return rowHeight;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var rowCell = scroller.GetCellView(rowCellPrefab) as TItem;
            int startIndex = dataIndex * cellsPerRow;
            rowCell.SetData(_data, startIndex, cellsPerRow);
            return rowCell;
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
            int rowCount = GetNumberOfCells(scroller);
            if (rowCount == 0) return;
            scroller.JumpToDataIndex(rowCount - 1, scrollerOffset: 1f, cellOffset: 1f, tweenTime: tweenTime);
        }

        /// <summary>
        /// Nhảy đến hàng chứa item tại flat index.
        /// </summary>
        [Button]
        public void JumpToItemIndex(int itemIndex, float tweenTime = 0f)
        {
            if (itemIndex < 0 || itemIndex >= _data.Count) return;
            int rowIndex = itemIndex / cellsPerRow;
            scroller.JumpToDataIndex(rowIndex, scrollerOffset: 0.5f, cellOffset: 0.5f, tweenTime: tweenTime);
        }

        #endregion
    }
}
