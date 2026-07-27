using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupBuyItem : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] public DraftUtils.Popup popup;
    [SerializeField] private ItemView itemView;
    [SerializeField] private Button watchAdsButton;
    [SerializeField] private Button buyButton;

    [ShowInInspector]
    [ReadOnly]
    private ItemType _data;

    private void Start()
    {
        popup.closeButton.RegisterClickEvents();
        popup.closeButton.OnClickAction = ClickClose;
        buyButton.onClick.AddListener(ClickBuy);
        watchAdsButton.onClick.AddListener(ClickWatchAds);
    }

    public void SetData(ItemType data)
    {
        _data = data;
        itemView.SetData(data);
        itemView.SetRemaningTextActive();
        RefreshBuyAvailability();
    }

    private void ClickBuy()
    {
        if (DataManager.Instance.costItems.TryGetValue(_data, out var cost))
        {
            int currentGold = DataManager.Instance.remainningItems[ItemType.Gold].Value;
            if (currentGold >= cost)
            {
                DataManager.Instance.Using(ItemType.Gold, -cost);
                DataManager.Instance.Using(_data, 1);
                // Optionally close after buying or keep it open. Let's close it so they can use it.
                popup.HideWithAnimation();
            }
            else
            {
                ShowNotEnoughGold();
            }
        }
    }

    private void ClickWatchAds()
    {
        DraftUtils.Ads.AdsManager.Instance.ShowRewarded($"BuyItem_{_data}", (success) =>
        {
            if (!success)
            {
                return;
            }

            DataManager.Instance.Using(_data, 1);
            popup.HideWithAnimation();
        });
    }

    private void ClickClose()
    {
        popup.HideWithAnimation();
    }

    private void RefreshBuyAvailability()
    {
        if (!DataManager.Instance.costItems.TryGetValue(_data, out var cost))
        {
            buyButton.interactable = false;
            return;
        }

        bool canBuy = DataManager.Instance.remainningItems[ItemType.Gold].Value >= cost;
        buyButton.interactable = canBuy;
        if (!canBuy)
        {
            ShowNotEnoughGold();
        }
    }

    private void ShowNotEnoughGold()
    {
        buyButton.interactable = false;
        var label = buyButton.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
        {
            label.SetText("Not enough Gold");
        }
    }
}
