using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// View cho 1 ô ngày trong lịch Daily Challenge.
/// </summary>
public class DailyCalendarDayView : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] private DraftUtils.OptionalTMPTextGroup dayText = new();
    [SerializeField] private DraftUtils.OptionalButtonGroup button = new();
    [SerializeField] private DraftUtils.OptionalGameObjectGroup playedObject = new();
    [SerializeField] private DraftUtils.OptionalGameObjectGroup outsideCurrentMonthObject = new();
    [SerializeField] private DraftUtils.OptionalGameObjectGroup normalObject = new();
    [SerializeField] private DraftUtils.OptionalGameObjectGroup hightlightObject = new();

    private DateTime _data;
    public DateTime Data => _data;

    public DraftUtils.OptionalButtonGroup Button => button;

    private void Start()
    {
        button.RegisterClickEvents();
    }

    public void SetData(DateTime data)
    {
        _data = data;
        SetDayText();
        SetPlayedObject();
        SetOutsideCurrentMonthObject();
    }
    public void Select(DateTime selectedDate)
    {
        hightlightObject.SetActive(false);
        normalObject.SetActive(false);

        if (_data.Month != DateTime.Today.Month)
        {
            return;
        }
        if (DataManager.Instance.dailyChallengeManager.PlayedDates.Value.HasDate(_data))
        {
            return;
        }
        bool selected = DraftUtils.Utils.DateTimeUtils.IsSameDate(_data, selectedDate);

        hightlightObject.SetActive(selected);
        normalObject.SetActive(!selected);
    }
    private void SetDayText()
    {
        dayText.SetText(_data.Day.ToString());
    }
    private void SetPlayedObject()
    {
        playedObject.SetActive(false);
        if (_data.Month != DateTime.Today.Month)
        {
            return;
        }
        bool active = DataManager.Instance.dailyChallengeManager.PlayedDates.Value.HasDate(_data);
        playedObject.SetActive(active);
    }
    private void SetOutsideCurrentMonthObject()
    {
        outsideCurrentMonthObject.SetActive(false);
        if (_data.Month == DateTime.Today.Month)
        {
            return;
        }
        outsideCurrentMonthObject.SetActive(true);
    }
}
