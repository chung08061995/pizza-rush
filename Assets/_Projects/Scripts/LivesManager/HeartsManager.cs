using Sirenix.OdinInspector;
using System;
using UnityEngine;

/// <summary>
/// Quản lý chính toàn bộ hệ thống Tim (bao gồm Tim giới hạn hồi phục theo thời gian và Tim vô hạn).
/// Class này hoạt động như một Singleton xuyên suốt các Scene.
/// </summary>
public class HeartsManager : DraftUtils.SingletonDontDestroyOnLoadMonoBehaviour<HeartsManager>
{
    [Header("Controllers")]
    [SerializeField] private UnlimitedHeartsController unlimitedHeartsController;
    [SerializeField] private HeartRecoveryController heartRecoveryController;

    /// <summary>
    /// Bộ đếm thời gian vô hạn tim.
    /// </summary>
    public UnlimitedHeartsController UnlimitedHeartsController => unlimitedHeartsController;

    /// <summary>
    /// Bộ đếm thời gian hồi phục tim giới hạn.
    /// </summary>
    public HeartRecoveryController HeartRecoveryController => heartRecoveryController;

    /// <summary>
    /// Kiểm tra xem chế độ vô hạn tim có đang hoạt động hay không.
    /// </summary>
    public bool IsUnlimitedActive()
    {
        return DataManager.Instance.unlimitedHeartsState.Value.IsActive(TimeUtils.NowUnixSeconds);
    }

    /// <summary>
    /// Tiêu thụ 1 tim của người chơi (nếu đang không ở chế độ vô hạn).
    /// </summary>
    [Button("Sử dụng 1 Tim")]
    public void UseHeart()
    {
        // Nếu đang vô hạn tim thì không cần tiêu tốn tim.
        if (IsUnlimitedActive())
        {
            return;
        }

        HeartRecoveryState recoveryState = DataManager.Instance.heartRecoveryState.Value;
        if (recoveryState.CanConsume())
        {
            recoveryState.Consume(TimeUtils.NowUnixSeconds);
            DataManager.Instance.heartRecoveryState.Save();
        }
    }

    public bool IsRemainingHeart()
    {
        if (IsUnlimitedActive())
        {
            return true;
        }

        HeartRecoveryState recoveryState = DataManager.Instance.heartRecoveryState.Value;
        return recoveryState.CanConsume();
    }

    /// <summary>
    /// Kích hoạt hoặc cộng thêm thời gian chơi vô hạn tim.
    /// </summary>
    /// <param name="durationSeconds">Thời gian vô hạn được cộng thêm (tính bằng giây).</param>
    [Button("Cộng thời gian vô hạn")]
    public void AddUnlimitedDuration(long durationSeconds)
    {
        DataManager.Instance.unlimitedHeartsState.Value.AddDuration(durationSeconds, TimeUtils.NowUnixSeconds);
        DataManager.Instance.unlimitedHeartsState.Save();
    }
}
