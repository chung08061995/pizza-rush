using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using DG.Tweening;

public class ProductionLine : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] private Transform root;
    [SerializeField] private DraftUtils.Pooler<Production> productionPooler = new();
    [SerializeField] private DraftUtils.RendererMonoBehaviour rendererMono;
    [SerializeField] private List<Place> places = new();

    public DraftUtils.RendererMonoBehaviour RendererMono => rendererMono;
    public DraftUtils.Pooler<Production> ProductionPooler => productionPooler;
    public List<Place> Places => places;

    [ShowInInspector] private ProductionLineRuntimeData _data;
    public ProductionLineRuntimeData Data => _data;

    [Button]
    private void Rotate90()
    {
        if (_data == null)
        {
            InitEmptyData();
        }
        if (_data.productionLineSaveData == null)
        {
            _data.productionLineSaveData = new ProductionLineSaveData();
        }

        var targetRotateType = RotationTypeExtensions.Rotate90(_data.productionLineSaveData.rotationType);
        _data.productionLineSaveData.rotationType = targetRotateType;
        root.localEulerAngles = new Vector3(0, RotationTypeExtensions.ConvertToAngle(targetRotateType), 0);

    }

    [Button]
    private void InitEmptyData()
    {
        _data = new ProductionLineRuntimeData();
        _data.productionLineSaveData = new ProductionLineSaveData();
        _data.productionLineSaveData.rotationType = RotationType.Rotate_0;
    }

    [Button]
    private void GetAllPlaces()
    {
        places.Clear();
        var foundPlaces = GetComponentsInChildren<Place>();
        places.AddRange(foundPlaces);
    }
    public void SetData(ProductionLineSaveData data)
    {
        _data = new();
        ProductionLineRuntimeDataExensions.SetData(_data, data);

        if (root != null)
        {
            var scale = root.localScale;
            scale.x = Mathf.Abs(scale.x) *
                (data.productionLineVisualType == ProductionLineVisualType.SafeCurvedLeft ? -1f : 1f);
            root.localScale = scale;
            root.localEulerAngles = new Vector3(0, RotationTypeExtensions.ConvertToAngle(data.rotationType), 0);
        }

        productionPooler.Factory = new DraftUtils.CallbackInstantiatePoolFactory<Production>();

        // Số lượng production cần spawn = min(giới hạn config, số màu đã lưu)
        int spawnCount = Mathf.Min(
            DataManager.Instance.ParametterGameConfigSO.MaxProductionOnLine,
            _data.productionColors.Count
        );

        for (int i = 0; i < spawnCount; i++)
        {
            var productionGo = productionPooler.Spawn();
            productionGo.SetData(_data.productionColors[i]);
            productionGo.CurrentIndex = i;
            productionGo.transform.position = DraftUtils.Utils.CameraInput.GetSpawnPositionByIndex<Place>(i, places, place => place.transform.position);
        }
    }

    public void SetupProductionTransform(Production production, int index)
    {
        if (places.Count == 0) return;

        if (index < places.Count)
        {
            production.transform.SetParent(places[index].transform, true);
            production.transform.localPosition = Vector3.zero;
        }
        else
        {
            Vector3 spacing = Vector3.back;
            if (places.Count >= 2)
            {
                spacing = places[places.Count - 1].transform.localPosition - places[places.Count - 2].transform.localPosition;
            }
            var lastPlace = places[places.Count - 1];
            production.transform.SetParent(lastPlace.transform.parent, true);

            int extraIndex = index - places.Count + 1;
            production.transform.localPosition = lastPlace.transform.localPosition + spacing * extraIndex;
        }
    }

    public void AnimateProductionToShift(Production production, int index, float duration)
    {
        if (places.Count == 0) return;

        if (index < places.Count)
        {
            production.transform.SetParent(places[index].transform, true);
            production.transform.DOLocalMove(Vector3.zero, duration).SetEase(DG.Tweening.Ease.Linear);
        }
        else
        {
            Vector3 spacing = Vector3.back;
            if (places.Count >= 2)
            {
                spacing = places[places.Count - 1].transform.localPosition - places[places.Count - 2].transform.localPosition;
            }
            var lastPlace = places[places.Count - 1];
            production.transform.SetParent(lastPlace.transform.parent, true);

            int extraIndex = index - places.Count + 1;
            var targetLocalPos = lastPlace.transform.localPosition + spacing * extraIndex;
            production.transform.DOLocalMove(targetLocalPos, duration).SetEase(DG.Tweening.Ease.Linear);
        }
    }

    public void ChangeProductionLineColor()
    {
        if (productionPooler.ActiveItems.Count > 0)
        {
            var firstProduction = productionPooler.ActiveItems[0];
            if (firstProduction != null)
            {
                var colorType = firstProduction.ColorType;
                if (DataManager.Instance.ProductionLineColorsSO.Dictionary.TryGetValue(colorType, out var color))
                {
                    SetColor(color);
                }
            }
        }
        else
        {
            if (DataManager.Instance.ProductionLineColorsSO.Dictionary.TryGetValue(ColorType.White, out var color))
            {
                SetColor(color);
            }
            // If there are no active productions, reset to default material
            //rendererMono.ResetMaterial();
        }
    }
    public List<Production> GetAllProductionInLineSampleColorAsContainer(ColorType colorType, ProductionLine productionLine)
    {
        List<Production> productions = new();
        foreach (var production in productionLine.ProductionPooler.ActiveItems)
        {
            if (production.ColorType != colorType)
            {
                break;
            }
            productions.Add(production);
        }
        return productions;
    }

    public void Creat(int targetTotalCount)
    {
        var currentIndex = productionPooler.ActiveItems.Count;
        int spawnCount = targetTotalCount - currentIndex;
        if (spawnCount <= 0) return;

        spawnCount = Mathf.Min(spawnCount, _data.productionColors.Count - currentIndex);

        for (int i = 0; i < spawnCount; i++)
        {
            var productionGo = productionPooler.Spawn();
            int indexInData = currentIndex + i;
            productionGo.SetData(_data.productionColors[indexInData]);
            productionGo.CurrentIndex = indexInData;
            productionGo.transform.position = DraftUtils.Utils.CameraInput.GetSpawnPositionByIndex<Place>(indexInData, places, place => place.transform.position);
        }
    }

    internal void SetColor(Color color)
    {
        foreach (var renderer in rendererMono.Renderers)
        {
            renderer.material.color = color;
        }
    }
}
