using Sirenix.OdinInspector;
using UnityEngine;

public class ContainerMovementView : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] private DraftUtils.OptionalValue<GameObject> horizontalDirectionObject = new();
    [SerializeField] private DraftUtils.OptionalValue<GameObject> verticalDirectionObject = new();

    private ContainerData _data;

    public void SetData(ContainerData data)
    {
        _data = data;
        SetHorizontalDirectionObject();
        SetVerticalDirectionObject();
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
}
