using System;
using UnityEngine;

public class PopupCongratulation : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] public DraftUtils.Popup popup;

    private float _duration = 1000;

    private void Start()
    {
        popup.closeButton.OnClickAction = ClickCloseButton;
    }

    private void ClickCloseButton()
    {
        _duration = 10000;
        popup.HideWithAnimation();
    }

    /// <summary>
    /// Thiết lập thời gian hiển thị cho popup.
    /// </summary>
    /// <param name="duration">Thời gian chờ (giây) trước khi tự động đóng.</param>
    public void SetData(float duration)
    {
        _duration = duration;
    }

    private void Update()
    {
        _duration -= Time.deltaTime;
        if (_duration <= 0)
        {
            popup.HideWithAnimation();
            _duration = 10000;
        }
    }
}
