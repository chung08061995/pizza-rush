using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = GameConstain.ScriptableObjectsPath.LevelUpReward + nameof(LevelUpRewardSO))]
public class LevelUpRewardSO : ScriptableObject
{
    public int level;
    public List<RewardData> rewardDatas = new();
}
