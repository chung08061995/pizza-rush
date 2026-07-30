using UnityEditor;
using UnityEngine;

public class StartLevelWindow : EditorWindow
{
    private int level = 1;

    public static void Open()
    {
        var window = GetWindow<StartLevelWindow>(true, "Start From Level", true);
        window.minSize = new Vector2(280f, 105f);
        window.maxSize = new Vector2(420f, 105f);
        window.level = Mathf.Max(1, PlayerPrefs.GetInt(GameConstain.PlayerPrefsKey.Level, 1));
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(10f);
        level = Mathf.Max(1, EditorGUILayout.IntField("Level", level));
        EditorGUILayout.Space(8f);

        if (GUILayout.Button("Start Gameplay", GUILayout.Height(30f)))
        {
            ToolMenuEditor.StartFromLevel(level);
            Close();
        }
    }
}
