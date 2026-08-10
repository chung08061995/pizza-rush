using UnityEngine;
using UnityEngine.UI;

internal static class AudioToggleVisualState
{
    private const string MuteIndicatorName = "Disable";

    public static void Apply(DraftUtils.AnimatedToggleController controller, bool soundEnabled)
    {
        if (controller == null)
        {
            return;
        }

        var indicator = controller.transform.Find(MuteIndicatorName);
        if (indicator == null || !indicator.TryGetComponent(out Image indicatorImage))
        {
            Debug.LogWarning($"Mute indicator '{MuteIndicatorName}' is missing on {controller.name}.", controller);
            return;
        }

        indicatorImage.fillAmount = soundEnabled ? 0f : 1f;
        indicator.gameObject.SetActive(!soundEnabled);
    }
}
