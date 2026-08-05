using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerPlaces : MonoBehaviour
{
    [SerializeField] private List<ContainerPlace> places = new ();
    [SerializeField] private Vector2Int partPosition;

    public List<ContainerPlace> Places => places;
    public Vector2Int PartPosition => partPosition;

    private PizzaQuarterAssemblyVisual quarterAssemblyVisual;

    private void Awake()
    {
        quarterAssemblyVisual = GetComponent<PizzaQuarterAssemblyVisual>();
        if (quarterAssemblyVisual == null)
        {
            quarterAssemblyVisual = gameObject.AddComponent<PizzaQuarterAssemblyVisual>();
        }
        quarterAssemblyVisual.Initialize(this);
    }

    internal void NotifyAssigned(ContainerPlace place)
    {
        quarterAssemblyVisual?.NotifyAssigned(place);
    }

    internal void NotifyLanded(ContainerPlace place, bool animate)
    {
        quarterAssemblyVisual?.NotifyLanded(place, animate);
    }

    internal void NotifyRemoved(ContainerPlace place)
    {
        quarterAssemblyVisual?.NotifyRemoved(place);
    }

    public void RefreshQuarterAssemblyImmediate()
    {
        quarterAssemblyVisual?.RefreshImmediate();
    }
}
