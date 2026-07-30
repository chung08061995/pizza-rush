using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class ToolMenuEditor
{
    private const string InitScenePath = "Assets/_Projects/Scenes/Init.unity";
    private const int DebugGoldAmount = 99_999;
    private const string PendingStartLevelSessionKey = "PizzaRush.Debug.PendingStartLevel";

    static ToolMenuEditor()
    {
        EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(InitScenePath);
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    [MenuItem("MyMenu/" + nameof(StartGame))]
    public static void StartGame()
    {
        EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(InitScenePath);
        EditorApplication.isPlaying = true;
    }

    [MenuItem("MyMenu/Debug/Infinite Time")]
    public static void ToggleInfiniteTime()
    {
        EditorDebugSettings.InfiniteTime = !EditorDebugSettings.InfiniteTime;
        Menu.SetChecked("MyMenu/Debug/Infinite Time", EditorDebugSettings.InfiniteTime);

        if (EditorApplication.isPlaying)
        {
            var levelRunner = Object.FindFirstObjectByType<LevelRunner>();
            levelRunner?.RefreshEditorInfiniteTime();
        }

        Debug.Log($"Infinite gameplay time: {(EditorDebugSettings.InfiniteTime ? "ON" : "OFF")}.");
    }

    [MenuItem("MyMenu/Debug/Infinite Time", true)]
    private static bool ValidateInfiniteTime()
    {
        Menu.SetChecked("MyMenu/Debug/Infinite Time", EditorDebugSettings.InfiniteTime);
        return true;
    }

    [MenuItem("MyMenu/Debug/Start From Level...")]
    public static void ShowStartLevelWindow()
    {
        StartLevelWindow.Open();
    }

    public static void StartFromLevel(int level)
    {
        level = Mathf.Max(1, level);
        PlayerPrefs.SetInt(GameConstain.PlayerPrefsKey.Level, level);
        PlayerPrefs.Save();

        if (EditorApplication.isPlaying)
        {
            ApplyLevelAndLoadGameplay(level);
            return;
        }

        SessionState.SetInt(PendingStartLevelSessionKey, level);
        StartGame();
    }

    [MenuItem("MyMenu/Debug/Add 99999 Gold")]
    public static void Add99999Gold()
    {
        if (EditorApplication.isPlaying)
        {
            var dataManager = Object.FindFirstObjectByType<DataManager>();
            if (dataManager != null)
            {
                dataManager.Using(ItemType.Gold, DebugGoldAmount);
                Debug.Log($"Added {DebugGoldAmount:N0} Gold. Current Gold: {dataManager.gold.Value:N0}.");
                return;
            }
        }

        var currentGold = PlayerPrefs.GetInt(GameConstain.PlayerPrefsKey.Gold, 0);
        var updatedGold = currentGold > int.MaxValue - DebugGoldAmount
            ? int.MaxValue
            : currentGold + DebugGoldAmount;

        PlayerPrefs.SetInt(GameConstain.PlayerPrefsKey.Gold, updatedGold);
        PlayerPrefs.Save();
        Debug.Log($"Added {DebugGoldAmount:N0} Gold to PlayerPrefs. Current Gold: {updatedGold:N0}.");
    }

    [MenuItem("MyMenu/Debug/Clear PlayerPrefs")]
    public static void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("PlayerPrefs cleared. Restart Play Mode to reload all default player data.");
            return;
        }

        Debug.Log("PlayerPrefs cleared. The next game run will create fresh default player data.");
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode
            && SessionState.GetInt(PendingStartLevelSessionKey, 0) > 0)
        {
            EditorApplication.update -= TryLoadPendingLevel;
            EditorApplication.update += TryLoadPendingLevel;
        }
        else if (state == PlayModeStateChange.ExitingPlayMode)
        {
            EditorApplication.update -= TryLoadPendingLevel;
        }
    }

    private static void TryLoadPendingLevel()
    {
        var level = SessionState.GetInt(PendingStartLevelSessionKey, 0);
        var mainScene = SceneManager.GetSceneByName(GameConstain.SenceName.Main);
        if (level <= 0
            || DataManager.Instance == null
            || !mainScene.IsValid()
            || !mainScene.isLoaded)
        {
            return;
        }

        EditorApplication.update -= TryLoadPendingLevel;
        SessionState.EraseInt(PendingStartLevelSessionKey);
        ApplyLevelAndLoadGameplay(level);
    }

    private static void ApplyLevelAndLoadGameplay(int level)
    {
        if (DataManager.Instance == null)
        {
            Debug.LogError($"Cannot start Level {level}: DataManager is not ready.");
            return;
        }

        DataManager.Instance.Level.SetValue(level);
        DataManager.Instance.Level.Notifier.Notify();
        DataManager.Instance.Level.Save();
        PopupManager.Instance?.HideAllPopupInGameplay();
        SceneControllerExtensions.LoadGameplay();
        Debug.Log($"Starting gameplay at Level {level}.");
    }
}
