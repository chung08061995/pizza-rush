using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ContainerMaterialView : DraftUtils.DraftMonoBehaviour
{
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
            part.gameObject.SetActive(true);
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
