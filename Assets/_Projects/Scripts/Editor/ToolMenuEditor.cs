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

    static ToolMenuEditor()
    {
        EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(InitScenePath);
    }

    [MenuItem("MyMenu/" + nameof(StartGame))]
    public static void StartGame()
    {
        EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(InitScenePath);
        EditorApplication.isPlaying = true;
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
}
