using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quản lý trạng thái Daily Challenge: streak, ngày chơi cuối, đã chơi hôm nay chưa.
/// </summary>
/// 
[System.Serializable]
public class DateCollections
{
    [ShowInInspector][ReadOnly] private List<string> _days = new();
    public List<string> Days => _days;


    public bool HasToday()
    {
        return HasDate(DateTime.Today);
    }
    public bool HasDate(DateTime date)
    {
        var dateStr = DraftUtils.Utils.DateTimeUtils.DateToString(date);
        return _days.Contains(dateStr);
    }
    public bool AddDate(DateTime date)
    {
        var dateStr = DraftUtils.Utils.DateTimeUtils.DateToString(date);
        return DraftUtils.Utils.ListUtils.AddIfNotExists(_days, dateStr);
    }
    public bool AddToday()
    {
        return AddDate(DateTime.Today);
    }
}

public class DateCollectionsExtensions
{

    /// <summary>Lấy số ngày đã chơi trong tháng hiện tại.</summary>
    public static int GetDaysCountInMonth(DateCollections data, int year, int month)
    {
        int count = 0;
        foreach (var dateStr in data.Days)
        {
            if (DraftUtils.Utils.DateTimeUtils.StringToDate(dateStr, out var date))
            {
                if (date.Year == year && date.Month == month)
                {
                    count++;
                }
            }
        }
        return count;
    }

    public static int GetDaysCountInCurrentMonth(DateCollections data)
    {
        return GetDaysCountInMonth(data, DateTime.Today.Year, DateTime.Today.Month);
    }
}
public class DailyChallengeManager
{
    [ShowInInspector][ReadOnly] private DraftUtils.PersistentValue<DateCollections> _playedDates = new();
    public DraftUtils.PersistentValue<DateCollections> PlayedDates => _playedDates;


    public void Initialize()
    {
        _playedDates.SetDefaultValue(new());
        _playedDates.Storage.SetKey(GameConstain.PlayerPrefsKey.DailyChallenge_StreakDays);
        _playedDates.Load();
    }


}
