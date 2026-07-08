using System;
using UnityEngine;

/// <summary>
/// Bộ điều khiển cập nhật tiến trình hồi phục tim giới hạn mỗi khung hình.
/// </summary>
public class HeartRecoveryController : DraftUtils.DraftMonoBehaviour
{
    private DraftUtils.PersistentValue<HeartRecoveryState> _data => DataManager.Instance.heartRecoveryState;

    /// <summary>
    /// Sự kiện được kích hoạt mỗi khi bộ đếm được cập nhật.
    /// UI View sẽ đăng ký sự kiện này để làm mới giao diện.
    /// </summary>
    public Action OnUpdateAction { get; set; }

    private void Update()
    {
        // Kích hoạt sự kiện cập nhật để UI vẽ lại đếm ngược.
        OnUpdateAction?.Invoke();

        // Cập nhật trạng thái hồi phục tim.
        bool hasRecovered = _data.Value.UpdateRecovery(TimeUtils.NowUnixSeconds);
        if (hasRecovered)
        {
            _data.Save();
        }
    }
}
