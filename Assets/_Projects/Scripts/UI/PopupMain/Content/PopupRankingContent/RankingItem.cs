using EnhancedUI.EnhancedScroller;
using Sirenix.OdinInspector;
using UnityEngine;

public class RankingItem : EnhancedScrollerCellView, ICellView1<RankItemData>
{
    [SerializeField] private DraftUtils.OptionalGameObjectGroup top1Icon = new();
    [SerializeField] private DraftUtils.OptionalGameObjectGroup top2Icon = new();
    [SerializeField] private DraftUtils.OptionalGameObjectGroup top3Icon = new();
    [SerializeField] private DraftUtils.OptionalGameObjectGroup outTopObj = new();

    [SerializeField] private DraftUtils.OptionalTMPTextGroup nameText;
    [SerializeField] private DraftUtils.OptionalTMPTextGroup scoreText;
    [SerializeField] private DraftUtils.OptionalTMPTextGroup rankText;
    [SerializeField] private DraftUtils.OptionalValue<ItemView> avatarView;

    [ShowInInspector][ReadOnly] private RankItemData _data;

    public void SetData(RankItemData data, int index)
    {
        SetData(data);
    }
    public void SetData(RankItemData data)
    {
        _data = data;
        SetTop1Icon();
        SetTop2Icon();
        SetTop3Icon();
        SetOutTop();
        SetName();
        SetScore();
        SetRank();
        SetAvatar();
    }
    private void SetAvatar()
    {
        if (!avatarView.isPresent)
        {
            return;
        }
        avatarView.value.SetData(_data.avatarType);
    }
    private void SetTop1Icon()
    {
        top1Icon.SetActive(_data.rank == 1);
    }
    private void SetTop2Icon()
    {
        top2Icon.SetActive(_data.rank == 2);
    }
    private void SetTop3Icon()
    {
        top3Icon.SetActive(_data.rank == 3);
    }
    private void SetOutTop()
    {
        outTopObj.SetActive(_data.rank > 3);
    }

    private void SetName()
    {
        nameText.SetText(_data.name);
    }

    private void SetScore()
    {
        scoreText.SetText(_data.score);
    }

    private void SetRank()
    {
        rankText.SetText(_data.rank > 1000 ? "+1000" : _data.rank);
    }
}
