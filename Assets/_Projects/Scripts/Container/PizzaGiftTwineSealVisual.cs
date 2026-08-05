using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Replaces the legacy gift ribbon/bow on a completed pizza-box lid with the
/// approved kraft parcel treatment: thin crossed twine and a wax seal.
/// This component is presentation-only and never adds colliders.
/// </summary>
public sealed class PizzaGiftTwineSealVisual : MonoBehaviour
{
    private const string GeneratedRootName = "__PizzaGiftTwineSeal";
    private const float StrandWidth = 0.026f;
    private const float StrandHeight = 0.032f;
    private const float StrandSpacing = 0.034f;
    private const float SealRadius = 0.13f;
    private const float SealHeight = 0.026f;

    private static readonly Color TwineLightColor = new(0.89f, 0.74f, 0.44f, 1f);
    private static readonly Color TwineDarkColor = new(0.63f, 0.43f, 0.22f, 1f);
    private static readonly Color KraftLidColor = new(0.78f, 0.52f, 0.27f, 1f);
    private static readonly Color WaxColor = new(0.55f, 0.055f, 0.035f, 1f);
    private static readonly Color WaxStampColor = new(0.72f, 0.11f, 0.065f, 1f);

    private static Material twineLightMaterial;
    private static Material twineDarkMaterial;
    private static Material waxMaterial;
    private static Material waxStampMaterial;

    private readonly List<Mesh> generatedMeshes = new();
    private Transform generatedRoot;

    public static void ApplyTo(GameObject coverInstance)
    {
        if (coverInstance == null)
        {
            return;
        }

        var visual = coverInstance.GetComponent<PizzaGiftTwineSealVisual>();
        if (visual == null)
        {
            visual = coverInstance.AddComponent<PizzaGiftTwineSealVisual>();
        }
        visual.Build();
    }

    private void Build()
    {
        ClearGenerated();

        Transform lidRoot = FindDescendant(transform, "GiftLidOptionD") ?? transform;
        Transform horizontal = FindDescendant(lidRoot, "RibbonHorizontal");
        Transform vertical = FindDescendant(lidRoot, "RibbonVertical");
        Transform bow = FindDescendant(lidRoot, "KenneyBow");

        Vector3 sealLocalPosition = bow != null
            ? lidRoot.InverseTransformPoint(bow.position)
            : Vector3.zero;

        TintKraftLid(transform);
        SetDecorationActive(lidRoot, "AccentHorizontal", false);
        SetDecorationActive(lidRoot, "AccentVertical", false);
        if (bow != null)
        {
            bow.gameObject.SetActive(false);
        }

        generatedRoot = new GameObject(GeneratedRootName).transform;
        generatedRoot.SetParent(lidRoot, false);
        generatedRoot.gameObject.layer = gameObject.layer;

        float highestTwineY = sealLocalPosition.y;
        if (horizontal != null)
        {
            highestTwineY = Mathf.Max(
                highestTwineY,
                AddTwinePair(horizontal, lidRoot, true));
            horizontal.gameObject.SetActive(false);
        }
        if (vertical != null)
        {
            highestTwineY = Mathf.Max(
                highestTwineY,
                AddTwinePair(vertical, lidRoot, false));
            vertical.gameObject.SetActive(false);
        }

        sealLocalPosition.y = highestTwineY + SealHeight * 0.85f;
        AddWaxSeal(sealLocalPosition);
    }

    private float AddTwinePair(
        Transform source,
        Transform lidRoot,
        bool horizontal)
    {
        var sourceFilter = source.GetComponent<MeshFilter>();
        if (sourceFilter == null || sourceFilter.sharedMesh == null)
        {
            return lidRoot.InverseTransformPoint(source.position).y;
        }

        Vector3 sourcePosition = lidRoot.InverseTransformPoint(source.position);
        Quaternion sourceRotation = Quaternion.Inverse(lidRoot.rotation) * source.rotation;
        Vector3 sourceScale = source.lossyScale;
        Vector3 rootScale = lidRoot.lossyScale;
        sourceScale = new Vector3(
            SafeDivide(sourceScale.x, rootScale.x),
            SafeDivide(sourceScale.y, rootScale.y),
            SafeDivide(sourceScale.z, rootScale.z));

        Vector3 strandScale = horizontal
            ? new Vector3(sourceScale.x, StrandHeight, StrandWidth)
            : new Vector3(StrandWidth, StrandHeight, sourceScale.z);
        Vector3 spacing = horizontal
            ? new Vector3(0f, 0f, StrandSpacing * 0.5f)
            : new Vector3(StrandSpacing * 0.5f, 0f, 0f);

        AddStrand(
            sourceFilter.sharedMesh,
            sourcePosition - spacing,
            sourceRotation,
            strandScale,
            GetTwineLightMaterial(),
            horizontal ? "TwineHorizontalLight" : "TwineVerticalLight");
        AddStrand(
            sourceFilter.sharedMesh,
            sourcePosition + spacing,
            sourceRotation,
            strandScale,
            GetTwineDarkMaterial(),
            horizontal ? "TwineHorizontalDark" : "TwineVerticalDark");

        return sourcePosition.y + StrandHeight * 0.5f;
    }

    private void AddStrand(
        Mesh mesh,
        Vector3 localPosition,
        Quaternion localRotation,
        Vector3 localScale,
        Material material,
        string objectName)
    {
        var strand = new GameObject(objectName);
        strand.layer = gameObject.layer;
        strand.transform.SetParent(generatedRoot, false);
        strand.transform.localPosition = localPosition;
        strand.transform.localRotation = localRotation;
        strand.transform.localScale = localScale;
        strand.AddComponent<MeshFilter>().sharedMesh = mesh;
        ConfigureRenderer(strand.AddComponent<MeshRenderer>(), material);
    }

    private void AddWaxSeal(Vector3 localPosition)
    {
        Mesh sealMesh = BuildIrregularDisc("Pizza Gift Wax Seal", SealRadius, SealHeight, 18, 0.075f);
        generatedMeshes.Add(sealMesh);
        var seal = AddMeshObject("WaxSeal", sealMesh, GetWaxMaterial());
        seal.transform.localPosition = localPosition;

        Mesh stampMesh = BuildIrregularDisc(
            "Pizza Gift Wax Stamp",
            SealRadius * 0.54f,
            SealHeight * 0.35f,
            14,
            0.025f);
        generatedMeshes.Add(stampMesh);
        var stamp = AddMeshObject("WaxStamp", stampMesh, GetWaxStampMaterial());
        stamp.transform.localPosition = localPosition + Vector3.up * (SealHeight * 0.64f);
    }

    private GameObject AddMeshObject(string objectName, Mesh mesh, Material material)
    {
        var meshObject = new GameObject(objectName);
        meshObject.layer = gameObject.layer;
        meshObject.transform.SetParent(generatedRoot, false);
        meshObject.AddComponent<MeshFilter>().sharedMesh = mesh;
        ConfigureRenderer(meshObject.AddComponent<MeshRenderer>(), material);
        return meshObject;
    }

    private static Mesh BuildIrregularDisc(
        string meshName,
        float radius,
        float height,
        int segments,
        float irregularity)
    {
        var vertices = new List<Vector3>(segments * 2 + 2);
        var triangles = new List<int>(segments * 12);
        float halfHeight = height * 0.5f;
        vertices.Add(new Vector3(0f, halfHeight, 0f));
        vertices.Add(new Vector3(0f, -halfHeight, 0f));

        for (int index = 0; index < segments; index++)
        {
            float angle = index * Mathf.PI * 2f / segments;
            float variation = 1f + Mathf.Sin(index * 4.73f) * irregularity;
            float x = Mathf.Cos(angle) * radius * variation;
            float z = Mathf.Sin(angle) * radius * variation;
            vertices.Add(new Vector3(x, halfHeight, z));
            vertices.Add(new Vector3(x, -halfHeight, z));
        }

        for (int index = 0; index < segments; index++)
        {
            int next = (index + 1) % segments;
            int top = 2 + index * 2;
            int bottom = top + 1;
            int nextTop = 2 + next * 2;
            int nextBottom = nextTop + 1;

            triangles.Add(0);
            triangles.Add(nextTop);
            triangles.Add(top);
            triangles.Add(1);
            triangles.Add(bottom);
            triangles.Add(nextBottom);
            triangles.Add(top);
            triangles.Add(nextTop);
            triangles.Add(bottom);
            triangles.Add(nextTop);
            triangles.Add(nextBottom);
            triangles.Add(bottom);
        }

        var mesh = new Mesh { name = meshName };
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private static void SetDecorationActive(Transform root, string objectName, bool active)
    {
        Transform decoration = FindDescendant(root, objectName);
        if (decoration != null)
        {
            decoration.gameObject.SetActive(active);
        }
    }

    private static void TintKraftLid(Transform lidRoot)
    {
        var propertyBlock = new MaterialPropertyBlock();
        foreach (Renderer renderer in lidRoot.GetComponentsInChildren<Renderer>(true))
        {
            string objectName = renderer.name;
            if (objectName == "RibbonHorizontal" ||
                objectName == "RibbonVertical" ||
                objectName == "AccentHorizontal" ||
                objectName == "AccentVertical" ||
                objectName == "KenneyBow" ||
                objectName.StartsWith("Twine") ||
                objectName.StartsWith("Wax"))
            {
                continue;
            }

            renderer.GetPropertyBlock(propertyBlock);
            if (renderer.sharedMaterial != null && renderer.sharedMaterial.HasProperty("_BaseColor"))
            {
                propertyBlock.SetColor("_BaseColor", KraftLidColor);
            }
            if (renderer.sharedMaterial != null && renderer.sharedMaterial.HasProperty("_Color"))
            {
                propertyBlock.SetColor("_Color", KraftLidColor);
            }
            renderer.SetPropertyBlock(propertyBlock);
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }
    }

    private static Transform FindDescendant(Transform root, string objectName)
    {
        if (root == null)
        {
            return null;
        }
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == objectName)
            {
                return child;
            }
        }
        return null;
    }

    private static float SafeDivide(float value, float divisor)
    {
        return Mathf.Abs(divisor) <= Mathf.Epsilon ? value : value / divisor;
    }

    private static void ConfigureRenderer(MeshRenderer renderer, Material material)
    {
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.lightProbeUsage = LightProbeUsage.Off;
        renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
    }

    private static Material GetTwineLightMaterial()
    {
        return twineLightMaterial ??= CreateMaterial("Pizza Gift Twine Light", TwineLightColor, 0.08f);
    }

    private static Material GetTwineDarkMaterial()
    {
        return twineDarkMaterial ??= CreateMaterial("Pizza Gift Twine Dark", TwineDarkColor, 0.06f);
    }

    private static Material GetWaxMaterial()
    {
        return waxMaterial ??= CreateMaterial("Pizza Gift Wax Seal", WaxColor, 0.42f);
    }

    private static Material GetWaxStampMaterial()
    {
        return waxStampMaterial ??= CreateMaterial("Pizza Gift Wax Stamp", WaxStampColor, 0.3f);
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

    private void ClearGenerated()
    {
        if (generatedRoot == null)
        {
            generatedRoot = FindDescendant(transform, GeneratedRootName);
        }
        if (generatedRoot != null)
        {
            if (Application.isPlaying)
            {
                Destroy(generatedRoot.gameObject);
            }
            else
            {
                DestroyImmediate(generatedRoot.gameObject);
            }
            generatedRoot = null;
        }

        foreach (Mesh mesh in generatedMeshes)
        {
            if (mesh == null)
            {
                continue;
            }
            if (Application.isPlaying)
            {
                Destroy(mesh);
            }
            else
            {
                DestroyImmediate(mesh);
            }
        }
        generatedMeshes.Clear();
    }

    private void OnDestroy()
    {
        ClearGenerated();
    }
}
