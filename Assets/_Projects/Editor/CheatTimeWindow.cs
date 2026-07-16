using UnityEditor;
using UnityEngine;

namespace UI.Cheat
{
    /// <summary>
    /// Cửa sổ Editor Window hỗ trợ cheat/debug nhanh tốc độ game (TimeScale) trực tiếp từ Unity Editor.
    /// </summary>
    public class CheatTimeWindow : EditorWindow
    {
        private int selectedLevel = 1;
        private bool randomLoop;

        [MenuItem("Tools/Cheat Time Window")]
        public static void ShowWindow()
        {
            GetWindow<CheatTimeWindow>("Cheat Time");
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            GUILayout.Label("Cấu hình Tốc độ Game (Time Scale)", EditorStyles.boldLabel);
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Tốc độ hiện tại:", GUILayout.Width(100));
            GUILayout.Label($"x{Time.timeScale:F2}", EditorStyles.boldLabel);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            
            // Dòng các nút chọn tốc độ chậm
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("x0.1")) Time.timeScale = 0.1f;
            if (GUILayout.Button("x0.2")) Time.timeScale = 0.2f;
            if (GUILayout.Button("x0.5")) Time.timeScale = 0.5f;
            if (GUILayout.Button("x1.0")) Time.timeScale = 1.0f;
            GUILayout.EndHorizontal();

            // Dòng các nút chọn tốc độ nhanh
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("x2.0")) Time.timeScale = 2.0f;
            if (GUILayout.Button("x3.0")) Time.timeScale = 3.0f;
            if (GUILayout.Button("x5.0")) Time.timeScale = 5.0f;
            if (GUILayout.Button("x10.0")) Time.timeScale = 10.0f;
            if (GUILayout.Button("x16.0")) Time.timeScale = 16.0f;
            GUILayout.EndHorizontal();

            GUILayout.Space(15);
            if (GUILayout.Button("Reset về mặc định (x1.0)", GUILayout.Height(25)))
            {
                Time.timeScale = 1.0f;
            }

            GUILayout.Space(15);
            GUILayout.Label("Level preview", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Level", GUILayout.Width(45));
            selectedLevel = Mathf.Clamp(EditorGUILayout.IntField(selectedLevel), 1, LevelPreview.MaxLevel);
            if (GUILayout.Button("Set Level", GUILayout.Height(22)))
            {
                randomLoop = false;
                LevelPreview.Load(selectedLevel);
            }
            if (GUILayout.Button("Next", GUILayout.Height(22)))
            {
                LevelPreview.LoadNext(ref selectedLevel, ref randomLoop);
            }
            GUILayout.EndHorizontal();

            GUILayout.Label(
                $"Next đi tuần tự đến Level {LevelPreview.MaxLevel}, sau đó chọn ngẫu nhiên 1–{LevelPreview.MaxLevel}.",
                EditorStyles.wordWrappedMiniLabel);
        }
        
        private void Update()
        {
            // Tự động vẽ lại cửa sổ khi giá trị TimeScale thay đổi
            Repaint();
        }

        private static class LevelPreview
        {
            public const int MaxLevel = 100;

            public static void Load(int level)
            {
                if (!EditorApplication.isPlaying || DataManager.Instance == null)
                {
                    EditorUtility.DisplayDialog(
                        "Level preview",
                        "Hãy vào Play Mode bằng MyMenu > StartGame trước khi mở level.",
                        "OK");
                    return;
                }

                level = Mathf.Clamp(level, 1, MaxLevel);
                DataManager.Instance.Level.SetValue(level);
                DataManager.Instance.Level.Notifier.Notify();
                DataManager.Instance.Level.Save();
                SceneControllerExtensions.LoadGameplay();
            }

            public static void LoadNext(ref int selectedLevel, ref bool randomLoop)
            {
                if (!EditorApplication.isPlaying)
                {
                    Load(selectedLevel);
                    return;
                }

                var current = DataManager.Instance != null
                    ? DataManager.Instance.Level.Value
                    : selectedLevel;
                randomLoop |= current >= MaxLevel;
                selectedLevel = randomLoop
                    ? Random.Range(1, MaxLevel + 1)
                    : current + 1;
                Load(selectedLevel);
            }
        }
    }
}
