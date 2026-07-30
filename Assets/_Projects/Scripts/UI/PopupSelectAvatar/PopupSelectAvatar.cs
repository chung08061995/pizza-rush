using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupSelectAvatar : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] public DraftUtils.Popup popup;
    [SerializeField]
    private DraftUtils.Pooler<ItemView> avatarPooler = new();
    [SerializeField] private Button editButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TMP_InputField nameChangeInput;
    [SerializeField] private TMP_Text nameText;

    private DraftUtils.PersistentValue<string> _playerName => DataManager.Instance.playerName;
    private ItemType _currentAvatarType;

    private void Start()
    {
        popup.closeButton.RegisterClickEvents();
        popup.closeButton.OnClickAction = popup.HideWithAnimation;

        nameChangeInput.characterLimit = DataManager.PlayerNameMaxLength;
        nameChangeInput.onValueChanged.AddListener(OnNameChanged);

        editButton.onClick.AddListener(ClicEditButton);

        confirmButton.onClick.AddListener(ClickConfirmButton);

    }

    public void SetData()
    {
        _currentAvatarType = DataManager.Instance.currentAvatar.Value;
        SetNameChangeInput();
        SetNameText();
        UpdateNameValidation(_playerName.Value);
        GenerateItem();
    }

    private void GenerateItem()
    {
        avatarPooler.Factory = new DraftUtils.ComponentInstantiatePoolFactory<ItemView>();

        foreach (var avatarType in DataManager.Instance.avatarTypes)
        {
            var avatarGo = avatarPooler.Spawn();
            avatarGo.SetData(avatarType);
            avatarGo.Button.SetInteractable(avatarType != DataManager.Instance.currentAvatar.Value);
            avatarGo.Button.OnClickAction = () =>
            {
                _currentAvatarType = avatarGo.Data;
                SelectAvatar();
            };
        }
    }

    private void SelectAvatar()
    {
        foreach (var answerGo in avatarPooler.ActiveItems)
        {
            answerGo.Button.SetInteractable(answerGo.Data != _currentAvatarType);
        }
    }

    private void SetNameChangeInput()
    {
        nameChangeInput.text = _playerName.Value;
    }
    private void SetNameText()
    {
        nameText.text = _playerName.Value;
    }

    private void ClickConfirmButton()
    {
        var normalizedName = DataManager.NormalizePlayerName(nameChangeInput.text);
        if (!DataManager.IsValidPlayerName(normalizedName))
        {
            return;
        }

        _playerName.SetValueAndSaveNotify(normalizedName);
        DataManager.Instance.hasCustomizedProfile.SetValueAndSave(true);
        DataManager.Instance.currentAvatar.SetValueAndSaveNotify(_currentAvatarType);
        popup.HideWithAnimation();
    }

    private void ClicEditButton()
    {
        DraftUtils.Utils.TMPInputFieldUtils.Focus(nameChangeInput);
    }

    private void OnNameChanged(string value)
    {
        nameText.text = value;
        UpdateNameValidation(value);
    }

    private void UpdateNameValidation(string value)
    {
        confirmButton.interactable = DataManager.IsValidPlayerName(value);
    }
}
