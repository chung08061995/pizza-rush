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
}
