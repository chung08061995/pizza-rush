using System;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using DG.Tweening;

public class PopupManager : DraftUtils.SingletonDontDestroyOnLoadMonoBehaviour<PopupManager>
{
    [SerializeField] private Transform panel1;
    [SerializeField] private Transform panel2;
    [SerializeField] public HomeContentsController homeContentsController;
    [SerializeField] public DraftUtils.ComponentReference<PopupGameplay> popupGameplayReference;
    [SerializeField] public DraftUtils.ComponentReference<PopupSkillGameplay> popupSkillGameplayReference;
    [SerializeField] private DraftUtils.ComponentReference<PopupWin> popupWinReference;
    [SerializeField] private DraftUtils.ComponentReference<PopupLoading> popupLoadingReference;
    [SerializeField] public DraftUtils.ComponentReference<PopupUsingSkill> popupUsingSkillReference;
    [SerializeField] private DraftUtils.ComponentReference<PopupLose> popupLoseReference;
    [SerializeField] private DraftUtils.ComponentReference<PopupGiftCode> popupGiftCodeReference;
    [SerializeField] private DraftUtils.ComponentReference<PopupSettingContent> popupSettingContentReference;
    [SerializeField] private DraftUtils.ComponentReference<PopupProfile> popupProfileReference;
    [SerializeField] private DraftUtils.ComponentReference<PopupSelectAvatar> popupSelectAvatarReference;
    [SerializeField] private DraftUtils.ComponentReference<PopupSelectBooter> popupSelectBooterReference = new();
    [SerializeField] private DraftUtils.ComponentReference<PopupCheatLevel> popupCheatLevelReference;
    [SerializeField] private DraftUtils.ComponentReference<PopupIapDetail> popupNoAdsDetailReference = new();
    [SerializeField] private DraftUtils.ComponentReference<PopupDailyChallenge> popupDailyChallengeReference = new();
    [SerializeField] private DraftUtils.ComponentReference<PopupMoreLives> popupMoreLivesReference = new();
    [SerializeField] private DraftUtils.ComponentReference<PopupBuyItem> popupBuyItemReference = new();
    [SerializeField] private DraftUtils.ComponentReference<PopupConfirmReplay> popupConfirmReplayReference = new();
    [SerializeField] private DraftUtils.ComponentReference<PopupUsingBooter> popupUsingBooterReference = new();
    [SerializeField] private DraftUtils.ComponentReference<PopupCoffeeTime> popupCoffeeTimeReference = new();
    [SerializeField] private DraftUtils.ComponentReference<PopupBlockUser> popupBlockUserReference = new();
    [SerializeField] private DraftUtils.ComponentReference<PopupNewSkillItem> popupNewSkillItemReference = new();
    [SerializeField] private DraftUtils.ComponentReference<PopupCongratulation> popupCongratulationReference = new();
    [SerializeField] private DraftUtils.ComponentReference<PopupReward> popupRewardReference = new();

    private DraftUtils.PopupFactory _popupFactory = new();
    public DraftUtils.PopupFactory PopupFactory => _popupFactory;
    [Button]
    public PopupGameplay GetPopupGameplay()
    {
        var popup = _popupFactory.DestroyCurrentAndCreate(popupGameplayReference, panel1);
        return popup;
    }
    [Button]
    public void HidePopupGameplay()
    {
        if (popupGameplayReference.instance != null)
        {
            DestroyImmediate(popupGameplayReference.instance.gameObject);
        }
    }

    [Button]
    public PopupWin GetPopupWin(int goldReward = 0)
    {
        var popup = _popupFactory.DestroyCurrentAndCreate(popupWinReference, panel1);
        popup.popup.ShowWithAnimation();
        popup.SetData(goldReward);
        return popup;
    }

    [Button]
    public void ShowPopupLoading(float duration)
    {
        _popupQueue.Enqueue(Show);

        void Show()
        {
            var popup = GetPopupLoading(duration);
            var eventActions = DraftUtils.EventActions.Create(popup.transform);
            eventActions.onDisableAction = _popupQueue.ExecuteNext;
        }
        _popupQueue.TryExecute();
    }

    [Button]
    public PopupLoading GetPopupLoading(float duration, Action onComplete = null)
    {
        var popup = _popupFactory.DestroyCurrentAndCreate(popupLoadingReference, panel2);
        popup.SetData(duration, onComplete);
        return popup;
    }
    [Button]
    public PopupSkillGameplay GetPopupSkillGameplay()
    {
        var popup = _popupFactory.DestroyCurrentAndCreate(popupSkillGameplayReference, panel1);
        popup.SetData();
        popup.popup.Show();
        return popup;
    }

    [Button]
    public PopupUsingSkill GetPopupUsingSkill()
    {
        var popup = _popupFactory.DestroyCurrentAndCreate(popupUsingSkillReference, panel1);
        popup.popup.ShowWithAnimation();
        return popup;
    }

    [Button]
    public void HidePopupUsingSkill()
    {
        if (popupUsingSkillReference.instance != null)
        {
            DestroyImmediate(popupUsingSkillReference.instance.gameObject);
        }
    }

    [Button]
    public PopupLose GetPopupLose()
    {
        var popup = _popupFactory.DestroyCurrentAndCreate(popupLoseReference, panel1);
        popup.popup.ShowWithAnimation();
        popup.SetData();
        return popup;
    }

    [Button]
    public PopupGiftCode GetPopupGiftCode()
    {
        var popup = _popupFactory.DestroyCurrentAndCreate(popupGiftCodeReference, panel1);
        popup.popup.ShowWithAnimation();
        return popup;
    }
    [Button]
    public PopupSettingContent GetPopupSettingGameplay()
    {
        var popup = _popupFactory.DestroyCurrentAndCreate(popupSettingContentReference, panel1);
        popup.popup.ShowWithAnimation();
        return popup;
    }

    [Button]
    public PopupProfile GetPopupProfile()
    {
        var popup = _popupFactory.DestroyCurrentAndCreate(popupProfileReference, panel1);
        popup.popup.ShowWithAnimation();
        return popup;
    }

    [Button]
    public PopupSelectAvatar GetPopupSelectAvatar()
    {
        var popup = _popupFactory.DestroyCurrentAndCreate(popupSelectAvatarReference, panel1);
        popup.popup.ShowWithAnimation();
        popup.SetData();
        return popup;
    }

    [Button]
    public PopupSelectBooter GetPopupSelectBooter()
    {
        var popup = _popupFactory.DestroyCurrentAndCreate(popupSelectBooterReference, panel1);
        popup.popup.ShowWithAnimation();
        popup.SetData();
        return popup;
    }

    [Button]
    public PopupCheatLevel GetPopupCheatLevel()
    {
        var popup = _popupFactory.DestroyCurrentAndCreate(popupCheatLevelReference, panel1);
        return popup;
    }



    [Button]
    public PopupIapDetail GetPopupNoAdsDetail()
    {
        var popup = _popupFactory.DestroyCurrentAndCreate(popupNoAdsDetailReference, panel1);
        popup.popup.ShowWithAnimation();
        popup.SetData(DataManager.Instance.iapData.noAds);
        return popup;
    }

    [Button]
    public void HideAllPopupInGameplay()
    {
        popupGameplayReference.DestroyImmediate();
        popupUsingSkillReference.DestroyImmediate();
        popupSkillGameplayReference.DestroyImmediate();
        popupLoseReference.DestroyImmediate();
        popupWinReference.DestroyImmediate();
        popupSettingContentReference.DestroyImmediate();
        popupConfirmReplayReference.DestroyImmediate();
        popupUsingBooterReference.DestroyImmediate();
        popupCoffeeTimeReference.DestroyImmediate();
        popupBlockUserReference.DestroyImmediate();
        popupCongratulationReference.DestroyImmediate();
    }

    [Button]
    public PopupDailyChallenge GetPopupDailyChallenge()
    {
        var popup = _popupFactory.DestroyCurrentAndCreate(popupDailyChallengeReference, panel1);
        popup.popup.ShowWithAnimation();
        popup.SetData();
        return popup;
    }

    [Button]
    public PopupMoreLives GetPopupMoreLives(Action onCompletedAction, Action onCloseAction)
    {
        var popup = _popupFactory.DestroyCurrentAndCreate(popupMoreLivesReference, panel1);
        popup.popup.ShowWithAnimation();
        popup.SetData();
        RefreshBuyButton();
        popup.BuyButton.OnClickAction = ClickBuy;
        popup.WatchAdsButton.OnClickAction = ClickWatchAds;

        popup.popup.closeButton.OnClickAction = ClickClose;
        void ClickClose()
        {
            onCloseAction?.Invoke();
            popup.popup.HideWithAnimation();
        }
        return popup;

        void ClickBuy()
        {
            if (DataManager.Instance.costItems.TryGetValue(ItemType.Booter_LifeTime, out var cost))
            {
                int currentGold = DataManager.Instance.gold.Value;
                if (currentGold >= cost)
                {
                    DataManager.Instance.Using(ItemType.Gold, -cost);
                    DataManager.Instance.UpHeartRecoveryState(1);
                    onCompletedAction?.Invoke();
                    popup.popup.HideWithAnimation();
                }
                else
                {
                    popup.BuyButton.SetText("Not enough Gold");
                    popup.BuyButton.SetInteractable(false);
                }
            }
        }

        void RefreshBuyButton()
        {
            if (!DataManager.Instance.costItems.TryGetValue(ItemType.Booter_LifeTime, out var cost))
            {
                popup.BuyButton.SetInteractable(false);
                return;
            }

            bool canBuy = DataManager.Instance.gold.Value >= cost;
            popup.BuyButton.SetInteractable(canBuy);
            if (!canBuy)
            {
                popup.BuyButton.SetText("Not enough Gold");
            }
        }
        void ClickWatchAds()
        {
            DraftUtils.Ads.AdsManager.Instance.ShowRewarded("MoreLive", (success) =>
            {
                if (!success)
                {
                    return;
                }
                DataManager.Instance.UpHeartRecoveryState(1);
                onCompletedAction?.Invoke();
                popup.popup.HideWithAnimation();
            });

        }
    }

    [Button]
    public void ShowPopupMoreLives(Action onCompletedAction, Action onCloseAction)
    {
        _popupQueue.Enqueue(Show);

        void Show()
        {
            var popup = GetPopupMoreLives(onCompletedAction, onCloseAction);
            var eventActions = DraftUtils.EventActions.Create(popup.transform);
            eventActions.onDisableAction = _popupQueue.ExecuteNext;
        }
        _popupQueue.TryExecute();
    }

    [Button]
    public void TestShowPopupMoreLives() => ShowPopupMoreLives(null, null);

    [Button]
    public PopupBuyItem GetPopupBuyItem()
    {
        var popup = _popupFactory.DestroyCurrentAndCreate(popupBuyItemReference, panel1);
        popup.popup.ShowWithAnimation();
        return popup;
    }
    [Button]
    public PopupConfirmReplay GetPopupConfirmReplay()
    {
        var popup = _popupFactory.DestroyCurrentAndCreate(popupConfirmReplayReference, panel1);
        popup.popup.ShowWithAnimation();
        return popup;
    }

    [Button]
    public PopupUsingBooter GetPopupUsingBooter(ItemType itemType, float duration, Action onCompletedAction)
    {
        var popup = _popupFactory.DestroyCurrentAndCreate(popupUsingBooterReference, panel1);
        popup.SetData(itemType, duration, onCompletedAction);
        popup.popup.ShowWithAnimation();
        return popup;
    }

    [Button]
    public void ShowPopupUsingBooter(ItemType itemType, float duration, Action onCompletedAction)
    {
        _popupQueue.Enqueue(Show);

        void Show()
        {
            var popup = GetPopupUsingBooter(itemType, duration, onCompletedAction);
            var eventActions = DraftUtils.EventActions.Create(popup.transform);
            eventActions.onDisableAction = _popupQueue.ExecuteNext;
        }

        _popupQueue.TryExecute();
    }

    [Button]
    public PopupCoffeeTime GetPopupCoffeeTime(int bonusTimeValue, RectTransform targetTransform, Action onCompletedAction)
    {
        var popup = _popupFactory.DestroyCurrentAndCreate(popupCoffeeTimeReference, panel1);
        popup.SetData(bonusTimeValue, onCompletedAction);
        popup.PlayMoveToGameplayTimer(targetTransform);
        return popup;
    }

    [Button]
    public void ShowPopupCoffeeTime(int bonusTimeValue, RectTransform targetTransform, Action onCompletedAction)
    {
        _popupQueue.Enqueue(Show);

        void Show()
        {
            var popup = GetPopupCoffeeTime(bonusTimeValue, targetTransform, onCompletedAction);
            var eventActions = DraftUtils.EventActions.Create(popup.transform);
            eventActions.onDisableAction = _popupQueue.ExecuteNext;
        }

        _popupQueue.TryExecute();
    }
    [Button]
    public PopupBlockUser GetPopupBlockUser(float timeout)
    {
        var popup = _popupFactory.DestroyCurrentAndCreate(popupBlockUserReference, panel1);
        popup.SetData(timeout);
        return popup;
    }
    [Button]
    public PopupNewSkillItem GetPopupNewSkillItem()
    {
        var popup = _popupFactory.DestroyCurrentAndCreate(popupNewSkillItemReference, panel1);
        popup.popup.ShowWithAnimation();
        return popup;
    }

    [Button]
    public PopupReward GetPopupReward()
    {
        var popup = _popupFactory.DestroyCurrentAndCreate(popupRewardReference, panel1);
        popup.popup.ShowWithAnimation();
        return popup;
    }

    private readonly DraftUtils.CommandQueue _popupQueue = new();

    [Button]
    public void ShowPopupWin(int goldReward = 0)
    {
        _popupQueue.Enqueue(Show);

        void Show()
        {
            var popup = _popupFactory.DestroyCurrentAndCreate(popupWinReference, panel1);
            popup.popup.ShowWithAnimation();
            popup.SetData(goldReward);
            var eventActions = DraftUtils.EventActions.Create(popup.transform);
            eventActions.onDisableAction = _popupQueue.ExecuteNext;
        }
        _popupQueue.TryExecute();
    }

    [Button]
    public void ShowPopupNewSkillItem(ItemType itemType)
    {
        _popupQueue.Enqueue(Show);

        void Show()
        {
            var popup = _popupFactory.DestroyCurrentAndCreate(popupNewSkillItemReference, panel1);
            popup.popup.ShowWithAnimation();
            popup.SetData(itemType);
            var eventActions = DraftUtils.EventActions.Create(popup.transform);
            eventActions.onDisableAction = _popupQueue.ExecuteNext;
        }
        _popupQueue.TryExecute();
    }

    [Button]
    public PopupCongratulation GetPopupCongratulation()
    {
        var popup = _popupFactory.DestroyCurrentAndCreate(popupCongratulationReference, panel1);
        popup.popup.ShowWithAnimation();
        return popup;
    }

    [Button]
    public void ShowPopupCongratulation(float duration)
    {
        _popupQueue.Enqueue(Show);

        void Show()
        {
            var popup = _popupFactory.DestroyCurrentAndCreate(popupCongratulationReference, panel1);
            popup.popup.ShowWithAnimation();
            popup.SetData(duration);
            var eventActions = DraftUtils.EventActions.Create(popup.transform);
            eventActions.onDisableAction = _popupQueue.ExecuteNext;
        }
        _popupQueue.TryExecute();
    }

    [Button]
    public void ShowPopupRewardSingle(RewardData reward)
    {
        _popupQueue.Enqueue(Show);

        void Show()
        {
            var popup = _popupFactory.DestroyCurrentAndCreate(popupRewardReference, panel1);
            popup.popup.ShowWithAnimation();
            popup.SetDataSingle(reward);

            var eventActions = DraftUtils.EventActions.Create(popup.transform);
            eventActions.onDisableAction = _popupQueue.ExecuteNext;
        }
        _popupQueue.TryExecute();
    }
    [Button]
    public void ShowPopupMultipleIapReward(MultipleIAPData data)
    {
        _popupQueue.Enqueue(Show);

        void Show()
        {
            var popup = _popupFactory.DestroyCurrentAndCreate(popupRewardReference, panel1);
            popup.popup.ShowWithAnimation();
            if (data.features.Count > 0)
            {
                popup.SetDataFeatures(data.features);
            }
            var rewards = MultipleIAPDataExtensions.GetRewards(data);
            if(rewards.Count > 0)
            {
                popup.SetDataMultiple(rewards);
            }
            popup.RebuildLayout();

            // Lấy danh sách các transform cần chạy hiệu ứng
            var itemsToAnimate = new List<Transform>();
            if (popup.MultipleItemReward != null && popup.MultipleItemReward.gameObject.activeSelf)
            {
                foreach (var item in popup.MultipleItemReward.ActiveItems)
                {
                    if (item != null) itemsToAnimate.Add(item.transform);
                }
            }
            if (popup.RewardFeaturesView != null && popup.RewardFeaturesView.gameObject.activeSelf)
            {
                foreach (var item in popup.RewardFeaturesView.ActiveItems)
                {
                    if (item != null) itemsToAnimate.Add(item.transform);
                }
            }

            // Set scale về 0 ngay lập tức
            foreach (var t in itemsToAnimate)
            {
                t.localScale = Vector3.zero;
            }

            // Đợi 0.3s (khi popup hiển thị xong) rồi scale tuần tự về 1
            float startDelay = 0.3f;
            float stepDelay = 0.1f;
            for (int i = 0; i < itemsToAnimate.Count; i++)
            {
                itemsToAnimate[i].DOScale(Vector3.one, 0.4f)
                    .SetEase(Ease.OutBack)
                    .SetDelay(startDelay + i * stepDelay);
            }

            var eventActions = DraftUtils.EventActions.Create(popup.transform);
            eventActions.onDisableAction = _popupQueue.ExecuteNext;
        }
        _popupQueue.TryExecute();
    }

    [Button]
    public void ShowPopupRewardMultiple(List<RewardData> rewards)
    {
        _popupQueue.Enqueue(Show);

        void Show()
        {
            var popup = _popupFactory.DestroyCurrentAndCreate(popupRewardReference, panel1);
            popup.popup.ShowWithAnimation();
            popup.SetDataMultiple(rewards);

            var eventActions = DraftUtils.EventActions.Create(popup.transform);
            eventActions.onDisableAction = _popupQueue.ExecuteNext;
        }
        _popupQueue.TryExecute();
    }

    [Button]
    public void ShowPopupRewardFeatures(List<ItemType> features)
    {
        _popupQueue.Enqueue(Show);

        void Show()
        {
            var popup = _popupFactory.DestroyCurrentAndCreate(popupRewardReference, panel1);
            popup.popup.ShowWithAnimation();
            popup.SetDataFeatures(features);

            var eventActions = DraftUtils.EventActions.Create(popup.transform);
            eventActions.onDisableAction = _popupQueue.ExecuteNext;
        }
        _popupQueue.TryExecute();
    }
}
