using System.Collections.Generic;
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
    [SerializeField] private Material cleanUiMaterial;
    [SerializeField] private Sprite quickButtonBackground;
    [SerializeField] private float quickButtonSize = 132f;
    [SerializeField] private float quickButtonSpacing = 16f;
    [SerializeField] private float quickButtonRightMargin = 24f;
    [SerializeField] private float quickButtonTopOffset = 210f;
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
        AudioToggleVisualState.Apply(musicButton, _musicVolume.Value);
        vibrateButton.ApplyImmediate(_vibrate.Value);
        ConfigureQuickButtonVisuals();
        LayoutQuickButtons();
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

    private void ConfigureQuickButtonVisuals()
    {
        ApplyQuickButtonBackground(rankingButton);
        ApplyQuickButtonBackground(noAdsButton);
        ApplyQuickButtonBackground(musicButton);
        ApplyQuickButtonBackground(vibrateButton);
    }

    private void ApplyQuickButtonBackground(Component button)
    {
        if (button == null)
        {
            return;
        }

        var image = button.GetComponent<Image>();
        if (image == null)
        {
            return;
        }

        if (quickButtonBackground != null)
        {
            image.sprite = quickButtonBackground;
        }

        image.color = Color.white;
        image.preserveAspect = true;
    }

    private void LayoutQuickButtons()
    {
        if (noAdsButton == null || rankingButton == null || musicButton == null || vibrateButton == null)
        {
            return;
        }

        var root = noAdsButton.transform.parent as RectTransform;
        if (root == null || Screen.width <= 0 || Screen.height <= 0)
        {
            return;
        }

        var safeArea = Screen.safeArea;
        var safeTopRight = new Vector2(
            safeArea.xMax / Screen.width,
            safeArea.yMax / Screen.height);

        root.anchorMin = safeTopRight;
        root.anchorMax = safeTopRight;
        root.pivot = Vector2.one;
        root.anchoredPosition = new Vector2(-quickButtonRightMargin, -quickButtonTopOffset);

        var buttons = new Component[]
        {
            rankingButton,
            noAdsButton,
            musicButton,
            vibrateButton
        };

        int visibleIndex = 0;
        foreach (var button in buttons)
        {
            if (button == null || !button.gameObject.activeSelf)
            {
                continue;
            }

            var rect = button.transform as RectTransform;
            if (rect == null)
            {
                continue;
            }

            rect.anchorMin = Vector2.one;
            rect.anchorMax = Vector2.one;
            rect.pivot = Vector2.one;
            rect.sizeDelta = Vector2.one * quickButtonSize;
            rect.anchoredPosition = new Vector2(
                0f,
                -visibleIndex * (quickButtonSize + quickButtonSpacing));
            visibleIndex++;
        }

        float height = Mathf.Max(
            quickButtonSize,
            visibleIndex * quickButtonSize + Mathf.Max(0, visibleIndex - 1) * quickButtonSpacing);
        root.sizeDelta = new Vector2(quickButtonSize, height);
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
        rankingButton.gameObject.SetActive(isVisible);
        musicButton.gameObject.SetActive(isVisible);
        vibrateButton.gameObject.SetActive(isVisible);
        noAdsButton.gameObject.SetActive(isVisible && !hasNoAds);
        LayoutQuickButtons();
    }

    private void RefreshNoAdsVisibility()
    {
        noAdsButton.gameObject.SetActive(false);
        LayoutQuickButtons();
    }

    public void ApplyCleanTextRendering(Component content)
    {
        if (cleanUiFont == null)
        {
            foreach (var font in Resources.FindObjectsOfTypeAll<TMP_FontAsset>())
            {
                if (font.name == "Montserrat-Black SDF")
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

        Material material = cleanUiMaterial;
        if (material == null || material.mainTexture != cleanUiFont.atlasTexture)
        {
            material = cleanUiFont.material;
        }

        foreach (var text in content.GetComponentsInChildren<TMP_Text>(true))
        {
            text.font = cleanUiFont;
            text.fontSharedMaterial = material;
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
        AudioToggleVisualState.Apply(musicButton, _musicVolume.Value);
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

    private void OnRectTransformDimensionsChange()
    {
        if (Application.isPlaying && isActiveAndEnabled)
        {
            LayoutQuickButtons();
        }
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
