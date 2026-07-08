using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Popup Daily Challenge: lịch tháng + milestone streak + nút Play.
/// </summary>
public class PopupDailyChallenge : DraftUtils.DraftMonoBehaviour
{
    [Header("Popup")]
    [SerializeField] public DraftUtils.Popup popup;
    [SerializeField] private DraftUtils.RebuildLayouts rebuilder;
    [SerializeField] private DraftUtils.OptionalTMPTextGroup totalPlayDaysText = new();
    [SerializeField] private Slider progressSlider;
    [SerializeField] private DraftUtils.OptionalTMPTextGroup monthYearText = new();
    [SerializeField] private List<DailyMilestoneView> milestones = new();

    [Header("Calendar")]
    [SerializeField] private DraftUtils.Pooler<DailyCalendarDayView> dayPooler = new();

    [Header("Play Button")]
    [SerializeField] private DraftUtils.OptionalButtonGroup playButton = new();
    [SerializeField] private DraftUtils.OptionalButtonGroup playAdsButton = new();
    [SerializeField] private DraftUtils.OptionalButtonGroup payToPlayButton = new();

    private DailyChallengeManager _manager => DataManager.Instance.dailyChallengeManager;
    private DateTime _selectedDate;

    private void Start()
    {
        popup.closeButton.OnClickAction = popup.HideWithAnimation;

        playButton.RegisterClickEvents();
        playButton.OnClickAction = OnClickPlay;

        playAdsButton.RegisterClickEvents();
        playAdsButton.OnClickAction = OnClickPlayAds;

        payToPlayButton.RegisterClickEvents();
        payToPlayButton.OnClickAction = OnClickPayToPlay;
        _selectedDate = DateTime.Now;
        SetData();
    }
    public void SetData()
    {
        int playedDaysCount = DateCollectionsExtensions.GetDaysCountInCurrentMonth(_manager.PlayedDates.Value);
        SetTotalPlayDaysText(playedDaysCount);
        SetProgressSlider(playedDaysCount);



        SetMonthYearText();

        SetCalendar();
        SetMilestones();
        SetPlayButton();
        SetPlayAdsButton();
        SetPayToPlayButton();
    }
    private void SetProgressSlider(int playedDaysCount)
    {
        progressSlider.value = playedDaysCount / 25f;
    }
    private void SetTotalPlayDaysText(int playedDaysCount)
    {
        totalPlayDaysText.SetText(playedDaysCount.ToString());
    }
    private void SetMilestones()
    {
        DraftUtils.Utils.Common.SetItems(
            milestones,
            DataManager.Instance.milestonesDaily,
            (item, data) => item.SetData(data),
            item => item.gameObject.SetActive(false)
        );
    }
    private void SetMonthYearText()
    {
        var today = DateTime.Today;
        monthYearText.SetText(today.ToString("MMMM yyyy"));
    }
    private void SetCalendar()
    {
        dayPooler.Factory = new DraftUtils.ComponentInstantiatePoolFactory<DailyCalendarDayView>();
        dayPooler.DespawnAll();

        // Tính ngày đầu tiên của tháng và offset theo thứ trong tuần (Monday first)
        var firstDayOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        int startOffset = ((int)firstDayOfMonth.DayOfWeek + 6) % 7; // Monday=0

        int daysInMonth = DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month);
        int totalCells = startOffset + daysInMonth;
        // Round up to complete weeks
        totalCells = Mathf.CeilToInt(totalCells / 7f) * 7;

        for (int i = 0; i < totalCells; i++)
        {
            var date = firstDayOfMonth.AddDays(i - startOffset);
            var dayView = dayPooler.Spawn();
            dayView.SetData(date);
            dayView.Select(_selectedDate);
            dayView.Button.OnClickAction = () => OnClickDay(dayView);
        }
    }

    private void SetPlayButton()
    {
        playButton.SetActive(false);
        if (!DraftUtils.Utils.DateTimeUtils.IsSameDate(_selectedDate, DateTime.Today))
        {
            return;
        }
        if (DataManager.Instance.dailyChallengeManager.PlayedDates.Value.HasToday())
        {
            return;
        }
        playButton.SetActive(true);
    }
    private void SetPlayAdsButton()
    {
        playAdsButton.SetActive(false);
        if (DraftUtils.Utils.DateTimeUtils.IsSameDate(_selectedDate, DateTime.Today))
        {
            return;
        }
        if (DataManager.Instance.dailyChallengeManager.PlayedDates.Value.HasDate(_selectedDate))
        {
            return;
        }
        playAdsButton.SetActive(true);
    }
    private void SetPayToPlayButton()
    {
        payToPlayButton.SetActive(false);
        if (DraftUtils.Utils.DateTimeUtils.IsSameDate(_selectedDate, DateTime.Today))
        {
            return;
        }
        if (DataManager.Instance.dailyChallengeManager.PlayedDates.Value.HasDate(_selectedDate))
        {
            return;
        }
        payToPlayButton.SetActive(true);
    }

    private void OnClickDay(DailyCalendarDayView dayView)
    {
        _selectedDate = dayView.Data;
        
        foreach(var view in dayPooler.ActiveItems)
        {
            view.Select(_selectedDate);
        }

        SetPlayButton();
        SetPlayAdsButton();
        SetPayToPlayButton();
    }

    private void OnClickPlay()
    {

    }

    private void OnClickPlayAds()
    {

    }

    private void OnClickPayToPlay()
    {

    }
}
