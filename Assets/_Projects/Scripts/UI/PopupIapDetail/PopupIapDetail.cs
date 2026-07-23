using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class PopupIapDetail : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] public DraftUtils.Popup popup;
    [SerializeField] public MultipleIAPDataView multipleIAPDataView;
    [SerializeField] private Button buyButton;

    [ShowInInspector]
    [ReadOnly]
    private MultipleIAPData _data;

    private void Start()
    {
        popup.closeButton.OnClickAction = popup.HideWithAnimation;
        buyButton.onClick.AddListener(ClickBuyButton);
    }

    public void SetData(MultipleIAPData data)
    {
        _data = data;
        SetMultipleIAPDataView();

    }
    private void SetMultipleIAPDataView()
    {
        multipleIAPDataView.SetData(_data);
    }

    private void ClickBuyButton()
    {
        if (_data == null)
        {
            return;
        }

        string productId = string.IsNullOrEmpty(_data.productId)
            ? _data.itemType.ToString()
            : _data.productId;

        DraftUtils.IAP.IAPManager.Instance.Purchase(productId, result =>
        {
            if (!result.IsSuccess)
            {
                GameAnalytics.LogPurchaseEvent(
                    GameAnalytics.IapPurchaseFail,
                    productId,
                    result.FailureReason.ToString());
                return;
            }

            GameAnalytics.LogPurchaseEvent(GameAnalytics.IapPurchaseSuccess, productId);

            var rewards = MultipleIAPDataExtensions.GetRewards(_data);
            if (rewards.Count > 0)
            {
                DataManager.Instance.Reward(rewards);
            }

            PopupManager.Instance.ShowPopupMultipleIapReward(_data);
        });
    }
}
