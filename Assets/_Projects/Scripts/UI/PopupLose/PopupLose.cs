using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupLose : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] public DraftUtils.Popup popup;
    [SerializeField] private ItemView goldView;
    [SerializeField] private TMP_Text bonusTimeText;
    [SerializeField] private TMP_Text descriptbonusTimeText;
    [SerializeField] private Button playOnButton;
    [SerializeField] private Button watchAdsButton;
    [SerializeField] private Button giveUpButton;
    [SerializeField] private DraftUtils.HoldToViewLevelButtonComponent holdToViewLevelButtonComponent;
    [SerializeField] private ItemView playOnItemView;

    private void Start()
    {
        popup.closeButton.OnClickAction = ClickGiveUpButton;

        playOnButton.onClick.AddListener(ClickPlayOnButton);
        watchAdsButton.onClick.AddListener(ClickWatchAdsButton);
        giveUpButton.onClick.AddListener(ClickGiveUpButton);

        holdToViewLevelButtonComponent.Init();
    }

    private void ClickGiveUpButton()
    {
        popup.HideWithAnimation();
        HeartsManager.Instance.UseHeart();
        PopupManager.Instance.HideAllPopupInGameplay();
        SceneControllerExtensions.LoadMain();
    }

    private void ClickWatchAdsButton()
    {
        DraftUtils.Ads.AdsManager.Instance.ShowRewarded("PlayOnLose", (success) =>
        {
            if (!success)
            {
                return;
            }

            int bonusTime = 50;
            var levelRunner = LevelFactory.Instance.LevelRunner;
            if (levelRunner != null)
            {
                levelRunner.Timer.SetDuration(bonusTime);
                levelRunner.Timer.StartCountdown();
                levelRunner.GameplayStateMachine.ChangeToDragContainerState();
            }

            popup.HideWithAnimation();
        });
    }

    private void ClickPlayOnButton()
    {
        if (DataManager.Instance.costItems.TryGetValue(ItemType.Booter_PlayOn, out var cost))
        {
            int currentGold = DataManager.Instance.gold.Value;
            if (currentGold >= cost)
            {
                DataManager.Instance.Using(ItemType.Gold, -cost);

                int bonusTime = 20;
                var levelRunner = LevelFactory.Instance.LevelRunner;
                if (levelRunner != null)
                {
                    levelRunner.Timer.SetDuration(bonusTime);
                    levelRunner.Timer.StartCountdown();
                    levelRunner.GameplayStateMachine.ChangeToDragContainerState();
                }

                popup.HideWithAnimation();
            }
            else
            {
                PopupManager.Instance.GetPopupShop();
            }
        }
    }

    public void SetData()
    {
        int bonusTime = 20;
        SetBonusTimeText(bonusTime);
        SetDescriptionBonusTimeText(bonusTime);
        SetGoldView();
        SetPlayOnItemView();
    }

    private void SetBonusTimeText(int bonusTime)
    {
        bonusTimeText.text = string.Format(GameConstain.StringFormats.BonusTimePopupLose, bonusTime);
    }

    private void SetDescriptionBonusTimeText(int bonusTime)
    {
        descriptbonusTimeText.text = string.Format(GameConstain.StringFormats.DescriptionBonusTimePopupLose, bonusTime);
    }

    private void SetGoldView()
    {
        goldView.RemaningText.ValueToDisplayTextFunc = x => DraftUtils.Utils.Common.FormatNumber((int)x);
        goldView.SetData(ItemType.Gold);
    }

    private void SetPlayOnItemView()
    {
        if (playOnItemView != null)
        {
            playOnItemView.SetData(ItemType.Booter_PlayOn);
        }
    }
}
