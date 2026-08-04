using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
internal sealed class BoardGridThemeVisual : MonoBehaviour
{
    internal const string ObjectName = "PR3D_BoardGrid_InsetFrames";

    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private Mesh _mesh;
    private Material _grooveMaterial;
    private Material _highlightMaterial;

    internal void Rebuild(
        DraftUtils.GridXZ grid,
        IReadOnlyList<SerializableVector2Int> gridPositions,
        Color grooveColor,
        Color highlightColor,
        float insetCells,
        float lineWidthCells,
        float heightOffset)
    {
        EnsureRenderComponents();

        if (grid == null || gridPositions == null || gridPositions.Count == 0)
        {
            _mesh.Clear();
            return;
        }

        Vector2Int originCell = gridPositions[0].ToVector2Int();
        Vector3 origin = grid.CellToWorld(originCell);
        float cellSizeX = Mathf.Max(
            0.01f,
            Vector3.Distance(origin, grid.CellToWorld(originCell + Vector2Int.right)));
        float cellSizeZ = Mathf.Max(
            0.01f,
            Vector3.Distance(origin, grid.CellToWorld(originCell + Vector2Int.up)));
        float insetX = Mathf.Clamp(insetCells, 0f, 0.45f) * cellSizeX;
        float insetZ = Mathf.Clamp(insetCells, 0f, 0.45f) * cellSizeZ;
        float lineWidthX = Mathf.Clamp(lineWidthCells, 0.005f, 0.25f) * cellSizeX;
        float lineWidthZ = Mathf.Clamp(lineWidthCells, 0.005f, 0.25f) * cellSizeZ;
        float halfSpanX = Mathf.Max(lineWidthX * 0.5f, cellSizeX * 0.5f - insetX);
        float halfSpanZ = Mathf.Max(lineWidthZ * 0.5f, cellSizeZ * 0.5f - insetZ);

        float highlightInsetX = Mathf.Min(halfSpanX - lineWidthX, lineWidthX * 0.7f);
        float highlightInsetZ = Mathf.Min(halfSpanZ - lineWidthZ, lineWidthZ * 0.7f);
        float highlightWidthX = Mathf.Max(cellSizeX * 0.004f, lineWidthX * 0.42f);
        float highlightWidthZ = Mathf.Max(cellSizeZ * 0.004f, lineWidthZ * 0.42f);

        var vertices = new List<Vector3>(gridPositions.Count * 32);
        var grooveTriangles = new List<int>(gridPositions.Count * 24);
        var highlightTriangles = new List<int>(gridPositions.Count * 24);
        for (int i = 0; i < gridPositions.Count; i++)
        {
            Vector3 center = grid.CellToWorld(gridPositions[i].ToVector2Int());
            center.y += heightOffset;

            AddFrame(vertices, grooveTriangles, center, halfSpanX, halfSpanZ, lineWidthX, lineWidthZ);

            Vector3 highlightCenter = center + Vector3.up * 0.002f;
            AddFrame(
                vertices,
                highlightTriangles,
                highlightCenter,
                halfSpanX - highlightInsetX,
                halfSpanZ - highlightInsetZ,
                highlightWidthX,
                highlightWidthZ);
        }

        _mesh.Clear();
        _mesh.SetVertices(vertices);
        _mesh.subMeshCount = 2;
        _mesh.SetTriangles(grooveTriangles, 0);
        _mesh.SetTriangles(highlightTriangles, 1);
        _mesh.RecalculateBounds();
        SetMaterialColor(_grooveMaterial, grooveColor);
        SetMaterialColor(_highlightMaterial, highlightColor);
    }

    private void AddFrame(
        List<Vector3> vertices,
        List<int> triangles,
        Vector3 center,
        float halfSpanX,
        float halfSpanZ,
        float lineWidthX,
        float lineWidthZ)
    {
        AddQuad(vertices, triangles, center + Vector3.forward * (halfSpanZ - lineWidthZ * 0.5f),
                halfSpanX, lineWidthZ * 0.5f);
        AddQuad(vertices, triangles, center - Vector3.forward * (halfSpanZ - lineWidthZ * 0.5f),
                halfSpanX, lineWidthZ * 0.5f);
        AddQuad(vertices, triangles, center + Vector3.right * (halfSpanX - lineWidthX * 0.5f),
                lineWidthX * 0.5f, halfSpanZ);
        AddQuad(vertices, triangles, center - Vector3.right * (halfSpanX - lineWidthX * 0.5f),
                lineWidthX * 0.5f, halfSpanZ);
    }

    private void EnsureRenderComponents()
    {
        if (_meshFilter == null)
        {
            _meshFilter = GetComponent<MeshFilter>();
            if (_meshFilter == null)
            {
                _meshFilter = gameObject.AddComponent<MeshFilter>();
            }
        }

        if (_meshRenderer == null)
        {
            _meshRenderer = GetComponent<MeshRenderer>();
            if (_meshRenderer == null)
            {
                _meshRenderer = gameObject.AddComponent<MeshRenderer>();
            }
        }

        if (_mesh == null)
        {
            _mesh = new Mesh { name = "Board Grid Inset Frames Mesh" };
            _mesh.MarkDynamic();
            _meshFilter.sharedMesh = _mesh;
        }

        if (_grooveMaterial == null || _highlightMaterial == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Lit");
            }

            if (shader != null)
            {
                _grooveMaterial = CreateMaterial(shader, "Board Grid Groove Material");
                _highlightMaterial = CreateMaterial(shader, "Board Grid Highlight Material");
                _meshRenderer.sharedMaterials = new[] { _grooveMaterial, _highlightMaterial };
            }
        }

        _meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
        _meshRenderer.receiveShadows = false;
        _meshRenderer.lightProbeUsage = LightProbeUsage.Off;
        _meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        _meshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
    }

    private static Material CreateMaterial(Shader shader, string materialName)
    {
        return new Material(shader)
        {
            name = materialName,
            hideFlags = HideFlags.DontSave
        };
    }

    private static void SetMaterialColor(Material material, Color color)
    {
        if (material == null)
        {
            return;
        }

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }
        else if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }
    }

    private void AddQuad(
        List<Vector3> vertices,
        List<int> triangles,
        Vector3 worldCenter,
        float halfWidth,
        float halfDepth)
    {
        int index = vertices.Count;
        vertices.Add(transform.InverseTransformPoint(
            worldCenter + new Vector3(-halfWidth, 0f, -halfDepth)));
        vertices.Add(transform.InverseTransformPoint(
            worldCenter + new Vector3(-halfWidth, 0f, halfDepth)));
        vertices.Add(transform.InverseTransformPoint(
            worldCenter + new Vector3(halfWidth, 0f, halfDepth)));
        vertices.Add(transform.InverseTransformPoint(
            worldCenter + new Vector3(halfWidth, 0f, -halfDepth)));
        triangles.Add(index);
        triangles.Add(index + 1);
        triangles.Add(index + 2);
        triangles.Add(index);
        triangles.Add(index + 2);
        triangles.Add(index + 3);
    }

    private void OnDestroy()
    {
        if (_mesh != null)
        {
            Destroy(_mesh);
        }

        if (_grooveMaterial != null)
        {
            Destroy(_grooveMaterial);
        }

        if (_highlightMaterial != null)
        {
            Destroy(_highlightMaterial);
        }
    }
}
