using System;
using Firebase;
using Firebase.Analytics;
using Firebase.Crashlytics;
using Firebase.Extensions;
using UnityEngine;

public sealed class FirebaseBootstrap : MonoBehaviour
{
    private static FirebaseBootstrap _instance;

    public static bool IsInitializing { get; private set; }
    public static bool IsReady { get; private set; }
    public static string LastError { get; private set; } = string.Empty;

    public static event Action<bool, string> InitializationCompleted;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        Initialize();
    }

    public static void Initialize()
    {
        if (IsReady || IsInitializing)
        {
            return;
        }

        if (_instance == null)
        {
            var gameObject = new GameObject(nameof(FirebaseBootstrap));
            DontDestroyOnLoad(gameObject);
            _instance = gameObject.AddComponent<FirebaseBootstrap>();
        }

        IsInitializing = true;

        try
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                IsInitializing = false;

                if (task.IsCanceled || task.IsFaulted)
                {
                    Complete(false, task.Exception?.GetBaseException().Message ?? "Firebase dependency check failed.");
                    return;
                }

                if (task.Result != DependencyStatus.Available)
                {
                    Complete(false, $"Firebase dependencies are unavailable: {task.Result}");
                    return;
                }

                try
                {
                    _ = FirebaseApp.DefaultInstance;
                    FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                    Crashlytics.ReportUncaughtExceptionsAsFatal = true;
                    Crashlytics.SetCustomKey("app_version", Application.version);
                    Crashlytics.SetCustomKey("platform", Application.platform.ToString());
                    FirebaseRemoteConfigLoader.Initialize();
                    Complete(true, string.Empty);
                }
                catch (Exception exception)
                {
                    Complete(false, exception.GetBaseException().Message);
                }
            });
        }
        catch (Exception exception)
        {
            IsInitializing = false;
            Complete(false, exception.GetBaseException().Message);
        }
    }

    public static void RecordNonFatal(Exception exception)
    {
        if (exception == null)
        {
            return;
        }

        if (IsReady)
        {
            Crashlytics.LogException(exception);
        }
        else
        {
            Debug.LogException(exception);
        }
    }

    public static void SetCurrentLevel(int level)
    {
        if (!IsReady)
        {
            return;
        }

        Crashlytics.SetCustomKey("current_level", Mathf.Max(0, level).ToString());
        FirebaseAnalytics.SetUserProperty("current_level", Mathf.Max(0, level).ToString());
    }

    private static void Complete(bool success, string error)
    {
        IsReady = success;
        LastError = error ?? string.Empty;

        if (success)
        {
            Debug.Log("[Firebase] Initialized.");
        }
        else
        {
            Debug.LogWarning($"[Firebase] Initialization failed; game will continue without telemetry: {LastError}");
        }

        InitializationCompleted?.Invoke(success, LastError);
    }
}
