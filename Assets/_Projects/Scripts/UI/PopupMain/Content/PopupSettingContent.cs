using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PopupSettingContent : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] public DraftUtils.Popup popup;
    [SerializeField] private DraftUtils.OptionalButtonGroup quitButton = new();
    [SerializeField] private DraftUtils.OptionalButtonGroup privacyButton = new();
    [SerializeField] private DraftUtils.OptionalButtonGroup restorePurchaseButton = new();
    [SerializeField] private DraftUtils.OptionalButtonGroup contactUsButton = new();
    [SerializeField] private DraftUtils.OptionalButtonGroup giftCodeButton = new();

    [SerializeField] private DraftUtils.AnimatedToggleController musicButton;
    [SerializeField] private DraftUtils.AnimatedToggleController sfxButton;
    [SerializeField] private DraftUtils.AnimatedToggleController vibrateButton;


    private DraftUtils.PersistentValue<bool> musicVolume => DataManager.Instance.musicVolume;
    private DraftUtils.PersistentValue<bool> sfxVolume => DataManager.Instance.sfxVolume;
    private DraftUtils.PersistentValue<bool> vibrate => DataManager.Instance.vibrate;


    private void Start()
    {
        quitButton.RegisterClickEvents();
        quitButton.OnClickAction = ClickQuitButton;

        popup.closeButton.RegisterClickEvents();
        popup.closeButton.OnClickAction = popup.HideWithAnimation;

        privacyButton.RegisterClickEvents();
        privacyButton.OnClickAction = ClickPrivacyButton;

        restorePurchaseButton.RegisterClickEvents();
        restorePurchaseButton.OnClickAction = ClickRestorePurchaseButton;

        contactUsButton.RegisterClickEvents();
        contactUsButton.OnClickAction = ClickContactUsButton;

        giftCodeButton.RegisterClickEvents();
        giftCodeButton.OnClickAction = ClickGiftCodeButton;

        musicButton.Button.OnClickAction = ClickMusicButton;
        musicButton.ApplyImmediate(musicVolume.Value);

        sfxButton.Button.OnClickAction = ClickSfxButton;
        sfxButton.ApplyImmediate(sfxVolume.Value);

        vibrateButton.Button.OnClickAction = ClickVibrateButton;
        vibrateButton.ApplyImmediate(vibrate.Value);
    }

    private void ClickMusicButton()
    {
        musicVolume.Value = !musicVolume.Value;
        musicButton.ApplyWithAnimation(musicVolume.Value);
    }

    private void ClickSfxButton()
    {
        sfxVolume.Value = !sfxVolume.Value;
        sfxButton.ApplyWithAnimation(sfxVolume.Value);
    }

    private void ClickVibrateButton()
    {
        vibrate.Value = !vibrate.Value;
        vibrateButton.ApplyWithAnimation(vibrate.Value);
    }

    private void ClickQuitButton()
    {
        popup.HideWithAnimation();

        var confirmPopup = PopupManager.Instance.GetPopupConfirmReplay();
        confirmPopup.ReplayButton.OnClickAction = confirmPopup.GoToMain;
    }

    private void ClickPrivacyButton()
    {
        Debug.Log("Click Privacy Button");
    }

    private void ClickRestorePurchaseButton()
    {
        DraftUtils.IAP.IAPManager.Instance.Restore(success =>
        {
            Debug.Log($"Restore Purchase {(success ? "Success" : "Failed")}");
        });
    }

    private void ClickContactUsButton()
    {
        Debug.Log("Click Contact Us Button");
    }

    private void ClickGiftCodeButton()
    {
        var popup = PopupManager.Instance.GetPopupGiftCode();
    }
}
