using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PopupLoading : DraftUtils.DraftMonoBehaviour
{
    private const float MaxProgressDeltaPerFrame = 1f / 30f;

    [SerializeField] private DraftUtils.Popup popup;
    [SerializeField] private DraftUtils.ImageFilledSlider progressSlider;
    [SerializeField] private Image progressFillImage;
    private Coroutine progressCoroutine;
    private Vector2 progressFillOffsetMin;
    private Vector2 progressFillOffsetMax;

    private void Awake()
    {
        var fillRect = progressFillImage.rectTransform;
        progressFillOffsetMin = fillRect.offsetMin;
        progressFillOffsetMax = fillRect.offsetMax;
    }

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
            // Startup services can block the Editor main thread for a long frame.
            // Do not let that single frame consume the whole visual animation.
            elapsed += Mathf.Min(Time.unscaledDeltaTime, MaxProgressDeltaPerFrame);
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
        fillRect.offsetMin = progressFillOffsetMin;
        fillRect.offsetMax = new Vector2(
            Mathf.Lerp(progressFillOffsetMin.x, progressFillOffsetMax.x, progress),
            progressFillOffsetMax.y);
        progressFillImage.enabled = progress > 0f;
    }
}
