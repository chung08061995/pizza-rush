
using DG.DemiLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Production : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] private DraftUtils.RendererMonoBehaviour rendererMono;
    [SerializeField] private List<SkinnedMeshRenderer> skins = new();
    [SerializeField] private Renderer colorMarker;
    public List<SkinnedMeshRenderer> Skins => skins;

    public ColorType ColorType { get; set; }
    public DraftUtils.RendererMonoBehaviour RendererMono => rendererMono;
    public int CurrentIndex { get; set; }

    public void SetData(ColorType colorType)
    {
        if (DataManager.Instance.ProductionLineColorsSO.TryGetValue(colorType, out var materialColor))
        {
            SetMaterial(materialColor);
            this.ColorType = colorType;
        }
    }

    internal void SetMaterial(Color color)
    {
        SetSkinColor(color);
    }

    public void SetBlendShapeWeight(string name, float weight)
    {
        foreach (var skin in skins)
        {
            if (skin == null || skin.sharedMesh == null) continue;
            int index = skin.sharedMesh.GetBlendShapeIndex(name);
            if (index >= 0)
            {
                skin.SetBlendShapeWeight(index, weight);
            }
        }
    }
    public void SetSkinColor(Color color)
    {
        SetRendererColor(colorMarker, color);

        foreach (var skin in skins)
        {
            if (skin == null) continue;

            // Dùng materials (instance) để không ảnh hưởng tới shared material của các object khác
            foreach (var mat in skin.materials)
            {
                SetMaterialColor(mat, color);
            }
        }
    }

    private static void SetRendererColor(Renderer targetRenderer, Color color)
    {
        if (targetRenderer == null) return;

        foreach (var material in targetRenderer.materials)
        {
            SetMaterialColor(material, color);
        }
    }

    private static void SetMaterialColor(Material material, Color color)
    {
        if (material == null) return;

        // Tùy shader mà tên property khác nhau, phổ biến nhất là "_Color" hoặc "_BaseColor" (URP/HDRP)
        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", color);
        else if (material.HasProperty("_Color"))
            material.color = color;
    }
}
