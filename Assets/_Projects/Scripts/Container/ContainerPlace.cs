using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ContainerPlace : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] private Transform pin;
    [SerializeField] private Transform pizza;

    [ShowInInspector][ReadOnly] private Production _production;
    private ContainerPlaces owner;
    public Transform Pin => pin;
    public Transform Pizza => pizza;
    public Production Production => _production;

    private void Awake()
    {
        owner = GetComponentInParent<ContainerPlaces>();
    }

    public void SetProduction(Production production)
    {
        _production = production;
        owner ??= GetComponentInParent<ContainerPlaces>();
        owner?.NotifyAssigned(this);
    }

    public void NotifyLanded(bool animate = true)
    {
        owner ??= GetComponentInParent<ContainerPlaces>();
        owner?.NotifyLanded(this, animate);
    }

    public bool Empty()
    {
        return _production == null;
    }
    public bool IsFull()
    {
        return _production != null;
    }

    public void ClearProduction()
    {
        if (_production != null)
        {
            if (_production.gameObject != null)
            {
                _production.transform.DOKill();
                _production.QuarterVisual?.SetAssemblyMode(false);
                _production.QuarterVisual?.SetCompletionFlash(0f);
                _production.gameObject.SetActive(false);
                _production.transform.SetParent(null);
            }
            _production = null;
        }

        owner ??= GetComponentInParent<ContainerPlaces>();
        owner?.NotifyRemoved(this);

        if (pin != null)
        {
            pin.DOKill();
            pin.localScale = Vector3.one;
        }
    }

    public Production DetachProduction()
    {
        var production = _production;
        _production = null;

        if (production != null && production.gameObject != null)
        {
            production.transform.DOKill();
            production.QuarterVisual?.SetAssemblyMode(false);
            production.transform.SetParent(null, true);
        }

        owner ??= GetComponentInParent<ContainerPlaces>();
        owner?.NotifyRemoved(this);

        return production;
    }
}
