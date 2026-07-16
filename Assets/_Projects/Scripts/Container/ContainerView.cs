using System;
using UnityEngine;

public class ContainerView : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] private DraftUtils.OptionalValue<DraftUtils.RendererMonoBehaviour> borderRenderer;
    [SerializeField] private DraftUtils.OptionalValue<ContainerIceDataView> containerIceDataView;
    [SerializeField] private DraftUtils.OptionalValue<ContainerBoombDataView> containerBoombDataView;
    [SerializeField] private DraftUtils.OptionalValue<ContainerMovementView> containerMovementView;
    private ContainerData _data;
    public DraftUtils.OptionalValue<ContainerBoombDataView> ContainerBoombDataView => containerBoombDataView;
    public DraftUtils.OptionalValue<ContainerIceDataView> ContainerIceDataView => containerIceDataView;
    public void SetData(ContainerData data)
    {
        _data = data;
        SetBorderRenderer();
        SetContainerIceDataView();
        SetContainerBoombDataView();
        SetContainerMovementView();
    }

    private void SetBorderRenderer()
    {
        if (!borderRenderer.isPresent)
        {
            return;
        }
        if (_data.containerColorData.colorType == ColorType.None)
        {
            return;
        }
        var displayColor = _data.isStone ? ColorType.Gray : _data.containerColorData.colorType;
        if (DataManager.Instance.ProductionLineColorsSO.TryGetValue(displayColor, out var color))
        {
            if (borderRenderer.value != null && borderRenderer.value.Renderers != null)
            {
                foreach (var r in borderRenderer.value.Renderers)
                {
                    if (r != null)
                    {
                        Material mat = r.material;
                        if (mat.HasProperty("_Color"))
                        {
                            mat.SetColor("_Color", color);
                        }
                        else if (mat.HasProperty("Color"))
                        {
                            mat.SetColor("Color", color);
                        }
                    }
                }
            }

            var localRenderer = GetComponent<Renderer>();
            if (localRenderer != null)
            {
                Material mat = localRenderer.material;
                if (mat.HasProperty("_Color"))
                {
                    mat.SetColor("_Color", color);
                }
                else if (mat.HasProperty("Color"))
                {
                    mat.SetColor("Color", color);
                }
            }
        }
    }
    private void SetContainerIceDataView()
    {
        if (!containerIceDataView.isPresent)
        {
            return;
        }
        containerIceDataView.value.SetData(_data);
    }

    private void SetContainerBoombDataView()
    {
        if (!containerBoombDataView.isPresent)
        {
            return;
        }
        containerBoombDataView.value.SetData(_data);
    }

    private void SetContainerMovementView()
    {
        if (!containerMovementView.isPresent)
        {
            return;
        }
        containerMovementView.value.SetData(_data);
    }

    internal void HideAll()
    {
        if (containerIceDataView.isPresent)
        {
            containerIceDataView.value.gameObject.SetActive(false);
        }
        if (containerMovementView.isPresent)
        {
            containerMovementView.value.gameObject.SetActive(false);
        }
        if (containerBoombDataView.isPresent)
        {
            containerBoombDataView.value.gameObject.SetActive(false);
        }
    }
}
