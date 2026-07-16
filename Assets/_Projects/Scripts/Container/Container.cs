using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Container : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] private Transform shapeRoot;
    [SerializeField] private ContainerMaterialView containerMaterialView;
    [SerializeField] private ContainerShapeType shapeType;
    [SerializeField] private ContainerView containerView;
    [SerializeField] private List<ContainerPlaces> places = new();
    [ShowInInspector] private ContainerStateMachine stateMachine = new();
    [SerializeField] private Transform splitObject;
    [SerializeField] private Transform destroyObject;


    public List<ContainerPlace> Places => places.FindAll(p => p != null).SelectMany(p => p.Places).ToList();
    public List<ContainerPlaces> ContainerPlacesList => places;
    public ContainerShapeType ShapeType => shapeType;
    public Transform ShapeRoot => shapeRoot;
    public bool IsFlyingAway { get; set; } = false;
    public bool isAnimating { get; set; } = false;
    private int currentColorLayerIndex;

    [ShowInInspector] private ContainerSaveData _data;
    public ContainerSaveData Data => _data;
    public ContainerView ContainerView => containerView;

    public void Reload()
    {
        if (_data == null)
        {
            return;
        }
        SetData(_data);
    }
    private void Awake()
    {
        GetAllPlaces();
    }
    public void SetData(ContainerSaveData data)
    {
        IsFlyingAway = false;
        _data = data;
        currentColorLayerIndex = 0;
        EnsureRuntimeColors();
        containerView.SetData(data.containerData);
        containerMaterialView.SetData(_data.containerData.containerMaterialType);

        SetShapeRootTransform(shapeRoot, _data.rotationType, _data.flipX);
        foreach (var place in Places)
        {
            if (place != null)
            {
                place.ClearProduction();
            }
        }

        if (splitObject != null)
        {
            splitObject.gameObject.SetActive(false);
        }

        if (destroyObject != null)
        {
            destroyObject.gameObject.SetActive(false);
        }
    }

    public void ShowSplitObject(bool show)
    {
        if (splitObject != null)
        {
            splitObject.gameObject.SetActive(show);
        }
    }

    public void ShowDestroyObject(bool show)
    {
        if (destroyObject != null)
        {
            destroyObject.gameObject.SetActive(show);
        }
    }
    private void SetShapeRootTransform(Transform shapeRoot, RotationType rotationType, bool flipX)
    {
        shapeRoot.transform.localEulerAngles = new Vector3(0, RotationTypeExtensions.ConvertToAngle(rotationType), 0);
        var localScale = shapeRoot.transform.localScale;
        localScale.x = Mathf.Abs(localScale.x) * (flipX ? -1f : 1f);
        shapeRoot.transform.localScale = localScale;
    }
    [Button]
    private void Rotate90()
    {
        var targetRotateType = RotationTypeExtensions.Rotate90(_data.rotationType);
        _data.rotationType = targetRotateType;
        SetShapeRootTransform(shapeRoot, _data.rotationType, _data.flipX);
    }
    [Button]
    private void FlipX()
    {
        _data.flipX = !_data.flipX;
        SetShapeRootTransform(shapeRoot, _data.rotationType, _data.flipX);
    }
    [Button]
    private void InitEmptyData()
    {
        _data = new();
        _data.rotationType = RotationType.Rotate_0;
    }
    public ContainerStateMachine StateMachine => stateMachine;

    internal bool IsFull()
    {
        var allPlaces = Places;
        if (allPlaces.Count == 0)
        {
            return false;
        }
        return allPlaces.All(place => place.IsFull());
    }

    [Button]
    private void GetAllPlaces()
    {
        places = new List<ContainerPlaces>(GetComponentsInChildren<ContainerPlaces>());
    }

    private void Update()
    {
        stateMachine.StateMachine.Update();
    }

    public List<ContainerPlace> GetEmptyPlacesSortNearPosition(Vector3 position)
    {
        return Places
            .Where(place => place.Empty()) // Chỉ xem xét các vị trí trống
            .OrderByDescending(place => Vector3.Distance(place.transform.position, position))
            .ToList();
    }

    public List<ContainerPlace> GetEmptyPlacesSortLeftToRightTopToBottom()
    {
        return Places
            .Where(place => place.Empty())
            .OrderBy(place => place.transform.position.x)
            .ThenByDescending(place => place.transform.position.z)
            .ToList();
    }

    public bool CanAcceptColor(ColorType color)
    {
        if (_data == null || _data.containerData == null || _data.containerData.isStone)
        {
            return false;
        }
        var colorData = _data.containerData.containerColorData;
        if (colorData.isMultiColor)
        {
            if (!colorData.colors.Contains(color)) return false;
            var quota = GetColorQuota(color);
            return Places.Count(place => place.Production != null && place.Production.ColorType == color) < quota;
        }
        return colorData.colorType == color;
    }

    public List<ContainerPlace> GetEmptyPlacesForColor(ColorType color)
    {
        var empty = GetEmptyPlacesSortLeftToRightTopToBottom();
        var colorData = _data.containerData.containerColorData;
        if (!colorData.isMultiColor)
        {
            return colorData.colorType == color ? empty : new List<ContainerPlace>();
        }
        var quota = GetColorQuota(color);
        var filled = Places.Count(place => place.Production != null && place.Production.ColorType == color);
        return empty.Take(Mathf.Max(0, quota - filled)).ToList();
    }

    public bool HasNextColorLayer()
    {
        var colors = _data?.containerData?.containerColorData?.colors;
        return _data != null && _data.containerData.containerColorData.isLayerBox &&
               colors != null && currentColorLayerIndex + 1 < colors.Count;
    }

    public bool TryAdvanceColorLayer()
    {
        if (!HasNextColorLayer()) return false;
        currentColorLayerIndex++;
        foreach (var place in Places) place.ClearProduction();
        _data.containerData.containerColorData.colorType =
            _data.containerData.containerColorData.colors[currentColorLayerIndex];
        containerView.SetData(_data.containerData);
        return true;
    }

    private void EnsureRuntimeColors()
    {
        var colorData = _data.containerData.containerColorData ??= new ContainerColorData();
        colorData.colors ??= new List<ColorType>();
        if (colorData.colors.Count == 0)
        {
            colorData.colors.Add(colorData.colorType);
        }
        colorData.colorType = colorData.colors[0];
    }

    private int GetColorQuota(ColorType color)
    {
        var colorData = _data.containerData.containerColorData;
        var index = colorData.colors.IndexOf(color);
        if (index < 0) return 0;
        if (colorData.colorAmounts != null && index < colorData.colorAmounts.Count)
        {
            return colorData.colorAmounts[index];
        }
        return Places.Count / Mathf.Max(1, colorData.colors.Count);
    }

    public List<Vector2Int> GetPartPositions()
    {
        return ContainerSaveDataExtensions.GetPartPositions(_data);
    }

}
