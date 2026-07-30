using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Container : DraftUtils.DraftMonoBehaviour
{
    private static readonly Vector3 DestroyObjectImpactLocalPoint =
        new Vector3(0f, 0f, 73f);
    private static readonly Vector3 DestroyObjectStartLocalOffset =
        new Vector3(-0.39f, 0.3f, -0.0435f);
    private static readonly Vector3 DestroyObjectStartLocalEulerOffset =
        new Vector3(3.210121f, 26.663952f, 63.656345f);
    private const float DestroyObjectDisplayLift = 0.36f;

    [SerializeField] private Transform shapeRoot;
    [SerializeField] private ContainerMaterialView containerMaterialView;
    [SerializeField] private ContainerShapeType shapeType;
    [SerializeField] private ContainerView containerView;
    [SerializeField] private List<ContainerPlaces> places = new();
    [ShowInInspector] private ContainerStateMachine stateMachine = new();
    [SerializeField] private Transform splitObject;
    [SerializeField] private Transform destroyObject;

    private Tween splitObjectTween;
    private Tween destroyObjectTween;
    private Vector3 splitObjectRestLocalPosition;
    private Vector3 splitObjectRestLocalEulerAngles;
    private Quaternion splitObjectBaseLocalRotation;
    private Vector3 splitStrokeLocalDirection = Vector3.right;
    private Vector3 destroyObjectBaseLocalPosition;
    private Vector3 destroyObjectRestLocalPosition;
    private Quaternion destroyObjectBaseLocalRotation;
    private Quaternion destroyObjectRestLocalRotation;
    private bool skillObjectTransformsCached;

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
        CacheSkillObjectTransforms();
    }
    public void SetData(ContainerSaveData data)
    {
        IsFlyingAway = false;
        _data = data;
        currentColorLayerIndex = 0;
        EnsureRuntimeColors();
        SetShapeRootTransform(shapeRoot, _data.rotationType, _data.flipX);
        ConfigureSplitObjectOrientation();
        ConfigureDestroyObjectOrientation();
        ConfigureDestroyObjectPosition();
        containerView.SetData(data.containerData);
        containerMaterialView.SetData(
            _data.containerData.containerMaterialType,
            ContainerShapeTypeExtensions.GetPartPositions(_data.containerData.containerShapeType));
        foreach (var place in Places)
        {
            if (place != null)
            {
                place.ClearProduction();
            }
        }

        if (splitObject != null)
        {
            ResetSplitObjectTransform();
            splitObject.gameObject.SetActive(false);
        }

        if (destroyObject != null)
        {
            ResetDestroyObjectTransform();
            destroyObject.gameObject.SetActive(false);
        }
    }

    public void ShowSplitObject(bool show)
    {
        if (splitObject == null)
        {
            return;
        }

        CacheSkillObjectTransforms();
        splitObjectTween?.Kill();
        splitObjectTween = null;

        if (!show)
        {
            ResetSplitObjectTransform();
            splitObject.gameObject.SetActive(false);
            return;
        }

        splitObject.gameObject.SetActive(true);
        var displayLiftLocalOffset = splitObject.parent != null
            ? splitObject.parent.InverseTransformVector(Vector3.up * 0.18f)
            : Vector3.up * 0.18f;
        var displayLocalPosition = splitObjectRestLocalPosition + displayLiftLocalOffset;
        splitObject.localPosition =
            displayLocalPosition + Vector3.up * 0.35f - splitStrokeLocalDirection * 0.34f;
        splitObject.localEulerAngles = splitObjectRestLocalEulerAngles;

        splitObjectTween = DOTween.Sequence()
            .Append(splitObject
                .DOLocalMove(displayLocalPosition - splitStrokeLocalDirection * 0.34f, 0.1f)
                .SetEase(Ease.OutQuad))
            .Append(splitObject
                .DOLocalMove(displayLocalPosition + splitStrokeLocalDirection * 0.34f, 0.06f)
                .SetEase(Ease.InOutQuad)
                .SetLoops(6, LoopType.Yoyo));
    }

    public void ShowDestroyObject(bool show)
    {
        if (destroyObject == null)
        {
            return;
        }

        CacheSkillObjectTransforms();
        destroyObjectTween?.Kill();
        destroyObjectTween = null;

        if (!show)
        {
            ResetDestroyObjectTransform();
            destroyObject.gameObject.SetActive(false);
            return;
        }

        destroyObject.gameObject.SetActive(true);
        var parentRotation = destroyObject.parent != null
            ? destroyObject.parent.rotation
            : Quaternion.identity;
        var impactWorldRotation = parentRotation * destroyObjectRestLocalRotation;
        var camera = Camera.main;
        var swingAxis = camera != null ? camera.transform.forward : Vector3.forward;
        var reboundWorldRotation =
            Quaternion.AngleAxis(4f, swingAxis) * impactWorldRotation;
        var inverseParentRotation = Quaternion.Inverse(parentRotation);
        var startLocalRotation =
            destroyObjectRestLocalRotation
            * Quaternion.Euler(DestroyObjectStartLocalEulerOffset);
        var reboundLocalRotation = inverseParentRotation * reboundWorldRotation;

        destroyObject.localPosition =
            destroyObjectRestLocalPosition + DestroyObjectStartLocalOffset;
        destroyObject.localRotation = startLocalRotation;
        destroyObjectTween = DOTween.Sequence()
            .Append(destroyObject
                .DOLocalMove(destroyObjectRestLocalPosition, 0.22f)
                .SetEase(Ease.InQuad))
            .Join(destroyObject
                .DOLocalRotateQuaternion(destroyObjectRestLocalRotation, 0.22f)
                .SetEase(Ease.InQuad))
            .Append(destroyObject
                .DOLocalRotateQuaternion(reboundLocalRotation, 0.08f)
                .SetEase(Ease.OutQuad))
            .Append(destroyObject
                .DOLocalRotateQuaternion(destroyObjectRestLocalRotation, 0.08f)
                .SetEase(Ease.InQuad));
    }

    private void CacheSkillObjectTransforms()
    {
        if (skillObjectTransformsCached)
        {
            return;
        }

        if (splitObject != null)
        {
            splitObjectRestLocalPosition = splitObject.localPosition;
            splitObjectBaseLocalRotation = splitObject.localRotation;
            splitObjectRestLocalEulerAngles = splitObject.localEulerAngles;
        }

        if (destroyObject != null)
        {
            destroyObjectBaseLocalPosition = destroyObject.localPosition;
            destroyObjectRestLocalPosition = destroyObject.localPosition;
            destroyObjectBaseLocalRotation = destroyObject.localRotation;
            destroyObjectRestLocalRotation = destroyObjectBaseLocalRotation;
        }

        skillObjectTransformsCached = true;
    }

    private void ConfigureSplitObjectOrientation()
    {
        if (splitObject == null || _data == null)
        {
            return;
        }

        CacheSkillObjectTransforms();
        var partPositions = ContainerSaveDataExtensions.GetPartPositions(_data);
        if (partPositions.Count == 0)
        {
            return;
        }

        var width = partPositions.Max(position => position.x) - partPositions.Min(position => position.x);
        var depth = partPositions.Max(position => position.y) - partPositions.Min(position => position.y);
        var isVertical = depth > width;

        var worldStrokeDirection = isVertical ? Vector3.forward : Vector3.right;
        splitStrokeLocalDirection = splitObject.parent != null
            ? splitObject.parent.InverseTransformDirection(worldStrokeDirection).normalized
            : worldStrokeDirection;

        var camera = Camera.main;
        if (camera != null)
        {
            splitObject.rotation =
                camera.transform.rotation
                * Quaternion.AngleAxis(45f, Vector3.forward);
        }
        else
        {
            splitObject.localRotation =
                splitObjectBaseLocalRotation
                * Quaternion.AngleAxis(45f, Vector3.forward);
        }

        splitObjectRestLocalEulerAngles = splitObject.localEulerAngles;
    }

    private void ConfigureDestroyObjectPosition()
    {
        if (destroyObject == null || _data == null)
        {
            return;
        }

        CacheSkillObjectTransforms();
        destroyObject.localPosition = destroyObjectBaseLocalPosition;
        var partPositions = ContainerSaveDataExtensions.GetPartPositions(_data);
        if (partPositions.Count == 0)
        {
            destroyObjectRestLocalPosition = destroyObjectBaseLocalPosition;
            return;
        }

        var centerX =
            (partPositions.Min(position => position.x)
             + partPositions.Max(position => position.x)) * 0.5f;
        var centerZ =
            (partPositions.Min(position => position.y)
             + partPositions.Max(position => position.y)) * 0.5f;
        var targetWorldPosition =
            transform.TransformPoint(new Vector3(centerX, 0f, centerZ));
        var currentImpactWorldPosition =
            destroyObject.TransformPoint(DestroyObjectImpactLocalPoint);
        targetWorldPosition.y =
            currentImpactWorldPosition.y + DestroyObjectDisplayLift;
        destroyObject.position += targetWorldPosition - currentImpactWorldPosition;
        destroyObjectRestLocalPosition = destroyObject.localPosition;
    }

    private void ConfigureDestroyObjectOrientation()
    {
        if (destroyObject == null)
        {
            return;
        }

        CacheSkillObjectTransforms();
        destroyObject.localRotation = destroyObjectBaseLocalRotation;

        var camera = Camera.main;
        if (camera != null)
        {
            var screenAlignedRotation = Quaternion.LookRotation(
                camera.transform.up,
                -camera.transform.forward);
            destroyObject.rotation =
                Quaternion.AngleAxis(90f, camera.transform.forward)
                * screenAlignedRotation;
        }

        destroyObjectRestLocalRotation = destroyObject.localRotation;
    }

    private void ResetSplitObjectTransform()
    {
        splitObjectTween?.Kill();
        splitObjectTween = null;

        if (splitObject == null)
        {
            return;
        }

        splitObject.localPosition = splitObjectRestLocalPosition;
        splitObject.localEulerAngles = splitObjectRestLocalEulerAngles;
    }

    private void ResetDestroyObjectTransform()
    {
        destroyObjectTween?.Kill();
        destroyObjectTween = null;

        if (destroyObject == null)
        {
            return;
        }

        destroyObject.localPosition = destroyObjectRestLocalPosition;
        destroyObject.localRotation = destroyObjectRestLocalRotation;
    }

    private void OnDisable()
    {
        splitObjectTween?.Kill();
        destroyObjectTween?.Kill();
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
