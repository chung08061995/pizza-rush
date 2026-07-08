using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverFactory : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] private GameObject container_Straight_1x1;
    [SerializeField] private GameObject container_Straight_1x2;
    [SerializeField] private GameObject container_Straight_1x3;
    [SerializeField] private GameObject container_L_1x2;
    [SerializeField] private GameObject container_L_1x1;
    [SerializeField] private GameObject container_Rectangle_2x2;
    [SerializeField] private GameObject container_Rectangle_T;
    [SerializeField] private GameObject container_Rectangle_Plus;
    

    public bool TryGetPrefab(
        ContainerShapeType containerShapeType,
        out GameObject coverPrefab)
    {
        coverPrefab = null;

        if (containerShapeType == ContainerShapeType.L_1x2)
        {
            coverPrefab = container_L_1x2;
        }
        if (containerShapeType == ContainerShapeType.L_1x1)
        {
            coverPrefab = container_L_1x1;
        }
        if (containerShapeType == ContainerShapeType.Rectangle_1x1)
        {
            coverPrefab = container_Straight_1x1;
        }
        if (containerShapeType == ContainerShapeType.Rectangle_1x2)
        {
            coverPrefab = container_Straight_1x2;
        }
        if (containerShapeType == ContainerShapeType.Rectangle_1x3)
        {
            coverPrefab = container_Straight_1x3;
        }
        if (containerShapeType == ContainerShapeType.Rectangle_2x2)
        {
            coverPrefab = container_Rectangle_2x2;
        }
        if (containerShapeType == ContainerShapeType.T)
        {
            coverPrefab = container_Rectangle_T;
        }
        if (containerShapeType == ContainerShapeType.Plus)
        {
            coverPrefab = container_Rectangle_Plus;
        }
        return coverPrefab != null;
    }
}
