using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PopupLoading : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] private DraftUtils.Popup popup;
    [SerializeField] private DraftUtils.ImageFilledSlider progressSlider;
    [SerializeField] private Image progressFillImage;

    public void SetData(float duration, Action onComplete = null)
    {
        progressSlider.ValueToDisplayTextFunc = progressSlider.ValueToDisplayTextLoading;
        progressSlider.SetMaxValue(1);
        SetValue(0f);

        float value = 0;
        DOTween.To(() => value, x => SetValue(x), 1f, duration)
            .From(0f)
            .SetEase(Ease.Linear)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                SetValue(1f);
                popup.Hide();
                onComplete?.Invoke();
            });
    }
    private void SetValue(float progress)
    {
        progress = Mathf.Clamp01(progress);
        progressSlider.SetValue(progress);
        progressSlider.Apply();

        var fillRect = progressFillImage.rectTransform;
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(progress, 1f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        progressFillImage.enabled = progress > 0f;
    }
}
