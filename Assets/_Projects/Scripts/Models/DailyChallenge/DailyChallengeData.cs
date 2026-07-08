using System;
using System.Collections.Generic;

/// <summary>
/// Dữ liệu 1 mốc milestone trong Daily Challenge (3/7/14/25 ngày).
/// </summary>
[Serializable]
public class DailyChallengeMilestone
{
    /// <summary>Số ngày cần streak để nhận reward.</summary>
    public int requiredDays;
    /// <summary>Reward tại mốc này.</summary>
    public RewardData reward;
}

