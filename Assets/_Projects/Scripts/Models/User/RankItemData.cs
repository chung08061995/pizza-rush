using System.Collections.Generic;

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
    public static List<RankItemData> GenerateFakeData(int count)
    {
        List<RankItemData> list = new List<RankItemData>();
        int random = UnityEngine.Random.Range(0, 1000);
        int currentScore = 5000;
        for (int i = 1; i <= count; i++)
        {
            list.Add(new RankItemData()
            {
                rank = i,
                name = $"Player {random} {i}",
                avatarType = DraftUtils.Utils.ListUtils.GetRandomElement(DataManager.Instance.avatarTypes),
                score = currentScore
            });
            currentScore -= UnityEngine.Random.Range(10, 50);
        }
        return list;
    }
    public static RankItemData GetFakeMineData()
    {
        return new RankItemData()
        {
            rank = 10001,
            name = DataManager.Instance.playerName.Value,
            avatarType = DataManager.Instance.currentAvatar.Value,
            score = 1234
        };
    }
}