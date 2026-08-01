using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ContainerMaterialView : DraftUtils.DraftMonoBehaviour
{
    private static readonly int BaseMapStId = Shader.PropertyToID("_BaseMap_ST");

    [SerializeField] private Transform colorObject;
    [SerializeField] private Transform noAsignObject;
    [SerializeField] private Transform iceObject;
    private ContainerMaterialType _data;

    public void SetData(ContainerMaterialType data)
    {
        _data = data;
        ShowSingleMaterialObject();
    }

    /// <summary>
    /// Align the ice overlay to the actual occupied cells of the container.
    /// Some legacy shape prefabs contain a fixed vertical stack of IceParts;
    /// that stack can extend outside rotated or non-rectangular shapes.
    /// </summary>
    public void SetData(ContainerMaterialType data, IReadOnlyList<Vector2Int> occupiedCells)
    {
        SetData(data);
        if (data == ContainerMaterialType.Ice && occupiedCells != null)
        {
            NormalizeIceParts(occupiedCells);
        }
    }

    private void NormalizeIceParts(IReadOnlyList<Vector2Int> occupiedCells)
    {
        if (iceObject == null)
        {
            return;
        }

        // The old Ice prefab includes a gray perimeter intended for the
        // previous container shell. The pizza-box surface owns the silhouette
        // now, so keep the blue ice parts but hide that redundant perimeter.
        var legacyBorders = iceObject.Find("Borders");
        if (legacyBorders != null)
        {
            foreach (var renderer in legacyBorders.GetComponentsInChildren<Renderer>(true))
            {
                renderer.enabled = false;
            }
        }

        var partsRoot = iceObject.Find("Parts");
        if (partsRoot == null)
        {
            return;
        }

        var iceParts = partsRoot.Cast<Transform>()
            .Where(child => child != null && child.name.StartsWith("IcePart", System.StringComparison.Ordinal))
            .ToList();
        if (iceParts.Count == 0)
        {
            return;
        }

        var minX = occupiedCells.Min(cell => cell.x);
        var maxX = occupiedCells.Max(cell => cell.x);
        var minY = occupiedCells.Min(cell => cell.y);
        var maxY = occupiedCells.Max(cell => cell.y);
        var width = maxX - minX + 1;
        var height = maxY - minY + 1;
        var largestDimension = Mathf.Max(width, height);
        var uvScale = 1f / largestDimension;
        var horizontalPadding = (largestDimension - width) * 0.5f;
        var verticalPadding = (largestDimension - height) * 0.5f;

        var template = iceParts[0];
        for (var index = iceParts.Count; index < occupiedCells.Count; index++)
        {
            var generated = Instantiate(template, partsRoot);
            generated.name = $"IcePart (Generated {index + 1})";
            iceParts.Add(generated);
        }

        for (var index = 0; index < iceParts.Count; index++)
        {
            var part = iceParts[index];
            if (index >= occupiedCells.Count)
            {
                part.gameObject.SetActive(false);
                continue;
            }

            var cell = occupiedCells[index];
            part.localPosition = new Vector3(cell.x, part.localPosition.y, cell.y);
            part.localRotation = Quaternion.identity;
            part.localScale = Vector3.one;
            part.gameObject.SetActive(true);

            // Project one continuous ice texture across the complete footprint
            // instead of restarting the pattern on every occupied cell.
            var textureOffset = new Vector2(
                (cell.x - minX + horizontalPadding) * uvScale,
                (cell.y - minY + verticalPadding) * uvScale);
            ApplyIceSurface(part, uvScale, textureOffset);

            // Ice is a visual/progress layer, not a physics obstacle. Its
            // prefab contains trigger colliders that can otherwise interfere
            // with production-line overlap checks after generated parts are
            // added for larger shapes.
            foreach (var collider in part.GetComponentsInChildren<Collider>(true))
            {
                collider.enabled = false;
            }
        }
    }

    private static void ApplyIceSurface(Transform part, float uvScale, Vector2 textureOffset)
    {
        var propertyBlock = new MaterialPropertyBlock();
        foreach (var meshRenderer in part.GetComponentsInChildren<MeshRenderer>(true))
        {
            if (!meshRenderer.gameObject.activeSelf)
            {
                continue;
            }

            // Legacy shape prefabs trim and offset individual IcePart meshes
            // to fit the old border. With the border removed every occupied
            // cell must use a complete slab so neighbouring cells meet.
            var surfaceTransform = meshRenderer.transform;
            surfaceTransform.localPosition = new Vector3(0f, surfaceTransform.localPosition.y, 0f);
            surfaceTransform.localRotation = Quaternion.identity;
            surfaceTransform.localScale = new Vector3(1f, 0.08f, 1f);

            meshRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetVector(
                BaseMapStId,
                new Vector4(uvScale, uvScale, textureOffset.x, textureOffset.y));
            meshRenderer.SetPropertyBlock(propertyBlock);
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            meshRenderer.receiveShadows = true;
            propertyBlock.Clear();
        }
    }

    private void ShowSingleMaterialObject()
    {
        var allMaterials = new List<Transform>()
        {
            colorObject,
            noAsignObject,
            iceObject
        };
        allMaterials.ForEach(x => x.gameObject.SetActive(false));
        Transform selectMaterial = colorObject;
        if (_data == ContainerMaterialType.Color)
        {
            selectMaterial = colorObject;
        }
        if (_data == ContainerMaterialType.NoAsign)
        {
            selectMaterial = noAsignObject;
        }
        if (_data == ContainerMaterialType.Ice)
        {
            selectMaterial = iceObject;
        }
        selectMaterial.gameObject.SetActive(true);
    }
}
