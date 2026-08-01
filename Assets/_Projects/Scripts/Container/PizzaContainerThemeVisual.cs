using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Adds the approved pizza-box surface without changing container physics or level data.
/// </summary>
public sealed class PizzaContainerThemeVisual : MonoBehaviour
{
    private const string VisualRootName = "__PizzaContainerTheme";
    private static readonly Dictionary<ColorType, Material> SignalMaterials = new();
    private static Material baseMaterial;
    private static Material lidMaterial;
    private static Material stripeBackingMaterial;
    private static Material sealMaterial;

    private Transform visualRoot;

    public void Apply(ContainerData data, IReadOnlyList<Vector2Int> occupiedCells)
    {
        Clear();
        if (data == null || occupiedCells == null || occupiedCells.Count == 0 ||
            data.containerMaterialType == ContainerMaterialType.Ice)
        {
            return;
        }

        visualRoot = new GameObject(VisualRootName).transform;
        visualRoot.SetParent(transform, false);
        visualRoot.gameObject.layer = gameObject.layer;

        var occupied = new HashSet<Vector2Int>(occupiedCells);
        var signalType = data.isStone ? ColorType.Gray : data.containerColorData.colorType;
        var signalMaterial = GetSignalMaterial(signalType);

        foreach (var cell in occupiedCells)
        {
            var connectedX = occupied.Contains(cell + Vector2Int.left) || occupied.Contains(cell + Vector2Int.right);
            var connectedZ = occupied.Contains(cell + Vector2Int.up) || occupied.Contains(cell + Vector2Int.down);
            var center = new Vector3(cell.x, 0f, cell.y);

            CreateCube("BoxBase", center + Vector3.up * 0.215f,
                new Vector3(connectedX ? 1.02f : 0.90f, 0.10f, connectedZ ? 1.02f : 0.90f),
                GetBaseMaterial());
            CreateCube("KraftLid", center + Vector3.up * 0.285f,
                new Vector3(connectedX ? 1.02f : 0.84f, 0.075f, connectedZ ? 1.02f : 0.84f),
                GetLidMaterial());

            CreateCube("VerticalStripeBacking", center + Vector3.up * 0.340f,
                new Vector3(0.09f, 0.016f, 0.50f), GetStripeBackingMaterial());
            CreateCube("VerticalStripe", center + Vector3.up * 0.350f,
                new Vector3(0.05f, 0.016f, 0.50f), signalMaterial);
            CreateCube("HorizontalStripeBacking", center + Vector3.up * 0.340f,
                new Vector3(0.50f, 0.016f, 0.09f), GetStripeBackingMaterial());
            CreateCube("HorizontalStripe", center + Vector3.up * 0.350f,
                new Vector3(0.50f, 0.016f, 0.05f), signalMaterial);
            CreateCube("Seal", center + Vector3.up * 0.370f,
                new Vector3(0.09f, 0.008f, 0.09f), GetSealMaterial());
        }
    }

    private void Clear()
    {
        if (visualRoot == null)
        {
            visualRoot = transform.Find(VisualRootName);
        }
        if (visualRoot == null) return;

        if (Application.isPlaying) Destroy(visualRoot.gameObject);
        else DestroyImmediate(visualRoot.gameObject);
        visualRoot = null;
    }

    private void CreateCube(string objectName, Vector3 localPosition, Vector3 localScale, Material material)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = objectName;
        cube.layer = gameObject.layer;
        cube.transform.SetParent(visualRoot, false);
        cube.transform.localPosition = localPosition;
        cube.transform.localScale = localScale;
        var collider = cube.GetComponent<Collider>();
        if (Application.isPlaying) Destroy(collider);
        else DestroyImmediate(collider);
        cube.GetComponent<Renderer>().sharedMaterial = material;
    }

    private static Material GetBaseMaterial() =>
        baseMaterial != null ? baseMaterial : baseMaterial = CreateMaterial("Pizza Box Base", new Color(0.63f, 0.35f, 0.14f));

    private static Material GetLidMaterial() =>
        lidMaterial != null ? lidMaterial : lidMaterial = CreateMaterial("Pizza Box Kraft Lid", new Color(0.88f, 0.72f, 0.43f));

    private static Material GetStripeBackingMaterial() =>
        stripeBackingMaterial != null ? stripeBackingMaterial : stripeBackingMaterial = CreateMaterial("Pizza Box Stripe Backing", new Color(0.13f, 0.085f, 0.05f));

    private static Material GetSealMaterial() =>
        sealMaterial != null ? sealMaterial : sealMaterial = CreateMaterial("Pizza Box Seal", new Color(0.95f, 0.42f, 0.02f));

    private static Material GetSignalMaterial(ColorType colorType)
    {
        if (SignalMaterials.TryGetValue(colorType, out var material) && material != null) return material;
        var color = Color.white;
        DataManager.Instance.ProductionLineColorsSO.TryGetValue(colorType, out color);
        material = CreateMaterial($"Pizza Box Signal {colorType}", color);
        SignalMaterials[colorType] = material;
        return material;
    }

    private static Material CreateMaterial(string materialName, Color color)
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var material = new Material(shader) { name = materialName, color = color };
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.28f);
        return material;
    }
}
