using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupRankingContent : DraftUtils.DraftMonoBehaviour
{
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
        Debug.Log("Click Weekly Button");
        SetData();
    }

    private void ClickWorldButton()
    {
        tabButtons.Select(worldButton);
        Debug.Log("Click World Button");
        SetData();
    }

    private void ClickYourCountryButton()
    {
        tabButtons.Select(yourCountryButton);
        Debug.Log("Click Your Country Button");
        SetData();
    }

    public void SetData()
    {
        lockContent.SetData(unlockAtLevel);

        rankScroller.Initialize();
        var data = RankItemDataExtensions.GenerateFakeData(1000);
        rankScroller.UpdateData(data);
        rankScroller.JumpToTop();

        mineItem.SetData(RankItemDataExtensions.GetFakeMineData());

        SetTop1Item(data);
        SetTop2Item(data);
        SetTop3Item(data);

        var popupMain = GetComponentInParent<PopupMain>();
        if (popupMain != null)
        {
            popupMain.ApplyCleanTextRendering(this);
        }
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
