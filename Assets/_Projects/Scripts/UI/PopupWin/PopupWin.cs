using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupWin : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] public DraftUtils.Popup popup;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button x2RewardButton;

    private int _goldReward;

    private void Start()
    {
        nextButton.onClick.AddListener(OnNextButtonClicked);
        levelText.text = string.Format(GameConstain.StringFormats.LevelDisplayFormat, DataManager.Instance.Level.Value);
        RefreshRewardText();
    }

    public void SetData(int goldReward)
    {
        _goldReward = goldReward;
        RefreshRewardText();
    }

    private void RefreshRewardText()
    {
        if (rewardText == null)
        {
            rewardText = GetComponentInChildren<TMP_Text>(true);
        }

        if (rewardText != null)
        {
            rewardText.text = _goldReward > 0
                ? $"Gold +{_goldReward}"
                : "No reward";
        }
    }

    private void OnDisable()
    {
    }

    private void OnNextButtonClicked()
    {
        popup.HideWithAnimation();
        LevelFactory.Instance.LoadCurrentLevelData();
    }
}
