/// <summary>
/// Loại cell trong danh sách Level Up.
/// </summary>
public enum LevelUpCellType
{
    LevelUpItem,
    LevelUpSpace,
}

/// <summary>
/// Dữ liệu chung cho mỗi cell trong danh sách Level Up.
/// </summary>
[System.Serializable]
public class LevelUpCellData
{
    public LevelUpCellType cellType;
    public int currentLevel;
    public bool isLast;
    public LevelUpRewardSO rewardSO;
}
