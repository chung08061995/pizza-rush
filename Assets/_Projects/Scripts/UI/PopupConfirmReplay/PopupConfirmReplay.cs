using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PopupConfirmReplay : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] public DraftUtils.Popup popup;
    [SerializeField] private DraftUtils.OptionalButtonGroup replayButton = new();
    public DraftUtils.OptionalButtonGroup ReplayButton => replayButton;

    private void Start()
    {
        popup.closeButton.OnClickAction = popup.HideWithAnimation;
        replayButton.RegisterClickEvents();

    }
    public void GoToMain()
    {
        HeartsManager.Instance.UseHeart();
        PopupManager.Instance.HideAllPopupInGameplay();
        popup.HideWithAnimation();
        SceneControllerExtensions.LoadMain();
        popup.HideWithAnimation();
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
}
