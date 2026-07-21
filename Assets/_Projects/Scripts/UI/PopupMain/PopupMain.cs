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
    void Start()
    {
        PlayBackgroundLobby();

        // startText.Bind(DataManager.Instance.star);
        // goldText.Bind(DataManager.Instance.gold);

        shopButton.Button.OnClickAction = ClickShop;
        rankingButton.Button.OnClickAction = ClickRanking;
        homeButton.Button.OnClickAction = ClickHome;
        screenShotButton.Button.OnClickAction = ClickScreenShort;
        noAdsButton.onClick.AddListener(ClickNoAds);
        musicButton.Button.OnClickAction = ClickMusicButton;
        musicButton.ApplyImmediate(DataManager.Instance.musicVolume.Value);
        vibrateButton.Button.OnClickAction = ClickVibrateButton;
        vibrateButton.ApplyImmediate(DataManager.Instance.vibrate.Value);

        // goldMoreButton.RegisterClickEvents();
        // goldMoreButton.OnClickAction = ClickShop;


        tabSlideAnimator.RegisterTabs(
            typeof(PopupShopContent),
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
        NormalizeQuickButtonLayout();
        disableFollowerFollowTarget.Force();

    }

    private void NormalizeQuickButtonLayout()
    {
        if (rankingButton.transform is RectTransform rankingRect)
        {
            rankingRect.anchoredPosition = Vector2.zero;
        }
    }


    private void SelectHomeNavigationButton(HomeNavigationButton selectedButton)
    {
        List<HomeNavigationButton> btns = new()
            {
                shopButton, rankingButton, homeButton, screenShotButton
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
        popup.AddGoldButton.OnClickAction = ClickShop;
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

    private void ClickShop()
    {
        SelectHomeNavigationButton(shopButton);
        DoMoveDisableButtonBackground(shopButton.transform);
        PlayButtonPress(shopButton.transform);
        var popup = PopupManager.Instance.homeContentsController.ShowPopupShopContent(contentRoot);
        tabSlideAnimator.SwitchTo(popup);
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
        noAdsButton.transform.parent.gameObject.SetActive(isVisible);
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
        DataManager.Instance.musicVolume.Value = !DataManager.Instance.musicVolume.Value;
        musicButton.ApplyWithAnimation(DataManager.Instance.musicVolume.Value);
    }

    private void ClickVibrateButton()
    {
        DataManager.Instance.vibrate.Value = !DataManager.Instance.vibrate.Value;
        vibrateButton.ApplyWithAnimation(DataManager.Instance.vibrate.Value);
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
