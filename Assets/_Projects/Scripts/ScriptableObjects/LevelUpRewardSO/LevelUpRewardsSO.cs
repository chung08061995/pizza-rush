using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = GameConstain.ScriptableObjectsPath.LevelUpReward + nameof(LevelUpRewardsSO))]
public class LevelUpRewardsSO : ScriptableObject
{
    public List<LevelUpRewardSO> levelUpRewardSOs = new();
}
