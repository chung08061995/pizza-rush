using Sirenix.OdinInspector;
using System;
using UnityEngine;

/// <summary>
/// Lớp hiển thị giao diện của hệ thống hồi phục tim giới hạn (số tim hiện tại và thời gian đếm ngược).
/// </summary>
public class HeartRecoveryView : DraftUtils.DraftMonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private DraftUtils.OptionalTMPTextGroup remainingHeartsText = new();
    [SerializeField] private DraftUtils.OptionalTMPTextGroup countdownText = new();
    [SerializeField] private DraftUtils.OptionalButtonGroup addMoreButton = new();
    [SerializeField] private bool showAddMoreButton = true;
    public DraftUtils.OptionalButtonGroup AddMoreButton => addMoreButton;

    [ShowInInspector][ReadOnly] private HeartRecoveryState _data;

    private void Start()
    {
        if (HeartsManager.Instance == null)
        {
            Debug.LogError("[HeartRecoveryView] HeartsManager.Instance is null!");
            return;
        }
        if (HeartsManager.Instance.HeartRecoveryController == null)
        {
            Debug.LogError("[HeartRecoveryView] HeartsManager.Instance.HeartRecoveryController is null!");
            return;
        }
        addMoreButton.RegisterClickEvents();

        // Đăng ký sự kiện cập nhật giao diện mỗi khi bộ đếm hồi phục tick.
        HeartsManager.Instance.HeartRecoveryController.OnUpdateAction += OnRecoveryUpdate;
    }

    private void OnDestroy()
    {
        if (HeartsManager.Instance != null && HeartsManager.Instance.HeartRecoveryController != null)
        {
            HeartsManager.Instance.HeartRecoveryController.OnUpdateAction -= OnRecoveryUpdate;
        }
    }

    private void OnRecoveryUpdate()
    {
        UpdateRemainingHeartsText();
        UpdateCountdownText();
        SetAddMoreButton();
    }
    private void SetAddMoreButton()
    {
        if (_data == null) return;
        addMoreButton.SetActive(showAddMoreButton && !_data.IsMaxHearts());
    }
    /// <summary>
    /// Thiết lập dữ liệu trạng thái tim để hiển thị.
    /// </summary>
    /// <param name="data">Trạng thái hồi phục tim cần gán.</param>
    public void SetData(HeartRecoveryState data)
    {
        _data = data;
        UpdateRemainingHeartsText();
        UpdateCountdownText();
        SetAddMoreButton();
    }

    private void UpdateRemainingHeartsText()
    {
        if (_data == null) return;
        remainingHeartsText.SetText(_data.remainingHearts);
    }

    private void UpdateCountdownText()
    {
        if (_data == null) return;

        if (_data.IsMaxHearts())
        {
            countdownText.SetText("Full");
            return;
        }

        float secondsRemaining = _data.GetSecondsRemaining(TimeUtils.NowUnixSeconds);
        countdownText.SetText(TimeUtils.FormatCountdown(Mathf.Max(0f, secondsRemaining)));
    }
}
