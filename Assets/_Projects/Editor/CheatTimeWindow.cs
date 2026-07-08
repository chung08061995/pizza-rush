using UnityEditor;
using UnityEngine;

namespace UI.Cheat
{
    /// <summary>
    /// Cửa sổ Editor Window hỗ trợ cheat/debug nhanh tốc độ game (TimeScale) trực tiếp từ Unity Editor.
    /// </summary>
    public class CheatTimeWindow : EditorWindow
    {
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
        }
        
        private void Update()
        {
            // Tự động vẽ lại cửa sổ khi giá trị TimeScale thay đổi
            Repaint();
        }
    }
}
