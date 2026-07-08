using System;
using UnityEngine;

/// <summary>
/// Trạng thái vô hạn tim (chơi không giới hạn trong một khoảng thời gian).
/// Lớp này chứa dữ liệu và logic thuần C#, không phụ thuộc vào hệ thống lưu trữ bên ngoài.
/// </summary>
[System.Serializable]
public class UnlimitedHeartsState
{
    /// <summary>
    /// Mốc thời gian Unix (giây) bắt đầu kích hoạt chế độ vô hạn tim.
    /// </summary>
    public long startTimestamp;

    /// <summary>
    /// Tổng thời lượng vô hạn tim được hưởng (tính bằng giây).
    /// </summary>
    public long durationSeconds;

    /// <summary>
    /// Kiểm tra xem chế độ vô hạn tim có đang hoạt động hay không.
    /// </summary>
    /// <param name="currentTimestamp">Thời gian hiện tại (Unix timestamp bằng giây).</param>
    /// <returns>True nếu thời gian hiện tại vẫn nằm trong khoảng thời gian vô hạn.</returns>
    public bool IsActive(long currentTimestamp)
    {
        return currentTimestamp < startTimestamp + durationSeconds;
    }

    /// <summary>
    /// Lấy số giây còn lại của chế độ vô hạn tim.
    /// </summary>
    /// <param name="currentTimestamp">Thời gian hiện tại (Unix timestamp bằng giây).</param>
    /// <returns>Số giây còn lại (tối thiểu là 0).</returns>
    public float GetSecondsRemaining(long currentTimestamp)
    {
        long elapsed = currentTimestamp - startTimestamp;
        return Mathf.Max(0f, durationSeconds - elapsed);
    }

    /// <summary>
    /// Cộng thêm thời gian vô hạn tim (ví dụ khi nhận thưởng hoặc mua gói).
    /// </summary>
    /// <param name="duration">Thời lượng muốn cộng thêm (tính bằng giây).</param>
    /// <param name="currentTimestamp">Thời gian hiện tại (Unix timestamp bằng giây).</param>
    public void AddDuration(long duration, long currentTimestamp)
    {
        if (IsActive(currentTimestamp))
        {
            // Nếu đang trong thời gian vô hạn mạng, cộng dồn thêm thời lượng.
            durationSeconds += duration;
        }
        else
        {
            // Nếu đã hết hạn hoặc chưa từng có, bắt đầu đợt vô hạn mạng mới từ thời điểm hiện tại.
            startTimestamp = currentTimestamp;
            durationSeconds = duration;
        }
    }
}
