[System.Serializable]
public class RankItemData
{
    public string id;
    public int rank;
    public string name;
    public ItemType avatarType;
    public int score;

    public RankItemData Clone()
    {
        return new RankItemData
        {
            id = id,
            rank = rank,
            name = name,
            avatarType = avatarType,
            score = score
        };
    }
}
