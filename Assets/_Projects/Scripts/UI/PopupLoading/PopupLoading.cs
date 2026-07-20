using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PopupLoading : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] private DraftUtils.Popup popup;
    [SerializeField] private DraftUtils.ImageFilledSlider progressSlider;
    [SerializeField] private Image progressFillImage;
    private Coroutine progressCoroutine;

    public void SetData(float duration, Action onComplete = null)
    {
        progressSlider.ValueToDisplayTextFunc = progressSlider.ValueToDisplayTextLoading;
        progressSlider.SetMaxValue(1);

        if (progressCoroutine != null)
        {
            StopCoroutine(progressCoroutine);
        }

        progressCoroutine = StartCoroutine(AnimateProgress(duration, onComplete));
    }

    private IEnumerator AnimateProgress(float duration, Action onComplete)
    {
        duration = Mathf.Max(0.1f, duration);
        SetValue(0f);
        yield return null;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetValue(elapsed / duration);
            yield return null;
        }

        SetValue(1f);
        yield return null;

        progressCoroutine = null;
        popup.Hide();
        onComplete?.Invoke();
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
