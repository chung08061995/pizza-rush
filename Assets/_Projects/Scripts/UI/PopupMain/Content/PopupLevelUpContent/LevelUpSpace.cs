using UnityEngine;

public class LevelUpSpace : EnhancedUI.EnhancedScroller.EnhancedScrollerCellView, ICellView2<LevelUpCellData>
{
    [SerializeField] private DraftUtils.OptionalGameObjectGroup passedObject = new();
    [SerializeField] private DraftUtils.OptionalGameObjectGroup notReachedObject = new();

    private int _currentLevel;
    private LevelUpRewardSO _nextLevelUpSO;

    public void SetData(LevelUpCellData data, int index)
    {
        SetData(data.currentLevel, data.rewardSO);
    }

    public void SetData(int currentLevel, LevelUpRewardSO nextLevelUpSO)
    {
        _currentLevel = currentLevel;
        _nextLevelUpSO = nextLevelUpSO;

        if (_nextLevelUpSO == null) return;

        SetPassedObject();
        SetNotReachedObject();
    }

    private void SetPassedObject()
    {
        passedObject.SetActive(_currentLevel >= _nextLevelUpSO.level);
    }

    private void SetNotReachedObject()
    {
        notReachedObject.SetActive(_currentLevel < _nextLevelUpSO.level);
    }
}
