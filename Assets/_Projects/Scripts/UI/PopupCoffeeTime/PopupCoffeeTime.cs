using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupCoffeeTime : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] private TMP_Text bonusTime;

    private Action _onCompletedAction;
    private int _bonusTimeValue;


    public void SetData(int bonusTimeValue, Action onCompletedAction)
    {
        _bonusTimeValue = bonusTimeValue;
        _onCompletedAction = onCompletedAction;
        if (bonusTime != null)
        {
            bonusTime.text = $"+{bonusTimeValue}s";
        }
    }

    public void PlayMoveToGameplayTimer(RectTransform targetTransform)
    {
        if (bonusTime == null || targetTransform == null)
        {
            _onCompletedAction?.Invoke();
            return;
        }

        var rectTransform = bonusTime.rectTransform;

        // Giữ lại world position hiện tại trước khi đổi parent
        Vector3 currentWorldPos = rectTransform.position;

        // Đổi parent nhưng giữ nguyên vị trí world (worldPositionStays = true)
        rectTransform.SetParent(targetTransform.parent, true);
        rectTransform.position = currentWorldPos;

        // Quy đổi vị trí đích sang anchoredPosition theo parent mới
        Vector2 targetAnchoredPos = GetAnchoredPositionFromWorld(rectTransform, targetTransform.position);

        rectTransform.DOAnchorPos(targetAnchoredPos, 2f)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                _onCompletedAction?.Invoke();
                gameObject.SetActive(false);
                rectTransform.gameObject.SetActive(false);
            });
    }

    private Vector2 GetAnchoredPositionFromWorld(RectTransform rect, Vector3 worldPos)
    {
        var parentRect = rect.parent as RectTransform;
        if (parentRect == null) return rect.anchoredPosition;

        // Tìm canvas gốc để lấy đúng camera (null nếu canvas là Overlay)
        var canvas = rect.GetComponentInParent<Canvas>();
        Camera cam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            ? canvas.worldCamera
            : null;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, worldPos);

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPoint, cam, out localPoint);

        // Cộng thêm pivot offset để ra đúng anchoredPosition
        return localPoint + rect.anchoredPosition - (Vector2)parentRect.InverseTransformPoint(rect.position);
    }

}
