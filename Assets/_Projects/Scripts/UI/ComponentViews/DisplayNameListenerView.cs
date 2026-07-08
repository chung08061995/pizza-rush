using UnityEngine;

public class DisplayNameListenerView : MonoBehaviour
{
    [SerializeField] private DraftUtils.OptionalTMPTextGroup nameText = new();

    public DraftUtils.OptionalTMPTextGroup NameText => nameText;
    private DraftUtils.PersistentValue<string> _playerName => DataManager.Instance.playerName;

    void Start()
    {
        _playerName.Notifier.AddListener(PlayerNameChanged);
        PlayerNameChanged();

    }
    private void OnDestroy()
    {
        _playerName.Notifier.RemoveListener(PlayerNameChanged);
    }

    private void PlayerNameChanged()
    {
        nameText.SetText(_playerName.Value);
    }
}
