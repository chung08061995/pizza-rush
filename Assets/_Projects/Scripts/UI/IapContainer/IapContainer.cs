using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedUI.EnhancedScroller;

public class IapContainer : MonoBehaviour
{
    [SerializeField] private DraftUtils.RebuildLayouts rebuilder;
    [SerializeField] private DraftUtils.EnhancedScroller1<IapContainerItem, MultipleIAPData> scroller = new();
    [SerializeField] private DraftUtils.Pooler<IapContainerDot> dotPooler = new();
    [SerializeField] private float minScale = 0.8f;

    private List<IapContainerDot> _dots = new();
    private int _currentCenteredIndex = -1;

    private void Start()
    {
        LoadData();
    }

    [Button]
    public void LoadData()
    {
        scroller.Initialize();
        var iapData = DataManager.Instance.iapData;
        if (iapData == null) return;

        var list = new List<MultipleIAPData>
        {
            iapData.noAds,
            iapData.noAdsBundle,
            iapData.smallBundle,
            iapData.mediumBundle,
            iapData.largeBundle,
            iapData.starter
        };

        scroller.UpdateData(list);

        // Spawn dots
        dotPooler.DespawnAll();
        _dots.Clear();
        dotPooler.Factory = new DraftUtils.ComponentInstantiatePoolFactory<IapContainerDot>();
        for (int i = 0; i < list.Count; i++)
        {
            var dot = dotPooler.Spawn();
            _dots.Add(dot);
        }

        _currentCenteredIndex = 0;
        UpdateDots();
        rebuilder.Rebuild();
    }

    private void LateUpdate()
    {
        UpdateCellScale();
    }

    private void UpdateDots()
    {
        for (int i = 0; i < _dots.Count; i++)
        {
            if (_dots[i] != null)
            {
                _dots[i].SetActiveState(i == _currentCenteredIndex);
            }
        }
    }

    private void UpdateCellScale()
    {
        var scrollerComponent = scroller.Scroller;
        if (scrollerComponent == null) return;

        RectTransform viewport = scrollerComponent.ScrollRect.viewport;
        if (viewport == null)
        {
            viewport = scrollerComponent.GetComponent<RectTransform>();
        }
        if (viewport == null) return;

        Vector3[] viewportCorners = new Vector3[4];
        viewport.GetWorldCorners(viewportCorners);

        bool isHorizontal = scrollerComponent.scrollDirection == EnhancedScroller.ScrollDirectionEnum.Horizontal;
        float viewportCenter = isHorizontal
            ? (viewportCorners[0].x + viewportCorners[2].x) / 2f
            : (viewportCorners[0].y + viewportCorners[2].y) / 2f;

        float maxDistance = isHorizontal
            ? (viewportCorners[2].x - viewportCorners[0].x) * 0.5f
            : (viewportCorners[2].y - viewportCorners[0].y) * 0.5f;

        if (maxDistance <= 0) return;

        // Calculate the closest index to the center
        float centerPosition = scrollerComponent.ScrollPosition + scrollerComponent.ScrollRectSize * 0.5f;
        int closestIndex = scrollerComponent.GetCellViewIndexAtPosition(centerPosition);
        closestIndex = Mathf.Clamp(closestIndex, 0, scroller.Data.Count - 1);

        if (closestIndex != _currentCenteredIndex)
        {
            _currentCenteredIndex = closestIndex;
            UpdateDots();
        }

        for (int i = 0; i < scroller.Data.Count; i++)
        {
            var cellView = scrollerComponent.GetCellViewAtDataIndex(i);
            if (cellView == null) continue;

            RectTransform cellRt = cellView.GetComponent<RectTransform>();
            Vector3[] cellCorners = new Vector3[4];
            cellRt.GetWorldCorners(cellCorners);
            float cellCenter = isHorizontal
                ? (cellCorners[0].x + cellCorners[2].x) / 2f
                : (cellCorners[0].y + cellCorners[2].y) / 2f;

            float distance = Mathf.Abs(cellCenter - viewportCenter);
            float t = Mathf.Clamp01(distance / maxDistance);

            float scale = Mathf.Lerp(1.0f, minScale, t);
            cellRt.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
