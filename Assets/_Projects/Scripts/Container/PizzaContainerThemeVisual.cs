using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Adds the approved pizza-box surface without changing container physics or level data.
/// </summary>
public sealed class PizzaContainerThemeVisual : MonoBehaviour
{
    private const string VisualRootName = "__PizzaContainerTheme";
    private const float SurfaceDrop = 0.155f;
    private const float PerimeterHeight = 0.12f;
    private const float PerimeterThickness = 0.06f;
    private const float LidTintStrength = 0.55f;
    private static readonly Color KraftLidColor = new(0.88f, 0.72f, 0.43f);
    private static readonly Dictionary<ColorType, Material> SignalMaterials = new();
    private static readonly Dictionary<ColorType, Material> LidMaterials = new();
    private static Material baseMaterial;

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

        var occupied = new HashSet<Vector2Int>(occupiedCells);
        var signalType = data.isStone ? ColorType.Gray : data.containerColorData.colorType;
        var signalMaterial = GetSignalMaterial(signalType);

        foreach (var cell in occupiedCells)
        {
            var connectedX = occupied.Contains(cell + Vector2Int.left) || occupied.Contains(cell + Vector2Int.right);
            var connectedZ = occupied.Contains(cell + Vector2Int.up) || occupied.Contains(cell + Vector2Int.down);
            var center = new Vector3(cell.x, 0f, cell.y);

            CreateCube("BoxBase", center + Vector3.up * (0.215f - SurfaceDrop),
                new Vector3(connectedX ? 1.02f : 0.90f, 0.10f, connectedZ ? 1.02f : 0.90f),
                GetBaseMaterial());
            CreateCube("KraftLid", center + Vector3.up * (0.285f - SurfaceDrop),
                new Vector3(connectedX ? 1.02f : 0.84f, 0.075f, connectedZ ? 1.02f : 0.84f),
                GetLidMaterial(signalType));

            CreateCube("VerticalStripe", center + Vector3.up * (0.350f - SurfaceDrop),
                new Vector3(0.09f, 0.016f, 0.50f), signalMaterial);
            CreateCube("HorizontalStripe", center + Vector3.up * (0.350f - SurfaceDrop),
                new Vector3(0.50f, 0.016f, 0.09f), signalMaterial);
            CreateCube("StripeCenter", center + Vector3.up * (0.370f - SurfaceDrop),
                new Vector3(0.09f, 0.008f, 0.09f), signalMaterial);

            CreateItemPerimeter(cell, center, occupied, connectedX, connectedZ);
        }
    }

    private void CreateItemPerimeter(
        Vector2Int cell,
        Vector3 center,
        HashSet<Vector2Int> occupied,
        bool connectedX,
        bool connectedZ)
    {
        var perimeterY = 0.145f;
        var xEdgeOffset = connectedX ? 0.48f : 0.45f;
        var zEdgeOffset = connectedZ ? 0.48f : 0.45f;
        var horizontalLength = connectedX ? 1.02f : 0.90f;
        var verticalLength = connectedZ ? 1.02f : 0.90f;
        var material = GetBaseMaterial();

        // Only exposed edges get a rail. Shared edges between cells remain open,
        // so the brown frame reads as one perimeter around the complete item.
        if (!occupied.Contains(cell + Vector2Int.left))
        {
            CreateCube("ItemPerimeter_Left",
                center + new Vector3(-xEdgeOffset, perimeterY, 0f),
                new Vector3(PerimeterThickness, PerimeterHeight, verticalLength), material);
        }
        if (!occupied.Contains(cell + Vector2Int.right))
        {
            CreateCube("ItemPerimeter_Right",
                center + new Vector3(xEdgeOffset, perimeterY, 0f),
                new Vector3(PerimeterThickness, PerimeterHeight, verticalLength), material);
        }
        if (!occupied.Contains(cell + Vector2Int.down))
        {
            CreateCube("ItemPerimeter_Bottom",
                center + new Vector3(0f, perimeterY, -zEdgeOffset),
                new Vector3(horizontalLength, PerimeterHeight, PerimeterThickness), material);
        }
        if (!occupied.Contains(cell + Vector2Int.up))
        {
            CreateCube("ItemPerimeter_Top",
                center + new Vector3(0f, perimeterY, zEdgeOffset),
                new Vector3(horizontalLength, PerimeterHeight, PerimeterThickness), material);
        }
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
        cube.GetComponent<Renderer>().sharedMaterial = material;
    }

    private static Material GetBaseMaterial() =>
        baseMaterial != null ? baseMaterial : baseMaterial = CreateMaterial("Pizza Box Base", new Color(0.63f, 0.35f, 0.14f));

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
        DataManager.Instance.ProductionLineColorsSO.TryGetValue(colorType, out color);
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
