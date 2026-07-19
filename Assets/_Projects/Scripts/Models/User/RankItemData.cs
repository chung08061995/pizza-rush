using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RankItemData
{
    public int rank;
    public string name;
    public ItemType avatarType;
    public int score;
}

public static class RankItemDataExtensions
{
    public static List<RankItemData> GenerateFakeLeaderboard(
        int count,
        int topScore,
        out RankItemData mineData)
    {
        var list = new List<RankItemData>(count + 1);
        int nameSeed = Random.Range(100, 1000);
        int currentScore = topScore;

        for (int i = 0; i < count; i++)
        {
            list.Add(new RankItemData
            {
                name = $"Player {nameSeed} {i + 1}",
                avatarType = DraftUtils.Utils.ListUtils.GetRandomElement(DataManager.Instance.avatarTypes),
                score = Mathf.Max(0, currentScore)
            });

            currentScore -= Random.Range(35, 90);
        }

        int playerScore = 4500 + DataManager.Instance.Level.Value * 200;
        mineData = GetFakeMineData(playerScore);
        list.Add(mineData);
        list.Sort((left, right) => right.score.CompareTo(left.score));

        for (int i = 0; i < list.Count; i++)
        {
            list[i].rank = i + 1;
        }

        return list;
    }

    private static RankItemData GetFakeMineData(int score)
    {
        return new RankItemData
        {
            name = DataManager.Instance.playerName.Value,
            avatarType = DataManager.Instance.currentAvatar.Value,
            score = score
        };
    }
}
