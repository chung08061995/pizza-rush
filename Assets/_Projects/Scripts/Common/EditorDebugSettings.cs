#if UNITY_EDITOR
using UnityEditor;

public static class EditorDebugSettings
{
    private const string InfiniteTimeKey = "PizzaRush.Debug.InfiniteTime";

    public static bool InfiniteTime
    {
        get => EditorPrefs.GetBool(InfiniteTimeKey, false);
        set => EditorPrefs.SetBool(InfiniteTimeKey, value);
    }
}
#endif
