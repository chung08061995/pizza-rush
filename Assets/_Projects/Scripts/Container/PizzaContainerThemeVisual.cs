using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Adds the approved premium pizza-box surface without changing container
/// transforms, colliders, level data, or serialized prefab contracts.
/// </summary>
public sealed class PizzaContainerThemeVisual : MonoBehaviour
{
    private const string VisualRootName = "__PizzaContainerTheme";
    private const float LidTopY = 0.184f;
    private const float SideBottomY = 0.12f;
    private const float OuterInset = 0.045f;
    private const float CornerChamfer = 0.095f;
    private const float SeamWidth = 0.01f;
    private const float SeamEndInset = 0.105f;
    private const float OuterRimWidth = 0.032f;
    private const float CornerDimpleInset = 0.085f;
    private const float CornerDimpleRadius = 0.034f;
    private const float StampOuterRadius = 0.155f;
    private const float StampInnerRadius = 0.115f;
    private const float StampCoreRadius = 0.035f;
    private const float LidColorStrength = 0.92f;

    private static readonly Color KraftLidColor = new(0.88f, 0.72f, 0.43f, 1f);
    private static readonly Dictionary<ColorType, Material> LidMaterials = new();
    private static readonly Dictionary<ColorType, Material> SideMaterials = new();
    private static readonly Dictionary<ColorType, Material> RimMaterials = new();
    private static readonly Dictionary<ColorType, Material> SeamMaterials = new();
    private static readonly Dictionary<ColorType, Material> MarkerMaterials = new();
    private static readonly Dictionary<ColorType, Material> StampInnerMaterials = new();
    private static readonly Dictionary<ColorType, Material> StampCoreMaterials = new();
    private readonly List<Mesh> generatedMeshes = new();
    private Transform visualRoot;
    private Tween interactionTween;

    public void Apply(ContainerData data, IReadOnlyList<Vector2Int> occupiedCells)
    {
        Clear();
        bool useLegacySurface = data == null ||
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
        ColorType primaryColor = GetPrimaryColor(data);
        ColorType lidColor = data.containerColorData != null &&
                             data.containerColorData.isMultiColor
            ? ColorType.None
            : primaryColor;

        AddMeshObject(
            "PremiumBox_Lid",
            PizzaBoxFootprintMeshBuilder.BuildLid(
                occupiedCells, LidTopY, OuterInset, CornerChamfer),
            GetLidMaterial(lidColor));
        AddMeshObject(
            "PremiumBox_ColorSide",
            PizzaBoxFootprintMeshBuilder.BuildOuterSide(
                occupiedCells, LidTopY, SideBottomY, OuterInset, CornerChamfer),
            GetSideMaterial(lidColor));
        AddMeshObject(
            "PremiumBox_OuterRim",
            PizzaBoxFootprintMeshBuilder.BuildOuterRim(
                occupiedCells,
                LidTopY + 0.004f,
                OuterInset,
                CornerChamfer,
                OuterRimWidth),
            GetRimMaterial(lidColor));

        AddMeshObject(
            "PremiumBox_CornerDimples",
            PizzaBoxFootprintMeshBuilder.BuildOuterCornerDimples(
                occupiedCells,
                LidTopY + 0.007f,
                OuterInset,
                CornerDimpleInset,
                CornerDimpleRadius),
            GetSeamMaterial(primaryColor));

        if (occupiedCells.Count > 1)
        {
            AddMeshObject(
                "PremiumBox_PressedSeams",
                PizzaBoxFootprintMeshBuilder.BuildInternalSeams(
                    occupiedCells,
                    LidTopY + 0.002f,
                    SeamWidth,
                    SeamEndInset),
                GetSeamMaterial(primaryColor));
        }

        foreach (KeyValuePair<ColorType, List<Vector2Int>> markerGroup in
                 BuildMarkerGroups(data, occupiedCells))
        {
            AddMeshObject(
                $"PremiumBox_StampOuter_{markerGroup.Key}",
                PizzaBoxFootprintMeshBuilder.BuildRoundStampLayer(
                    markerGroup.Value,
                    LidTopY + 0.008f,
                    StampOuterRadius),
                GetMarkerMaterial(markerGroup.Key));
            AddMeshObject(
                $"PremiumBox_StampInner_{markerGroup.Key}",
                PizzaBoxFootprintMeshBuilder.BuildRoundStampLayer(
                    markerGroup.Value,
                    LidTopY + 0.010f,
                    StampInnerRadius),
                GetStampInnerMaterial(markerGroup.Key));
            AddMeshObject(
                $"PremiumBox_StampCore_{markerGroup.Key}",
                PizzaBoxFootprintMeshBuilder.BuildRoundStampLayer(
                    markerGroup.Value,
                    LidTopY + 0.012f,
                    StampCoreRadius),
                GetStampCoreMaterial(markerGroup.Key));
        }
        AlignPizzaLandingSlots(occupiedCells);
    }

    /// <summary>
    /// Raises the visual hierarchy slightly in scale while the player is
    /// dragging this container.  This is presentation-only; the gameplay
    /// transform and colliders remain owned by Container/state machine code.
    /// </summary>
    public void BeginInteraction()
    {
        if (visualRoot == null)
        {
            return;
        }

        interactionTween?.Kill();
        interactionTween = visualRoot
            .DOScale(Vector3.one * 1.035f, 0.12f)
            .SetEase(Ease.OutQuad);
    }

    public void EndInteraction(bool accepted)
    {
        if (visualRoot == null)
        {
            return;
        }

        interactionTween?.Kill();
        var sequence = DOTween.Sequence();
        if (accepted)
        {
            sequence.Append(visualRoot.DOScale(Vector3.one * 1.055f, 0.06f));
        }
        sequence.Append(visualRoot.DOScale(Vector3.one, accepted ? 0.16f : 0.12f))
            .SetEase(Ease.OutQuad);
        interactionTween = sequence;
    }

    public void ShowHintPulse()
    {
        if (visualRoot == null)
        {
            return;
        }

        interactionTween?.Kill();
        visualRoot.localScale = Vector3.one;
        interactionTween = visualRoot
            .DOPunchScale(Vector3.one * 0.035f, 0.5f, 2, 0.35f)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject, LinkBehaviour.KillOnDisable);
    }

    private static Dictionary<ColorType, List<Vector2Int>> BuildMarkerGroups(
        ContainerData data,
        IReadOnlyList<Vector2Int> occupiedCells)
    {
        var groups = new Dictionary<ColorType, List<Vector2Int>>();
        ColorType primaryColor = GetPrimaryColor(data);
        ContainerColorData colorData = data.containerColorData;
        if (data.isStone || colorData == null || !colorData.isMultiColor ||
            colorData.colors == null || colorData.colors.Count < 2)
        {
            groups[primaryColor] = new List<Vector2Int>(occupiedCells);
            return groups;
        }

        int cellIndex = 0;
        for (int colorIndex = 0;
             colorIndex < colorData.colors.Count && cellIndex < occupiedCells.Count;
             colorIndex++)
        {
            ColorType color = colorData.colors[colorIndex];
            int remainingColors = colorData.colors.Count - colorIndex;
            int remainingCells = occupiedCells.Count - cellIndex;
            int cellsForColor = Mathf.Max(1, remainingCells / remainingColors);
            if (colorData.colorAmounts != null && colorIndex < colorData.colorAmounts.Count)
            {
                cellsForColor = Mathf.Max(
                    1,
                    Mathf.RoundToInt(colorData.colorAmounts[colorIndex] / 4f));
            }

            for (int count = 0;
                 count < cellsForColor && cellIndex < occupiedCells.Count;
                 count++, cellIndex++)
            {
                AddMarkerCell(groups, color, occupiedCells[cellIndex]);
            }
        }

        while (cellIndex < occupiedCells.Count)
        {
            AddMarkerCell(groups, primaryColor, occupiedCells[cellIndex]);
            cellIndex++;
        }
        return groups;
    }

    private static void AddMarkerCell(
        Dictionary<ColorType, List<Vector2Int>> groups,
        ColorType color,
        Vector2Int cell)
    {
        if (!groups.TryGetValue(color, out List<Vector2Int> cells))
        {
            cells = new List<Vector2Int>();
            groups[color] = cells;
        }
        cells.Add(cell);
    }

    private static ColorType GetPrimaryColor(ContainerData data)
    {
        if (data == null)
        {
            return ColorType.None;
        }
        if (data.isStone)
        {
            return ColorType.Gray;
        }
        return data.containerColorData == null
            ? ColorType.None
            : data.containerColorData.colorType;
    }

    private void AddMeshObject(string objectName, Mesh mesh, Material material)
    {
        if (mesh == null || mesh.vertexCount == 0)
        {
            DestroyGeneratedObject(mesh);
            return;
        }

        generatedMeshes.Add(mesh);
        var meshObject = new GameObject(objectName);
        meshObject.layer = gameObject.layer;
        meshObject.transform.SetParent(visualRoot, false);
        meshObject.AddComponent<MeshFilter>().sharedMesh = mesh;
        var renderer = meshObject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.lightProbeUsage = LightProbeUsage.Off;
        renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
    }

    private void AlignPizzaLandingSlots(IReadOnlyList<Vector2Int> occupiedCells)
    {
        var landingRoots = new List<Transform>();
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            if (child.name == "PizzeCompleted")
            {
                landingRoots.Add(child);
            }
        }

        var remainingCells = new List<Vector2Int>(occupiedCells);
        foreach (Transform landingRoot in landingRoots)
        {
            if (landingRoot.childCount == 0 || remainingCells.Count == 0)
            {
                continue;
            }

            Vector3 slotCenter = Vector3.zero;
            for (int i = 0; i < landingRoot.childCount; i++)
            {
                slotCenter += transform.InverseTransformPoint(landingRoot.GetChild(i).position);
            }
            slotCenter /= landingRoot.childCount;

            int nearestIndex = 0;
            float nearestDistance = float.PositiveInfinity;
            for (int i = 0; i < remainingCells.Count; i++)
            {
                Vector2Int cell = remainingCells[i];
                float offsetX = cell.x - slotCenter.x;
                float offsetZ = cell.y - slotCenter.z;
                float distance = offsetX * offsetX + offsetZ * offsetZ;
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestIndex = i;
                }
            }

            Vector2Int targetCell = remainingCells[nearestIndex];
            var alignmentOffset = new Vector3(
                targetCell.x - slotCenter.x,
                0f,
                targetCell.y - slotCenter.z);
            landingRoot.position += transform.TransformVector(alignmentOffset);
            remainingCells.RemoveAt(nearestIndex);
        }
    }

    private void SetLegacySurfaceVisible(bool visible)
    {
        SetRenderersVisible(transform.Find("Color"), visible);
        SetRenderersVisible(transform.Find("NoAsign"), visible);
    }

    private static void SetRenderersVisible(Transform root, bool visible)
    {
        if (root == null)
        {
            return;
        }
        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            renderer.enabled = visible;
        }
    }

    private static void EnsureProductionColors()
    {
        ColorsSO colors = DataManager.Instance == null
            ? null
            : DataManager.Instance.ProductionLineColorsSO;
        if (colors != null && colors.Dictionary.Count == 0)
        {
            colors.BuildDictionary();
        }
    }

    private static Color GetSignalColor(ColorType colorType)
    {
        if (DataManager.Instance != null &&
            DataManager.Instance.ProductionLineColorsSO != null &&
            DataManager.Instance.ProductionLineColorsSO.TryGetValue(colorType, out Color color))
        {
            return color;
        }
        return colorType == ColorType.None ? KraftLidColor : Color.white;
    }

    private static Material GetLidMaterial(ColorType colorType)
    {
        if (LidMaterials.TryGetValue(colorType, out Material material) && material != null)
        {
            return material;
        }
        Color color = GetLidColor(colorType);
        material = CreateMaterial($"Premium Pizza Box Lid {colorType}", color, 0.3f);
        LidMaterials[colorType] = material;
        return material;
    }

    private static Material GetSideMaterial(ColorType colorType)
    {
        if (SideMaterials.TryGetValue(colorType, out Material material) && material != null)
        {
            return material;
        }
        Color color = ScaleRgb(GetLidColor(colorType), 0.82f);
        material = CreateMaterial($"Premium Pizza Box Side {colorType}", color, 0.18f);
        SideMaterials[colorType] = material;
        return material;
    }

    private static Material GetRimMaterial(ColorType colorType)
    {
        if (RimMaterials.TryGetValue(colorType, out Material material) && material != null)
        {
            return material;
        }
        Color color = Color.Lerp(GetLidColor(colorType), Color.white, 0.1f);
        material = CreateMaterial($"Premium Pizza Box Bevel {colorType}", color, 0.32f);
        RimMaterials[colorType] = material;
        return material;
    }

    private static Material GetSeamMaterial(ColorType colorType)
    {
        if (SeamMaterials.TryGetValue(colorType, out Material material) && material != null)
        {
            return material;
        }
        Color color = ScaleRgb(GetLidColor(colorType), 0.88f);
        material = CreateMaterial($"Premium Pizza Box Seam {colorType}", color, 0.14f);
        SeamMaterials[colorType] = material;
        return material;
    }

    private static Material GetMarkerMaterial(ColorType colorType)
    {
        if (MarkerMaterials.TryGetValue(colorType, out Material material) && material != null)
        {
            return material;
        }
        Color baseColor = GetSignalColor(colorType);
        float luminance = baseColor.r * 0.2126f + baseColor.g * 0.7152f + baseColor.b * 0.0722f;
        Color color = luminance > 0.58f
            ? ScaleRgb(baseColor, 0.76f)
            : Color.Lerp(baseColor, Color.white, 0.34f);
        material = CreateMaterial($"Premium Pizza Box Marker {colorType}", color, 0.34f);
        MarkerMaterials[colorType] = material;
        return material;
    }

    private static Material GetStampInnerMaterial(ColorType colorType)
    {
        if (StampInnerMaterials.TryGetValue(colorType, out Material material) && material != null)
        {
            return material;
        }
        Color color = Color.Lerp(GetLidColor(colorType), Color.white, 0.24f);
        material = CreateMaterial($"Premium Pizza Box Stamp Inner {colorType}", color, 0.24f);
        StampInnerMaterials[colorType] = material;
        return material;
    }

    private static Material GetStampCoreMaterial(ColorType colorType)
    {
        if (StampCoreMaterials.TryGetValue(colorType, out Material material) && material != null)
        {
            return material;
        }
        Color warmKraft = new(0.64f, 0.31f, 0.12f, 1f);
        Color color = Color.Lerp(GetLidColor(colorType), warmKraft, 0.24f);
        material = CreateMaterial($"Premium Pizza Box Stamp Core {colorType}", color, 0.2f);
        StampCoreMaterials[colorType] = material;
        return material;
    }

    private static Color GetLidColor(ColorType colorType)
    {
        return colorType == ColorType.None
            ? KraftLidColor
            : Color.Lerp(KraftLidColor, GetSignalColor(colorType), LidColorStrength);
    }

    private static Color ScaleRgb(Color color, float multiplier)
    {
        return new Color(
            color.r * multiplier,
            color.g * multiplier,
            color.b * multiplier,
            color.a);
    }

    private static Material CreateMaterial(string materialName, Color color, float smoothness)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var material = new Material(shader)
        {
            name = materialName,
            color = color,
            hideFlags = HideFlags.DontSave
        };
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }
        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", smoothness);
        }
        if (material.HasProperty("_Metallic"))
        {
            material.SetFloat("_Metallic", 0f);
        }
        return material;
    }

    private void Clear()
    {
        DestroyGeneratedMeshes();
        if (visualRoot == null)
        {
            visualRoot = transform.Find(VisualRootName);
        }
        if (visualRoot == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(visualRoot.gameObject);
        }
        else
        {
            DestroyImmediate(visualRoot.gameObject);
        }
        visualRoot = null;
    }

    private void OnDestroy()
    {
        interactionTween?.Kill();
        DestroyGeneratedMeshes();
    }

    private void DestroyGeneratedMeshes()
    {
        foreach (Mesh mesh in generatedMeshes)
        {
            DestroyGeneratedObject(mesh);
        }
        generatedMeshes.Clear();
    }

    private static void DestroyGeneratedObject(Object generatedObject)
    {
        if (generatedObject == null)
        {
            return;
        }
        if (Application.isPlaying)
        {
            Destroy(generatedObject);
        }
        else
        {
            DestroyImmediate(generatedObject);
        }
    }
}
