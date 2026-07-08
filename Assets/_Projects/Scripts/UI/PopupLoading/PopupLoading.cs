using DG.Tweening;
using UnityEngine;

public class PopupLoading : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] private DraftUtils.Popup popup;
    [SerializeField] private DraftUtils.ImageFilledSlider progressSlider;

    public void SetData(float duration)
    {
        progressSlider.ValueToDisplayTextFunc = progressSlider.ValueToDisplayTextLoading;
        progressSlider.SetMaxValue(1);
        progressSlider.SetValue(0);
        progressSlider.Apply();

        float value = 0;
        DOTween.To(() => value, x => SetValue(x), 1f, duration)
            .From(0f)
            .OnComplete(popup.Hide);
    }
    private void SetValue(float progress)
    {
        progressSlider.SetValue(progress);
        progressSlider.Apply();
    }
}