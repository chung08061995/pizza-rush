using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Cheat level: nhập số level rồi bấm Go để load level đó.
/// Đặt trong panel của PopupCheat.
/// </summary>
public class PopupCheatLevel : DraftUtils.DraftMonoBehaviour
{
    private const int MaxLevel = 100;

    [SerializeField] private InputListenerIntPersistentValue levelInput;
    [SerializeField] private Button nextButton;

    private bool randomLoop;

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
        randomLoop |= current >= MaxLevel;
        var next = randomLoop
            ? Random.Range(1, MaxLevel + 1)
            : current + 1;

        DataManager.Instance.Level.SetValue(next);
        DataManager.Instance.Level.Notifier.Notify();
        DataManager.Instance.Level.Save();

        PopupManager.Instance.HideAllPopupInGameplay();
        SceneControllerExtensions.LoadGameplay();
    }
}
