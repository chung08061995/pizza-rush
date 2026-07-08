using System;
using UnityEngine;

public class PopupUsingSkill : DraftUtils.DraftMonoBehaviour
{
    private DraftUtils.FormattedLogger _logger = new(nameof(PopupUsingSkill));
    [SerializeField] public DraftUtils.Popup popup;
    [SerializeField] private ItemView itemView;
    private ItemType _data;
    private void Start()
    {
        popup.closeButton.RegisterClickEvents();
        popup.closeButton.OnClickAction = ClickClose;
    }
    public void SetData(ItemType data)
    {
        _data = data;
        SetItemView();
    }

    private void SetItemView()
    {
        itemView.SetData(_data);
    }
    private void ClickClose()
    {
        LevelFactory.Instance.LevelRunner.GameplayStateMachine.ChangeToDragContainerState();
        PopupManager.Instance.GetPopupSkillGameplay();

        popup.Hide();
    }
}
