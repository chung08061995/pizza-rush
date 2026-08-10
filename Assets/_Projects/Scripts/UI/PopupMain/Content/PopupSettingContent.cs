using UnityEngine;
using UnityEngine.UI;
using DraftUtils.Ads;
using DraftUtils.IAP;

public class PopupSettingContent : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] private Button backButton;
    [SerializeField] public DraftUtils.Popup popup;
    [SerializeField] private DraftUtils.AnimatedToggleController musicButton;
    [SerializeField] private DraftUtils.AnimatedToggleController vibrateButton;

    private DraftUtils.PersistentValue<bool> _musicVolume;
    private DraftUtils.PersistentValue<bool> _vibrate;
    private bool _settingListenersRegistered;

    private void Start()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(ReturnHome);
        }
        else if (popup != null)
        {
            popup.closeButton.OnClickAction = popup.HideWithAnimation;
        }

        _musicVolume = DataManager.Instance.musicVolume;
        _vibrate = DataManager.Instance.vibrate;

        musicButton.Button.OnClickAction = ClickMusicButton;
        vibrateButton.Button.OnClickAction = ClickVibrateButton;

        _musicVolume.Notifier.AddListener(RefreshMusicButton);
        _vibrate.Notifier.AddListener(RefreshVibrateButton);
        _settingListenersRegistered = true;

        musicButton.ApplyImmediate(_musicVolume.Value);
        AudioToggleVisualState.Apply(musicButton, _musicVolume.Value);
        vibrateButton.ApplyImmediate(_vibrate.Value);
    }

    private void ReturnHome()
    {
        var popupMain = GetComponentInParent<PopupMain>();
        if (popupMain != null)
        {
            popupMain.ShowHome();
        }
    }

    private void ClickMusicButton()
    {
        _musicVolume.Value = !_musicVolume.Value;
    }

    private void ClickVibrateButton()
    {
        _vibrate.Value = !_vibrate.Value;
        if (_vibrate.Value)
        {
            VibrationManager.Vibrate(VibrationType.Selection);
        }
    }

    private void RefreshMusicButton()
    {
        musicButton.ApplyWithAnimation(_musicVolume.Value);
        AudioToggleVisualState.Apply(musicButton, _musicVolume.Value);
    }

    private void RefreshVibrateButton()
    {
        vibrateButton.ApplyWithAnimation(_vibrate.Value);
    }

    // Public UI entry points for Settings buttons on both platforms.
    public void RestorePurchases()
    {
        if (IAPManager.Instance == null) return;
        IAPManager.Instance.Restore(success =>
        {
            if (success && AdsManager.Instance != null && IAPManager.Instance.IsOwned(GameConstain.IAPProductId.NoAds))
                AdsManager.Instance.DisableAds();
        });
    }

    public void ShowPrivacyOptions()
    {
        if (AdsManager.Instance != null) AdsManager.Instance.ShowPrivacyOptions();
    }

    private void OnDestroy()
    {
        if (!_settingListenersRegistered)
        {
            return;
        }

        _musicVolume.Notifier.RemoveListener(RefreshMusicButton);
        _vibrate.Notifier.RemoveListener(RefreshVibrateButton);
    }
}
