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
    private const float CornerBracketY = 0.241f;
    private const float CornerBracketEdge = 0.39f;
    private const float CornerBracketInset = 0.09f;
    private const float CornerBracketLength = 0.22f;
    private const float CornerBracketWidth = 0.045f;
    private static readonly Color KraftLidColor = new(0.88f, 0.72f, 0.43f);
    private static readonly Color CornerBracketColor = new(0.34f, 0.14f, 0.055f);
    private static readonly Dictionary<ColorType, Material> SignalMaterials = new();
    private static readonly Dictionary<ColorType, Material> LidMaterials = new();
    private static Material cornerBracketMaterial;

    private Transform visualRoot;
    private readonly List<Mesh> generatedMeshes = new();

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

            CreateConnectedCellCube("KraftLid", center, 0.285f - SurfaceDrop, 0.075f, 0.42f, 0.42f, 0.95f, 0.95f,
                connectedLeft, connectedRight, connectedDown, connectedUp, GetLidMaterial(signalType));

            CreateCube("VerticalStripe", center + Vector3.up * (0.350f - SurfaceDrop),
                new Vector3(0.09f, 0.016f, 0.50f), signalMaterial);
            CreateCube("HorizontalStripe", center + Vector3.up * (0.350f - SurfaceDrop),
                new Vector3(0.50f, 0.016f, 0.09f), signalMaterial);
            CreateCube("StripeCenter", center + Vector3.up * (0.370f - SurfaceDrop),
                new Vector3(0.09f, 0.008f, 0.09f), signalMaterial);
        }

        CreateItemCornerBrackets(occupiedCells);
    }

    private void CreateItemCornerBrackets(IReadOnlyList<Vector2Int> occupiedCells)
    {
        var occupied = new HashSet<Vector2Int>(occupiedCells);
        var vertices = new List<Vector3>(32);
        var triangles = new List<int>(48);
        foreach (Vector2Int cell in occupiedCells)
        {
            bool leftExposed = !occupied.Contains(cell + Vector2Int.left);
            bool rightExposed = !occupied.Contains(cell + Vector2Int.right);
            bool bottomExposed = !occupied.Contains(cell + Vector2Int.down);
            bool topExposed = !occupied.Contains(cell + Vector2Int.up);

            if (leftExposed && bottomExposed)
            {
                AddCornerBracket(
                    vertices, triangles, cell, -1f, -1f);
            }
            if (rightExposed && bottomExposed)
            {
                AddCornerBracket(
                    vertices, triangles, cell, 1f, -1f);
            }
            if (leftExposed && topExposed)
            {
                AddCornerBracket(
                    vertices, triangles, cell, -1f, 1f);
            }
            if (rightExposed && topExposed)
            {
                AddCornerBracket(
                    vertices, triangles, cell, 1f, 1f);
            }
        }

        var bracketObject = new GameObject("ItemCornerBrackets");
        bracketObject.layer = gameObject.layer;
        bracketObject.transform.SetParent(visualRoot, false);
        var mesh = new Mesh
        {
            name = "Pizza Box Item Corner Brackets Mesh",
            hideFlags = HideFlags.DontSave
        };
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
        generatedMeshes.Add(mesh);

        bracketObject.AddComponent<MeshFilter>().sharedMesh = mesh;
        var renderer = bracketObject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = GetCornerBracketMaterial();
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        renderer.motionVectorGenerationMode =
            UnityEngine.MotionVectorGenerationMode.ForceNoMotion;
    }

    private static void AddCornerBracket(
        List<Vector3> vertices,
        List<int> triangles,
        Vector2Int cell,
        float edgeX,
        float edgeZ)
    {
        Vector3 corner = new(
            cell.x + edgeX * CornerBracketEdge,
            CornerBracketY,
            cell.y + edgeZ * CornerBracketEdge);
        float inwardX = -edgeX;
        float inwardZ = -edgeZ;
        AddTopQuad(
            vertices,
            triangles,
            corner + Vector3.right * inwardX * CornerBracketInset,
            CornerBracketLength * 0.5f,
            CornerBracketWidth * 0.5f);
        AddTopQuad(
            vertices,
            triangles,
            corner + Vector3.forward * inwardZ * CornerBracketInset,
            CornerBracketWidth * 0.5f,
            CornerBracketLength * 0.5f);
    }

    private static void AddTopQuad(
        List<Vector3> vertices,
        List<int> triangles,
        Vector3 center,
        float halfWidth,
        float halfDepth)
    {
        int index = vertices.Count;
        vertices.Add(center + new Vector3(-halfWidth, 0f, -halfDepth));
        vertices.Add(center + new Vector3(-halfWidth, 0f, halfDepth));
        vertices.Add(center + new Vector3(halfWidth, 0f, halfDepth));
        vertices.Add(center + new Vector3(halfWidth, 0f, -halfDepth));
        triangles.Add(index);
        triangles.Add(index + 1);
        triangles.Add(index + 2);
        triangles.Add(index);
        triangles.Add(index + 2);
        triangles.Add(index + 3);
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
        DestroyGeneratedMeshes();
        if (visualRoot == null)
        {
            visualRoot = transform.Find(VisualRootName);
        }
        if (visualRoot == null) return;

        if (Application.isPlaying) Destroy(visualRoot.gameObject);
        else DestroyImmediate(visualRoot.gameObject);
        visualRoot = null;
    }

    private void OnDestroy()
    {
        DestroyGeneratedMeshes();
    }

    private void DestroyGeneratedMeshes()
    {
        foreach (Mesh mesh in generatedMeshes)
        {
            if (mesh == null) continue;
            if (Application.isPlaying) Destroy(mesh);
            else DestroyImmediate(mesh);
        }
        generatedMeshes.Clear();
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

    private static Material GetCornerBracketMaterial()
    {
        if (cornerBracketMaterial != null)
        {
            return cornerBracketMaterial;
        }

        var shader = Shader.Find("Universal Render Pipeline/Unlit") ??
                     Shader.Find("Universal Render Pipeline/Lit") ??
                     Shader.Find("Standard");
        cornerBracketMaterial = new Material(shader)
        {
            name = "Pizza Box Item Corner Brackets",
            color = CornerBracketColor
        };
        if (cornerBracketMaterial.HasProperty("_BaseColor"))
        {
            cornerBracketMaterial.SetColor("_BaseColor", CornerBracketColor);
        }
        return cornerBracketMaterial;
    }

    private static Material CreateMaterial(string materialName, Color color)
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var material = new Material(shader) { name = materialName, color = color };
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.28f);
        return material;
    }
}
