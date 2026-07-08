using DraftUtils;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : DraftUtils.SingletonDontDestroyOnLoadMonoBehaviour<DataManager>
{
    [SerializeField] public SpriteItemsSO iconItemsSO;
    [SerializeField] private ParametterGameConfigSO parametterGameConfigSO;
    [SerializeField] public LevelUpRewardsSO levelUpData;
    [SerializeField] public AudioClipDataCreator audioClipDataCreator;
    [SerializeField] private ColorsSO productionLineColorsSO;
    [SerializeField] private ColorsSO containerColorsSO;
    [SerializeField] private ColorFactorySO productionColorsSO;

    public IAPData iapData = new();
    public List<DailyChallengeMilestone> milestonesDaily = new()
    {
        new DailyChallengeMilestone { requiredDays = 3,  reward = new RewardData { itemType = ItemType.Gold,            amount = 50 } },
        new DailyChallengeMilestone { requiredDays = 7,  reward = new RewardData { itemType = ItemType.Booter_LifeTime, amount = 60 } },
        new DailyChallengeMilestone { requiredDays = 14, reward = new RewardData { itemType = ItemType.Gold,            amount = 250 } },
        new DailyChallengeMilestone { requiredDays = 25, reward = new RewardData { itemType = ItemType.Gold,            amount = 500 } },
    };
    public DailyChallengeManager dailyChallengeManager = new();

    [ShowInInspector][ReadOnly] public DraftUtils.PersistentValue<string> playerName = new();
    [ShowInInspector][ReadOnly] public DraftUtils.PersistentValue<ItemType> currentAvatar = new();
    [ShowInInspector][ReadOnly] public DraftUtils.PersistentValue<int> level = new();
    public DraftUtils.PersistentValue<int> gold => remainningItems[ItemType.Gold];

    [ShowInInspector][ReadOnly] public DraftUtils.PersistentValue<bool> musicVolume = new();
    [ShowInInspector][ReadOnly] public DraftUtils.PersistentValue<bool> sfxVolume = new();
    [ShowInInspector][ReadOnly] public DraftUtils.PersistentValue<bool> vibrate = new();
    public ColorsSO ProductionLineColorsSO => productionLineColorsSO;
    public ColorsSO ContainerColorsSO => containerColorsSO;
    public ColorFactorySO ProductionColorsSO => productionColorsSO;
    public AudioClipDataCreator AudioClipDataCreator => audioClipDataCreator;
    public DraftUtils.PersistentValue<int> Level => level;
    public ParametterGameConfigSO ParametterGameConfigSO => parametterGameConfigSO;


    [ShowInInspector][ReadOnly] public DraftUtils.PersistentValue<HeartRecoveryState> heartRecoveryState = new();
    [ShowInInspector][ReadOnly] public DraftUtils.PersistentValue<UnlimitedHeartsState> unlimitedHeartsState = new();

    public Dictionary<ItemType, DraftUtils.PersistentValue<int>> remainningItems = new()
    {
        {ItemType.Gold, new() },
        {ItemType.Skill_AddTile, new() },
        {ItemType.Skill_DestroyContainer, new() },
        {ItemType.Skill_FreezeTime, new() },
        {ItemType.Skill_SplitContainer, new() },
        {ItemType.Booter_CoffeeTime, new() },
        {ItemType.Booter_Magic, new() },
    };

    public List<ItemType> avatarTypes = new()
    {
        ItemType.Avatar_1,
        ItemType.Avatar_2,
        ItemType.Avatar_3,
        ItemType.Avatar_4,
        ItemType.Avatar_5,
        ItemType.Avatar_6,
        ItemType.Avatar_7,
        ItemType.Avatar_8,
        ItemType.Avatar_9,
    };

    public Dictionary<ItemType, string> nameItems = new()
    {
        {ItemType.MultipleIAPData_NoAds, "No Ads" },
        {ItemType.MultipleIAPData_NoAdsBundle, "No Ads Bundle" },
        {ItemType.MultipleIAPData_SmallBundle, "Small Bundle" },
        {ItemType.MultipleIAPData_MediumBundle, "Medium Bundle" },
        {ItemType.MultipleIAPData_LargeBundle, "Large Bundle" },
        {ItemType.MultipleIAPData_Starter, "Starter" },


        {ItemType.Skill_AddTile, "Add Tile" },
        {ItemType.Skill_DestroyContainer, "Destroy Container" },
        {ItemType.Skill_FreezeTime, "Freeze Time" },
        {ItemType.Skill_SplitContainer, "Slit Container" },
    };

    public Dictionary<ItemType, string> descriptionItems = new()
    {
        {ItemType.Iap_Feature_RemoveAds, "Remove obligatory ads" },
        {ItemType.Iap_Feature_RemoveBottomBannerAds, "Remove bottom banner ads" },
        {ItemType.Iap_Feature_KeepRewardAdsForAds, "Keep optional ads for rewards" },
        {ItemType.Skill_FreezeTime, "Pause the timer briefly." },
        {ItemType.Skill_SplitContainer, "Split the container in two." },
        {ItemType.Skill_DestroyContainer, "Destroy the container." },
        {ItemType.Skill_AddTile, "Add one extra tile." },
    };
    public Dictionary<ItemType, string> descriptionNewItems = new()
    {
        {ItemType.Skill_FreezeTime, "Freeze Time" },
        {ItemType.Skill_SplitContainer, "Split container" },
        {ItemType.Skill_DestroyContainer, "Destroy container" },
        {ItemType.Skill_AddTile, "Add Tile" },
    };
    public Dictionary<ItemType, string> skillTutorialItems = new()
    {
        {ItemType.Skill_FreezeTime, "Pause the timer." },
        {ItemType.Skill_SplitContainer, "Split the container." },
        {ItemType.Skill_DestroyContainer, "Destroy the container." },
        {ItemType.Skill_AddTile, "Add an extra tile." },
    };
    public Dictionary<ItemType, int> levelUnlockItems = new()
    {
        {ItemType.Skill_FreezeTime, 2 },
        {ItemType.Skill_SplitContainer, 3 },
        {ItemType.Skill_DestroyContainer, 4 },
        {ItemType.Skill_AddTile, 5 },
    };
    public Dictionary<ItemType, int> costItems = new()
    {
        {ItemType.Skill_AddTile, 1200 },
        {ItemType.Skill_DestroyContainer, 1200 },
        {ItemType.Skill_FreezeTime, 1200 },
        {ItemType.Skill_SplitContainer, 1200 },
        {ItemType.Booter_LifeTime, 900 },
        {ItemType.Booter_PlayOn, 200 },

    };


    // Camera size: x, số ô chiều ngang của gird
    //              y: size camera
    public Dictionary<int, int> cameraSize = new()
    {
        {4, 7},
        {5, 9},
        {6, 11},
    };
    private void Start()
    {
        iconItemsSO.BuildDictionary();
        productionLineColorsSO.BuildDictionary();
        containerColorsSO.BuildDictionary();
        productionColorsSO.BuildDictionary();

        if (audioClipDataCreator != null)
        {
            audioClipDataCreator.Initialized();
        }
        dailyChallengeManager.Initialize();


        SettingPersistentValue(
            persistentValue: level,
            defaultValue: 1,
            playerPrefsKey: GameConstain.PlayerPrefsKey.Level
        );

        SettingPersistentValue(
            persistentValue: remainningItems[ItemType.Gold],
            defaultValue: 0,
            playerPrefsKey: GameConstain.PlayerPrefsKey.Gold
        );
        SettingPersistentValue(
            persistentValue: remainningItems[ItemType.Skill_AddTile],
            defaultValue: 1,
            playerPrefsKey: GameConstain.PlayerPrefsKey.SkillAddTile
        );
        SettingPersistentValue(
            persistentValue: remainningItems[ItemType.Skill_DestroyContainer],
            defaultValue: 1,
            playerPrefsKey: GameConstain.PlayerPrefsKey.SkillDestroyContainer
        );
        SettingPersistentValue(
            persistentValue: remainningItems[ItemType.Skill_FreezeTime],
            defaultValue: 1,
            playerPrefsKey: GameConstain.PlayerPrefsKey.Skill_FreezeTime
        );
        SettingPersistentValue(
            persistentValue: remainningItems[ItemType.Skill_SplitContainer],
            defaultValue: 1,
            playerPrefsKey: GameConstain.PlayerPrefsKey.Skill_SplitContainer
        );

        SettingPersistentValue(
            persistentValue: remainningItems[ItemType.Booter_CoffeeTime],
            defaultValue: 1,
            playerPrefsKey: GameConstain.PlayerPrefsKey.BooterCoffeeTime
        );
        SettingPersistentValue(
            persistentValue: remainningItems[ItemType.Booter_Magic],
            defaultValue: 1,
            playerPrefsKey: GameConstain.PlayerPrefsKey.BooterMagic
        );


        SettingPersistentValue(
            persistentValue: playerName,
            defaultValue: "Dao Van Nguyen",
            playerPrefsKey: GameConstain.PlayerPrefsKey.PlayerName
        );

        SettingPersistentValue(
            persistentValue: currentAvatar,
            defaultValue: ItemType.Avatar_1,
            playerPrefsKey: GameConstain.PlayerPrefsKey.CurrentAvatar
        );

        SettingPersistentValue(
            persistentValue: musicVolume,
            defaultValue: true,
            playerPrefsKey: GameConstain.PlayerPrefsKey.MusicVolume
        );

        SettingPersistentValue(
            persistentValue: sfxVolume,
            defaultValue: true,
            playerPrefsKey: GameConstain.PlayerPrefsKey.SfxVolume
        );

        SettingPersistentValue(
            persistentValue: vibrate,
            defaultValue: true,
            playerPrefsKey: GameConstain.PlayerPrefsKey.Vibrate
        );

        SettingPersistentValue(
            persistentValue: heartRecoveryState,
            defaultValue: new HeartRecoveryState()
            {
                remainingHearts = 5,
                lastRecoveryTimestamp = 0,
            },
            playerPrefsKey: GameConstain.PlayerPrefsKey.LimitedLivesData
        );

        SettingPersistentValue(
            persistentValue: unlimitedHeartsState,
            defaultValue: new UnlimitedHeartsState()
            {
                startTimestamp = 0,
                durationSeconds = 0,
            },
            playerPrefsKey: GameConstain.PlayerPrefsKey.UnlimitedLivesData
        );
    }


    private void SettingPersistentValue<T>(DraftUtils.PersistentValue<T> persistentValue, T defaultValue, string playerPrefsKey)
    {
        persistentValue.SetDefaultValue(defaultValue);
        persistentValue.Storage.SetKey(playerPrefsKey);
        persistentValue.Load();
    }

    public void Reward(List<RewardData> rewards)
    {
        foreach (var reward in rewards)
        {
            if (reward.itemType == ItemType.Booter_LifeTime)
            {
                unlimitedHeartsState.Value.AddDuration(reward.amount * 60L, TimeUtils.NowUnixSeconds);
                unlimitedHeartsState.Save();
                unlimitedHeartsState.Notify();
                continue;
            }

            if (remainningItems.TryGetValue(reward.itemType, out var item))
            {
                item.SetValue(item.Value + reward.amount);
                item.Save();
                item.Notify();
            }
        }
    }

    public void Using(ItemType type, int amount) 
    {
        if (remainningItems.TryGetValue(type, out var item)) 
        {
            item.SetValue(item.Value + amount);
            item.Save();
            item.Notify();
        }
    }
    public bool IsRemaning(ItemType type)
    {
        if (remainningItems.TryGetValue(type, out var item))
        {
            return item.Value > 0;
        }
        return false;
    }

    public void UpHeartRecoveryState(int bonus)
    {
        heartRecoveryState.Value.remainingHearts += bonus;
        heartRecoveryState.Save();
        heartRecoveryState.Notify();
    }
}
