using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Builds the approved midpoint-colored pizza rail and required-color contact lamp.
/// Visual geometry is collider-free and follows the live production queue.
/// </summary>
public sealed class PizzaProductionLineThemeVisual : MonoBehaviour
{
    private const string VisualRootName = "__PizzaProductionLineTheme";
    private static readonly Dictionary<ColorType, Material> RailMaterials = new();
    private ProductionLine productionLine;
    private bool refreshRequested;
    private Transform visualRoot;

    public void Initialize(ProductionLine owner)
    {
        productionLine = owner;
        RequestRefresh();
    }

    public void RequestRefresh()
    {
        refreshRequested = true;
    }

    private void LateUpdate()
    {
        if (!refreshRequested) return;
        refreshRequested = false;
        Rebuild();
    }

    private void Rebuild()
    {
        Clear();
        if (productionLine == null) productionLine = GetComponent<ProductionLine>();
        if (productionLine == null) return;

        var productions = productionLine.ProductionPooler.ActiveItems
            .Where(item => item != null && item.gameObject.activeInHierarchy)
            .OrderBy(item => item.CurrentIndex)
            .ToList();
        if (productions.Count == 0) return;

        visualRoot = new GameObject(VisualRootName).transform;
        visualRoot.SetParent(transform, false);
        visualRoot.gameObject.layer = gameObject.layer;

        CreateRailRuns(productions);
        CreateContactLamp(productions[0].ColorType);
    }

    private void CreateRailRuns(IReadOnlyList<Production> productions)
    {
        var start = 0;
        while (start < productions.Count)
        {
            var end = start;
            while (end + 1 < productions.Count && productions[end + 1].ColorType == productions[start].ColorType)
            {
                end++;
            }

            var firstPosition = productions[start].transform.position;
            var lastPosition = productions[end].transform.position;
            var direction = GetLineDirection(productions, start, end);
            var spacing = GetSpacing(productions, start, end);
            var runStart = start > 0
                ? Vector3.Lerp(productions[start - 1].transform.position, firstPosition, 0.5f)
                : firstPosition - direction * spacing * 0.5f;
            var runEnd = end + 1 < productions.Count
                ? Vector3.Lerp(lastPosition, productions[end + 1].transform.position, 0.5f)
                : lastPosition + direction * spacing * 0.5f;

            CreateRailSegment(runStart, runEnd, productions[start].ColorType);
            start = end + 1;
        }
    }

    private static Vector3 GetLineDirection(IReadOnlyList<Production> productions, int start, int end)
    {
        Vector3 delta;
        if (end > start) delta = productions[end].transform.position - productions[start].transform.position;
        else if (start + 1 < productions.Count) delta = productions[start + 1].transform.position - productions[start].transform.position;
        else if (start > 0) delta = productions[start].transform.position - productions[start - 1].transform.position;
        else delta = Vector3.forward;

        delta.y = 0f;
        if (delta.sqrMagnitude < 0.0001f) return Vector3.forward;
        return Mathf.Abs(delta.x) > Mathf.Abs(delta.z)
            ? new Vector3(Mathf.Sign(delta.x), 0f, 0f)
            : new Vector3(0f, 0f, Mathf.Sign(delta.z));
    }

    private static float GetSpacing(IReadOnlyList<Production> productions, int start, int end)
    {
        if (end > start) return Vector3.Distance(productions[start].transform.position, productions[start + 1].transform.position);
        if (start + 1 < productions.Count) return Vector3.Distance(productions[start].transform.position, productions[start + 1].transform.position);
        if (start > 0) return Vector3.Distance(productions[start - 1].transform.position, productions[start].transform.position);
        return 0.5f;
    }

    private void CreateRailSegment(Vector3 start, Vector3 end, ColorType colorType)
    {
        var delta = end - start;
        delta.y = 0f;
        var length = delta.magnitude;
        if (length < 0.05f) return;

        var center = (start + end) * 0.5f;
        center.y = 0.011f;
        var alongX = Mathf.Abs(delta.x) > Mathf.Abs(delta.z);
        var scale = alongX
            ? new Vector3(length + 0.025f, 0.016f, 0.34f)
            : new Vector3(0.34f, 0.016f, length + 0.025f);
        CreateCube($"Rail_{colorType}", center, scale, GetRailMaterial(colorType), true);
    }

    private void CreateContactLamp(ColorType requiredColor)
    {
        var bridge = FindDescendant(transform, "Brigde") ?? FindDescendant(transform, "Bridge");
        if (bridge == null) return;

        var renderers = bridge.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return;
        var bounds = renderers[0].bounds;
        for (var i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);

        var scale = new Vector3(Mathf.Max(0.12f, bounds.size.x * 1.18f), 0.055f,
            Mathf.Max(0.12f, bounds.size.z * 1.18f));
        var center = new Vector3(bounds.center.x, bounds.max.y + 0.012f, bounds.center.z);
        CreateCube($"ContactLamp_{requiredColor}", center, scale, GetRailMaterial(requiredColor), true);
    }

    private void CreateCube(string objectName, Vector3 position, Vector3 scale, Material material, bool worldSpace)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = objectName;
        cube.layer = gameObject.layer;
        cube.transform.SetParent(visualRoot, worldSpace);
        cube.transform.position = position;
        cube.transform.rotation = Quaternion.identity;
        cube.transform.localScale = scale;
        var collider = cube.GetComponent<Collider>();
        if (Application.isPlaying) Destroy(collider);
        else DestroyImmediate(collider);
        cube.GetComponent<Renderer>().sharedMaterial = material;
    }

    private static Transform FindDescendant(Transform parent, string targetName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == targetName) return child;
            var found = FindDescendant(child, targetName);
            if (found != null) return found;
        }
        return null;
    }

    private static Material GetRailMaterial(ColorType colorType)
    {
        if (RailMaterials.TryGetValue(colorType, out var material) && material != null) return material;
        var color = Color.white;
        DataManager.Instance.ProductionLineColorsSO.TryGetValue(colorType, out color);
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        material = new Material(shader) { name = $"Pizza Rail {colorType}", color = color };
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.38f);
        if (material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 0.12f);
        }
        RailMaterials[colorType] = material;
        return material;
    }

    private void Clear()
    {
        if (visualRoot == null) visualRoot = transform.Find(VisualRootName);
        if (visualRoot == null) return;
        if (Application.isPlaying) Destroy(visualRoot.gameObject);
        else DestroyImmediate(visualRoot.gameObject);
        visualRoot = null;
    }
}
