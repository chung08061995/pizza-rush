using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupWin : DraftUtils.DraftMonoBehaviour
{
    private const string DoubleGoldPlacement = "WinDoubleGold";

    [SerializeField] public DraftUtils.Popup popup;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button x2RewardButton;

    private int _goldReward;
    private bool _doubleRewardClaimed;
    private bool _rewardRequestInProgress;

    private void Start()
    {
        ResolveX2RewardButton();
        nextButton.onClick.AddListener(OnNextButtonClicked);
        x2RewardButton?.onClick.AddListener(OnX2RewardButtonClicked);
        levelText.text = string.Format(GameConstain.StringFormats.LevelDisplayFormat, DataManager.Instance.Level.Value);
        RefreshRewardText();
        RefreshX2RewardButton();
    }

    public void SetData(int goldReward)
    {
        _goldReward = goldReward;
        _doubleRewardClaimed = false;
        _rewardRequestInProgress = false;
        RefreshRewardText();
        RefreshX2RewardButton();
    }

    private void RefreshRewardText()
    {
        if (rewardText == null)
        {
            rewardText = GetComponentInChildren<TMP_Text>(true);
        }

        if (rewardText != null)
        {
            int displayedReward = _doubleRewardClaimed ? _goldReward * 2 : _goldReward;
            rewardText.text = displayedReward > 0
                ? $"Gold +{displayedReward}"
                : "No reward";
        }
    }

    private void ResolveX2RewardButton()
    {
        if (x2RewardButton != null && x2RewardButton != nextButton)
        {
            return;
        }

        foreach (var button in GetComponentsInChildren<Button>(true))
        {
            if (button.gameObject.name == "Button_Rectangle_Green")
            {
                x2RewardButton = button;
                return;
            }
        }
    }

    private void RefreshX2RewardButton()
    {
        ResolveX2RewardButton();
        if (x2RewardButton == null)
        {
            return;
        }

        bool canOfferDoubleReward =
            _goldReward > 0 &&
            !_doubleRewardClaimed &&
            MonetizationConfig.RewardedEnabled &&
            DraftUtils.Ads.AdsManager.Instance != null;

        x2RewardButton.gameObject.SetActive(canOfferDoubleReward);
        x2RewardButton.interactable = canOfferDoubleReward && !_rewardRequestInProgress;
    }

    private void OnX2RewardButtonClicked()
    {
        if (_doubleRewardClaimed || _rewardRequestInProgress || _goldReward <= 0)
        {
            return;
        }

        var adsManager = DraftUtils.Ads.AdsManager.Instance;
        if (adsManager == null || !MonetizationConfig.RewardedEnabled)
        {
            RefreshX2RewardButton();
            return;
        }

        _rewardRequestInProgress = true;
        x2RewardButton.interactable = false;
        nextButton.interactable = false;
        GameAnalytics.Log(GameAnalytics.RewardedAdShow);

        adsManager.ShowRewarded(DoubleGoldPlacement, success =>
        {
            if (this == null)
            {
                return;
            }

            _rewardRequestInProgress = false;
            nextButton.interactable = true;

            if (!success)
            {
                RefreshX2RewardButton();
                return;
            }

            _doubleRewardClaimed = true;
            DataManager.Instance.Reward(new()
            {
                new RewardData { itemType = ItemType.Gold, amount = _goldReward }
            });
            GameAnalytics.Log(GameAnalytics.RewardedAdComplete);
            RefreshRewardText();
            RefreshX2RewardButton();
        });
    }

    private void OnNextButtonClicked()
    {
        popup.HideWithAnimation();
        LevelFactory.Instance.LoadCurrentLevelData();
    }
}
