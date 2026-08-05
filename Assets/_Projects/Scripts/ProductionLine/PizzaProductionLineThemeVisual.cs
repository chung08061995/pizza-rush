using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Builds the approved midpoint-colored pizza rail and required-color contact lamp.
/// Visual geometry is collider-free and follows the live production queue.
/// </summary>
public sealed class PizzaProductionLineThemeVisual : MonoBehaviour
{
    private struct ContactLampVisual
    {
        public Renderer renderer;
        public Color color;
        public ColorType colorType;
        public Vector3 baseScale;

        public ContactLampVisual(Renderer renderer, Color color, ColorType colorType, Vector3 baseScale)
        {
            this.renderer = renderer;
            this.color = color;
            this.colorType = colorType;
            this.baseScale = baseScale;
        }
    }

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
    private const float ContactLampLongSize = 0.96f;
    private const float ContactLampShortSize = 0.24f;
    private const float RailColorWidth = 0.32f;
    private const float ContactLampHeight = 0.055f;
    private const float ContactLampPulseDuration = 1.35f;
    private const float ContactLampMinEmission = 0.08f;
    private const float ContactLampMaxEmission = 0.68f;
    private const float ContactLampMaxColorLift = 0.12f;
    private const float ContactLampMaxScaleLift = 0.025f;
    private static readonly Dictionary<ColorType, Material> RailMaterials = new();
    private static readonly Dictionary<ColorType, Material> IngredientMaterials = new();
    private static ColorType highlightedColor = ColorType.None;
    private static ColorType hintedColor = ColorType.None;
    private static float hintExpiresAt;
    private readonly List<ContactLampVisual> contactLamps = new();
    private readonly List<Mesh> generatedMeshes = new();
    private MaterialPropertyBlock contactLampPropertyBlock;
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

    public static void SetHighlightedColor(ColorType colorType)
    {
        highlightedColor = colorType;
    }

    public static void ClearHighlightedColor()
    {
        highlightedColor = ColorType.None;
    }

    public static void ShowHintColor(ColorType colorType, float duration)
    {
        hintedColor = colorType;
        hintExpiresAt = Time.unscaledTime + Mathf.Max(0f, duration);
    }

    private void LateUpdate()
    {
        if (refreshRequested)
        {
            refreshRequested = false;
            Rebuild();
        }

        AnimateContactLamps();
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
            ? new Vector3(length + 0.025f, 0.016f, RailColorWidth)
            : new Vector3(RailColorWidth, 0.016f, length + 0.025f);
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

        var alongX = bounds.size.x >= bounds.size.z;
        var scale = alongX
            ? new Vector3(ContactLampLongSize, ContactLampHeight, ContactLampShortSize)
            : new Vector3(ContactLampShortSize, ContactLampHeight, ContactLampLongSize);
        var center = new Vector3(bounds.center.x, bounds.max.y + 0.012f, bounds.center.z);
        var color = GetSignalColor(requiredColor);
        var renderer = CreateCube($"ContactLamp_{requiredColor}", center, scale,
            GetRailMaterial(requiredColor), true);
        if (renderer == null) return;

        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        contactLamps.Add(new ContactLampVisual(renderer, color, requiredColor, scale));
        CreateIngredientIcon(requiredColor, center, bounds.max.y + 0.075f);
    }

    private void CreateIngredientIcon(ColorType colorType, Vector3 center, float topY)
    {
        Mesh mesh = PizzaBoxFootprintMeshBuilder.BuildIngredientIcon(
            colorType,
            Vector2.zero,
            0.145f,
            0f);
        if (mesh == null || mesh.vertexCount == 0)
        {
            if (Application.isPlaying) Destroy(mesh);
            else DestroyImmediate(mesh);
            return;
        }
        generatedMeshes.Add(mesh);

        var iconObject = new GameObject($"ContactIngredient_{colorType}");
        iconObject.layer = gameObject.layer;
        iconObject.transform.SetParent(visualRoot, true);
        iconObject.transform.position = new Vector3(center.x, topY, center.z);
        iconObject.AddComponent<MeshFilter>().sharedMesh = mesh;
        var renderer = iconObject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = GetIngredientMaterial(colorType);
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
    }

    private void AnimateContactLamps()
    {
        if (contactLamps.Count == 0) return;
        contactLampPropertyBlock ??= new MaterialPropertyBlock();

        var normalizedPulse = (Mathf.Sin(Time.time * Mathf.PI * 2f / ContactLampPulseDuration) + 1f) * 0.5f;
        normalizedPulse = Mathf.SmoothStep(0f, 1f, normalizedPulse);

        for (var index = contactLamps.Count - 1; index >= 0; index--)
        {
            var lamp = contactLamps[index];
            if (lamp.renderer == null)
            {
                contactLamps.RemoveAt(index);
                continue;
            }

            bool hintActive = highlightedColor == ColorType.None &&
                              hintedColor != ColorType.None &&
                              Time.unscaledTime < hintExpiresAt;
            if (!hintActive && Time.unscaledTime >= hintExpiresAt)
            {
                hintedColor = ColorType.None;
            }
            bool isHighlighted = (highlightedColor != ColorType.None &&
                                  highlightedColor == lamp.colorType) ||
                                 (hintActive && hintedColor == lamp.colorType);
            var pulse = isHighlighted ? normalizedPulse : 0f;
            var emissionStrength = Mathf.Lerp(ContactLampMinEmission, ContactLampMaxEmission, pulse);
            var scaleMultiplier = 1f + ContactLampMaxScaleLift * pulse;

            lamp.renderer.GetPropertyBlock(contactLampPropertyBlock);
            var lampColor = Color.Lerp(lamp.color, Color.white,
                ContactLampMaxColorLift * pulse);
            contactLampPropertyBlock.SetColor("_BaseColor", lampColor);
            contactLampPropertyBlock.SetColor("_Color", lampColor);
            contactLampPropertyBlock.SetColor("_EmissionColor", lamp.color * emissionStrength);
            lamp.renderer.SetPropertyBlock(contactLampPropertyBlock);
            lamp.renderer.transform.localScale = new Vector3(
                lamp.baseScale.x * scaleMultiplier,
                lamp.baseScale.y,
                lamp.baseScale.z * scaleMultiplier);
        }
    }

    private Renderer CreateCube(string objectName, Vector3 position, Vector3 scale, Material material,
        bool worldSpace)
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
        var renderer = cube.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        return renderer;
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
        var color = GetSignalColor(colorType);
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

    private static Material GetIngredientMaterial(ColorType colorType)
    {
        if (IngredientMaterials.TryGetValue(colorType, out var material) && material != null)
        {
            return material;
        }

        var baseColor = GetSignalColor(colorType);
        float luminance = baseColor.r * 0.2126f + baseColor.g * 0.7152f + baseColor.b * 0.0722f;
        var iconColor = luminance > 0.55f
            ? baseColor * 0.58f
            : Color.Lerp(baseColor, Color.white, 0.58f);
        iconColor.a = 1f;
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        material = new Material(shader)
        {
            name = $"Pizza Gate Ingredient {colorType}",
            color = iconColor,
            hideFlags = HideFlags.DontSave
        };
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", iconColor);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.28f);
        IngredientMaterials[colorType] = material;
        return material;
    }

    private static Color GetSignalColor(ColorType colorType)
    {
        var colors = DataManager.Instance == null ? null : DataManager.Instance.ProductionLineColorsSO;
        if (colors == null) return Color.white;
        if (colors.Dictionary.Count == 0) colors.BuildDictionary();
        return colors.TryGetValue(colorType, out var color) ? color : Color.white;
    }

    private void Clear()
    {
        contactLamps.Clear();
        foreach (Mesh mesh in generatedMeshes)
        {
            if (mesh == null) continue;
            if (Application.isPlaying) Destroy(mesh);
            else DestroyImmediate(mesh);
        }
        generatedMeshes.Clear();
        if (visualRoot == null) visualRoot = transform.Find(VisualRootName);
        if (visualRoot == null) return;
        if (Application.isPlaying) Destroy(visualRoot.gameObject);
        else DestroyImmediate(visualRoot.gameObject);
        visualRoot = null;
    }
}
