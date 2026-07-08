using System;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DraftUtils
{
    /// <summary>
    /// Generic wrapper cho EnhancedScroller dạng Grid + Selection.
    /// Giống EnhancedScroller3 nhưng sub-cell có thể click để select,
    /// và row cell biết selectedIndex để highlight đúng ô.
    /// 
    /// TItem: Row cell view (kế thừa EnhancedScrollerCellView, implement ICellView10&lt;TData&gt;)
    /// TData: Loại dữ liệu của mỗi item trong grid.
    /// 
    /// Ví dụ sử dụng:
    /// [SerializeField] private EnhancedScroller10&lt;MyRowCell, MyData&gt; gridScroller = new();
    /// gridScroller.OnItemSelected += (index, data) => { ... };
    /// </summary>
    [Serializable]
    public class EnhancedScroller10<TItem, TData> : IEnhancedScrollerDelegate
        where TItem : EnhancedScrollerCellView, ICellView10<TData>
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
        /// Danh sách dữ liệu (flat list).
        /// </summary>
        private IList<TData> _data = new List<TData>();

        /// <summary>
        /// Flat index item đang được chọn. -1 = chưa chọn.
        /// </summary>
        [ShowInInspector][ReadOnly] private int _selectedIndex = -1;

        public IList<TData> Data => _data;
        public EnhancedScroller Scroller => scroller;
        public int CellsPerRow => cellsPerRow;
        public int SelectedIndex => _selectedIndex;

        /// <summary>
        /// Sự kiện khi 1 item trong grid được select.
        /// Tham số: flat index, data tương ứng.
        /// </summary>
        public event Action<int, TData> OnItemSelected;

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
        public void UpdateData(IList<TData> data)
        {
            _data = data ?? new List<TData>();
            _selectedIndex = -1;
            scroller.ReloadData();
        }

        /// <summary>
        /// Chọn item tại flat index từ bên ngoài.
        /// </summary>
        [Button]
        public void Select(int index)
        {
            if (index == _selectedIndex) return;
            _selectedIndex = index;
            scroller.RefreshActiveCellViews();

            if (index >= 0 && index < _data.Count)
                OnItemSelected?.Invoke(index, _data[index]);
        }

        /// <summary>
        /// Callback khi sub-cell click (được truyền vào row cell).
        /// </summary>
        private void HandleItemClicked(int flatIndex)
        {
            Select(flatIndex);
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
            rowCell.SetData(_data, startIndex, cellsPerRow, _selectedIndex, HandleItemClicked);
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
