using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

internal static class RuntimeTypographyAudit
{
    private const string MenuRoot = "MyMenu/Typography/";
    private const string GeneratedMaterialFolder = "Assets/_Projects/Fonts/Runtime Typography";
    private const string MontserratBitmapGuid = "9f9709f69473d46fba947a0840e8a2f2";
    private const string MontserratSdfGuid = "382c958a303ca654c8890bc7358214c9";
    private const string LilitaBitmapGuid = "0d83287e96eb4456f8c5a7451f964f30";
    private const string LilitaSdfGuid = "190bffe64bf5a6745ac2964dbf271a5c";
    private const float MaximumOutlineWidth = 0.10f;
    private const float TargetSharpness = 0.50f;

    private static readonly string[] RuntimeRoots =
    {
        "Assets/_Projects/Prefabs",
        "Assets/_Projects/Scenes",
        "Assets/_Projects/new UI"
    };

    private static readonly string[] ExcludedPathParts =
    {
        "/Editor/",
        "/LevelCreator",
        "/TestUI",
        "/Cheat",
        "/Debug",
        "/FPS"
    };

    private sealed class AuditResult
    {
        public int TextCount;
        public int FixedCount;
        public readonly List<string> Errors = new();
    }

    [MenuItem(MenuRoot + "Report Runtime UI")]
    private static void Report()
    {
        AuditResult result = ProcessRuntimeAssets(false);
        LogResult("Typography report", result);
    }

    [MenuItem(MenuRoot + "Fix Runtime UI")]
    private static void Fix()
    {
        AssetDatabase.StartAssetEditing();
        AuditResult result;
        try
        {
            EnsureMaterialFolder();
            result = ProcessRuntimeAssets(true);
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        AuditResult validation = ProcessRuntimeAssets(false);
        validation.FixedCount = result.FixedCount;
        LogResult("Typography fix", validation);
        if (validation.Errors.Count > 0)
        {
            throw new BuildFailedException(
                $"Runtime typography fix left {validation.Errors.Count} validation error(s). See Console.");
        }
    }

    [MenuItem(MenuRoot + "Validate Runtime UI")]
    internal static void Validate()
    {
        AuditResult result = ProcessRuntimeAssets(false);
        LogResult("Typography validation", result);
        if (result.Errors.Count > 0)
        {
            throw new BuildFailedException(
                $"Runtime typography validation failed with {result.Errors.Count} error(s). See Console.");
        }
    }

    private static AuditResult ProcessRuntimeAssets(bool applyFix)
    {
        AuditResult result = new();
        foreach (string path in FindRuntimeAssetPaths("t:Prefab"))
        {
            ProcessPrefab(path, applyFix, result);
        }

        foreach (string path in FindRuntimeAssetPaths("t:Scene"))
        {
            ProcessScene(path, applyFix, result);
        }

        return result;
    }

    private static IEnumerable<string> FindRuntimeAssetPaths(string filter)
    {
        return AssetDatabase.FindAssets(filter, RuntimeRoots)
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(path => !string.IsNullOrEmpty(path) && !IsExcluded(path))
            .Distinct()
            .OrderBy(path => path, StringComparer.Ordinal);
    }

    private static bool IsExcluded(string path)
    {
        string normalized = path.Replace('\\', '/');
        return ExcludedPathParts.Any(part =>
            normalized.IndexOf(part, StringComparison.OrdinalIgnoreCase) >= 0);
    }

    private static void ProcessPrefab(string path, bool applyFix, AuditResult result)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        bool changed = false;
        try
        {
            foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
            {
                changed |= ProcessText(text, path, applyFix, result);
            }

            if (applyFix && changed)
            {
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void ProcessScene(string path, bool applyFix, AuditResult result)
    {
        Scene scene = SceneManager.GetSceneByPath(path);
        bool wasLoaded = scene.IsValid() && scene.isLoaded;
        if (!wasLoaded)
        {
            scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
        }

        bool changed = false;
        try
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
                {
                    changed |= ProcessText(text, path, applyFix, result);
                }
            }

            if (applyFix && changed)
            {
                EditorSceneManager.SaveScene(scene);
            }
        }
        finally
        {
            if (!wasLoaded)
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }
    }

    private static bool ProcessText(TMP_Text text, string assetPath, bool applyFix, AuditResult result)
    {
        result.TextCount++;
        bool changed = false;
        TMP_FontAsset replacementFont = GetSdfReplacement(text.font);
        bool usesBitmapFont = replacementFont != null;
        bool outlinedRole = IsOutlinedRole(text);

        if (applyFix)
        {
            if (usesBitmapFont)
            {
                text.font = replacementFont;
                changed = true;
            }

            TMP_FontAsset targetFont = replacementFont != null ? replacementFont : text.font;
            if (targetFont != null &&
                IsSupportedRuntimeFont(targetFont) &&
                (usesBitmapFont || NeedsCleanMaterial(text.fontSharedMaterial, outlinedRole)))
            {
                Material cleanMaterial = GetOrCreateCleanMaterial(
                    text.fontSharedMaterial,
                    targetFont,
                    outlinedRole);
                if (cleanMaterial != null && text.fontSharedMaterial != cleanMaterial)
                {
                    text.fontSharedMaterial = cleanMaterial;
                    changed = true;
                }
            }

            FontStyles disallowedStyles = FontStyles.Underline | FontStyles.Strikethrough;
            if ((text.fontStyle & disallowedStyles) != 0)
            {
                text.fontStyle &= ~disallowedStyles;
                changed = true;
            }

            if (text.enableVertexGradient)
            {
                text.enableVertexGradient = false;
                changed = true;
            }

            if (changed)
            {
                EditorUtility.SetDirty(text);
                result.FixedCount++;
            }
        }

        ValidateText(text, assetPath, result);
        return changed;
    }

    private static void ValidateText(TMP_Text text, string assetPath, AuditResult result)
    {
        string objectPath = GetObjectPath(text.transform);
        TMP_FontAsset replacement = GetSdfReplacement(text.font);
        if (replacement != null)
        {
            result.Errors.Add($"{assetPath} :: {objectPath} uses Bitmap font '{text.font.name}'.");
        }

        if ((text.fontStyle & (FontStyles.Underline | FontStyles.Strikethrough)) != 0)
        {
            result.Errors.Add($"{assetPath} :: {objectPath} uses underline/strikethrough.");
        }

        if (text.enableVertexGradient)
        {
            result.Errors.Add($"{assetPath} :: {objectPath} uses a vertex gradient.");
        }

        Material material = text.fontSharedMaterial;
        if (material == null)
        {
            return;
        }

        if (HasUnderlay(material))
        {
            result.Errors.Add($"{assetPath} :: {objectPath} uses underlay material '{material.name}'.");
        }

        if (material.HasProperty(ShaderUtilities.ID_OutlineWidth) &&
            material.GetFloat(ShaderUtilities.ID_OutlineWidth) > MaximumOutlineWidth + 0.001f)
        {
            result.Errors.Add(
                $"{assetPath} :: {objectPath} outline is {material.GetFloat(ShaderUtilities.ID_OutlineWidth):0.###}.");
        }

        if (IsSupportedRuntimeFont(text.font))
        {
            float expectedOutline = IsOutlinedRole(text) ? MaximumOutlineWidth : 0f;
            float actualOutline = GetFloat(material, ShaderUtilities.ID_OutlineWidth);
            if (Mathf.Abs(actualOutline - expectedOutline) > 0.001f)
            {
                result.Errors.Add(
                    $"{assetPath} :: {objectPath} outline is {actualOutline:0.###}; " +
                    $"role requires {expectedOutline:0.###}.");
            }

            float sharpness = GetFloat(material, ShaderUtilities.ID_Sharpness);
            if (Mathf.Abs(sharpness - TargetSharpness) > 0.001f)
            {
                result.Errors.Add(
                    $"{assetPath} :: {objectPath} sharpness is {sharpness:0.###}; " +
                    $"runtime target is {TargetSharpness:0.###}.");
            }
        }
    }

    private static TMP_FontAsset GetSdfReplacement(TMP_FontAsset font)
    {
        if (font == null)
        {
            return null;
        }

        string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(font));
        return guid switch
        {
            MontserratBitmapGuid => LoadFont(MontserratSdfGuid),
            LilitaBitmapGuid => LoadFont(LilitaSdfGuid),
            _ => null
        };
    }

    private static bool IsSupportedRuntimeFont(TMP_FontAsset font)
    {
        string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(font));
        return guid == MontserratSdfGuid || guid == LilitaSdfGuid;
    }

    private static TMP_FontAsset LoadFont(string guid)
    {
        return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guid));
    }

    private static Material GetOrCreateCleanMaterial(
        Material source,
        TMP_FontAsset targetFont,
        bool outlined)
    {
        Material baseMaterial = IsMaterialCompatible(source, targetFont)
            ? source
            : targetFont.material;
        if (baseMaterial == null)
        {
            return null;
        }

        string role = outlined ? "Outline" : "Body";
        string safeName = SanitizeFileName($"{baseMaterial.name} {role} Clean");
        string path = $"{GeneratedMaterialFolder}/{safeName}.mat";
        Material clean = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (clean == null)
        {
            clean = new Material(baseMaterial)
            {
                name = safeName
            };
            NormalizeMaterial(clean, outlined);
            AssetDatabase.CreateAsset(clean, path);
        }
        else
        {
            NormalizeMaterial(clean, outlined);
            EditorUtility.SetDirty(clean);
        }

        return clean;
    }

    private static bool IsMaterialCompatible(Material material, TMP_FontAsset font)
    {
        if (material == null || font == null || material.shader == null)
        {
            return false;
        }

        return material.shader.name.IndexOf("Distance Field", StringComparison.OrdinalIgnoreCase) >= 0 &&
               font.atlasTexture != null &&
               material.HasProperty(ShaderUtilities.ID_MainTex) &&
               material.GetTexture(ShaderUtilities.ID_MainTex) == font.atlasTexture;
    }

    private static bool NeedsCleanMaterial(Material material, bool outlined)
    {
        if (material == null || HasUnderlay(material))
        {
            return true;
        }

        float expectedOutline = outlined ? MaximumOutlineWidth : 0f;
        return Mathf.Abs(GetFloat(material, ShaderUtilities.ID_OutlineWidth) - expectedOutline) > 0.001f ||
               Mathf.Abs(GetFloat(material, ShaderUtilities.ID_Sharpness) - TargetSharpness) > 0.001f;
    }

    private static void NormalizeMaterial(Material material, bool outlined)
    {
        SetFloat(material, ShaderUtilities.ID_OutlineWidth, outlined ? MaximumOutlineWidth : 0f);
        SetFloat(material, ShaderUtilities.ID_FaceDilate, 0f);
        SetFloat(material, ShaderUtilities.ID_Sharpness, TargetSharpness);
        SetFloat(material, ShaderUtilities.ID_UnderlayOffsetX, 0f);
        SetFloat(material, ShaderUtilities.ID_UnderlayOffsetY, 0f);
        SetFloat(material, ShaderUtilities.ID_UnderlayDilate, 0f);
        SetFloat(material, ShaderUtilities.ID_UnderlaySoftness, 0f);
        material.DisableKeyword(ShaderUtilities.Keyword_Underlay);
        material.DisableKeyword("UNDERLAY_INNER");

        if (material.HasProperty(ShaderUtilities.ID_FaceColor))
        {
            Color face = material.GetColor(ShaderUtilities.ID_FaceColor);
            face.a = 1f;
            material.SetColor(ShaderUtilities.ID_FaceColor, face);
        }
    }

    private static void SetFloat(Material material, int propertyId, float value)
    {
        if (material.HasProperty(propertyId))
        {
            material.SetFloat(propertyId, value);
        }
    }

    private static bool HasUnderlay(Material material)
    {
        return material.IsKeywordEnabled(ShaderUtilities.Keyword_Underlay) ||
               material.IsKeywordEnabled("UNDERLAY_INNER") ||
               GetFloat(material, ShaderUtilities.ID_UnderlayOffsetX) != 0f ||
               GetFloat(material, ShaderUtilities.ID_UnderlayOffsetY) != 0f ||
               GetFloat(material, ShaderUtilities.ID_UnderlayDilate) != 0f ||
               GetFloat(material, ShaderUtilities.ID_UnderlaySoftness) != 0f;
    }

    private static float GetFloat(Material material, int propertyId)
    {
        return material.HasProperty(propertyId) ? material.GetFloat(propertyId) : 0f;
    }

    private static bool IsOutlinedRole(TMP_Text text)
    {
        string objectPath = GetObjectPath(text.transform);
        if (objectPath.IndexOf("H1", StringComparison.OrdinalIgnoreCase) >= 0 ||
            objectPath.IndexOf("H2", StringComparison.OrdinalIgnoreCase) >= 0 ||
            objectPath.IndexOf("H3", StringComparison.OrdinalIgnoreCase) >= 0 ||
            objectPath.IndexOf("Title", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return true;
        }

        Transform current = text.transform;
        while (current != null)
        {
            if (current.GetComponent<Button>() != null ||
                current.name.IndexOf("Button", StringComparison.OrdinalIgnoreCase) >= 0 ||
                current.name.StartsWith("Btn", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private static string GetObjectPath(Transform transform)
    {
        Stack<string> names = new();
        Transform current = transform;
        while (current != null)
        {
            names.Push(current.name);
            current = current.parent;
        }

        return string.Join("/", names);
    }

    private static void EnsureMaterialFolder()
    {
        if (!AssetDatabase.IsValidFolder(GeneratedMaterialFolder))
        {
            AssetDatabase.CreateFolder("Assets/_Projects/Fonts", "Runtime Typography");
        }
    }

    private static string SanitizeFileName(string value)
    {
        foreach (char invalid in Path.GetInvalidFileNameChars())
        {
            value = value.Replace(invalid, '_');
        }

        return value;
    }

    private static void LogResult(string label, AuditResult result)
    {
        string summary =
            $"{label}: {result.TextCount} runtime TMP text(s), {result.FixedCount} fixed, " +
            $"{result.Errors.Count} validation error(s).";
        if (result.Errors.Count == 0)
        {
            Debug.Log(summary);
            return;
        }

        Debug.LogError(summary + "\n" + string.Join("\n", result.Errors));
    }
}
