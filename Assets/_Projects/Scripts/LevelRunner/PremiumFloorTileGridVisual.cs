using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
[DisallowMultipleComponent]
internal sealed class PremiumFloorTileGridVisual : MonoBehaviour
{
    private const string VisualObjectName = "__PremiumFloorTileGrid";

    [SerializeField, Min(0.25f)] private float tileSize = 1.5f;
    [SerializeField, Min(0.005f)] private float grooveWidth = 0.026f;
    [SerializeField, Min(0.002f)] private float highlightWidth = 0.012f;
    [SerializeField, Min(0f)] private float highlightOffset = 0.024f;
    [SerializeField, Min(0f)] private float heightOffset = 0.004f;
    [SerializeField] private Color grooveColor = new(0.54f, 0.49f, 0.42f, 1f);
    [SerializeField] private Color highlightColor = new(0.69f, 0.63f, 0.54f, 1f);

    private Transform visualRoot;
    private Mesh generatedMesh;
    private Material grooveMaterial;
    private Material highlightMaterial;

    private void OnEnable()
    {
        Rebuild();
    }

    private void OnValidate()
    {
        if (isActiveAndEnabled)
        {
            Rebuild();
        }
    }

    private void Rebuild()
    {
        ClearVisual();

        MeshFilter sourceFilter = GetComponent<MeshFilter>();
        if (sourceFilter == null || sourceFilter.sharedMesh == null)
        {
            return;
        }

        Bounds bounds = sourceFilter.sharedMesh.bounds;
        float scaleX = Mathf.Max(0.0001f, Mathf.Abs(transform.lossyScale.x));
        float scaleY = Mathf.Max(0.0001f, Mathf.Abs(transform.lossyScale.y));
        float scaleZ = Mathf.Max(0.0001f, Mathf.Abs(transform.lossyScale.z));
        float tileX = tileSize / scaleX;
        float tileZ = tileSize / scaleZ;
        float grooveX = grooveWidth / scaleX;
        float grooveZ = grooveWidth / scaleZ;
        float highlightX = highlightWidth / scaleX;
        float highlightZ = highlightWidth / scaleZ;
        float offsetX = highlightOffset / scaleX;
        float offsetZ = highlightOffset / scaleZ;
        float localY = heightOffset / scaleY;

        var vertices = new List<Vector3>();
        var grooveTriangles = new List<int>();
        var highlightTriangles = new List<int>();

        float firstX = Mathf.Ceil(bounds.min.x / tileX) * tileX;
        for (float x = firstX; x <= bounds.max.x + 0.0001f; x += tileX)
        {
            AddQuad(
                vertices,
                grooveTriangles,
                x - grooveX * 0.5f,
                x + grooveX * 0.5f,
                bounds.min.z,
                bounds.max.z,
                localY);
            AddQuad(
                vertices,
                highlightTriangles,
                x + offsetX - highlightX * 0.5f,
                x + offsetX + highlightX * 0.5f,
                bounds.min.z,
                bounds.max.z,
                localY + 0.0001f);
        }

        float firstZ = Mathf.Ceil(bounds.min.z / tileZ) * tileZ;
        for (float z = firstZ; z <= bounds.max.z + 0.0001f; z += tileZ)
        {
            AddQuad(
                vertices,
                grooveTriangles,
                bounds.min.x,
                bounds.max.x,
                z - grooveZ * 0.5f,
                z + grooveZ * 0.5f,
                localY);
            AddQuad(
                vertices,
                highlightTriangles,
                bounds.min.x,
                bounds.max.x,
                z + offsetZ - highlightZ * 0.5f,
                z + offsetZ + highlightZ * 0.5f,
                localY + 0.0001f);
        }

        generatedMesh = new Mesh { name = "Premium Floor Tile Grid Mesh" };
        generatedMesh.SetVertices(vertices);
        generatedMesh.subMeshCount = 2;
        generatedMesh.SetTriangles(grooveTriangles, 0);
        generatedMesh.SetTriangles(highlightTriangles, 1);
        generatedMesh.RecalculateBounds();

        visualRoot = new GameObject(VisualObjectName).transform;
        visualRoot.SetParent(transform, false);
        visualRoot.gameObject.layer = gameObject.layer;
        visualRoot.gameObject.hideFlags = HideFlags.DontSave;

        visualRoot.gameObject.AddComponent<MeshFilter>().sharedMesh = generatedMesh;
        MeshRenderer renderer = visualRoot.gameObject.AddComponent<MeshRenderer>();
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Standard");
        grooveMaterial = CreateMaterial(shader, "Premium Floor Tile Groove", grooveColor);
        highlightMaterial = CreateMaterial(shader, "Premium Floor Tile Highlight", highlightColor);
        renderer.sharedMaterials = new[] { grooveMaterial, highlightMaterial };
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.lightProbeUsage = LightProbeUsage.Off;
        renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
    }

    private static void AddQuad(
        List<Vector3> vertices,
        List<int> triangles,
        float minX,
        float maxX,
        float minZ,
        float maxZ,
        float y)
    {
        int first = vertices.Count;
        vertices.Add(new Vector3(minX, y, minZ));
        vertices.Add(new Vector3(minX, y, maxZ));
        vertices.Add(new Vector3(maxX, y, maxZ));
        vertices.Add(new Vector3(maxX, y, minZ));
        triangles.Add(first);
        triangles.Add(first + 1);
        triangles.Add(first + 2);
        triangles.Add(first);
        triangles.Add(first + 2);
        triangles.Add(first + 3);
    }

    private static Material CreateMaterial(Shader shader, string materialName, Color color)
    {
        if (shader == null)
        {
            return null;
        }

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
        return material;
    }

    private void OnDisable()
    {
        ClearVisual();
    }

    private void ClearVisual()
    {
        if (visualRoot == null)
        {
            visualRoot = transform.Find(VisualObjectName);
        }
        DestroyGeneratedObject(visualRoot == null ? null : visualRoot.gameObject);
        DestroyGeneratedObject(generatedMesh);
        DestroyGeneratedObject(grooveMaterial);
        DestroyGeneratedObject(highlightMaterial);
        visualRoot = null;
        generatedMesh = null;
        grooveMaterial = null;
        highlightMaterial = null;
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
