using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Builds the approved midpoint-colored pizza rail and required-color contact lamp.
/// Visual geometry is collider-free and follows the live production queue.
/// </summary>
public sealed class PizzaProductionLineThemeVisual : MonoBehaviour
{
    private struct RailPiece
    {
        public Vector3 start;
        public Vector3 end;
        public ColorType colorType;

        public RailPiece(Vector3 start, Vector3 end, ColorType colorType)
        {
            this.start = start;
            this.end = end;
            this.colorType = colorType;
        }
    }

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
        if (productions.Count == 1)
        {
            var center = productions[0].transform.position;
            var direction = GetAxisDirection(transform.forward);
            CreateRailSegment(center - direction * 0.25f, center + direction * 0.25f,
                productions[0].ColorType);
            return;
        }

        var pieces = new List<RailPiece>();
        var firstPosition = productions[0].transform.position;
        var secondPosition = productions[1].transform.position;
        var firstDirection = GetAxisDirection(secondPosition - firstPosition);
        var firstSpacing = Vector3.Distance(firstPosition, secondPosition);
        pieces.Add(new RailPiece(
            firstPosition - firstDirection * firstSpacing * 0.5f,
            firstPosition,
            productions[0].ColorType));

        for (var index = 0; index < productions.Count - 1; index++)
        {
            var currentPosition = productions[index].transform.position;
            var nextPosition = productions[index + 1].transform.position;
            var midpoint = Vector3.Lerp(currentPosition, nextPosition, 0.5f);
            pieces.Add(new RailPiece(currentPosition, midpoint, productions[index].ColorType));
            pieces.Add(new RailPiece(midpoint, nextPosition, productions[index + 1].ColorType));
        }

        var lastIndex = productions.Count - 1;
        var previousPosition = productions[lastIndex - 1].transform.position;
        var lastPosition = productions[lastIndex].transform.position;
        var lastDirection = GetAxisDirection(lastPosition - previousPosition);
        var lastSpacing = Vector3.Distance(previousPosition, lastPosition);
        pieces.Add(new RailPiece(
            lastPosition,
            lastPosition + lastDirection * lastSpacing * 0.5f,
            productions[lastIndex].ColorType));

        var currentPiece = pieces[0];
        for (var index = 1; index < pieces.Count; index++)
        {
            var nextPiece = pieces[index];
            if (CanMerge(currentPiece, nextPiece))
            {
                currentPiece.end = nextPiece.end;
                continue;
            }

            CreateRailSegment(currentPiece.start, currentPiece.end, currentPiece.colorType);
            currentPiece = nextPiece;
        }
        CreateRailSegment(currentPiece.start, currentPiece.end, currentPiece.colorType);
    }

    private static Vector3 GetAxisDirection(Vector3 delta)
    {
        delta.y = 0f;
        if (delta.sqrMagnitude < 0.0001f) return Vector3.forward;
        return Mathf.Abs(delta.x) > Mathf.Abs(delta.z)
            ? new Vector3(Mathf.Sign(delta.x), 0f, 0f)
            : new Vector3(0f, 0f, Mathf.Sign(delta.z));
    }

    private static bool CanMerge(RailPiece current, RailPiece next)
    {
        if (current.colorType != next.colorType || Vector3.Distance(current.end, next.start) > 0.03f)
        {
            return false;
        }

        var currentDirection = GetAxisDirection(current.end - current.start);
        var nextDirection = GetAxisDirection(next.end - next.start);
        if (Vector3.Dot(currentDirection, nextDirection) < 0.99f)
        {
            return false;
        }

        var alongX = Mathf.Abs(currentDirection.x) > 0.5f;
        return alongX
            ? Mathf.Abs(current.start.z - next.start.z) < 0.03f
            : Mathf.Abs(current.start.x - next.start.x) < 0.03f;
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
