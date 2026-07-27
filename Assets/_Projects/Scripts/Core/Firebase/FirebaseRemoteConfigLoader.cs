using System;
using System.Collections.Generic;
using Firebase.Extensions;
using Firebase.RemoteConfig;
using UnityEngine;

public static class FirebaseRemoteConfigLoader
{
    private static bool _started;

    public static bool HasAppliedValues { get; private set; }
    public static string LastError { get; private set; } = string.Empty;

    public static void Initialize()
    {
        if (_started)
        {
            return;
        }

        _started = true;
        FirebaseRemoteConfig remoteConfig;

        try
        {
            remoteConfig = FirebaseRemoteConfig.DefaultInstance;
        }
        catch (Exception exception)
        {
            Fail(exception);
            return;
        }

        remoteConfig.SetDefaultsAsync(CreateDefaults()).ContinueWithOnMainThread(defaultsTask =>
        {
            if (!IsSuccessful(defaultsTask, "set defaults"))
            {
                return;
            }

            ApplyValues(remoteConfig);
            remoteConfig.FetchAsync(GetCacheExpiration()).ContinueWithOnMainThread(fetchTask =>
            {
                if (!IsSuccessful(fetchTask, "fetch"))
                {
                    return;
                }

                if (remoteConfig.Info.LastFetchStatus != LastFetchStatus.Success)
                {
                    Fail($"Fetch status: {remoteConfig.Info.LastFetchStatus}; reason: {remoteConfig.Info.LastFetchFailureReason}");
                    return;
                }

                remoteConfig.ActivateAsync().ContinueWithOnMainThread(activateTask =>
                {
                    if (!IsSuccessful(activateTask, "activate"))
                    {
                        return;
                    }

                    ApplyValues(remoteConfig);
                    Debug.Log($"[Firebase] Remote Config applied. Fetch time: {remoteConfig.Info.FetchTime:u}");
                });
            });
        });
    }

    private static Dictionary<string, object> CreateDefaults()
    {
        return new Dictionary<string, object>
        {
            { MonetizationConfig.InterstitialEnabledKey, MonetizationConfig.InterstitialEnabled },
            { MonetizationConfig.InterstitialStartLevelKey, MonetizationConfig.InterstitialStartLevel },
            { MonetizationConfig.InterstitialWinIntervalKey, MonetizationConfig.InterstitialWinInterval },
            { MonetizationConfig.InterstitialLoseIntervalKey, MonetizationConfig.InterstitialLoseInterval },
            { MonetizationConfig.InterstitialCooldownSecondsKey, MonetizationConfig.InterstitialCooldownSeconds },
            { MonetizationConfig.BannerEnabledKey, MonetizationConfig.BannerEnabled },
            { MonetizationConfig.BannerStartLevelKey, MonetizationConfig.BannerStartLevel },
            { MonetizationConfig.RewardedEnabledKey, MonetizationConfig.RewardedEnabled },
        };
    }

    private static void ApplyValues(FirebaseRemoteConfig remoteConfig)
    {
        MonetizationConfig.Apply(
            remoteConfig.GetValue(MonetizationConfig.InterstitialEnabledKey).BooleanValue,
            ToInt(remoteConfig.GetValue(MonetizationConfig.InterstitialStartLevelKey).LongValue),
            ToInt(remoteConfig.GetValue(MonetizationConfig.InterstitialWinIntervalKey).LongValue),
            ToInt(remoteConfig.GetValue(MonetizationConfig.InterstitialLoseIntervalKey).LongValue),
            ToFloat(remoteConfig.GetValue(MonetizationConfig.InterstitialCooldownSecondsKey).DoubleValue),
            remoteConfig.GetValue(MonetizationConfig.BannerEnabledKey).BooleanValue,
            ToInt(remoteConfig.GetValue(MonetizationConfig.BannerStartLevelKey).LongValue),
            remoteConfig.GetValue(MonetizationConfig.RewardedEnabledKey).BooleanValue);

        HasAppliedValues = true;
        LastError = string.Empty;
    }

    private static bool IsSuccessful(System.Threading.Tasks.Task task, string operation)
    {
        if (task.IsCanceled || task.IsFaulted)
        {
            Fail(task.Exception?.GetBaseException().Message ?? $"Remote Config {operation} failed.");
            return false;
        }

        return true;
    }

    private static void Fail(Exception exception)
    {
        Fail(exception.GetBaseException().Message);
    }

    private static void Fail(string error)
    {
        LastError = string.IsNullOrEmpty(error) ? "Unknown Remote Config error." : error;
        Debug.LogWarning($"[Firebase] Remote Config kept local/cached values: {LastError}");
    }

    private static TimeSpan GetCacheExpiration()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        return TimeSpan.Zero;
#else
        return TimeSpan.FromHours(12d);
#endif
    }

    private static int ToInt(long value)
    {
        return value > int.MaxValue ? int.MaxValue : value < int.MinValue ? int.MinValue : (int)value;
    }

    private static float ToFloat(double value)
    {
        if (double.IsNaN(value)) return 0f;
        if (value > float.MaxValue) return float.MaxValue;
        if (value < -float.MaxValue) return -float.MaxValue;
        return (float)value;
    }
}
