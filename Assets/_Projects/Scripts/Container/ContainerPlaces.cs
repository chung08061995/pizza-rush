using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerPlaces : MonoBehaviour
{
    [SerializeField] private List<ContainerPlace> places = new ();
    [SerializeField] private Vector2Int partPosition;

    public List<ContainerPlace> Places => places;
    public Vector2Int PartPosition => partPosition;
}
