using System;
using UnityEngine;

public class PopupUsingBooter : MonoBehaviour
{
    [SerializeField] public DraftUtils.Popup popup;
    [SerializeField] private ItemView itemView;

    private float _duration = 1000f;
    private Action _onCompletedAction;

    private void Start()
    {
        if (popup != null)
        {
            popup.closeButton.OnClickAction = HidePopup;
        }
    }

    public void SetData(ItemType itemType, float duration, Action onCompletedAction)
    {
        _duration = duration;
        _onCompletedAction = onCompletedAction;
        itemView.SetData(itemType);
    }

    private void Update()
    {
        if (_duration <= 0f)
        {
            return;
        }

        _duration -= Time.deltaTime;
        if (_duration <= 0f)
        {
            _onCompletedAction?.Invoke();
            HidePopup();
        }
    }

    private void HidePopup()
    {
        if (popup != null)
        {
            popup.HideWithAnimation();
        }
    }
}
