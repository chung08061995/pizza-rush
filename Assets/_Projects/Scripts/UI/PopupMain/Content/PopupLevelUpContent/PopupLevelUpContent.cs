using System.Collections.Generic;
using DraftUtils;
using UnityEngine;

public class PopupLevelUpContent : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] private EnhancedScroller2<LevelUpCellData> levelUpScroller = new();
    [SerializeField] private DraftUtils.CellInfo levelUpItemCellInfo;
    [SerializeField] private DraftUtils.CellInfo levelUpSpaceCellInfo;
    [SerializeField] private LockContentComponent lockContent;

    private int unlockAtLevel = 2;
    public void SetData()
    {
        lockContent.SetData(unlockAtLevel);

        levelUpScroller.GetCellInfoFunc = GetCellInfo;
        levelUpScroller.Initialize();

        var rewardList = DataManager.Instance.levelUpData.levelUpRewardSOs;
        int currentLevel = DataManager.Instance.Level.Value;

        var cellDataList = new List<LevelUpCellData>();

        for (int i = rewardList.Count -1; i > -1; i--)
        {
            var rewardData = rewardList[i];

            cellDataList.Add(new LevelUpCellData
            {
                cellType = LevelUpCellType.LevelUpSpace,
                currentLevel = currentLevel,
                rewardSO = rewardData,
            });

            bool isLast = (i < rewardList.Count - 1)
                ? (rewardData.level <= currentLevel && currentLevel < rewardList[i + 1].level)
                : (rewardData.level <= currentLevel);

            cellDataList.Add(new LevelUpCellData
            {
                cellType = LevelUpCellType.LevelUpItem,
                currentLevel = currentLevel,
                isLast = isLast,
                rewardSO = rewardData,
            });
        }

        levelUpScroller.UpdateData(cellDataList);

        int targetIndex = FindCurrentLevelIndex(cellDataList, currentLevel);
        if (targetIndex >= 0)
        {
            levelUpScroller.JumpToIndex(targetIndex);
        }
        else
        {
            levelUpScroller.JumpToBottom();
        }
    }

    private int FindCurrentLevelIndex(List<LevelUpCellData> cellDataList, int currentLevel)
    {
        for (int i = 0; i < cellDataList.Count; i++)
        {
            var cellData = cellDataList[i];
            if (cellData?.rewardSO == null || cellData.cellType != LevelUpCellType.LevelUpItem)
            {
                continue;
            }

            if (cellData.rewardSO.level <= currentLevel)
            {
                return i;
            }
        }

        return -1;
    }

    private CellInfo GetCellInfo(LevelUpCellData data, int index)
    {
        return data.cellType switch
        {
            LevelUpCellType.LevelUpItem => levelUpItemCellInfo,
            LevelUpCellType.LevelUpSpace => levelUpSpaceCellInfo,
            _ => levelUpSpaceCellInfo,
        };
    }
}
