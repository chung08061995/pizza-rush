using System;
using UnityEngine;
using UnityEngine.UI;

public class PopupProfile : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] public DraftUtils.Popup popup;
    [SerializeField] private AvatarDataListenerView avatarDataListenerView;
    [SerializeField] private DraftUtils.PersistentIntValueTextBinder levelText;
    [SerializeField] private Button editButton;

    private void Start()
    {
        popup.closeButton.RegisterClickEvents();
        popup.closeButton.OnClickAction = popup.HideWithAnimation;

        editButton.onClick.AddListener(ClickEditButton);

        levelText.Bind(DataManager.Instance.level);
        avatarDataListenerView.ItemView.Button.OnClickAction = ClickEditButton;
    }

    private void ClickEditButton()
    {
        PopupManager.Instance.GetPopupSelectAvatar();
    }
}
