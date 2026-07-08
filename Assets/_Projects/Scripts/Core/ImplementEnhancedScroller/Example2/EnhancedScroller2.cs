using System;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DraftUtils
{
    /// <summary>
    /// Kết quả trả về từ factory func: prefab nào và cellSize bao nhiêu.
    /// </summary>
    [System.Serializable]
    public struct CellInfo
    {
        public EnhancedScrollerCellView prefab;
        public float cellSize;
    }

    /// <summary>
    /// Generic wrapper cho EnhancedScroller hỗ trợ multiple cell types.
    /// Truyền vào 1 Func để quyết định từ data → prefab nào + height bao nhiêu.
    /// 
    /// Ví dụ sử dụng:
    /// [SerializeField] private EnhancedScroller2&lt;LevelUpCellData&gt; scroller = new();
    /// scroller.GetCellInfoFunc = (data, index) => ...;
    /// scroller.Initialize();
    /// scroller.UpdateData(list);
    /// </summary>
    [Serializable]
    public class EnhancedScroller2<TData> : IEnhancedScrollerDelegate
    {
        /// <summary>
        /// Tham chiếu đến EnhancedScroller component trên scene.
        /// </summary>
        [SerializeField] private EnhancedScroller scroller;

        /// <summary>
        /// Danh sách dữ liệu hiện tại.
        /// </summary>
        private IList<TData> _data = new List<TData>();

        /// <summary>
        /// Func quyết định từ data → prefab + cellSize.
        /// Phải set trước khi gọi Initialize().
        /// </summary>
        public Func<TData, int, CellInfo> GetCellInfoFunc { get; set; }

        public IList<TData> Data => _data;
        public EnhancedScroller Scroller => scroller;

        /// <summary>
        /// Sự kiện khi cell được click.
        /// </summary>
        public event Action<int, TData> OnCellClicked;

        /// <summary>
        /// Khởi tạo scroller. Gọi trong Start() sau khi đã set GetCellInfoFunc.
        /// </summary>
        public void Initialize()
        {
            scroller.Delegate = this;
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

        #region IEnhancedScrollerDelegate

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return _data.Count;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return GetCellInfoFunc(_data[dataIndex], dataIndex).cellSize;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var info = GetCellInfoFunc(_data[dataIndex], dataIndex);
            var cellView = scroller.GetCellView(info.prefab);

            if (cellView is ICellView2<TData> typedCell)
            {
                typedCell.SetData(_data[dataIndex], dataIndex);
            }

            return cellView;
        }

        #endregion

        #region Refresh

        /// <summary>
        /// Refresh lại các cell đang hiển thị (không reload toàn bộ data).
        /// Hữu ích khi thay đổi trạng thái mà không đổi data.
        /// </summary>
        [Button]
        public void RefreshActiveCells()
        {
            scroller.RefreshActiveCellViews();
        }

        #endregion        #region Navigation

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

    }
}
