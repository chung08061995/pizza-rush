using System;
using UnityEngine;

public class SingleIAPDataView : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] private ItemView itemView;
    [SerializeField] private DraftUtils.OptionalValue<RewardDataView> rewardDataView = new();
    [SerializeField] private DraftUtils.OptionalTMPTextGroup priceText = new();
    [SerializeField] private DraftUtils.OptionalButtonGroup button = new();

    private SingleIAPData _data;

    public DraftUtils.OptionalButtonGroup Button => button;

    private void Start()
    {
        button.RegisterClickEvents();

        button.OnClickAction = ClickBuyButton;
    }

    private void ClickBuyButton()
    {
        string prodId = string.IsNullOrEmpty(_data.productId) ? _data.itemType.ToString() : _data.productId;
        
        DraftUtils.IAP.IAPManager.Instance.Purchase(prodId, result =>
        {
            if (result.IsSuccess)
            {
                GameAnalytics.LogPurchaseEvent(GameAnalytics.IapPurchaseSuccess, prodId);
                DataManager.Instance.Reward(new() { _data.reward });
                PopupManager.Instance.ShowPopupRewardSingle(_data.reward);
            }
            else
            {
                GameAnalytics.LogPurchaseEvent(GameAnalytics.IapPurchaseFail, prodId, result.FailureReason.ToString());
            }
        });
    }

    public void SetData(SingleIAPData data)
    {
        _data = data;
        SetRewardDataView();
        SetPriceText();
        SetItemView();
    }

    private void SetRewardDataView()
    {
        if (!rewardDataView.isPresent)
        {
            return;
        }
        rewardDataView.value.SetData(_data.reward);
    }

    private void SetPriceText()
    {
        priceText.SetText(DraftUtils.IAP.IAPManager.Instance.GetDisplayPrice(
            _data.productId,
            () => DataManager.Instance.iapData.GetCost(_data.productId).ToString()));
    }
    private void SetItemView()
    {
        itemView.SetData(_data.itemType);

    }
}
