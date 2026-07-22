using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Cheat level: nhập số level rồi bấm Go để load level đó.
/// Đặt trong panel của PopupCheat.
/// </summary>
public class PopupCheatLevel : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] private InputListenerIntPersistentValue levelInput;
    [SerializeField] private Button nextButton;

    private void Start()
    {
        levelInput.SetPersistentValue(DataManager.Instance.Level);
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(LoadNextLevel);
        }
    }

    private void LoadNextLevel()
    {
        var current = DataManager.Instance.Level.Value;
        // Keep the player-facing level increasing. LevelFactory detects values
        // above the authored range and loads a random authored board.
        var next = current + 1;

        DataManager.Instance.Level.SetValue(next);
        DataManager.Instance.Level.Notifier.Notify();
        DataManager.Instance.Level.Save();

        PopupManager.Instance.HideAllPopupInGameplay();
        SceneControllerExtensions.LoadGameplay();
    }

}
