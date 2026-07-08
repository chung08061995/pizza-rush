using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ToolMenuEditor
{
    [MenuItem("MyMenu/" + nameof(StartGame))]
    public static void StartGame()
    {
        EditorSceneManager.OpenScene("Assets/_Projects/Scenes/Init.unity");
        EditorApplication.isPlaying = true;

    }
}

