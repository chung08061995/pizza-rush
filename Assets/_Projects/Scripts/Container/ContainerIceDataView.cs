using Sirenix.OdinInspector;
using UnityEngine;

public class ContainerIceDataView : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] private DraftUtils.OptionalTMPTextGroup amountText = new();
    private ContainerData _data;

    public void SetData(ContainerData data)
    {
        _data = data;
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

        SetAmountText(remainingAmount);
    }
}
