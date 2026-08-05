using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class ContainerIceDataView : DraftUtils.DraftMonoBehaviour
{
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    [SerializeField] private DraftUtils.OptionalTMPTextGroup amountText = new();
    private ContainerData _data;
    private int _lastRemainingAmount = -1;
    private Vector3 _restScale;

    private void Awake()
    {
        _restScale = transform.localScale;
    }

    public void SetData(ContainerData data)
    {
        _data = data;
        _lastRemainingAmount = data?.containerIceData?.iceAmount ?? -1;
        SetAmountText();
    }

    [Button]
    public void Reload()
    {
        if (_data != null)
        {
            SetData(_data);
        }
    }

    private void SetAmountText()
    {
        amountText.SetActive(false);
        if (_data == null)
        {
            return;
        }
        if (_data.containerIceData == null)
        {
            return;
        }
        if (_data.containerIceData.iceAmount <= 0)
        {
            return;
        }
        SetAmountText(_data.containerIceData.iceAmount);
    }

    private void SetAmountText(int iceAmount)
    {
        amountText.SetActive(iceAmount > 0);
        amountText.SetText($"{iceAmount}");
    }

    public void UpdateAmountText(int resolvedContainer)
    {
        int remainingAmount = _data.containerIceData.iceAmount - resolvedContainer;
        if (remainingAmount < 0) remainingAmount = 0;

        if (_lastRemainingAmount >= 0 && remainingAmount < _lastRemainingAmount)
        {
            PlayHitFeedback();
        }
        _lastRemainingAmount = remainingAmount;
        SetAmountText(remainingAmount);
    }

    private void PlayHitFeedback()
    {
        transform.DOKill();
        transform.localScale = _restScale;

        var sequence = DOTween.Sequence();
        sequence.Append(transform.DOPunchScale(Vector3.one * 0.065f, 0.2f, 5, 0.55f));
        sequence.Join(transform.DOShakeRotation(0.16f, new Vector3(0f, 3.5f, 0f), 7, 65f));
        FlashIceSurface(true);
        sequence.InsertCallback(0.11f, () => FlashIceSurface(false));
        sequence.SetLink(gameObject, LinkBehaviour.KillOnDisable);
    }

    private void FlashIceSurface(bool highlighted)
    {
        var propertyBlock = new MaterialPropertyBlock();
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
        {
            Material material = renderer.sharedMaterial;
            if (material == null)
            {
                continue;
            }

            int colorProperty = material.HasProperty(BaseColorId)
                ? BaseColorId
                : material.HasProperty(ColorId)
                    ? ColorId
                    : -1;
            if (colorProperty < 0)
            {
                continue;
            }

            Color baseColor = material.GetColor(colorProperty);
            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(
                colorProperty,
                highlighted ? Color.Lerp(baseColor, Color.white, 0.52f) : baseColor);
            renderer.SetPropertyBlock(propertyBlock);
        }
    }

    private void OnDisable()
    {
        transform.DOKill();
        FlashIceSurface(false);
        if (_restScale != Vector3.zero)
        {
            transform.localScale = _restScale;
        }
    }
}
