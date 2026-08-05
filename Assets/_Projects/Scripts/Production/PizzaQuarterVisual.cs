using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Additive, shared-mesh visual for one quarter of a completed pizza.
/// Natural pizza colours are stored in vertex colours. Vertex alpha marks the
/// paper liner so one shared material can tint only the gameplay-colour region.
/// </summary>
public sealed class PizzaQuarterVisual : MonoBehaviour
{
    private const string VisualRootName = "__PizzaQuarterVisual";
    private const string ShaderName = "PizzaRush/Pizza Quarter Vertex Color";
    private const float QuarterRadius = 0.312f;
    private const float QuarterHalfExtent = QuarterRadius * 0.5f;
    private const int ArcSegments = 10;

    private static readonly int GameplayColorId = Shader.PropertyToID("_GameplayColor");
    private static readonly int CompletionFlashId = Shader.PropertyToID("_CompletionFlash");

    private static readonly Color LinerVertexColor = new(0.92f, 0.92f, 0.92f, 1f);
    private static readonly Color CrustVertexColor = new(0.86f, 0.39f, 0.12f, 0f);
    private static readonly Color CheeseVertexColor = new(1f, 0.66f, 0.16f, 0f);
    private static readonly Color ToppingVertexColor = new(0.78f, 0.12f, 0.055f, 0f);

    private static Mesh sharedMesh;
    private static Material sharedMaterial;

    private MeshRenderer meshRenderer;
    private Transform visualRoot;
    private MaterialPropertyBlock properties;
    private Color gameplayColor = Color.white;
    private float completionFlash;

    public void Initialize()
    {
        EnsureSharedResources();
        EnsureRenderer();
        gameplayColor = Color.white;
        completionFlash = 0f;
        SetAssemblyMode(false);
        ApplyProperties();
    }

    public void SetAssemblyMode(bool assembled)
    {
        EnsureSharedResources();
        EnsureRenderer();
        visualRoot.localPosition = assembled
            ? Vector3.zero
            : new Vector3(QuarterHalfExtent, 0f, QuarterHalfExtent);
    }

    public void SetGameplayColor(Color color)
    {
        gameplayColor = color;
        ApplyProperties();
    }

    public void SetCompletionFlash(float intensity)
    {
        completionFlash = Mathf.Max(0f, intensity);
        ApplyProperties();
    }

    private void OnDisable()
    {
        completionFlash = 0f;
        if (meshRenderer != null)
        {
            ApplyProperties();
        }
    }

    private void ApplyProperties()
    {
        EnsureSharedResources();
        EnsureRenderer();
        properties ??= new MaterialPropertyBlock();
        properties.Clear();
        properties.SetColor(GameplayColorId, gameplayColor);
        properties.SetFloat(CompletionFlashId, completionFlash);
        meshRenderer.SetPropertyBlock(properties);
    }

    private void EnsureRenderer()
    {
        if (meshRenderer != null)
        {
            return;
        }

        visualRoot = transform.Find(VisualRootName);
        if (visualRoot == null)
        {
            visualRoot = new GameObject(VisualRootName).transform;
            visualRoot.SetParent(transform, false);
            visualRoot.gameObject.layer = gameObject.layer;
        }

        var meshFilter = visualRoot.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = visualRoot.gameObject.AddComponent<MeshFilter>();
        }
        meshFilter.sharedMesh = sharedMesh;

        meshRenderer = visualRoot.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = visualRoot.gameObject.AddComponent<MeshRenderer>();
        }
        meshRenderer.sharedMaterial = sharedMaterial;
        meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
        meshRenderer.lightProbeUsage = LightProbeUsage.Off;
        meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        meshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
    }

    private static void EnsureSharedResources()
    {
        if (sharedMesh == null)
        {
            sharedMesh = BuildQuarterPizzaMesh();
            sharedMesh.name = "PR3D Pizza Quarter Shared Mesh";
        }

        if (sharedMaterial == null)
        {
            Shader shader = Shader.Find(ShaderName);
            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Unlit");
                Debug.LogError($"Missing shader '{ShaderName}'. Using URP Unlit fallback.");
            }

            sharedMaterial = new Material(shader)
            {
                name = "PR3D Pizza Quarter Shared Material",
                hideFlags = HideFlags.DontSave
            };
        }
    }

    private static Mesh BuildQuarterPizzaMesh()
    {
        var vertices = new List<Vector3>();
        var colors = new List<Color>();
        var triangles = new List<int>();

        // Coloured greaseproof paper, then a raised cheese body and crust rim.
        AddSectorPrism(vertices, colors, triangles, 0f, QuarterRadius + 0.018f, 0.006f, 0.018f, ArcSegments, LinerVertexColor);
        AddSectorPrism(vertices, colors, triangles, 0f, QuarterRadius * 0.80f, 0.034f, 0.092f, ArcSegments, CheeseVertexColor);
        AddSectorPrism(vertices, colors, triangles, QuarterRadius * 0.76f, QuarterRadius, 0.03f, 0.115f, ArcSegments, CrustVertexColor);

        // Two toppings keep the silhouette readable without extra material slots.
        AddCylinder(vertices, colors, triangles, new Vector2(-0.035f, 0.045f), 0.038f, 0.092f, 0.112f, 8, ToppingVertexColor);
        AddCylinder(vertices, colors, triangles, new Vector2(0.052f, -0.048f), 0.032f, 0.092f, 0.113f, 8, ToppingVertexColor);

        var mesh = new Mesh
        {
            indexFormat = vertices.Count > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16
        };
        mesh.SetVertices(vertices);
        mesh.SetColors(colors);
        mesh.SetTriangles(triangles, 0, false);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.UploadMeshData(true);
        return mesh;
    }

    private static void AddSectorPrism(
        List<Vector3> vertices,
        List<Color> colors,
        List<int> triangles,
        float innerRadius,
        float outerRadius,
        float bottomY,
        float topY,
        int segments,
        Color vertexColor)
    {
        // In assembly mode the radial centre is the Production pivot. The
        // rail mode adds a half-quarter offset so the wedge remains centred
        // inside the narrow lane.
        Vector2 center = Vector2.zero;
        float startAngle = Mathf.PI;
        float endAngle = Mathf.PI * 1.5f;

        if (innerRadius <= 0.0001f)
        {
            int bottomCenter = AddVertex(vertices, colors, center.x, bottomY, center.y, vertexColor);
            int topCenter = AddVertex(vertices, colors, center.x, topY, center.y, vertexColor);
            var bottomArc = new int[segments + 1];
            var topArc = new int[segments + 1];
            for (int i = 0; i <= segments; i++)
            {
                float angle = Mathf.Lerp(startAngle, endAngle, i / (float)segments);
                float x = center.x + Mathf.Cos(angle) * outerRadius;
                float z = center.y + Mathf.Sin(angle) * outerRadius;
                bottomArc[i] = AddVertex(vertices, colors, x, bottomY, z, vertexColor);
                topArc[i] = AddVertex(vertices, colors, x, topY, z, vertexColor);
            }

            for (int i = 0; i < segments; i++)
            {
                AddTriangle(triangles, topCenter, topArc[i], topArc[i + 1]);
                AddTriangle(triangles, bottomCenter, bottomArc[i + 1], bottomArc[i]);
                AddQuad(triangles, bottomArc[i], bottomArc[i + 1], topArc[i + 1], topArc[i]);
            }
            AddQuad(triangles, bottomCenter, bottomArc[0], topArc[0], topCenter);
            AddQuad(triangles, bottomArc[segments], bottomCenter, topCenter, topArc[segments]);
            return;
        }

        var outerBottom = new int[segments + 1];
        var outerTop = new int[segments + 1];
        var innerBottom = new int[segments + 1];
        var innerTop = new int[segments + 1];
        for (int i = 0; i <= segments; i++)
        {
            float angle = Mathf.Lerp(startAngle, endAngle, i / (float)segments);
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);
            outerBottom[i] = AddVertex(vertices, colors, center.x + cos * outerRadius, bottomY, center.y + sin * outerRadius, vertexColor);
            outerTop[i] = AddVertex(vertices, colors, center.x + cos * outerRadius, topY, center.y + sin * outerRadius, vertexColor);
            innerBottom[i] = AddVertex(vertices, colors, center.x + cos * innerRadius, bottomY, center.y + sin * innerRadius, vertexColor);
            innerTop[i] = AddVertex(vertices, colors, center.x + cos * innerRadius, topY, center.y + sin * innerRadius, vertexColor);
        }

        for (int i = 0; i < segments; i++)
        {
            AddQuad(triangles, innerTop[i], outerTop[i], outerTop[i + 1], innerTop[i + 1]);
            AddQuad(triangles, innerBottom[i + 1], outerBottom[i + 1], outerBottom[i], innerBottom[i]);
            AddQuad(triangles, outerBottom[i], outerBottom[i + 1], outerTop[i + 1], outerTop[i]);
            AddQuad(triangles, innerBottom[i + 1], innerBottom[i], innerTop[i], innerTop[i + 1]);
        }
        AddQuad(triangles, innerBottom[0], outerBottom[0], outerTop[0], innerTop[0]);
        AddQuad(triangles, outerBottom[segments], innerBottom[segments], innerTop[segments], outerTop[segments]);
    }

    private static void AddCylinder(
        List<Vector3> vertices,
        List<Color> colors,
        List<int> triangles,
        Vector2 center,
        float radius,
        float bottomY,
        float topY,
        int segments,
        Color vertexColor)
    {
        int bottomCenter = AddVertex(vertices, colors, center.x, bottomY, center.y, vertexColor);
        int topCenter = AddVertex(vertices, colors, center.x, topY, center.y, vertexColor);
        var bottomRing = new int[segments];
        var topRing = new int[segments];
        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            float x = center.x + Mathf.Cos(angle) * radius;
            float z = center.y + Mathf.Sin(angle) * radius;
            bottomRing[i] = AddVertex(vertices, colors, x, bottomY, z, vertexColor);
            topRing[i] = AddVertex(vertices, colors, x, topY, z, vertexColor);
        }

        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments;
            AddTriangle(triangles, topCenter, topRing[i], topRing[next]);
            AddTriangle(triangles, bottomCenter, bottomRing[next], bottomRing[i]);
            AddQuad(triangles, bottomRing[i], bottomRing[next], topRing[next], topRing[i]);
        }
    }

    private static int AddVertex(List<Vector3> vertices, List<Color> colors, float x, float y, float z, Color color)
    {
        vertices.Add(new Vector3(x, y, z));
        colors.Add(color);
        return vertices.Count - 1;
    }

    private static void AddTriangle(List<int> triangles, int a, int b, int c)
    {
        triangles.Add(a);
        triangles.Add(b);
        triangles.Add(c);
    }

    private static void AddQuad(List<int> triangles, int a, int b, int c, int d)
    {
        AddTriangle(triangles, a, b, c);
        AddTriangle(triangles, a, c, d);
    }
}
