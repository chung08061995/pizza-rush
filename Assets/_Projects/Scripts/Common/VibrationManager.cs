using UnityEngine;

public enum VibrationType
{
    Selection,
    ItemPlaced,
    Completion,
    Success,
    Warning,
}

public static class VibrationManager
{
    private const float MinIntervalSeconds = 0.08f;
    private static float _lastVibrationTime = float.NegativeInfinity;

    public static void Vibrate(VibrationType type)
    {
        if (!DataManager.Instance.vibrate.Value ||
            Time.realtimeSinceStartup - _lastVibrationTime < MinIntervalSeconds)
        {
            return;
        }

        _lastVibrationTime = Time.realtimeSinceStartup;

#if UNITY_ANDROID && !UNITY_EDITOR
        VibrateAndroid(type);
#elif UNITY_IOS && !UNITY_EDITOR
        Handheld.Vibrate();
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private static void VibrateAndroid(VibrationType type)
    {
        GetAndroidPattern(type, out long durationMilliseconds, out int amplitude);

        try
        {
            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            using var vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");

            if (vibrator == null || !vibrator.Call<bool>("hasVibrator"))
            {
                return;
            }

            using var version = new AndroidJavaClass("android.os.Build$VERSION");
            if (version.GetStatic<int>("SDK_INT") >= 26)
            {
                using var vibrationEffect = new AndroidJavaClass("android.os.VibrationEffect");
                using var effect = vibrationEffect.CallStatic<AndroidJavaObject>(
                    "createOneShot",
                    durationMilliseconds,
                    amplitude);
                vibrator.Call("vibrate", effect);
            }
            else
            {
                vibrator.Call("vibrate", durationMilliseconds);
            }
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning($"Unable to vibrate this Android device: {exception.Message}");
        }
    }

    private static void GetAndroidPattern(
        VibrationType type,
        out long durationMilliseconds,
        out int amplitude)
    {
        switch (type)
        {
            case VibrationType.Selection:
                durationMilliseconds = 25;
                amplitude = 70;
                break;
            case VibrationType.ItemPlaced:
                durationMilliseconds = 18;
                amplitude = 55;
                break;
            case VibrationType.Completion:
                durationMilliseconds = 45;
                amplitude = 110;
                break;
            case VibrationType.Success:
                durationMilliseconds = 90;
                amplitude = 160;
                break;
            case VibrationType.Warning:
                durationMilliseconds = 180;
                amplitude = 220;
                break;
            default:
                durationMilliseconds = 45;
                amplitude = 110;
                break;
        }
    }
#endif
}
