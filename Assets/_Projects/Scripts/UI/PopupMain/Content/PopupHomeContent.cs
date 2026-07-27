using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PopupHomeContent : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] private AvatarDataListenerView avatarDataListenerView;
    [SerializeField] private Button startButton;
    [SerializeField] private DraftUtils.PersistentIntValueTextBinder levelText;
    [SerializeField] private ItemView goldView;
    [SerializeField] private Button noAdsButton;
    [SerializeField] private Button starterButton;
    [SerializeField] private Button dailyButton;
    [SerializeField] private DraftUtils.OptionalButtonGroup addGoldButton;
    [SerializeField] private HeartRecoveryView heartRecoveryView;
    [SerializeField] private UnlimitedHeartsView unlimitedHeartsView;
    public DraftUtils.OptionalButtonGroup AddGoldButton => addGoldButton;

    private void Start()
    {
        avatarDataListenerView.ItemView.Button.OnClickAction = ClickAvatar;
        startButton.onClick.AddListener(OnClickStart);

        if (levelText != null)
        {
            levelText.TextView.ValueToDisplayTextFunc = (value) => string.Format(GameConstain.StringFormats.LevelDisplayFormat, value);
            levelText.Bind(DataManager.Instance.level);
        }

        goldView.RemaningText.ValueToDisplayTextFunc = x => DraftUtils.Utils.Common.FormatNumber((int)x);
        goldView.SetData(ItemType.Gold);

        noAdsButton.onClick.AddListener(ClickNoAdsButton);
        if (DraftUtils.Ads.AdsManager.Instance != null)
        {
            DraftUtils.Ads.AdsManager.Instance.OnNoAdsEntitlementChanged += RefreshNoAdsVisibility;
        }
        RefreshNoAdsVisibility();
        starterButton.gameObject.SetActive(false);
        dailyButton.onClick.AddListener(ClickDailyButton);

        addGoldButton.Disable();


        heartRecoveryView.SetData(DataManager.Instance.heartRecoveryState.Value);
        unlimitedHeartsView.SetData(DataManager.Instance.unlimitedHeartsState.Value);

        HeartsManager.Instance.HeartRecoveryController.OnUpdateAction += UpdateLivesVisibility;
        HeartsManager.Instance.UnlimitedHeartsController.OnUpdateAction += UpdateLivesVisibility;
        UpdateLivesVisibility();

        heartRecoveryView.AddMoreButton.OnClickAction = OnClickAddMoreLives;
    }

    private void OnDestroy()
    {
        if (DraftUtils.Ads.AdsManager.Instance != null)
        {
            DraftUtils.Ads.AdsManager.Instance.OnNoAdsEntitlementChanged -= RefreshNoAdsVisibility;
        }

        if (HeartsManager.Instance != null)
        {
            if (HeartsManager.Instance.HeartRecoveryController != null)
            {
                HeartsManager.Instance.HeartRecoveryController.OnUpdateAction -= UpdateLivesVisibility;
            }
            if (HeartsManager.Instance.UnlimitedHeartsController != null)
            {
                HeartsManager.Instance.UnlimitedHeartsController.OnUpdateAction -= UpdateLivesVisibility;
            }
        }
    }

    private void RefreshNoAdsVisibility()
    {
        bool hasNoAds = DraftUtils.Ads.AdsManager.Instance != null &&
                        DraftUtils.Ads.AdsManager.Instance.HasNoAds;
        noAdsButton.gameObject.SetActive(!hasNoAds);
    }

    private void UpdateLivesVisibility()
    {
        bool isUnlimited = HeartsManager.Instance.IsUnlimitedActive();
        if (heartRecoveryView != null)
        {
            heartRecoveryView.gameObject.SetActive(!isUnlimited);
        }
        if (unlimitedHeartsView != null)
        {
            unlimitedHeartsView.gameObject.SetActive(isUnlimited);
        }
    }

    private void ClickDailyButton()
    {
        PopupManager.Instance.GetPopupDailyChallenge();
    }

    private void ClickNoAdsButton()
    {
        PopupManager.Instance.GetPopupNoAdsDetail();
    }

    private void OnClickStart()
    {
        if (HeartsManager.Instance.IsRemainingHeart())
        {
            OpenSelectBooterPopup();
        }
        else
        {
            PopupManager.Instance.GetPopupMoreLives(StartGame, null);
        }
    }

    private void OpenSelectBooterPopup()
    {
        var popup = PopupManager.Instance.GetPopupSelectBooter();
        popup.SetData(StartGame);
    }

    private void StartGame()
    {
        PopupManager.Instance.ShowPopupLoading(.7f);
        SceneControllerExtensions.LoadGameplay();
    }
    private void OnClickAddMoreLives()
    {
        PopupManager.Instance.ShowPopupMoreLives(null, null);
    }

    private void ClickAvatar()
    {
        PopupManager.Instance.GetPopupProfile();
    }
}
