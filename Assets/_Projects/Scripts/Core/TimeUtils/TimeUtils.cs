using System;
using UnityEngine;

/// <summary>
/// Các tiện ích xử lý thời gian trong game.
/// Dùng để chuyển đổi giữa DateTime và Unix timestamp (giây),
/// hỗ trợ tính toán thời gian thực cho các hệ thống như Lives, Daily Reward, v.v.
/// </summary>
public static class TimeUtils
{
    /// <summary>
    /// Mốc thời gian bắt đầu (Unix epoch: 1/1/1970 00:00:00 UTC).
    /// </summary>
    private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Trả về thời điểm hiện tại theo UTC dưới dạng Unix timestamp (số giây kể từ 1/1/1970).
    /// </summary>
    public static long NowUnixSeconds => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    /// <summary>
    /// Chuyển đổi DateTime sang Unix timestamp (số giây kể từ 1/1/1970 UTC).
    /// </summary>
    /// <param name="date">Thời điểm cần chuyển đổi.</param>
    /// <returns>Số giây từ Unix epoch đến thời điểm đó.</returns>
    public static long ToUnixSeconds(DateTime date)
    {
        return (long)(date.ToUniversalTime() - UnixEpoch).TotalSeconds;
    }

    /// <summary>
    /// Chuyển đổi Unix timestamp (giây) sang DateTime (UTC).
    /// </summary>
    /// <param name="unixSeconds">Số giây kể từ Unix epoch.</param>
    /// <returns>Thời điểm tương ứng dưới dạng DateTime UTC.</returns>
    public static DateTime FromUnixSeconds(long unixSeconds)
    {
        return UnixEpoch.AddSeconds(unixSeconds);
    }

    /// <summary>
    /// Tính số giây còn lại từ thời điểm hiện tại đến một thời điểm trong tương lai.
    /// Trả về 0 nếu thời điểm đó đã qua.
    /// </summary>
    /// <param name="futureUnixSeconds">Thời điểm đích (Unix timestamp, giây).</param>
    /// <returns>Số giây còn lại (>= 0).</returns>
    public static float SecondsUntil(long futureUnixSeconds)
    {
        long remaining = futureUnixSeconds - NowUnixSeconds;
        return Mathf.Max(0f, (float)remaining);
    }

    /// <summary>
    /// Kiểm tra xem thời điểm chỉ định (Unix timestamp) đã đến hay chưa.
    /// </summary>
    /// <param name="unixSeconds">Thời điểm cần kiểm tra.</param>
    /// <returns>true nếu đã đến hoặc đã qua.</returns>
    public static bool HasReached(long unixSeconds)
    {
        return NowUnixSeconds >= unixSeconds;
    }

    /// <summary>
    /// Định dạng số giây thành chuỗi MM:SS (ví dụ: "09:30").
    /// Dùng để hiển thị countdown timer trên UI.
    /// </summary>
    /// <param name="totalSeconds">Tổng số giây cần định dạng.</param>
    /// <returns>Chuỗi dạng "MM:SS".</returns>
    public static string FormatCountdown(float totalSeconds)
    {
        int secs = Mathf.Max(0, Mathf.CeilToInt(totalSeconds));
        int minutes = secs / 60;
        int seconds = secs % 60;
        return $"{minutes:00}:{seconds:00}";
    }
}
