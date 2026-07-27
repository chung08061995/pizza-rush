using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using Firebase.Analytics;
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
    private const int MaxQueuedEvents = 64;
    private static readonly Queue<PendingEvent> PendingEvents = new();
    private static bool _subscribed;

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
        Log(eventName, null);
    }

    public static void Log(string eventName, Dictionary<string, object> parameters)
    {
        eventName = SanitizeName(eventName);
        parameters ??= new Dictionary<string, object>();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(parameters.Count == 0
            ? $"[Analytics] {eventName}"
            : $"[Analytics] {eventName}: {string.Join(", ", FormatParameters(parameters))}");
#endif

        EnsureFirebaseSubscription();
        if (!FirebaseBootstrap.IsReady)
        {
            QueueEvent(eventName, parameters);
            FirebaseBootstrap.Initialize();
            return;
        }

        TryLogNow(eventName, parameters);
    }

    public static void LogLevelEvent(string eventName)
    {
        int level = DataManager.Instance != null ? DataManager.Instance.Level.Value : 0;
        FirebaseBootstrap.SetCurrentLevel(level);
        Log(eventName, new Dictionary<string, object>
        {
            { "level", level },
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

    private static void EnsureFirebaseSubscription()
    {
        if (_subscribed)
        {
            return;
        }

        _subscribed = true;
        FirebaseBootstrap.InitializationCompleted += OnFirebaseInitializationCompleted;
    }

    private static void OnFirebaseInitializationCompleted(bool success, string error)
    {
        if (!success)
        {
            return;
        }

        while (PendingEvents.Count > 0)
        {
            var pending = PendingEvents.Dequeue();
            TryLogNow(pending.Name, pending.Parameters);
        }
    }

    private static void QueueEvent(string eventName, Dictionary<string, object> parameters)
    {
        if (PendingEvents.Count >= MaxQueuedEvents)
        {
            PendingEvents.Dequeue();
        }

        PendingEvents.Enqueue(new PendingEvent(
            eventName,
            new Dictionary<string, object>(parameters)));
    }

    private static void TryLogNow(string eventName, Dictionary<string, object> parameters)
    {
        try
        {
            var firebaseParameters = new List<Parameter>(parameters.Count);
            foreach (var pair in parameters)
            {
                string name = SanitizeName(pair.Key);
                switch (pair.Value)
                {
                    case null:
                        firebaseParameters.Add(new Parameter(name, string.Empty));
                        break;
                    case bool boolValue:
                        firebaseParameters.Add(new Parameter(name, boolValue ? 1L : 0L));
                        break;
                    case byte byteValue:
                        firebaseParameters.Add(new Parameter(name, (long)byteValue));
                        break;
                    case short shortValue:
                        firebaseParameters.Add(new Parameter(name, (long)shortValue));
                        break;
                    case int intValue:
                        firebaseParameters.Add(new Parameter(name, (long)intValue));
                        break;
                    case long longValue:
                        firebaseParameters.Add(new Parameter(name, longValue));
                        break;
                    case float floatValue:
                        firebaseParameters.Add(new Parameter(name, (double)floatValue));
                        break;
                    case double doubleValue:
                        firebaseParameters.Add(new Parameter(name, doubleValue));
                        break;
                    default:
                        firebaseParameters.Add(new Parameter(name, pair.Value.ToString()));
                        break;
                }
            }

            FirebaseAnalytics.LogEvent(eventName, firebaseParameters.ToArray());
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"[Analytics] Failed to log '{eventName}': {exception.GetBaseException().Message}");
        }
    }

    private static string SanitizeName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "unnamed_event";
        }

        value = value.Trim().ToLowerInvariant();
        var chars = value.ToCharArray();
        for (int index = 0; index < chars.Length; index++)
        {
            if (!char.IsLetterOrDigit(chars[index]) && chars[index] != '_')
            {
                chars[index] = '_';
            }
        }

        string sanitized = new string(chars);
        if (!char.IsLetter(sanitized[0]))
        {
            sanitized = "e_" + sanitized;
        }

        return sanitized.Length <= 40 ? sanitized : sanitized.Substring(0, 40);
    }

    private readonly struct PendingEvent
    {
        public readonly string Name;
        public readonly Dictionary<string, object> Parameters;

        public PendingEvent(string name, Dictionary<string, object> parameters)
        {
            Name = name;
            Parameters = parameters;
        }
    }
}
