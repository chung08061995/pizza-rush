using UnityEngine;
using UnityEngine.UI;

public class PopupSettingContent : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] private Button backButton;
    [SerializeField] public DraftUtils.Popup popup;
    [SerializeField] private DraftUtils.AnimatedToggleController musicButton;
    [SerializeField] private DraftUtils.AnimatedToggleController vibrateButton;

    private DraftUtils.PersistentValue<bool> musicVolume => DataManager.Instance.musicVolume;
    private DraftUtils.PersistentValue<bool> vibrate => DataManager.Instance.vibrate;

    private void Start()
    {
        backButton.onClick.AddListener(ReturnHome);

        musicButton.Button.OnClickAction = ClickMusicButton;
        musicButton.ApplyImmediate(musicVolume.Value);

        vibrateButton.Button.OnClickAction = ClickVibrateButton;
        vibrateButton.ApplyImmediate(vibrate.Value);
    }

    private void ReturnHome()
    {
        GetComponentInParent<PopupMain>().ShowHome();
    }

    private void ClickMusicButton()
    {
        musicVolume.Value = !musicVolume.Value;
        musicButton.ApplyWithAnimation(musicVolume.Value);
    }

    private void ClickVibrateButton()
    {
        vibrate.Value = !vibrate.Value;
        vibrateButton.ApplyWithAnimation(vibrate.Value);
    }
}
