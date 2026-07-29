using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class PopupMain : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] private Transform disableFollower;
    [SerializeField] private Transform contentRoot;
    [SerializeField] private HomeNavigationButton shopButton;
    [SerializeField] private HomeNavigationButton rankingButton;
    [SerializeField] private HomeNavigationButton homeButton;
    [SerializeField] private HomeNavigationButton screenShotButton;
    [SerializeField] private Button noAdsButton;
    [SerializeField] private DraftUtils.AnimatedToggleController musicButton;
    [SerializeField] private DraftUtils.AnimatedToggleController vibrateButton;
    [SerializeField] private TMP_FontAsset cleanUiFont;
    // [SerializeField] private DraftUtils.PersistentValueTextBinder<int> startText;
    // [SerializeField] private DraftUtils.PersistentValueTextBinder<int> goldText;
    [SerializeField] private DraftUtils.OptionalButtonGroup goldMoreButton;

    private DraftUtils.TabSlideAnimator tabSlideAnimator = new();
    private DraftUtils.SmoothFollow disableFollowerFollowTarget = new();
    private DraftUtils.PersistentValue<bool> _musicVolume;
    private DraftUtils.PersistentValue<bool> _vibrate;
    private bool _settingListenersRegistered;

    void Start()
    {
        PlayBackgroundLobby();

        // startText.Bind(DataManager.Instance.star);
        // goldText.Bind(DataManager.Instance.gold);

        if (shopButton != null)
        {
            shopButton.gameObject.SetActive(false);
        }
        rankingButton.Button.OnClickAction = ClickRanking;
        homeButton.Button.OnClickAction = ClickHome;
        screenShotButton.Button.OnClickAction = ClickScreenShort;
        noAdsButton.onClick.AddListener(ClickNoAds);
        if (DraftUtils.Ads.AdsManager.Instance != null)
        {
            DraftUtils.Ads.AdsManager.Instance.OnNoAdsEntitlementChanged += RefreshNoAdsVisibility;
        }

        _musicVolume = DataManager.Instance.musicVolume;
        _vibrate = DataManager.Instance.vibrate;

        musicButton.Button.OnClickAction = ClickMusicButton;
        vibrateButton.Button.OnClickAction = ClickVibrateButton;

        _musicVolume.Notifier.AddListener(RefreshMusicButton);
        _vibrate.Notifier.AddListener(RefreshVibrateButton);
        _settingListenersRegistered = true;

        musicButton.ApplyImmediate(_musicVolume.Value);
        vibrateButton.ApplyImmediate(_vibrate.Value);
        ConfigureQuickButtonHover();

        goldMoreButton.Disable();


        tabSlideAnimator.RegisterTabs(
            typeof(PopupRankingContent),
            typeof(PopupHomeContent),
            typeof(PopupLevelUpContent)
        );

        disableFollowerFollowTarget.SetModel(disableFollower.transform);
        disableFollowerFollowTarget.SetTarget(homeButton.transform);
        disableFollowerFollowTarget.SetSpeed(DataManager.Instance.ParametterGameConfigSO.MainPopupFollowerSpeed);
        Canvas.ForceUpdateCanvases();
        disableFollowerFollowTarget.Force();

        ClickHome();
        Canvas.ForceUpdateCanvases();
        disableFollowerFollowTarget.Force();

    }

    private void ConfigureQuickButtonHover()
    {
        var hoverController = musicButton.GetComponent<Animator>()?.runtimeAnimatorController;
        if (hoverController == null)
        {
            return;
        }

        ApplyHoverAnimation(noAdsButton, hoverController);
        ApplyHoverAnimation(rankingButton.GetComponent<Button>(), hoverController);
    }

    private static void ApplyHoverAnimation(Button button, RuntimeAnimatorController hoverController)
    {
        if (button == null)
        {
            return;
        }

        var animator = button.GetComponent<Animator>();
        if (animator == null)
        {
            animator = button.gameObject.AddComponent<Animator>();
        }

        animator.runtimeAnimatorController = hoverController;
        button.transition = Selectable.Transition.Animation;
    }


    private void SelectHomeNavigationButton(HomeNavigationButton selectedButton)
    {
        List<HomeNavigationButton> btns = new()
            {
                rankingButton, homeButton, screenShotButton
            };

        btns.Remove(selectedButton);

        foreach (var btn in btns)
        {
            btn.Deselect();
        }

        selectedButton.Select();
    }
    private void ClickHome()
    {
        SetMainButtonsVisible(true);
        SelectHomeNavigationButton(homeButton);
        DoMoveDisableButtonBackground(homeButton.transform);
        PlayButtonPress(homeButton.transform);
        var popup = PopupManager.Instance.homeContentsController.ShowPopupHomeContent(contentRoot);
        popup.AddGoldButton.Disable();
        tabSlideAnimator.SwitchTo(popup);
    }

    public void ShowHome()
    {
        ClickHome();
    }
    private void ClickRanking()
    {
        SelectHomeNavigationButton(rankingButton);
        DoMoveDisableButtonBackground(rankingButton.transform);
        PlayButtonPress(rankingButton.transform);
        var popup = PopupManager.Instance.homeContentsController.ShowPopupRankingContent(contentRoot);
        ApplyCleanTextRendering(popup);
        tabSlideAnimator.SwitchTo(popup);
        SetMainButtonsVisible(false);
    }

    private void ClickScreenShort()
    {
        SelectHomeNavigationButton(screenShotButton);
        DoMoveDisableButtonBackground(screenShotButton.transform);
        PlayButtonPress(screenShotButton.transform);
        var popup = PopupManager.Instance.homeContentsController.ShowPopupLevelUpContent(contentRoot);
        tabSlideAnimator.SwitchTo(popup);
    }
    private void SetMainButtonsVisible(bool isVisible)
    {
        bool hasNoAds = DraftUtils.Ads.AdsManager.Instance != null &&
                        DraftUtils.Ads.AdsManager.Instance.HasNoAds;
        noAdsButton.transform.parent.gameObject.SetActive(isVisible && !hasNoAds);
    }

    private void RefreshNoAdsVisibility()
    {
        noAdsButton.transform.parent.gameObject.SetActive(false);
    }

    public void ApplyCleanTextRendering(Component content)
    {
        if (cleanUiFont == null)
        {
            foreach (var font in Resources.FindObjectsOfTypeAll<TMP_FontAsset>())
            {
                if (font.name == "Montserrat-Black NewUI Bitmap")
                {
                    cleanUiFont = font;
                    break;
                }
            }
        }

        if (cleanUiFont == null)
        {
            Debug.LogError("Clean UI font could not be loaded.");
            return;
        }

        foreach (var text in content.GetComponentsInChildren<TMP_Text>(true))
        {
            text.font = cleanUiFont;
            text.fontSharedMaterial = cleanUiFont.material;
            text.fontStyle &= ~(FontStyles.Underline | FontStyles.Strikethrough);
        }
    }

    private void ClickNoAds()
    {
        PlayButtonPress(noAdsButton.transform);
        PopupManager.Instance.GetPopupNoAdsDetail();
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
    }

    private void RefreshVibrateButton()
    {
        vibrateButton.ApplyWithAnimation(_vibrate.Value);
    }

    private void OnDestroy()
    {
        if (DraftUtils.Ads.AdsManager.Instance != null)
        {
            DraftUtils.Ads.AdsManager.Instance.OnNoAdsEntitlementChanged -= RefreshNoAdsVisibility;
        }

        if (!_settingListenersRegistered)
        {
            return;
        }

        _musicVolume.Notifier.RemoveListener(RefreshMusicButton);
        _vibrate.Notifier.RemoveListener(RefreshVibrateButton);
    }

    private void DoMoveDisableButtonBackground(Transform activeButton)
    {
        disableFollowerFollowTarget.SetTarget(activeButton);
    }

    private void PlayButtonPress(Transform button)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayButtonPress(button);
        }
    }

    private void PlayBackgroundLobby()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBackgroundLobby(transform);
        }
    }

    private void LateUpdate()
    {
        disableFollowerFollowTarget.LateUpdate();
    }
}
