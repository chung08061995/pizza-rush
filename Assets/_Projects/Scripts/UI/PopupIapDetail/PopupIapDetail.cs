using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DraftUtils.IAP;

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
        if (IAPManager.Instance != null) IAPManager.Instance.OnInitialized += OnIapInitialized;
    }

    public void SetData(MultipleIAPData data)
    {
        _data = data;
        SetMultipleIAPDataView();
        RefreshAvailability();

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

        buyButton.interactable = false;
        IAPManager.Instance.Purchase(productId, result =>
        {
            if (!result.IsSuccess)
            {
                GameAnalytics.LogPurchaseEvent(
                    GameAnalytics.IapPurchaseFail,
                    productId,
                    result.FailureReason.ToString());
                RefreshAvailability();
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

    private void OnIapInitialized(bool success) => RefreshAvailability();

    private void RefreshAvailability()
    {
        if (buyButton == null || _data == null) return;
        var manager = IAPManager.Instance;
        string productId = GameConstain.IAPProductId.NoAds;
        bool available = manager != null && manager.Service != null && manager.Service.IsInitialized &&
                         manager.Service.GetProductInfo(productId) != null && !manager.IsOwned(productId);
        buyButton.interactable = available;
        var label = buyButton.GetComponentInChildren<TMP_Text>(true);
        if (label != null && !available)
            label.SetText(manager != null && manager.IsOwned(productId) ? "Owned" : "Store unavailable");
    }

    private void OnDestroy()
    {
        if (IAPManager.Instance != null) IAPManager.Instance.OnInitialized -= OnIapInitialized;
    }
}
