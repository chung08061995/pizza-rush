using System;
using UnityEngine;

public static class MonetizationConfig
{
    public const int MinimumInterstitialStartLevel = 5;
    public const float MinimumInterstitialCooldownSeconds = 90f;

    public const string InterstitialEnabledKey = "ads_inter_enabled";
    public const string InterstitialStartLevelKey = "ads_inter_start_level";
    public const string InterstitialWinIntervalKey = "ads_inter_win_interval";
    public const string InterstitialLoseIntervalKey = "ads_inter_lose_interval";
    public const string InterstitialCooldownSecondsKey = "ads_inter_cooldown_seconds";
    public const string BannerEnabledKey = "ads_banner_enabled";
    public const string BannerStartLevelKey = "ads_banner_start_level";
    public const string RewardedEnabledKey = "ads_rewarded_enabled";

    public static bool InterstitialEnabled { get; private set; } = true;
    public static int InterstitialStartLevel { get; private set; } = MinimumInterstitialStartLevel;
    public static int InterstitialWinInterval { get; private set; } = 1;
    public static int InterstitialLoseInterval { get; private set; } = 1;
    public static float InterstitialCooldownSeconds { get; private set; } = MinimumInterstitialCooldownSeconds;
    public static bool BannerEnabled { get; private set; } = true;
    public static int BannerStartLevel { get; private set; } = 10;
    public static bool RewardedEnabled { get; private set; } = true;

    public static event Action ConfigChanged;

    public static void Apply(
        bool interstitialEnabled,
        int interstitialStartLevel,
        int interstitialWinInterval,
        int interstitialLoseInterval,
        float interstitialCooldownSeconds,
        bool bannerEnabled,
        int bannerStartLevel,
        bool rewardedEnabled)
    {
        InterstitialEnabled = interstitialEnabled;
        InterstitialStartLevel = Mathf.Max(MinimumInterstitialStartLevel, interstitialStartLevel);
        InterstitialWinInterval = Mathf.Max(1, interstitialWinInterval);
        InterstitialLoseInterval = Mathf.Max(1, interstitialLoseInterval);
        InterstitialCooldownSeconds = Mathf.Max(MinimumInterstitialCooldownSeconds, interstitialCooldownSeconds);
        BannerEnabled = bannerEnabled;
        BannerStartLevel = Mathf.Max(1, bannerStartLevel);
        RewardedEnabled = rewardedEnabled;
        ConfigChanged?.Invoke();
    }

    public static bool CanShowBanner(int level)
    {
        return BannerEnabled && level >= BannerStartLevel;
    }

    public static bool CanShowInterstitial(int level)
    {
        return InterstitialEnabled && level >= InterstitialStartLevel;
    }
}
