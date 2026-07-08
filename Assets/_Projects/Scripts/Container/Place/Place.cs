using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class Place : DraftUtils.DraftMonoBehaviour
{
    [ShowInInspector][ReadOnly] private Production _production;

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
    }
}
