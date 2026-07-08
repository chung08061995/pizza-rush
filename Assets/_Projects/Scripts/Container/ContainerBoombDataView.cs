using Sirenix.OdinInspector;
using UnityEngine;

public class ContainerBoombDataView : DraftUtils.DraftMonoBehaviour
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
        if (_data.containerBoombData == null)
        {
            return;
        }
        if (_data.containerBoombData.boombAmount <= 0)
        {
            return;
        }
        SetAmountText(_data.containerBoombData.boombAmount);
    }
    private void SetAmountText(int boombAmount)
    {
        amountText.SetActive(boombAmount > 0);
        amountText.SetText($"{boombAmount}");
    }
    public void UpdateAmountText(int dragContainerTimes)
    {
        int remainingAmount = _data.containerBoombData.boombAmount - dragContainerTimes;
        if (remainingAmount < 0) remainingAmount = 0;

        SetAmountText(remainingAmount);
    }
}
