using UnityEngine;

public class LevelUpRewardSOView : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] private DraftUtils.OptionalTMPTextGroup levelText = new();
    private string levelFormat = "{0}";
    [SerializeField] private DraftUtils.Pooler<RewardDataView> rewardDataPooler = new();

    private LevelUpRewardSO _data;

    public void SetData(LevelUpRewardSO data)
    {
        _data = data;
        SetLevelText();
        GenerateRewards();
    }

    private void SetLevelText()
    {
        levelText.SetText(string.Format(levelFormat, _data.level));
    }

    private void GenerateRewards()
    {
        rewardDataPooler.Factory = new DraftUtils.ComponentInstantiatePoolFactory<RewardDataView>();
        rewardDataPooler.DespawnAll();

        foreach (var rewardData in _data.rewardDatas)
        {
            var rewardView = rewardDataPooler.Spawn();
            rewardView.SetData(rewardData);
        }
    }
}
