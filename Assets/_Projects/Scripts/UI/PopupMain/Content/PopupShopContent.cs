using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupShopContent : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] public DraftUtils.Popup popup;
    [SerializeField] private DraftUtils.RebuildLayouts rebuilder;

    [SerializeField] private DraftUtils.Pooler<SingleIAPDataView> singleIAPPooler = new();
    [SerializeField] private DraftUtils.Pooler<IapContainerItem> multipleIAPPooler = new();

    private void Start()
    {
        popup.closeButton.OnClickAction = popup.HideWithAnimation;
        GenerateSingleIAPItems();
        GenerateMultipleIAPItems();

        rebuilder.Rebuild();
    }

    private void GenerateSingleIAPItems()
    {
        singleIAPPooler.Factory = new DraftUtils.ComponentInstantiatePoolFactory<SingleIAPDataView>();

        foreach (var singleIAPData in DataManager.Instance.iapData.singleIaps)
        {
            var item = singleIAPPooler.Spawn();
            item.SetData(singleIAPData);
        }
    }

    private void GenerateMultipleIAPItems()
    {
        multipleIAPPooler.Factory = new DraftUtils.ComponentInstantiatePoolFactory<IapContainerItem>();

        var iapData = DataManager.Instance.iapData;
        if (iapData == null) return;

        var list = new List<MultipleIAPData>
        {
            iapData.noAds,
            iapData.noAdsBundle,
            iapData.smallBundle,
            iapData.mediumBundle,
            iapData.largeBundle,
            iapData.starter
        };

        foreach (var data in list)
        {
            var item = multipleIAPPooler.Spawn();
            item.SetData(data);
            item.Items.ForEach(x => x.Button.OnClickAction = () => ClickMultipleIAPButton(x));
        }
    }

    private void ClickMultipleIAPButton(MultipleIAPDataView x)
    {
        string prodId = string.IsNullOrEmpty(x.Data.productId) ? x.Data.itemType.ToString() : x.Data.productId;

        DraftUtils.IAP.IAPManager.Instance.Purchase(prodId, result =>
        {
            if (result.IsSuccess)
            {
                GameAnalytics.LogPurchaseEvent(GameAnalytics.IapPurchaseSuccess, prodId);
                var rewards = MultipleIAPDataExtensions.GetRewards(x.Data);
                if (rewards.Count > 0)
                {
                    DataManager.Instance.Reward(rewards);
                }
                PopupManager.Instance.ShowPopupMultipleIapReward(x.Data);
            }
            else
            {
                GameAnalytics.LogPurchaseEvent(GameAnalytics.IapPurchaseFail, prodId, result.FailureReason.ToString());
            }
        });
    }
}
