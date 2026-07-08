using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class MultipleIAPData
{
    public ItemType itemType;
    public string productId;
    public string name;

    public List<ItemType> features = new();
    public List<RewardData> economyRewards = new();
    public List<RewardData> skillRewards = new();
    public RewardData gold;
}
public static class MultipleIAPDataExtensions
{
    public static List<RewardData> GetRewards(MultipleIAPData data)
    {
        List<RewardData> rewards = new();
        if (data.gold != null && data.gold.amount > 0)
        {
            rewards.Add(data.gold);
        }
        rewards.AddRange(data.economyRewards);
        rewards.AddRange(data.skillRewards);
        return rewards;
    }
}