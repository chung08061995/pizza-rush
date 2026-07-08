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

    public List<Vector2Int> GetPartPositions()
    {
        return ContainerSaveDataExtensions.GetPartPositions(_data);
    }

}
