using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupConfirmReplay : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] public DraftUtils.Popup popup;
    [SerializeField] private DraftUtils.OptionalButtonGroup replayButton = new();
    [SerializeField] private Sprite quitContentSprite;

    private const string ReplayTitle = "Notice";
    private const string ReplayAction = "Replay";
    private const string QuitTitle = "Quit Level?";
    private const string QuitAction = "Quit";

    private void Start()
    {
        popup.closeButton.OnClickAction = popup.HideWithAnimation;
        replayButton.RegisterClickEvents();
    }

    public void ShowReplayConfirmation()
    {
        SetContent(ReplayTitle, ReplayAction, null);
        replayButton.OnClickAction = Replay;
    }

    public void ShowQuitConfirmation()
    {
        SetContent(QuitTitle, QuitAction, quitContentSprite);
        replayButton.OnClickAction = GoToMain;
    }

    public void GoToMain()
    {
        HeartsManager.Instance.UseHeart();
        PopupManager.Instance.HideAllPopupInGameplay();
        SceneControllerExtensions.LoadMain();
    }

    public void Replay()
    {
        if (HeartsManager.Instance.IsRemainingHeart())
        {
            ReloadLevel();
        }
        else
        {
            PopupManager.Instance.ShowPopupMoreLives(ReloadLevel, GoToMain);
            
        }
    }
    private void ReloadLevel()
    {
        HeartsManager.Instance.UseHeart();
        LevelFactory.Instance.LoadCurrentLevelData();
        popup.HideWithAnimation();
    }

    private void SetContent(string title, string action, Sprite contentSprite)
    {
        TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
        TMP_Text titleText = texts.FirstOrDefault(text => text.text == ReplayTitle);
        TMP_Text actionText = texts.FirstOrDefault(text => text.text == ReplayAction);

        if (titleText != null)
        {
            titleText.text = title;
        }

        if (actionText != null)
        {
            actionText.text = action;
            CenterActionLabel(actionText);
        }

        if (contentSprite == null)
        {
            return;
        }

        Image contentImage = GetComponentsInChildren<Image>(true)
            .FirstOrDefault(image => image.gameObject.name == "Icon");
        if (contentImage != null)
        {
            contentImage.sprite = contentSprite;
            contentImage.preserveAspect = true;
        }
    }

    private static void CenterActionLabel(TMP_Text actionText)
    {
        Button actionButton = actionText.GetComponentInParent<Button>();
        if (actionButton == null)
        {
            return;
        }

        RectTransform labelRect = actionText.rectTransform;
        labelRect.SetParent(actionButton.transform, false);
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        labelRect.localScale = Vector3.one;
        actionText.alignment = TextAlignmentOptions.Center;
    }
}
