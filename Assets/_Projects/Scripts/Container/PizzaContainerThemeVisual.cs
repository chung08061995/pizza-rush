using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Adds the approved pizza-box surface without changing container physics or level data.
/// </summary>
public sealed class PizzaContainerThemeVisual : MonoBehaviour
{
    private const string VisualRootName = "__PizzaContainerTheme";
    private const float SurfaceDrop = 0.155f;
    // Slightly overlap only internal joins so rasterization cannot expose hairline seams.
    private const float JoinOverlap = 0.006f;
    private const float LidTintStrength = 0.55f;
    private static readonly Color KraftLidColor = new(0.88f, 0.72f, 0.43f);
    private static readonly Dictionary<ColorType, Material> SignalMaterials = new();
    private static readonly Dictionary<ColorType, Material> LidMaterials = new();

    private Transform visualRoot;

    public void Apply(ContainerData data, IReadOnlyList<Vector2Int> occupiedCells)
    {
        Clear();
        var useLegacySurface = data == null ||
            data.containerMaterialType == ContainerMaterialType.Ice;
        SetLegacySurfaceVisible(useLegacySurface);
        if (useLegacySurface || occupiedCells == null || occupiedCells.Count == 0)
        {
            return;
        }

        visualRoot = new GameObject(VisualRootName).transform;
        visualRoot.SetParent(transform, false);
        visualRoot.gameObject.layer = gameObject.layer;

        EnsureProductionColors();
        var occupied = new HashSet<Vector2Int>(occupiedCells);
        var signalType = data.isStone ? ColorType.Gray : data.containerColorData.colorType;
        var signalMaterial = GetSignalMaterial(signalType);

        foreach (var cell in occupiedCells)
        {
            var connectedLeft = occupied.Contains(cell + Vector2Int.left);
            var connectedRight = occupied.Contains(cell + Vector2Int.right);
            var connectedDown = occupied.Contains(cell + Vector2Int.down);
            var connectedUp = occupied.Contains(cell + Vector2Int.up);
            var center = new Vector3(cell.x, 0f, cell.y);

            CreateConnectedCellCube("KraftLid", center, 0.285f - SurfaceDrop, 0.075f, 0.42f, 0.42f, 0f, 0f,
                connectedLeft, connectedRight, connectedDown, connectedUp, GetLidMaterial(signalType));

            CreateCube("VerticalStripe", center + Vector3.up * (0.350f - SurfaceDrop),
                new Vector3(0.09f, 0.016f, 0.50f), signalMaterial);
            CreateCube("HorizontalStripe", center + Vector3.up * (0.350f - SurfaceDrop),
                new Vector3(0.50f, 0.016f, 0.09f), signalMaterial);
            CreateCube("StripeCenter", center + Vector3.up * (0.370f - SurfaceDrop),
                new Vector3(0.09f, 0.008f, 0.09f), signalMaterial);
        }
    }

    private void CreateConnectedCellCube(
        string objectName,
        Vector3 cellCenter,
        float localY,
        float height,
        float exposedHalfExtentX,
        float exposedHalfExtentZ,
        float fixedXScale,
        float fixedZScale,
        bool connectedLeft,
        bool connectedRight,
        bool connectedDown,
        bool connectedUp,
        Material material)
    {
        var minX = connectedLeft ? -0.5f - JoinOverlap : -exposedHalfExtentX;
        var maxX = connectedRight ? 0.5f + JoinOverlap : exposedHalfExtentX;
        var minZ = connectedDown ? -0.5f - JoinOverlap : -exposedHalfExtentZ;
        var maxZ = connectedUp ? 0.5f + JoinOverlap : exposedHalfExtentZ;
        if (fixedXScale > 0f)
        {
            minX = -fixedXScale * 0.5f;
            maxX = fixedXScale * 0.5f;
        }
        if (fixedZScale > 0f)
        {
            minZ = -fixedZScale * 0.5f;
            maxZ = fixedZScale * 0.5f;
        }
        var offset = new Vector3((minX + maxX) * 0.5f, localY, (minZ + maxZ) * 0.5f);
        var scale = new Vector3(maxX - minX, height, maxZ - minZ);

        CreateCube(objectName, cellCenter + offset, scale, material);
    }

    private void SetLegacySurfaceVisible(bool visible)
    {
        SetRenderersVisible(transform.Find("Color"), visible);
        SetRenderersVisible(transform.Find("NoAsign"), visible);
    }

    private static void SetRenderersVisible(Transform root, bool visible)
    {
        if (root == null) return;
        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            renderer.enabled = visible;
        }
    }

    private static void EnsureProductionColors()
    {
        var colors = DataManager.Instance == null
            ? null
            : DataManager.Instance.ProductionLineColorsSO;
        if (colors != null && colors.Dictionary.Count == 0)
        {
            colors.BuildDictionary();
        }
    }

    private void Clear()
    {
        if (visualRoot == null)
        {
            visualRoot = transform.Find(VisualRootName);
        }
        if (visualRoot == null) return;

        if (Application.isPlaying) Destroy(visualRoot.gameObject);
        else DestroyImmediate(visualRoot.gameObject);
        visualRoot = null;
    }

    private void CreateCube(string objectName, Vector3 localPosition, Vector3 localScale, Material material)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = objectName;
        cube.layer = gameObject.layer;
        cube.transform.SetParent(visualRoot, false);
        cube.transform.localPosition = localPosition;
        cube.transform.localScale = localScale;
        var collider = cube.GetComponent<Collider>();
        if (Application.isPlaying) Destroy(collider);
        else DestroyImmediate(collider);
        var renderer = cube.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        if (objectName == "KraftLid" || objectName == "BoxBase")
        {
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }

    private static Material GetLidMaterial(ColorType colorType)
    {
        if (LidMaterials.TryGetValue(colorType, out var material) && material != null) return material;

        if (!DataManager.Instance.ProductionLineColorsSO.TryGetValue(colorType, out var signalColor))
        {
            signalColor = KraftLidColor;
        }
        var tintedLidColor = Color.Lerp(KraftLidColor, signalColor, LidTintStrength);
        material = CreateMaterial($"Pizza Box Tinted Lid {colorType}", tintedLidColor);
        LidMaterials[colorType] = material;
        return material;
    }

    private static Material GetSignalMaterial(ColorType colorType)
    {
        if (SignalMaterials.TryGetValue(colorType, out var material) && material != null) return material;
        var color = Color.white;
        if (!DataManager.Instance.ProductionLineColorsSO.TryGetValue(colorType, out color))
        {
            color = Color.white;
        }
        material = CreateMaterial($"Pizza Box Signal {colorType}", color);
        SignalMaterials[colorType] = material;
        return material;
    }

    private static Material CreateMaterial(string materialName, Color color)
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var material = new Material(shader) { name = materialName, color = color };
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.28f);
        return material;
    }
}
