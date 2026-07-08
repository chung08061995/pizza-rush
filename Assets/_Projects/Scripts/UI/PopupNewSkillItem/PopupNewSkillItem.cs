using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupNewSkillItem : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] public DraftUtils.Popup popup;
    [SerializeField] private ItemView itemView;
    private void Start()
    {
        popup.closeButton.OnClickAction = popup.HideWithAnimation;
    }
    public void SetData(ItemType itemType)
    {
        itemView.SetData(itemType);
    }
}
