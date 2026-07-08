using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

/// <summary>
/// Editor Window: kéo GameObject (runtime) vào, kéo SpriteAtlas vào.
/// Click button → tìm tất cả Image trong con của object đó,
/// lấy sprite (loại trùng), và import vào atlas.
/// Menu: Tools > Sprite Atlas Importer
/// </summary>
public class SpriteAtlasImporterWindow : EditorWindow
{
    private GameObject targetObject;
    private SpriteAtlas spriteAtlas;
    private Vector2 scrollPos;
    private List<Sprite> foundSprites = new();
    private List<Object> packableAssets = new();
    private List<string> assetPaths = new();

    [MenuItem("Tools/Sprite Atlas Importer")]
    public static void ShowWindow()
    {
        GetWindow<SpriteAtlasImporterWindow>("Sprite Atlas Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Sprite Atlas Importer", EditorStyles.boldLabel);
        GUILayout.Space(10);

        targetObject = (GameObject)EditorGUILayout.ObjectField("Target Object", targetObject, typeof(GameObject), true);
        spriteAtlas = (SpriteAtlas)EditorGUILayout.ObjectField("Sprite Atlas", spriteAtlas, typeof(SpriteAtlas), false);

        GUILayout.Space(10);

        GUI.enabled = targetObject != null;
        if (GUILayout.Button("Find Sprites In Children", GUILayout.Height(26)))
        {
            FindSprites();
        }

        GUI.enabled = targetObject != null && spriteAtlas != null;
        if (GUILayout.Button("Find & Add To Sprite Atlas", GUILayout.Height(30)))
        {
            FindAndImport();
        }
        GUI.enabled = true;

        GUILayout.Space(10);

        if (foundSprites.Count > 0)
        {
            GUILayout.Label($"Found {foundSprites.Count} unique sprites:", EditorStyles.helpBox);
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.MaxHeight(300));
            for (int i = 0; i < foundSprites.Count; i++)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.ObjectField(foundSprites[i], typeof(Sprite), false);
                EditorGUILayout.SelectableLabel(assetPaths[i], EditorStyles.miniLabel, GUILayout.Height(16));
                EditorGUILayout.EndVertical();
            }
            GUILayout.EndScrollView();
        }
    }

    private void FindSprites()
    {
        foundSprites.Clear();
        packableAssets.Clear();
        assetPaths.Clear();

        // Tìm tất cả UI Image trong con
        var images = targetObject.GetComponentsInChildren<Image>(true);
        var spriteRenderers = targetObject.GetComponentsInChildren<SpriteRenderer>(true);
        var uniqueSprites = new HashSet<Sprite>();

        foreach (var image in images)
        {
            if (image.sprite != null)
            {
                uniqueSprites.Add(image.sprite);
            }
        }

        foreach (var spriteRenderer in spriteRenderers)
        {
            if (spriteRenderer.sprite != null)
            {
                uniqueSprites.Add(spriteRenderer.sprite);
            }
        }

        foundSprites = uniqueSprites.ToList();

        if (foundSprites.Count == 0)
        {
            Debug.LogWarning("[SpriteAtlasImporter] Không tìm thấy sprite nào trong children.");
            return;
        }

        foreach (var sprite in foundSprites)
        {
            var path = AssetDatabase.GetAssetPath(sprite);
            if (string.IsNullOrEmpty(path))
            {
                continue;
            }

            var packable = AssetDatabase.LoadMainAssetAtPath(path);
            if (packable == null)
            {
                continue;
            }

            packableAssets.Add(packable);
            assetPaths.Add(path);
        }
    }

    private void FindAndImport()
    {
        FindSprites();

        if (packableAssets.Count == 0)
        {
            return;
        }

        var existingPaths = new HashSet<string>(
            SpriteAtlasExtensions.GetPackables(spriteAtlas)
                .Where(packable => packable != null)
                .Select(AssetDatabase.GetAssetPath)
                .Where(path => !string.IsNullOrEmpty(path))
        );

        var assetsToAdd = new List<Object>();
        foreach (var packable in packableAssets)
        {
            var path = AssetDatabase.GetAssetPath(packable);
            if (string.IsNullOrEmpty(path) || existingPaths.Contains(path))
            {
                continue;
            }

            assetsToAdd.Add(packable);
            existingPaths.Add(path);
        }

        if (assetsToAdd.Count > 0)
        {
            SpriteAtlasExtensions.Add(spriteAtlas, assetsToAdd.ToArray());
        }

        EditorUtility.SetDirty(spriteAtlas);
        AssetDatabase.SaveAssets();

        Debug.Log($"[SpriteAtlasImporter] Đã thêm {assetsToAdd.Count} assets vào atlas (tìm thấy {foundSprites.Count} sprites unique).");
    }
}
