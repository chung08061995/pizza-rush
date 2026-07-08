using System;
using UnityEngine;

/// <summary>
/// Bộ điều khiển cập nhật tiến trình đếm ngược thời gian vô hạn tim mỗi khung hình.
/// </summary>
public class UnlimitedHeartsController : DraftUtils.DraftMonoBehaviour
{
    private DraftUtils.PersistentValue<UnlimitedHeartsState> _data => DataManager.Instance.unlimitedHeartsState;

    /// <summary>
    /// Sự kiện được kích hoạt mỗi khi bộ đếm vô hạn mạng được cập nhật.
    /// UI View sẽ đăng ký sự kiện này để cập nhật thời gian hiển thị.
    /// </summary>
    public Action OnUpdateAction { get; set; }

    private void Update()
    {
        OnUpdateAction?.Invoke();
    }
}
