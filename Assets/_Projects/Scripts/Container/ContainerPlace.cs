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
    public Transform Pin => pin;
    public Transform Pizza => pizza;
    public Production Production => _production;

    public void SetProduction(Production production)
    {
        _production = production;
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
                _production.gameObject.SetActive(false);
                _production.transform.SetParent(null);
            }
            _production = null;
        }

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
            production.transform.SetParent(null, true);
        }

        return production;
    }
}
