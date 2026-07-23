using System.Collections.Generic;
using UnityEngine;


public class IAPData
{
    public List<SingleIAPData> singleIaps = new()
    {
        new SingleIAPData
        {
            itemType = ItemType.SingleIAPData_1_000_Coin,
            productId = "",
            reward = new RewardData
            {
                itemType = ItemType.Gold,
                amount = 1_000,
            }
        },
        new SingleIAPData
        {
            itemType = ItemType.SingleIAPData_5_000_Coin,
            productId = "",
            reward = new RewardData
            {
                itemType = ItemType.Gold,
                amount = 5_000,
            }
        },
        new SingleIAPData
        {
            itemType = ItemType.SingleIAPData_10_000_Coin,
            productId = "",
            reward = new RewardData
            {
                itemType = ItemType.Gold,
                amount = 10_000,
            }
        },
        new SingleIAPData
        {
            itemType = ItemType.SingleIAPData_25_000_Coin,
            productId = "",
            reward = new RewardData
            {
                itemType = ItemType.Gold,
                amount = 25_000,
            }
        },
        new SingleIAPData
        {
            itemType = ItemType.SingleIAPData_50_000_Coin,
            productId = "",
            reward = new RewardData
            {
                itemType = ItemType.Gold,
                amount = 50_000,
            }
        },
        new SingleIAPData
        {
            itemType = ItemType.SingleIAPData_100_000_Coin,
            productId = "",
            reward = new RewardData
            {
                itemType = ItemType.Gold,
                amount = 100_000,
            }
        }
    };


    public MultipleIAPData noAds = new()
    {
        itemType = ItemType.MultipleIAPData_NoAds,
        features = new()
        {
            ItemType.Iap_Feature_RemoveAds,
            ItemType.Iap_Feature_RemoveBottomBannerAds,
            ItemType.Iap_Feature_KeepRewardAdsForAds
        },
        economyRewards = new(),
        skillRewards = new(),
    };

    public MultipleIAPData noAdsBundle = new()
    {

        itemType = ItemType.MultipleIAPData_NoAdsBundle,
        features = new()
        {
            ItemType.Iap_Feature_RemoveAds,
            ItemType.Iap_Feature_RemoveBottomBannerAds,
            ItemType.Iap_Feature_KeepRewardAdsForAds
        },
        economyRewards = new(),
        skillRewards = new()
        {
            new()
            {
                itemType = ItemType.Skill_FreezeTime,
                amount = 2,
            },
            new()
            {
                itemType = ItemType.Skill_DestroyContainer,
                amount = 2,
            },
            new()
            {
                itemType = ItemType.Skill_SplitContainer,
                amount = 2,
            },
            new()
            {
                itemType = ItemType.Skill_AddTile,
                amount = 2,
            },
        },
        gold = new()
        {
            itemType = ItemType.Gold,
            amount = 2000,
        }
    };
    public MultipleIAPData smallBundle = new()
    {

        itemType = ItemType.MultipleIAPData_SmallBundle,
        features = new(),
        economyRewards = new()
        {
            new()
            {
                itemType = ItemType. Booter_Magic,
                amount = 1,
            },
            new()
            {
                itemType = ItemType.Booter_CoffeeTime,
                amount = 1,
            },
            new()
            {
                itemType = ItemType.Booter_LifeTime,
                amount = 60,
            },
        },
        skillRewards = new()
        {
            new()
            {
                itemType = ItemType.Skill_FreezeTime,
                amount = 1,
            },
            new()
            {
                itemType = ItemType.Skill_DestroyContainer,
                amount = 1,
            },
            new()
            {
                itemType = ItemType.Skill_SplitContainer,
                amount = 1,
            },
            new()
            {
                itemType = ItemType.Skill_AddTile,
                amount = 1,
            },
        },
        gold = new()
        {
            itemType = ItemType.Gold,
            amount = 4000,
        }
    };
    public MultipleIAPData mediumBundle = new()
    {

        itemType = ItemType.MultipleIAPData_MediumBundle,
        features = new(),
        economyRewards = new()
        {
            new()
            {
                itemType = ItemType. Booter_Magic,
                amount = 2,
            },
            new()
            {
                itemType = ItemType.Booter_CoffeeTime,
                amount = 2,
            },
            new()
            {
                itemType = ItemType.Booter_LifeTime,
                amount = 180,
            },
        },
        skillRewards = new()
        {
            new()
            {
                itemType = ItemType.Skill_FreezeTime,
                amount = 2,
            },
            new()
            {
                itemType = ItemType.Skill_DestroyContainer,
                amount = 2,
            },
            new()
            {
                itemType = ItemType.Skill_SplitContainer,
                amount = 2,
            },
            new()
            {
                itemType = ItemType.Skill_AddTile,
                amount = 2,
            },
        },
        gold = new()
        {
            itemType = ItemType.Gold,
            amount = 9000,
        }
    };
    public MultipleIAPData largeBundle = new()
    {

        itemType = ItemType.MultipleIAPData_LargeBundle,
        features = new(),
        economyRewards = new()
        {
            new()
            {
                itemType = ItemType. Booter_Magic,
                amount = 4,
            },
            new()
            {
                itemType = ItemType.Booter_CoffeeTime,
                amount = 4,
            },
            new()
            {
                itemType = ItemType.Booter_LifeTime,
                amount = 360,
            },
        },
        skillRewards = new()
        {
            new()
            {
                itemType = ItemType.Skill_FreezeTime,
                amount = 4,
            },
            new()
            {
                itemType = ItemType.Skill_DestroyContainer,
                amount = 4,
            },
            new()
            {
                itemType = ItemType.Skill_SplitContainer,
                amount = 4,
            },
            new()
            {
                itemType = ItemType.Skill_AddTile,
                amount = 4,
            },
        },
        gold = new()
        {
            itemType = ItemType.Gold,
            amount = 12000,
        }
    };
    public MultipleIAPData starter = new()
    {

        itemType = ItemType.MultipleIAPData_Starter,
        features = new(),
        economyRewards = new()
        {
            new()
            {
                itemType = ItemType. Booter_Magic,
                amount = 1,
            },
            new()
            {
                itemType = ItemType.Booter_CoffeeTime,
                amount = 1,
            },

            new()
            {
                itemType = ItemType.Gold,
                amount = 1500,
            },
            new()
            {
                itemType = ItemType.Booter_LifeTime,
                amount = 60,
            },
        },
        skillRewards = new()
        {
            new()
            {
                itemType = ItemType.Skill_FreezeTime,
                amount = 1,
            },
            new()
            {
                itemType = ItemType.Skill_DestroyContainer,
                amount = 1,
            },
            new()
            {
                itemType = ItemType.Skill_SplitContainer,
                amount = 1,
            },
            new()
            {
                itemType = ItemType.Skill_AddTile,
                amount = 1,
            },
        },
        gold = new()
        {
            itemType = ItemType.Gold,
            amount = 12000,
        }
    };
    public Dictionary<ItemType, string> productIds = new()
    {
        { ItemType.SingleIAPData_1_000_Coin, "test.1000gold" },
        { ItemType.SingleIAPData_5_000_Coin, "test.5000gold" },
        { ItemType.SingleIAPData_10_000_Coin, "test.10000gold" },
        { ItemType.SingleIAPData_25_000_Coin, "test.25000gold" },
        { ItemType.SingleIAPData_50_000_Coin, "test.50000gold" },
        { ItemType.SingleIAPData_100_000_Coin, "test.100000gold" },

        { ItemType.MultipleIAPData_NoAds, GameConstain.IAPProductId.NoAds },
        { ItemType.MultipleIAPData_NoAdsBundle, "test.noadsbundle" },
        { ItemType.MultipleIAPData_SmallBundle, "test.smallbundle" },
        { ItemType.MultipleIAPData_MediumBundle, "test.mediumbundle" },
        { ItemType.MultipleIAPData_LargeBundle, "test.largebundle" },
        { ItemType.MultipleIAPData_Starter, "test.starter" },
    };

    public IAPData()
    {
        foreach (var iap in singleIaps)
        {
            if (productIds.TryGetValue(iap.itemType, out var id))
            {
                iap.productId = id;
            }
        }

        if (productIds.TryGetValue(noAds.itemType, out var noAdsId)) noAds.productId = noAdsId;
        if (productIds.TryGetValue(noAdsBundle.itemType, out var noAdsBundleId)) noAdsBundle.productId = noAdsBundleId;
        if (productIds.TryGetValue(smallBundle.itemType, out var smallBundleId)) smallBundle.productId = smallBundleId;
        if (productIds.TryGetValue(mediumBundle.itemType, out var mediumBundleId)) mediumBundle.productId = mediumBundleId;
        if (productIds.TryGetValue(largeBundle.itemType, out var largeBundleId)) largeBundle.productId = largeBundleId;
        if (productIds.TryGetValue(starter.itemType, out var starterId)) starter.productId = starterId;
    }

    public int GetCost(string productId)
    {
        return 51_000;
    }
}
