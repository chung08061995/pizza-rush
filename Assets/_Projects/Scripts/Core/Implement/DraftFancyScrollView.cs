using System.Collections.Generic;
using DG.Tweening;
using FancyScrollView;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace DraftUtils
{
    public class DraftFancyScrollView<TItem, TData> : FancyScrollRect<TData, FancyContext> where TItem : DraftFancyCell<TData>
    {
        [TitleGroup("Fancy Settings")]
        [SerializeField] private float cellSize;
        [SerializeField] private TItem cellPrefab;

        [TitleGroup("Events")]
        [SerializeField] private UnityEvent<int, TData> onCellClickedEvent;

        protected override float CellSize => cellSize;
        protected override GameObject CellPrefab => cellPrefab != null ? cellPrefab.gameObject : null;
        public int DataCount => ItemsSource.Count;

        #region Setup & Data
        protected override void Initialize()
        {
            base.Initialize();
            Context.OnCellClicked = HandleClickCell;
        }

        public void UpdateData(IList<TData> items, bool jumpToFirst = false)
        {
            UpdateContents(items);
            if (jumpToFirst && items.Count > 0)
            {
                JumpToFirst();
            }
        }
        #endregion

        #region Interaction
        private void HandleClickCell(int index)
        {
            if (index < 0 || index >= ItemsSource.Count) return;

            SelectCell(index);
            onCellClickedEvent?.Invoke(index, ItemsSource[index]);
        }
        [Button]
        public void SelectCell(int index)
        {
            if (index < 0 || index >= ItemsSource.Count || index == Context.SelectedIndex)
            {
                return;
            }

            UpdateSelection(index);
            ScrollTo(index, 0.35f, Ease.OutCubic);
        }

        public void SelectNextCell()
        {
            SelectCell(Context.SelectedIndex + 1);
        }

        public void SelectPrevCell()
        {
            SelectCell(Context.SelectedIndex - 1);
        }
        #endregion

        #region Movement (Scroll & Jump)
        public void ScrollTo(int index, float duration, Ease easing, FancyAlignment alignment = FancyAlignment.Middle)
        {
            UpdateSelection(index);
            ScrollTo(index, duration, easing, alignment);
        }

        [Button]
        public void JumpTo(int index, FancyAlignment alignment = FancyAlignment.Middle)
        {
            UpdateSelection(index);
            JumpTo(index, GetAlignment(alignment));
        }

        [Button]
        public void JumpToFirst()
        {
            JumpTo(0);
        }

        private float GetAlignment(FancyAlignment alignment)
        {
            switch (alignment)
            {
                case FancyAlignment.Upper: return 0.0f;
                case FancyAlignment.Middle: return 0.5f;
                case FancyAlignment.Lower: return 1.0f;
                default: return 0.5f;
            }
        }

        private void UpdateSelection(int index)
        {
            if (Context.SelectedIndex == index) return;

            Context.SelectedIndex = index;
            Refresh();
        }
        #endregion

        #region Layout & Padding
        public float PaddingTop
        {
            get => paddingHead;
            set
            {
                paddingHead = value;
                Relayout();
            }
        }

        public float PaddingBottom
        {
            get => paddingTail;
            set
            {
                paddingTail = value;
                Relayout();
            }
        }

        public float Spacing
        {
            get => spacing;
            set
            {
                spacing = value;
                Relayout();
            }
        }
        #endregion
    }
}