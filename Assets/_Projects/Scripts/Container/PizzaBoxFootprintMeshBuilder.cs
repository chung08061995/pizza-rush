using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builds presentation-only meshes from a container's occupied grid cells.
/// The generated geometry has no colliders and does not alter gameplay bounds.
/// </summary>
internal static class PizzaBoxFootprintMeshBuilder
{
    internal static Mesh BuildLid(
        IReadOnlyList<Vector2Int> cells,
        float topY,
        float outerInset,
        float cornerChamfer)
    {
        var occupied = new HashSet<Vector2Int>(cells);
        var vertices = new List<Vector3>(cells.Count * 10);
        var triangles = new List<int>(cells.Count * 24);

        foreach (Vector2Int cell in cells)
        {
            GetExposure(occupied, cell, out bool left, out bool right, out bool down, out bool up);
            AddCellTop(vertices, triangles, cell, topY, outerInset, cornerChamfer, left, right, down, up);
        }

        return CreateMesh("Premium Pizza Box Lid", vertices, triangles);
    }

    internal static Mesh BuildOuterSide(
        IReadOnlyList<Vector2Int> cells,
        float topY,
        float bottomY,
        float outerInset,
        float cornerChamfer)
    {
        var occupied = new HashSet<Vector2Int>(cells);
        var vertices = new List<Vector3>(cells.Count * 24);
        var triangles = new List<int>(cells.Count * 36);
        float edge = 0.5f - outerInset;

        foreach (Vector2Int cell in cells)
        {
            GetExposure(occupied, cell, out bool left, out bool right, out bool down, out bool up);
            float x0 = cell.x - edge;
            float x1 = cell.x + edge;
            float z0 = cell.y - edge;
            float z1 = cell.y + edge;

            if (left)
            {
                AddDoubleSidedWall(vertices, triangles,
                    new Vector2(x0, z0 + (down ? cornerChamfer : 0f)),
                    new Vector2(x0, z1 - (up ? cornerChamfer : 0f)), topY, bottomY);
            }
            if (up)
            {
                AddDoubleSidedWall(vertices, triangles,
                    new Vector2(x0 + (left ? cornerChamfer : 0f), z1),
                    new Vector2(x1 - (right ? cornerChamfer : 0f), z1), topY, bottomY);
            }
            if (right)
            {
                AddDoubleSidedWall(vertices, triangles,
                    new Vector2(x1, z1 - (up ? cornerChamfer : 0f)),
                    new Vector2(x1, z0 + (down ? cornerChamfer : 0f)), topY, bottomY);
            }
            if (down)
            {
                AddDoubleSidedWall(vertices, triangles,
                    new Vector2(x1 - (right ? cornerChamfer : 0f), z0),
                    new Vector2(x0 + (left ? cornerChamfer : 0f), z0), topY, bottomY);
            }

            if (left && down)
            {
                AddDoubleSidedWall(vertices, triangles,
                    new Vector2(x0 + cornerChamfer, z0),
                    new Vector2(x0, z0 + cornerChamfer), topY, bottomY);
            }
            if (left && up)
            {
                AddDoubleSidedWall(vertices, triangles,
                    new Vector2(x0, z1 - cornerChamfer),
                    new Vector2(x0 + cornerChamfer, z1), topY, bottomY);
            }
            if (right && up)
            {
                AddDoubleSidedWall(vertices, triangles,
                    new Vector2(x1 - cornerChamfer, z1),
                    new Vector2(x1, z1 - cornerChamfer), topY, bottomY);
            }
            if (right && down)
            {
                AddDoubleSidedWall(vertices, triangles,
                    new Vector2(x1, z0 + cornerChamfer),
                    new Vector2(x1 - cornerChamfer, z0), topY, bottomY);
            }
        }

        return CreateMesh("Premium Pizza Box Kraft Side", vertices, triangles);
    }

    internal static Mesh BuildOuterRim(
        IReadOnlyList<Vector2Int> cells,
        float topY,
        float outerInset,
        float cornerChamfer,
        float rimWidth)
    {
        var occupied = new HashSet<Vector2Int>(cells);
        var vertices = new List<Vector3>(cells.Count * 32);
        var triangles = new List<int>(cells.Count * 48);
        float edge = 0.5f - outerInset;

        foreach (Vector2Int cell in cells)
        {
            GetExposure(occupied, cell, out bool left, out bool right, out bool down, out bool up);
            float x0 = cell.x - edge;
            float x1 = cell.x + edge;
            float z0 = cell.y - edge;
            float z1 = cell.y + edge;

            if (left)
            {
                AddTopRect(vertices, triangles,
                    x0, x0 + rimWidth,
                    z0 + (down ? cornerChamfer : 0f),
                    z1 - (up ? cornerChamfer : 0f), topY);
            }
            if (right)
            {
                AddTopRect(vertices, triangles,
                    x1 - rimWidth, x1,
                    z0 + (down ? cornerChamfer : 0f),
                    z1 - (up ? cornerChamfer : 0f), topY);
            }
            if (down)
            {
                AddTopRect(vertices, triangles,
                    x0 + (left ? cornerChamfer : 0f),
                    x1 - (right ? cornerChamfer : 0f),
                    z0, z0 + rimWidth, topY);
            }
            if (up)
            {
                AddTopRect(vertices, triangles,
                    x0 + (left ? cornerChamfer : 0f),
                    x1 - (right ? cornerChamfer : 0f),
                    z1 - rimWidth, z1, topY);
            }

            if (left && down)
            {
                AddChamferRim(vertices, triangles, topY,
                    new Vector2(x0 + cornerChamfer, z0),
                    new Vector2(x0, z0 + cornerChamfer),
                    new Vector2(x0 + rimWidth, z0 + cornerChamfer + rimWidth),
                    new Vector2(x0 + cornerChamfer + rimWidth, z0 + rimWidth));
            }
            if (left && up)
            {
                AddChamferRim(vertices, triangles, topY,
                    new Vector2(x0, z1 - cornerChamfer),
                    new Vector2(x0 + cornerChamfer, z1),
                    new Vector2(x0 + cornerChamfer + rimWidth, z1 - rimWidth),
                    new Vector2(x0 + rimWidth, z1 - cornerChamfer - rimWidth));
            }
            if (right && up)
            {
                AddChamferRim(vertices, triangles, topY,
                    new Vector2(x1 - cornerChamfer, z1),
                    new Vector2(x1, z1 - cornerChamfer),
                    new Vector2(x1 - rimWidth, z1 - cornerChamfer - rimWidth),
                    new Vector2(x1 - cornerChamfer - rimWidth, z1 - rimWidth));
            }
            if (right && down)
            {
                AddChamferRim(vertices, triangles, topY,
                    new Vector2(x1, z0 + cornerChamfer),
                    new Vector2(x1 - cornerChamfer, z0),
                    new Vector2(x1 - cornerChamfer - rimWidth, z0 + rimWidth),
                    new Vector2(x1 - rimWidth, z0 + cornerChamfer + rimWidth));
            }
        }

        return CreateMesh("Premium Pizza Box Outer Rim", vertices, triangles);
    }

    internal static Mesh BuildInternalSeams(
        IReadOnlyList<Vector2Int> cells,
        float topY,
        float seamWidth,
        float seamEndInset)
    {
        var occupied = new HashSet<Vector2Int>(cells);
        var vertices = new List<Vector3>(cells.Count * 8);
        var triangles = new List<int>(cells.Count * 12);
        float halfLength = Mathf.Max(0.05f, 0.5f - seamEndInset);
        float halfWidth = seamWidth * 0.5f;

        foreach (Vector2Int cell in cells)
        {
            if (occupied.Contains(cell + Vector2Int.right))
            {
                AddTopRect(vertices, triangles,
                    cell.x + 0.5f - halfWidth,
                    cell.x + 0.5f + halfWidth,
                    cell.y - halfLength,
                    cell.y + halfLength, topY);
            }
            if (occupied.Contains(cell + Vector2Int.up))
            {
                AddTopRect(vertices, triangles,
                    cell.x - halfLength,
                    cell.x + halfLength,
                    cell.y + 0.5f - halfWidth,
                    cell.y + 0.5f + halfWidth, topY);
            }
        }

        return CreateMesh("Premium Pizza Box Pressed Seams", vertices, triangles);
    }

    internal static Mesh BuildMarkers(
        IReadOnlyList<Vector2Int> cells,
        float bottomY,
        float topY,
        float markerLength,
        float markerWidth)
    {
        var vertices = new List<Vector3>(cells.Count * 40);
        var triangles = new List<int>(cells.Count * 60);
        foreach (Vector2Int cell in cells)
        {
            var center = new Vector3(cell.x, 0f, cell.y);
            AddBox(vertices, triangles, center, markerWidth, markerLength, bottomY, topY);
            AddBox(vertices, triangles, center, markerLength, markerWidth, bottomY, topY);
        }
        return CreateMesh("Premium Pizza Box Embossed Markers", vertices, triangles);
    }

    private static void GetExposure(
        HashSet<Vector2Int> occupied,
        Vector2Int cell,
        out bool left,
        out bool right,
        out bool down,
        out bool up)
    {
        left = !occupied.Contains(cell + Vector2Int.left);
        right = !occupied.Contains(cell + Vector2Int.right);
        down = !occupied.Contains(cell + Vector2Int.down);
        up = !occupied.Contains(cell + Vector2Int.up);
    }

    private static void AddCellTop(
        List<Vector3> vertices,
        List<int> triangles,
        Vector2Int cell,
        float y,
        float outerInset,
        float chamfer,
        bool left,
        bool right,
        bool down,
        bool up)
    {
        float minX = cell.x - (left ? 0.5f - outerInset : 0.5f);
        float maxX = cell.x + (right ? 0.5f - outerInset : 0.5f);
        float minZ = cell.y - (down ? 0.5f - outerInset : 0.5f);
        float maxZ = cell.y + (up ? 0.5f - outerInset : 0.5f);
        var polygon = new List<Vector2>(8);

        AddCorner(polygon, minX, minZ, chamfer, left && down, Corner.BottomLeft);
        AddCorner(polygon, minX, maxZ, chamfer, left && up, Corner.TopLeft);
        AddCorner(polygon, maxX, maxZ, chamfer, right && up, Corner.TopRight);
        AddCorner(polygon, maxX, minZ, chamfer, right && down, Corner.BottomRight);
        AddTopPolygon(vertices, triangles, polygon, y);
    }

    private enum Corner
    {
        BottomLeft,
        TopLeft,
        TopRight,
        BottomRight
    }

    private static void AddCorner(
        List<Vector2> polygon,
        float x,
        float z,
        float chamfer,
        bool useChamfer,
        Corner corner)
    {
        if (!useChamfer)
        {
            polygon.Add(new Vector2(x, z));
            return;
        }

        switch (corner)
        {
            case Corner.BottomLeft:
                polygon.Add(new Vector2(x + chamfer, z));
                polygon.Add(new Vector2(x, z + chamfer));
                break;
            case Corner.TopLeft:
                polygon.Add(new Vector2(x, z - chamfer));
                polygon.Add(new Vector2(x + chamfer, z));
                break;
            case Corner.TopRight:
                polygon.Add(new Vector2(x - chamfer, z));
                polygon.Add(new Vector2(x, z - chamfer));
                break;
            case Corner.BottomRight:
                polygon.Add(new Vector2(x, z + chamfer));
                polygon.Add(new Vector2(x - chamfer, z));
                break;
        }
    }

    private static void AddTopRect(
        List<Vector3> vertices,
        List<int> triangles,
        float minX,
        float maxX,
        float minZ,
        float maxZ,
        float y)
    {
        if (maxX <= minX || maxZ <= minZ)
        {
            return;
        }
        AddTopPolygon(vertices, triangles, new List<Vector2>
        {
            new(minX, minZ),
            new(minX, maxZ),
            new(maxX, maxZ),
            new(maxX, minZ)
        }, y);
    }

    private static void AddChamferRim(
        List<Vector3> vertices,
        List<int> triangles,
        float y,
        params Vector2[] points)
    {
        AddTopPolygon(vertices, triangles, new List<Vector2>(points), y);
    }

    private static void AddTopPolygon(
        List<Vector3> vertices,
        List<int> triangles,
        List<Vector2> polygon,
        float y)
    {
        if (polygon.Count < 3)
        {
            return;
        }

        float signedArea = 0f;
        Vector2 center = Vector2.zero;
        for (int i = 0; i < polygon.Count; i++)
        {
            Vector2 current = polygon[i];
            Vector2 next = polygon[(i + 1) % polygon.Count];
            signedArea += current.x * next.y - next.x * current.y;
            center += current;
        }
        if (signedArea > 0f)
        {
            polygon.Reverse();
        }
        center /= polygon.Count;

        int centerIndex = vertices.Count;
        vertices.Add(new Vector3(center.x, y, center.y));
        int first = vertices.Count;
        foreach (Vector2 point in polygon)
        {
            vertices.Add(new Vector3(point.x, y, point.y));
        }
        for (int i = 0; i < polygon.Count; i++)
        {
            triangles.Add(centerIndex);
            triangles.Add(first + i);
            triangles.Add(first + (i + 1) % polygon.Count);
        }
    }

    private static void AddBox(
        List<Vector3> vertices,
        List<int> triangles,
        Vector3 center,
        float sizeX,
        float sizeZ,
        float bottomY,
        float topY)
    {
        float halfX = sizeX * 0.5f;
        float halfZ = sizeZ * 0.5f;
        AddTopRect(vertices, triangles,
            center.x - halfX, center.x + halfX,
            center.z - halfZ, center.z + halfZ, topY);
        AddDoubleSidedWall(vertices, triangles,
            new Vector2(center.x - halfX, center.z - halfZ),
            new Vector2(center.x - halfX, center.z + halfZ), topY, bottomY);
        AddDoubleSidedWall(vertices, triangles,
            new Vector2(center.x - halfX, center.z + halfZ),
            new Vector2(center.x + halfX, center.z + halfZ), topY, bottomY);
        AddDoubleSidedWall(vertices, triangles,
            new Vector2(center.x + halfX, center.z + halfZ),
            new Vector2(center.x + halfX, center.z - halfZ), topY, bottomY);
        AddDoubleSidedWall(vertices, triangles,
            new Vector2(center.x + halfX, center.z - halfZ),
            new Vector2(center.x - halfX, center.z - halfZ), topY, bottomY);
    }

    private static void AddDoubleSidedWall(
        List<Vector3> vertices,
        List<int> triangles,
        Vector2 from,
        Vector2 to,
        float topY,
        float bottomY)
    {
        int index = vertices.Count;
        vertices.Add(new Vector3(from.x, bottomY, from.y));
        vertices.Add(new Vector3(from.x, topY, from.y));
        vertices.Add(new Vector3(to.x, topY, to.y));
        vertices.Add(new Vector3(to.x, bottomY, to.y));

        // Boundary points are supplied clockwise from the top view. Using
        // tangent x up produces an outward-facing normal for every wall.
        triangles.Add(index);
        triangles.Add(index + 3);
        triangles.Add(index + 2);
        triangles.Add(index);
        triangles.Add(index + 2);
        triangles.Add(index + 1);
    }

    private static Mesh CreateMesh(
        string name,
        List<Vector3> vertices,
        List<int> triangles)
    {
        var mesh = new Mesh
        {
            name = name,
            hideFlags = HideFlags.DontSave
        };
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        if (vertices.Count > 0)
        {
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }
        return mesh;
    }
}
