using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupGiftCode : DraftUtils.DraftMonoBehaviour
{
    [SerializeField]
    public DraftUtils.Popup popup;
    [SerializeField] private Button confrimButton;
    [SerializeField] private TMP_InputField giftCodeInput;

    private void Start()
    {
        popup.closeButton.RegisterClickEvents();
        popup.closeButton.OnClickAction = popup.HideWithAnimation;
        confrimButton.onClick.AddListener(ClickConfirm);

    }

    private void ClickConfirm()
    {
        throw new NotImplementedException();
    }
}
