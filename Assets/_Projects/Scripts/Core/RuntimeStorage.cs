using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeStorage : DraftUtils.SingletonDontDestroyOnLoadMonoBehaviour<RuntimeStorage>
{
    [ShowInInspector] [ReadOnly] private readonly Dictionary<string, object> _storage = new();

    public void Set<T>(string key, T value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            Debug.LogWarning("RuntimeStorage.Set called with an empty key.");
            return;
        }

        _storage[key] = value;
    }

    public bool TryGet<T>(string key, out T value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            value = default;
            return false;
        }

        if (_storage.TryGetValue(key, out var storedValue))
        {
            if (storedValue is null)
            {
                value = default;
                return default(T) is null;
            }

            if (storedValue is T typedValue)
            {
                value = typedValue;
                return true;
            }
        }

        value = default;
        return false;
    }

    public T Get<T>(string key, T defaultValue = default)
    {
        return TryGet(key, out T value) ? value : defaultValue;
    }

    public bool ContainsKey(string key)
    {
        return !string.IsNullOrWhiteSpace(key) && _storage.ContainsKey(key);
    }

    public void Remove(string key)
    {
        if (!string.IsNullOrWhiteSpace(key))
        {
            _storage.Remove(key);
        }
    }

    public void Clear()
    {
        _storage.Clear();
    }

    public int Count => _storage.Count;
}

public static class GameAnalytics
{
    public const string AppStart = "app_start";
    public const string LevelStart = "level_start";
    public const string LevelWin = "level_win";
    public const string LevelLose = "level_lose";
    public const string LevelRetry = "level_retry";
    public const string BoosterUse = "booster_use";
    public const string SkillUse = "skill_use";
    public const string RewardedAdShow = "rewarded_ad_show";
    public const string RewardedAdComplete = "rewarded_ad_complete";
    public const string IapPurchaseSuccess = "iap_purchase_success";
    public const string IapPurchaseFail = "iap_purchase_fail";

    public static void Log(string eventName)
    {
        Debug.Log($"[Analytics] {eventName}");
    }

    public static void Log(string eventName, Dictionary<string, object> parameters)
    {
        if (parameters == null || parameters.Count == 0)
        {
            Log(eventName);
            return;
        }

        Debug.Log($"[Analytics] {eventName}: {string.Join(", ", FormatParameters(parameters))}");
    }

    public static void LogLevelEvent(string eventName)
    {
        Log(eventName, new Dictionary<string, object>
        {
            { "level", DataManager.Instance != null ? DataManager.Instance.Level.Value : 0 },
        });
    }

    public static void LogItemEvent(string eventName, ItemType itemType)
    {
        Log(eventName, new Dictionary<string, object>
        {
            { "item_type", itemType.ToString() },
            { "level", DataManager.Instance != null ? DataManager.Instance.Level.Value : 0 },
        });
    }

    public static void LogPurchaseEvent(string eventName, string productId, string failureReason = "")
    {
        var parameters = new Dictionary<string, object>
        {
            { "product_id", productId },
            { "level", DataManager.Instance != null ? DataManager.Instance.Level.Value : 0 },
        };

        if (!string.IsNullOrEmpty(failureReason))
        {
            parameters["failure_reason"] = failureReason;
        }

        Log(eventName, parameters);
    }

    private static IEnumerable<string> FormatParameters(Dictionary<string, object> parameters)
    {
        foreach (var pair in parameters)
        {
            yield return $"{pair.Key}={pair.Value}";
        }
    }
}
