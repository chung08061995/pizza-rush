using Sirenix.OdinInspector;
using UnityEngine;

public class ContainerMovementView : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] private DraftUtils.OptionalValue<GameObject> horizontalDirectionObject = new();
    [SerializeField] private DraftUtils.OptionalValue<GameObject> verticalDirectionObject = new();
    [SerializeField] private float surfaceOffset = 0.03f;

    private ContainerData _data;

    public void SetData(ContainerData data)
    {
        _data = data;
        SetHorizontalDirectionObject();
        SetVerticalDirectionObject();
        AlignCanvasToContainerBounds();
    }

    [Button]
    public void Reload()
    {
        if (_data != null)
        {
            SetData(_data);
        }
    }
    private void SetHorizontalDirectionObject()
    {
        if (!horizontalDirectionObject.isPresent)
        {
            return;
        }
        horizontalDirectionObject.value.SetActive(_data.containerMovementType == ContainerMovementType.Horizontal);
    }
    private void SetVerticalDirectionObject()
    {
        if (!verticalDirectionObject.isPresent)
        {
            return;
        }
        verticalDirectionObject.value.SetActive(_data.containerMovementType == ContainerMovementType.Vertical);
    }

    private void AlignCanvasToContainerBounds()
    {
        var container = GetComponentInParent<Container>();
        var canvas = GetComponentInParent<Canvas>();
        if (container == null || canvas == null || canvas.transform is not RectTransform canvasRect)
        {
            return;
        }

        var renderers = container.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            return;
        }

        var bounds = renderers[0].bounds;
        for (var i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        canvasRect.position = new Vector3(bounds.center.x, bounds.max.y + surfaceOffset, bounds.center.z);
        canvasRect.rotation = Quaternion.Euler(90f, 0f, 0f);

        var scaleX = Mathf.Abs(canvasRect.lossyScale.x);
        var scaleY = Mathf.Abs(canvasRect.lossyScale.y);
        if (scaleX <= Mathf.Epsilon || scaleY <= Mathf.Epsilon)
        {
            return;
        }

        canvasRect.sizeDelta = new Vector2(bounds.size.x / scaleX, bounds.size.z / scaleY);
    }
}
