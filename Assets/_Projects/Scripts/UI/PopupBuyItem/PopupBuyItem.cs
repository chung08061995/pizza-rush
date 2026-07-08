using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupBuyItem : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] public DraftUtils.Popup popup;
    [SerializeField] private ItemView itemView;
    [SerializeField] private ItemView goldView;
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
        goldView.RemaningText.ValueToDisplayTextFunc = x => DraftUtils.Utils.Common.FormatNumber((int)x);
        goldView.SetData(ItemType.Gold);
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
                // Not enough gold, maybe open shop?
                PopupManager.Instance.GetPopupShop();
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
}
