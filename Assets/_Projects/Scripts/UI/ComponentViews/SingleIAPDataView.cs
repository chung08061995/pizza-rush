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
                DataManager.Instance.Reward(new() { _data.reward });
                PopupManager.Instance.ShowPopupRewardSingle(_data.reward);
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
        priceText.SetText(DataManager.Instance.iapData.GetCost(_data.productId));
    }
    private void SetItemView()
    {
        itemView.SetData(_data.itemType);

    }
}
