using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PopupGameplay : DraftUtils.SingletonDontDestroyOnLoadMonoBehaviour<PopupGameplay>
{
    [SerializeField] private DraftUtils.Popup popup;
    [SerializeField] private Button replayButton;
    [FormerlySerializedAs("settingButton")]
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private ItemView goldView;
    [SerializeField] private DraftUtils.OptionalTMPTextGroup timeText;
    public DraftUtils.OptionalTMPTextGroup TimeText => timeText;

    private const float CoffeeTimeBonusSeconds = 10f;
    private const float CoffeeTimeFlyDuration = 0.7f;

    private void Start()
    {
        replayButton.onClick.AddListener(OnReplayButtonClicked);
        backButton.onClick.AddListener(OnBackButtonClicked);
    }

    public void SetData(float time)
    {
        SetTimeText(time);
        levelText.text = string.Format(GameConstain.StringFormats.LevelDisplayFormat, DataManager.Instance.level.Value);
        goldView.RemaningText.ValueToDisplayTextFunc = x => DraftUtils.Utils.Common.FormatNumber((int)x);
        goldView.SetData(ItemType.Gold);
        ApplyHudTypography();
    }

    private void ApplyHudTypography()
    {
        if (levelText == null ||
            levelText.font == null ||
            levelText.fontSharedMaterial == null)
        {
            return;
        }

        ApplyTypography(timeText, levelText.font, levelText.fontSharedMaterial);
        ApplyTypography(goldView.RemaningText, levelText.font, levelText.fontSharedMaterial);
    }

    private static void ApplyTypography(
        DraftUtils.OptionalTMPTextGroup textGroup,
        TMP_FontAsset font,
        Material material)
    {
        if (textGroup?.value == null ||
            !textGroup.value.isPresent ||
            textGroup.value.values == null)
        {
            return;
        }

        foreach (var textItem in textGroup.value.values)
        {
            if (textItem?.Text == null)
            {
                continue;
            }

            textItem.Text.font = font;
            textItem.Text.fontSharedMaterial = material;
            textItem.Text.fontStyle &= ~(FontStyles.Underline | FontStyles.Strikethrough);
        }
    }

    private void SetTimeText(float time)
    {
        var formattedTime = DraftUtils.Utils.TimeFormatter.SecondsToFormattedString(
            time,
            DraftUtils.Utils.TimeFormatter.DefaultFormat
        );
        timeText.SetText(formattedTime);
    }

    public void ShowCoffeeTimeBonus(Action onCompleted = null)
    {
        if (timeText == null || !timeText.value.isPresent || timeText.value.values == null || timeText.value.values.Count == 0)
        {
            onCompleted?.Invoke();
            return;
        }

        var targetText = timeText.value.values[0].Text;
        if (targetText == null)
        {
            onCompleted?.Invoke();
            return;
        }

        var floatingText = new GameObject("CoffeeTimeBonusText", typeof(RectTransform));
        floatingText.transform.SetParent(targetText.transform.parent, false);

        var tmp = floatingText.AddComponent<TextMeshProUGUI>();
        tmp.text = $"+{CoffeeTimeBonusSeconds:0}s";
        tmp.font = targetText.font;
        tmp.fontSharedMaterial = targetText.fontSharedMaterial;
        tmp.fontSize = targetText.fontSize + 8f;
        tmp.color = new Color(1f, 0.85f, 0.2f, 1f);
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;

        var rect = (RectTransform)floatingText.transform;
        rect.sizeDelta = new Vector2(180f, 60f);
        rect.anchoredPosition = new Vector2(0f, 90f);
        rect.localScale = Vector3.one;

        var targetRect = (RectTransform)targetText.transform;
        rect.DOMove(targetRect.position, CoffeeTimeFlyDuration)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                Destroy(floatingText);
                onCompleted?.Invoke();
            });

        rect.DOScale(Vector3.one * 1.2f, 0.2f).SetEase(Ease.OutBack);
    }
    private void OnBackButtonClicked()
    {
        PopupConfirmReplay confirmPopup = PopupManager.Instance.GetPopupConfirmReplay();
        confirmPopup.ShowQuitConfirmation();
    }

    private void OnReplayButtonClicked()
    {
        PopupConfirmReplay confirmPopup = PopupManager.Instance.GetPopupConfirmReplay();
        confirmPopup.ShowReplayConfirmation();
    }
}
