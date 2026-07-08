using UnityEngine;

/// <summary>
/// View cho 1 mốc milestone trong Daily Challenge (icon reward + amount + số ngày).
/// </summary>
public class DailyMilestoneView : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] private DraftUtils.OptionalValue<RewardDataView> rewardDataView = new();
    [SerializeField] private DraftUtils.OptionalTMPTextGroup daysText = new();
    [SerializeField] private DraftUtils.OptionalGameObjectGroup reachedMark = new();

    private DailyChallengeMilestone _data;

    public void SetData(DailyChallengeMilestone milestone)
    {
        _data = milestone;

        if (rewardDataView.isPresent)
            rewardDataView.value.SetData(milestone.reward);

        daysText.SetText(milestone.requiredDays.ToString());
    }
}
