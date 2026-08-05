using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builds presentation-only meshes from a container's occupied grid cells.
/// The generated geometry has no colliders and does not alter gameplay bounds.
/// </summary>
internal static class PizzaBoxFootprintMeshBuilder
{
    internal static Mesh BuildIngredientIcon(
        ColorType colorType,
        Vector2 center,
        float size,
        float topY)
    {
        var vertices = new List<Vector3>(32);
        var triangles = new List<int>(48);
        float half = size * 0.5f;

        switch (colorType)
        {
            case ColorType.Red: // pepperoni
                AddDisc(vertices, triangles, center, half, topY, 12);
                break;
            case ColorType.Green: // basil leaf
            case ColorType.Lime:
                AddLeaf(vertices, triangles, center, half, topY,
                    colorType == ColorType.Lime ? -28f : 24f);
                break;
            case ColorType.Blue: // olive ring
            case ColorType.Cyan: // onion ring
            case ColorType.DarkPurple:
                AddRing(vertices, triangles, center, half, half * 0.48f, topY, 12);
                break;
            case ColorType.White: // mozzarella
            case ColorType.Pink: // ham
                AddRoundedStamp(vertices, triangles, center, half, topY,
                    colorType == ColorType.Pink ? 18f : 45f);
                break;
            case ColorType.Orange: // pepper slice
                AddRing(vertices, triangles, center, half, half * 0.58f, topY, 8);
                break;
            case ColorType.Yellow: // cheese wedge
                AddTriangle(vertices, triangles, center, half, topY);
                break;
            case ColorType.Brown: // mushroom
            case ColorType.Gray:
                AddMushroom(vertices, triangles, center, half, topY);
                break;
            case ColorType.Violet: // eggplant
                AddLeaf(vertices, triangles, center, half, topY, -48f);
                break;
            case ColorType.Navy: // anchovy
                AddFish(vertices, triangles, center, half, topY);
                break;
            default:
                AddDisc(vertices, triangles, center, half, topY, 10);
                break;
        }

        return CreateMesh($"Pizza Ingredient Icon {colorType}", vertices, triangles);
    }

    private static void AddDisc(
        List<Vector3> vertices,
        List<int> triangles,
        Vector2 center,
        float radius,
        float y,
        int segments)
    {
        int centerIndex = vertices.Count;
        vertices.Add(new Vector3(center.x, y, center.y));
        for (int index = 0; index < segments; index++)
        {
            float angle = index * Mathf.PI * 2f / segments;
            vertices.Add(new Vector3(
                center.x + Mathf.Cos(angle) * radius,
                y,
                center.y + Mathf.Sin(angle) * radius));
        }
        for (int index = 0; index < segments; index++)
        {
            int next = (index + 1) % segments;
            triangles.Add(centerIndex);
            triangles.Add(centerIndex + next + 1);
            triangles.Add(centerIndex + index + 1);
        }
    }

    private static void AddRing(
        List<Vector3> vertices,
        List<int> triangles,
        Vector2 center,
        float outerRadius,
        float innerRadius,
        float y,
        int segments)
    {
        int start = vertices.Count;
        for (int index = 0; index < segments; index++)
        {
            float angle = index * Mathf.PI * 2f / segments;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);
            vertices.Add(new Vector3(center.x + cos * outerRadius, y, center.y + sin * outerRadius));
            vertices.Add(new Vector3(center.x + cos * innerRadius, y, center.y + sin * innerRadius));
        }
        for (int index = 0; index < segments; index++)
        {
            int next = (index + 1) % segments;
            int outer = start + index * 2;
            int inner = outer + 1;
            int nextOuter = start + next * 2;
            int nextInner = nextOuter + 1;
            triangles.Add(outer);
            triangles.Add(inner);
            triangles.Add(nextOuter);
            triangles.Add(nextOuter);
            triangles.Add(inner);
            triangles.Add(nextInner);
        }
    }

    private static void AddLeaf(
        List<Vector3> vertices,
        List<int> triangles,
        Vector2 center,
        float half,
        float y,
        float rotation)
    {
        AddRotatedPolygon(vertices, triangles, center, y, rotation,
            new Vector2(0f, half),
            new Vector2(-half * 0.72f, 0f),
            new Vector2(0f, -half),
            new Vector2(half * 0.72f, 0f));
    }

    private static void AddRoundedStamp(
        List<Vector3> vertices,
        List<int> triangles,
        Vector2 center,
        float half,
        float y,
        float rotation)
    {
        float corner = half * 0.64f;
        AddRotatedPolygon(vertices, triangles, center, y, rotation,
            new Vector2(-corner, half),
            new Vector2(corner, half),
            new Vector2(half, corner),
            new Vector2(half, -corner),
            new Vector2(corner, -half),
            new Vector2(-corner, -half),
            new Vector2(-half, -corner),
            new Vector2(-half, corner));
    }

    private static void AddTriangle(
        List<Vector3> vertices,
        List<int> triangles,
        Vector2 center,
        float half,
        float y)
    {
        AddTopPolygon(vertices, triangles, new List<Vector2>
        {
            center + new Vector2(0f, half),
            center + new Vector2(-half, -half * 0.75f),
            center + new Vector2(half, -half * 0.75f)
        }, y);
    }

    private static void AddMushroom(
        List<Vector3> vertices,
        List<int> triangles,
        Vector2 center,
        float half,
        float y)
    {
        AddTopRect(vertices, triangles,
            center.x - half * 0.25f,
            center.x + half * 0.25f,
            center.y - half,
            center.y + half * 0.18f,
            y);
        AddTopPolygon(vertices, triangles, new List<Vector2>
        {
            center + new Vector2(-half, half * 0.05f),
            center + new Vector2(-half * 0.72f, half * 0.72f),
            center + new Vector2(0f, half),
            center + new Vector2(half * 0.72f, half * 0.72f),
            center + new Vector2(half, half * 0.05f)
        }, y);
    }

    private static void AddFish(
        List<Vector3> vertices,
        List<int> triangles,
        Vector2 center,
        float half,
        float y)
    {
        AddTopPolygon(vertices, triangles, new List<Vector2>
        {
            center + new Vector2(-half * 0.55f, 0f),
            center + new Vector2(0f, half * 0.55f),
            center + new Vector2(half * 0.72f, 0f),
            center + new Vector2(0f, -half * 0.55f)
        }, y);
        AddTopPolygon(vertices, triangles, new List<Vector2>
        {
            center + new Vector2(-half * 0.45f, 0f),
            center + new Vector2(-half, half * 0.62f),
            center + new Vector2(-half, -half * 0.62f)
        }, y);
    }

    private static void AddRotatedPolygon(
        List<Vector3> vertices,
        List<int> triangles,
        Vector2 center,
        float y,
        float rotation,
        params Vector2[] points)
    {
        float radians = rotation * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);
        var polygon = new List<Vector2>(points.Length);
        foreach (Vector2 point in points)
        {
            polygon.Add(center + new Vector2(
                point.x * cos - point.y * sin,
                point.x * sin + point.y * cos));
        }
        AddTopPolygon(vertices, triangles, polygon, y);
    }

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

    internal static Mesh BuildOuterCornerDimples(
        IReadOnlyList<Vector2Int> cells,
        float topY,
        float outerInset,
        float centerInset,
        float radius)
    {
        var occupied = new HashSet<Vector2Int>(cells);
        var vertices = new List<Vector3>(cells.Count * 13);
        var triangles = new List<int>(cells.Count * 36);
        float edge = 0.5f - outerInset;

        foreach (Vector2Int cell in cells)
        {
            GetExposure(occupied, cell, out bool left, out bool right, out bool down, out bool up);
            float x0 = cell.x - edge;
            float x1 = cell.x + edge;
            float z0 = cell.y - edge;
            float z1 = cell.y + edge;

            if (left && down)
                AddDisc(vertices, triangles, new Vector2(x0 + centerInset, z0 + centerInset), radius, topY, 10);
            if (left && up)
                AddDisc(vertices, triangles, new Vector2(x0 + centerInset, z1 - centerInset), radius, topY, 10);
            if (right && up)
                AddDisc(vertices, triangles, new Vector2(x1 - centerInset, z1 - centerInset), radius, topY, 10);
            if (right && down)
                AddDisc(vertices, triangles, new Vector2(x1 - centerInset, z0 + centerInset), radius, topY, 10);
        }

        return CreateMesh("Premium Pizza Box Corner Dimples", vertices, triangles);
    }

    internal static Mesh BuildRoundStampLayer(
        IReadOnlyList<Vector2Int> cells,
        float topY,
        float radius)
    {
        var vertices = new List<Vector3>(cells.Count * 17);
        var triangles = new List<int>(cells.Count * 48);
        foreach (Vector2Int cell in cells)
        {
            AddDisc(
                vertices,
                triangles,
                new Vector2(cell.x, cell.y),
                radius,
                topY,
                16);
        }
        return CreateMesh("Premium Pizza Box Round Stamp Layer", vertices, triangles);
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
