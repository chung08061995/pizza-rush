using System;
using UnityEngine;
using UnityEngine.UI;

public class PopupProfile : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] public DraftUtils.Popup popup;
    [SerializeField] private AvatarDataListenerView avatarDataListenerView;
    [SerializeField] private DraftUtils.PersistentIntValueTextBinder levelText;
    [SerializeField] private Button editButton;
    [SerializeField] private Button nameButton;

    private void Start()
    {
        popup.closeButton.RegisterClickEvents();
        popup.closeButton.OnClickAction = popup.HideWithAnimation;

        editButton.onClick.AddListener(ClickEditButton);
        if (nameButton != null)
        {
            nameButton.onClick.AddListener(ClickEditButton);
        }

        levelText.Bind(DataManager.Instance.level);
        avatarDataListenerView.ItemView.Button.OnClickAction = ClickEditButton;
    }

    private void ClickEditButton()
    {
        PopupManager.Instance.GetPopupSelectAvatar();
    }
}
