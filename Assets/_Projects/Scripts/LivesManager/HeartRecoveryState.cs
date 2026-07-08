using System;
using UnityEngine;

/// <summary>
/// Trạng thái hồi phục tim (mạng chơi giới hạn).
/// Lớp này chứa dữ liệu và logic thuần C#, không phụ thuộc vào hệ thống lưu trữ bên ngoài,
/// giúp dễ dàng mang sang các dự án khác.
/// </summary>
[System.Serializable]
public class HeartRecoveryState
{
    /// <summary>
    /// Số tim hiện tại đang có.
    /// </summary>
    public int remainingHearts;

    /// <summary>
    /// Mốc thời gian Unix (giây) của lần sử dụng tim/hồi phục tim gần nhất.
    /// </summary>
    public long lastRecoveryTimestamp;

    /// <summary>
    /// Số tim tối đa mà người chơi có thể tích lũy tự nhiên.
    /// </summary>
    public const int MaxHearts = 5;

    /// <summary>
    /// Thời gian cần thiết để hồi phục 1 tim (tính bằng giây). 10 phút = 600 giây.
    /// </summary>
    public const float RecoveryDurationSeconds = 10 * 60;

    /// <summary>
    /// Kiểm tra xem người chơi có thể sử dụng tim được không (còn tim hay không).
    /// </summary>
    /// <returns>True nếu còn ít nhất 1 tim.</returns>
    public bool CanConsume()
    {
        return remainingHearts > 0;
    }

    /// <summary>
    /// Thực hiện tiêu thụ 1 tim.
    /// </summary>
    /// <param name="currentTimestamp">Thời gian hiện tại (Unix timestamp bằng giây).</param>
    /// <returns>True nếu tiêu thụ thành công, False nếu không còn tim.</returns>
    public bool Consume(long currentTimestamp)
    {
        if (!CanConsume())
        {
            return false;
        }

        bool wasMaxHearts = IsMaxHearts();
        remainingHearts--;

        // Nếu trước đó đang đầy tim, việc tiêu thụ tim sẽ bắt đầu tính thời gian hồi phục từ bây giờ.
        if (wasMaxHearts)
        {
            lastRecoveryTimestamp = currentTimestamp;
        }

        return true;
    }

    /// <summary>
    /// Kiểm tra xem tim đã đầy hay chưa.
    /// </summary>
    public bool IsMaxHearts()
    {
        return remainingHearts >= MaxHearts;
    }

    /// <summary>
    /// Lấy số giây còn lại cho đến lần hồi phục tim tiếp theo.
    /// </summary>
    /// <param name="currentTimestamp">Thời gian hiện tại (Unix timestamp bằng giây).</param>
    /// <returns>Số giây còn lại (float). Trả về giá trị âm hoặc 0 nếu đã sẵn sàng hồi phục.</returns>
    public float GetSecondsRemaining(long currentTimestamp)
    {
        long elapsed = currentTimestamp - lastRecoveryTimestamp;
        return RecoveryDurationSeconds - elapsed;
    }

    /// <summary>
    /// Cập nhật logic hồi phục tim theo thời gian.
    /// Hàm này nên được gọi trong vòng lặp Update hoặc khi người chơi mở lại game.
    /// </summary>
    /// <param name="currentTimestamp">Thời gian hiện tại (Unix timestamp bằng giây).</param>
    /// <returns>True nếu trạng thái tim có thay đổi (hồi phục thêm tim), False nếu không có thay đổi.</returns>
    public bool UpdateRecovery(long currentTimestamp)
    {
        // Nếu đã đầy tim thì không cần hồi phục.
        if (IsMaxHearts())
        {
            return false;
        }

        bool stateChanged = false;
        
        // Vòng lặp hồi phục từng tim nếu thời gian trôi qua đủ lớn.
        while (!IsMaxHearts() && GetSecondsRemaining(currentTimestamp) <= 0)
        {
            remainingHearts++;
            lastRecoveryTimestamp += (long)RecoveryDurationSeconds;
            stateChanged = true;
        }

        return stateChanged;
    }
}
