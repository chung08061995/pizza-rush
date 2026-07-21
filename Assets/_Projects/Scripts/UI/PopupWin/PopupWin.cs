using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupWin : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] public DraftUtils.Popup popup;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button x2RewardButton;

    private int _goldReward;

    private void Start()
    {
        nextButton.onClick.AddListener(OnNextButtonClicked);
        levelText.text = string.Format(GameConstain.StringFormats.LevelDisplayFormat, DataManager.Instance.Level.Value);
        RefreshRewardText();
        NormalizeLayout();
        StartCoroutine(NormalizeLayoutAfterUnityLayoutPass());
    }

    private IEnumerator NormalizeLayoutAfterUnityLayoutPass()
    {
        // Popup animation/layout can write its final RectTransforms after Start.
        // Re-apply once after the animation settles so both actions stay in the panel.
        yield return null;
        for (var i = 0; i < 12; i++)
        {
            Canvas.ForceUpdateCanvases();
            NormalizeLayout();
            yield return new WaitForSeconds(0.15f);
        }
    }

    private void NormalizeLayout()
    {
        var panel = transform.Find("Popup/PanelPopup");
        var bottom = panel != null ? panel.Find("Bottom") : null;
        if (bottom == null)
        {
            return;
        }

        var bottomRect = bottom.GetComponent<RectTransform>();
        bottomRect.anchorMin = new Vector2(0.5f, 0.5f);
        bottomRect.anchorMax = new Vector2(0.5f, 0.5f);
        bottomRect.pivot = new Vector2(0.5f, 0.5f);
        bottomRect.anchoredPosition = new Vector2(0f, -275f);
        bottomRect.sizeDelta = new Vector2(460f, 270f);

        var horizontalLayout = bottom.GetComponent<HorizontalLayoutGroup>();
        if (horizontalLayout != null)
        {
            horizontalLayout.enabled = false;
        }

        ConfigureButton(bottom.Find("Button_Rectangle_Yellow_Text"), "Next", 65f);
        ConfigureButton(bottom.Find("Button_Rectangle_Green"), "x2 Value", -75f);
    }

    private static void ConfigureButton(Transform buttonTransform, string textValue, float y)
    {
        if (buttonTransform == null)
        {
            return;
        }

        var buttonRect = buttonTransform.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = new Vector2(0f, y);
        buttonRect.sizeDelta = new Vector2(420f, 112f);

        var button = buttonTransform.GetComponentInChildren<Button>(true);
        if (button != null)
        {
            var actualButtonRect = button.GetComponent<RectTransform>();
            if (actualButtonRect != buttonRect)
            {
                StretchToParent(actualButtonRect);
            }
            if (button.targetGraphic is Image buttonImage)
            {
                buttonImage.type = Image.Type.Simple;
            }
        }

        foreach (var image in buttonTransform.GetComponentsInChildren<Image>(true))
        {
            if (image.gameObject.name == "Background" || image.gameObject.name == "Button")
            {
                StretchToParent(image.rectTransform);
                image.type = Image.Type.Simple;
                var color = image.color;
                color.a = 1f;
                image.color = color;
            }
        }

        var text = buttonTransform.GetComponentInChildren<TMP_Text>(true);
        if (text == null)
        {
            return;
        }

        text.text = textValue;
        text.fontSize = 54f;
        text.enableAutoSizing = false;
        text.alignment = TextAlignmentOptions.Center;
        var textRect = text.rectTransform;
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = new Vector2(textValue == "x2 Value" ? 45f : 0f, 0f);
        textRect.sizeDelta = new Vector2(textValue == "x2 Value" ? 330f : 390f, 80f);

        if (textValue == "x2 Value")
        {
            RectTransform icon = null;
            foreach (var rect in buttonTransform.GetComponentsInChildren<RectTransform>(true))
            {
                if (rect.name == "Icon")
                {
                    icon = rect;
                    break;
                }
            }
            if (icon != null)
            {
                icon.anchorMin = new Vector2(0f, 0.5f);
                icon.anchorMax = new Vector2(0f, 0.5f);
                icon.pivot = new Vector2(0.5f, 0.5f);
                icon.anchoredPosition = new Vector2(50f, 0f);
                icon.sizeDelta = new Vector2(62f, 62f);
            }
        }
    }

    private static void StretchToParent(RectTransform rect)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    public void SetData(int goldReward)
    {
        _goldReward = goldReward;
        RefreshRewardText();
    }

    private void RefreshRewardText()
    {
        if (rewardText == null)
        {
            rewardText = GetComponentInChildren<TMP_Text>(true);
        }

        if (rewardText != null)
        {
            rewardText.text = _goldReward > 0
                ? $"Gold +{_goldReward}"
                : "No reward";
        }
    }

    private void OnDisable()
    {
    }

    private void OnNextButtonClicked()
    {
        popup.HideWithAnimation();
        LevelFactory.Instance.LoadCurrentLevelData();
    }
}
