using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedUI.EnhancedScroller;
using System;

public class IapContainerItem : EnhancedScrollerCellView, ICellView1<MultipleIAPData>
{
    [SerializeField] private MultipleIAPDataView noAdsView;
    [SerializeField] private MultipleIAPDataView noAdsBundleView;
    [SerializeField] private MultipleIAPDataView smallBundleView;
    [SerializeField] private MultipleIAPDataView mediumBundleView;
    [SerializeField] private MultipleIAPDataView largeBundleView;
    [SerializeField] private MultipleIAPDataView starterView;

    public List<MultipleIAPDataView> Items => new()
    {
        noAdsView,
        noAdsBundleView,
        smallBundleView, 
        mediumBundleView, 
        largeBundleView,
        starterView
    };

    public void SetData(MultipleIAPData data, int index)
    {
        SetData(data);
    }

    public void SetData(MultipleIAPData data)
    {
        noAdsView.gameObject.SetActive(false);
        noAdsBundleView.gameObject.SetActive(false);
        smallBundleView.gameObject.SetActive(false);
        mediumBundleView.gameObject.SetActive(false);
        largeBundleView.gameObject.SetActive(false);
        starterView.gameObject.SetActive(false);

        if (data == null) return;

        MultipleIAPDataView targetView = null;
        switch (data.itemType)
        {
            case ItemType.MultipleIAPData_NoAds:
                targetView = noAdsView;
                break;
            case ItemType.MultipleIAPData_NoAdsBundle:
                targetView = noAdsBundleView;
                break;
            case ItemType.MultipleIAPData_SmallBundle:
                targetView = smallBundleView;
                break;
            case ItemType.MultipleIAPData_MediumBundle:
                targetView = mediumBundleView;
                break;
            case ItemType.MultipleIAPData_LargeBundle:
                targetView = largeBundleView;
                break;
            case ItemType.MultipleIAPData_Starter:
                targetView = starterView;
                break;
        }

        if (targetView != null)
        {
            targetView.gameObject.SetActive(true);
            targetView.SetData(data);
        }
    }
}
