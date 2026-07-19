using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupRankingContent : DraftUtils.DraftMonoBehaviour
{
    private enum RankingTab
    {
        Weekly,
        World,
        Country
    }

    [SerializeField] private Button backButton;
    [SerializeField] private Button weeklyButton;
    [SerializeField] private Button worldButton;
    [SerializeField] private Button yourCountryButton;
    [SerializeField] private DraftUtils.EnhancedScroller1<RankingItem, RankItemData> rankScroller = new();
    [SerializeField] private RankingItem mineItem;
    [SerializeField] private RankingItem top1Item;
    [SerializeField] private RankingItem top2Item;
    [SerializeField] private RankingItem top3Item;
    [SerializeField] private LockContentComponent lockContent;
    [ShowInInspector][ReadOnly]
    private int unlockAtLevel = 2;

    private SingleSelectableButtonGroup tabButtons = new();
    private readonly Dictionary<RankingTab, List<RankItemData>> rankingData = new();
    private readonly Dictionary<RankingTab, RankItemData> mineData = new();

    private void Start()
    {
        backButton.onClick.AddListener(ReturnHome);
        weeklyButton.onClick.AddListener(ClickWeeklyButton);
        worldButton.onClick.AddListener(ClickWorldButton);
        yourCountryButton.onClick.AddListener(ClickYourCountryButton);

        tabButtons.AddRange(
            weeklyButton,
            worldButton,
            yourCountryButton
        );

        ClickWeeklyButton();
    }

    private void ReturnHome()
    {
        GetComponentInParent<PopupMain>().ShowHome();
    }


    private void ClickWeeklyButton()
    {
        tabButtons.Select(weeklyButton);
        SetData(RankingTab.Weekly);
    }

    private void ClickWorldButton()
    {
        tabButtons.Select(worldButton);
        SetData(RankingTab.World);
    }

    private void ClickYourCountryButton()
    {
        tabButtons.Select(yourCountryButton);
        SetData(RankingTab.Country);
    }

    private void SetData(RankingTab selectedTab)
    {
        lockContent.SetData(unlockAtLevel);

        if (!rankingData.TryGetValue(selectedTab, out var data))
        {
            data = RankItemDataExtensions.GenerateFakeLeaderboard(
                count: 100,
                topScore: GetTopScore(selectedTab),
                out var playerData);
            rankingData.Add(selectedTab, data);
            mineData.Add(selectedTab, playerData);
        }

        rankScroller.Initialize();
        rankScroller.UpdateData(data);
        rankScroller.JumpToTop();

        mineItem.SetData(mineData[selectedTab]);

        SetTop1Item(data);
        SetTop2Item(data);
        SetTop3Item(data);

        var popupMain = GetComponentInParent<PopupMain>();
        if (popupMain != null)
        {
            popupMain.ApplyCleanTextRendering(this);
        }
    }

    private static int GetTopScore(RankingTab selectedTab)
    {
        return selectedTab switch
        {
            RankingTab.Weekly => 9000,
            RankingTab.World => 12000,
            RankingTab.Country => 7500,
            _ => 9000
        };
    }

    private void SetTop1Item(List<RankItemData> data)
    {
        if (top1Item != null)
        {
            bool hasData = data != null && data.Count > 0;
            top1Item.gameObject.SetActive(hasData);
            if (hasData) top1Item.SetData(data[0]);
        }
    }

    private void SetTop2Item(IList<RankItemData> data)
    {
        if (top2Item != null)
        {
            bool hasData = data != null && data.Count > 1;
            top2Item.gameObject.SetActive(hasData);
            if (hasData) top2Item.SetData(data[1]);
        }
    }

    private void SetTop3Item(IList<RankItemData> data)
    {
        if (top3Item != null)
        {
            bool hasData = data != null && data.Count > 2;
            top3Item.gameObject.SetActive(hasData);
            if (hasData) top3Item.SetData(data[2]);
        }
    }
}
