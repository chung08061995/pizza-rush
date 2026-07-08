using System.Collections.Generic;
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
