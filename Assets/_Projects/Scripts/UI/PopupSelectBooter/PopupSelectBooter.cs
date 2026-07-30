using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PopupSelectBooter : MonoBehaviour
{
    private const float PreferredBoosterSpacing = 44f;
    private const float MinimumBoosterSpacing = 16f;
    private const float MinimumBoosterSideMargin = 24f;
    private const float LayoutWidthChangeThreshold = .1f;

    [SerializeField] public DraftUtils.Popup popup;
    [SerializeField] private SelectBooterItem coffeeItemView;
    [SerializeField] private SelectBooterItem magicItemView;
    [SerializeField] private Button playButton;

    private List<ItemType> _booterItems = new();
    private Action _onPlay;
    private HorizontalLayoutGroup _boosterLayout;
    private RectTransform _boosterLayoutRect;
    private RectTransform _coffeeItemRect;
    private RectTransform _magicItemRect;
    private float _preferredBoosterItemSize;
    private float _lastBoosterLayoutWidth = -1f;
    private bool _isRefreshingLayout;

    private void Awake()
    {
        CacheResponsiveLayout();
    }

    private void OnEnable()
    {
        Canvas.willRenderCanvases += RefreshResponsiveLayoutIfNeeded;
        RefreshResponsiveLayout();
    }

    private void OnDisable()
    {
        Canvas.willRenderCanvases -= RefreshResponsiveLayoutIfNeeded;
    }

    private void Start()
    {
        popup.closeButton.OnClickAction = popup.HideWithAnimation;

        playButton.onClick.AddListener(ClickPlay);
        coffeeItemView.SetData(ItemType.Booter_CoffeeTime);
        magicItemView.SetData(ItemType.Booter_Magic);

        coffeeItemView.ItemView.Button.OnClickAction = ClickCoffeeButton;
        magicItemView.ItemView.Button.OnClickAction = ClickMagicButton;

    }

    private void OnRectTransformDimensionsChange()
    {
        if (isActiveAndEnabled)
        {
            RefreshResponsiveLayout();
        }
    }

    private void CacheResponsiveLayout()
    {
        _coffeeItemRect = coffeeItemView != null ? coffeeItemView.transform as RectTransform : null;
        _magicItemRect = magicItemView != null ? magicItemView.transform as RectTransform : null;

        if (_preferredBoosterItemSize <= 0f
            && _coffeeItemRect != null
            && _magicItemRect != null)
        {
            _preferredBoosterItemSize = Mathf.Min(
                _coffeeItemRect.rect.width,
                _magicItemRect.rect.width
            );
        }

        foreach (var candidate in GetComponentsInChildren<HorizontalLayoutGroup>(true))
        {
            if (_coffeeItemRect != null
                && _magicItemRect != null
                && _coffeeItemRect.parent == candidate.transform
                && _magicItemRect.parent == candidate.transform)
            {
                _boosterLayout = candidate;
                _boosterLayoutRect = candidate.transform as RectTransform;
                break;
            }
        }
    }

    private void RefreshResponsiveLayoutIfNeeded()
    {
        if (_boosterLayoutRect == null)
        {
            CacheResponsiveLayout();
        }

        if (_boosterLayoutRect != null
            && Mathf.Abs(_boosterLayoutRect.rect.width - _lastBoosterLayoutWidth)
            > LayoutWidthChangeThreshold)
        {
            RefreshResponsiveLayout();
        }
    }

    private void RefreshResponsiveLayout()
    {
        if (_isRefreshingLayout)
        {
            return;
        }

        if (_boosterLayout == null)
        {
            CacheResponsiveLayout();
        }

        if (_boosterLayout == null
            || _boosterLayoutRect == null
            || _coffeeItemRect == null
            || _magicItemRect == null)
        {
            return;
        }

        if (_preferredBoosterItemSize <= 0f)
        {
            _preferredBoosterItemSize = Mathf.Min(
                _coffeeItemRect.rect.width,
                _magicItemRect.rect.width
            );
        }

        if (_preferredBoosterItemSize <= 0f)
        {
            return;
        }

        float contentWidth = _boosterLayoutRect.rect.width;
        _lastBoosterLayoutWidth = contentWidth;

        float maximumItemSize = (
            contentWidth
            - MinimumBoosterSideMargin * 2f
            - MinimumBoosterSpacing
        ) * .5f;
        float itemSize = Mathf.Min(_preferredBoosterItemSize, maximumItemSize);

        if (itemSize <= 0f)
        {
            return;
        }

        float remainingWidth = contentWidth - itemSize * 2f;
        float spacing = Mathf.Clamp(
            remainingWidth - MinimumBoosterSideMargin * 2f,
            MinimumBoosterSpacing,
            PreferredBoosterSpacing
        );

        _isRefreshingLayout = true;

        _coffeeItemRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, itemSize);
        _coffeeItemRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, itemSize);
        _magicItemRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, itemSize);
        _magicItemRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, itemSize);
        _boosterLayout.spacing = spacing;

        LayoutRebuilder.ForceRebuildLayoutImmediate(_boosterLayoutRect);
        _isRefreshingLayout = false;
    }

    private void ClickMagicButton()
    {
        SelectItem(magicItemView);
    }

    private void ClickCoffeeButton()
    {
        SelectItem(coffeeItemView);
    }

    private void ClickPlay()
    {
        RuntimeStorage.Instance.Set(GameConstain.RuntimeStorage.StartBooterItems, _booterItems);
        popup.HideWithAnimation();
        _onPlay?.Invoke();
    }

    private void SelectItem(SelectBooterItem item)
    {
        if (item == null)
        {
            return;
        }

        if (GetRemainingFromDataManager(item.GetData()) <= 0)
        {
            return;
        }

        if (_booterItems.Contains(item.GetData()))
        {
            _booterItems.Remove(item.GetData());
            item.Deselect();
        }
        else
        {
            _booterItems.Add(item.GetData());
            item.Select();
            GameAnalytics.LogItemEvent(GameAnalytics.BoosterUse, item.GetData());
        }
    }

    private int GetRemainingFromDataManager(ItemType itemType)
    {
        if(DataManager.Instance.remainningItems.TryGetValue(itemType, out var data))
        {
            return data.Value;
        }
        return 0;
    }

    public void SetData(Action onPlay = null)
    {
        _onPlay = onPlay;
        _booterItems.Clear();
        RefreshResponsiveLayout();
        coffeeItemView.SetData(ItemType.Booter_CoffeeTime);
        magicItemView.SetData(ItemType.Booter_Magic);
    }
}
