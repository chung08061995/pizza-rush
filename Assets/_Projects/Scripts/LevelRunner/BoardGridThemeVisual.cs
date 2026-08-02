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
    private Material _material;

    internal void Rebuild(
        DraftUtils.GridXZ grid,
        IReadOnlyList<SerializableVector2Int> gridPositions,
        Color color,
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

        var vertices = new List<Vector3>(gridPositions.Count * 16);
        var triangles = new List<int>(gridPositions.Count * 24);
        for (int i = 0; i < gridPositions.Count; i++)
        {
            Vector3 center = grid.CellToWorld(gridPositions[i].ToVector2Int());
            center.y += heightOffset;

            AddQuad(vertices, triangles, center + Vector3.forward * (halfSpanZ - lineWidthZ * 0.5f),
                halfSpanX, lineWidthZ * 0.5f);
            AddQuad(vertices, triangles, center - Vector3.forward * (halfSpanZ - lineWidthZ * 0.5f),
                halfSpanX, lineWidthZ * 0.5f);
            AddQuad(vertices, triangles, center + Vector3.right * (halfSpanX - lineWidthX * 0.5f),
                lineWidthX * 0.5f, halfSpanZ);
            AddQuad(vertices, triangles, center - Vector3.right * (halfSpanX - lineWidthX * 0.5f),
                lineWidthX * 0.5f, halfSpanZ);
        }

        _mesh.Clear();
        _mesh.SetVertices(vertices);
        _mesh.SetTriangles(triangles, 0);
        _mesh.RecalculateBounds();
        SetMaterialColor(color);
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

        if (_material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Lit");
            }

            if (shader != null)
            {
                _material = new Material(shader)
                {
                    name = "Board Grid Inset Frames Material",
                    hideFlags = HideFlags.DontSave
                };
                _meshRenderer.sharedMaterial = _material;
            }
        }

        _meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
        _meshRenderer.receiveShadows = false;
        _meshRenderer.lightProbeUsage = LightProbeUsage.Off;
        _meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        _meshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
    }

    private void SetMaterialColor(Color color)
    {
        if (_material == null)
        {
            return;
        }

        if (_material.HasProperty("_BaseColor"))
        {
            _material.SetColor("_BaseColor", color);
        }
        else if (_material.HasProperty("_Color"))
        {
            _material.SetColor("_Color", color);
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

        if (_material != null)
        {
            Destroy(_material);
        }
    }
}
