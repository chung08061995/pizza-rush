using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupIapDetail : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] public DraftUtils.Popup popup;
    [SerializeField] public MultipleIAPDataView multipleIAPDataView;
    [SerializeField] private Button buyButton;

    [ShowInInspector]
    [ReadOnly]
    private MultipleIAPData _data;

    private void Start()
    {
        popup.closeButton.OnClickAction = popup.HideWithAnimation;
    }
    public void SetData(MultipleIAPData data)
    {
        _data = data;
        SetMultipleIAPDataView();

    }
    private void SetMultipleIAPDataView()
    {
        multipleIAPDataView.SetData(_data);
    }
}
