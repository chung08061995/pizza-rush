using Sirenix.OdinInspector;
using System;
using UnityEngine;

/// <summary>
/// Lớp hiển thị giao diện đếm ngược của thời gian vô hạn tim.
/// </summary>
public class UnlimitedHeartsView : DraftUtils.DraftMonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private DraftUtils.OptionalTMPTextGroup countdownText = new();

    [ShowInInspector][ReadOnly] private UnlimitedHeartsState _data;

    private void Start()
    {
        if (HeartsManager.Instance == null)
        {
            Debug.LogError("[UnlimitedHeartsView] HeartsManager.Instance is null!");
            return;
        }
        if (HeartsManager.Instance.UnlimitedHeartsController == null)
        {
            Debug.LogError("[UnlimitedHeartsView] HeartsManager.Instance.UnlimitedHeartsController is null!");
            return;
        }

        // Đăng ký sự kiện cập nhật giao diện mỗi khi bộ đếm vô hạn tick.
        HeartsManager.Instance.UnlimitedHeartsController.OnUpdateAction += OnUnlimitedUpdate;
    }

    private void OnDestroy()
    {
        if (HeartsManager.Instance != null && HeartsManager.Instance.UnlimitedHeartsController != null)
        {
            HeartsManager.Instance.UnlimitedHeartsController.OnUpdateAction -= OnUnlimitedUpdate;
        }
    }

    private void OnUnlimitedUpdate()
    {
        UpdateCountdownText();
    }

    /// <summary>
    /// Thiết lập dữ liệu vô hạn tim để hiển thị.
    /// </summary>
    /// <param name="data">Trạng thái vô hạn tim cần gán.</param>
    public void SetData(UnlimitedHeartsState data)
    {
        _data = data;
        UpdateCountdownText();
    }

    private void UpdateCountdownText()
    {
        if (_data == null) return;

        if (!_data.IsActive(TimeUtils.NowUnixSeconds))
        {
            countdownText.SetText("00:00");
            return;
        }

        float remainingSeconds = _data.GetSecondsRemaining(TimeUtils.NowUnixSeconds);
        countdownText.SetText(TimeUtils.FormatCountdown(remainingSeconds));
    }
}
