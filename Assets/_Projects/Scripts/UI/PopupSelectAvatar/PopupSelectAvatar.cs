using System;
using System.Collections;
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
        ConfigureNameInputVisual();

        popup.closeButton.RegisterClickEvents();
        popup.closeButton.OnClickAction = popup.HideWithAnimation;

        nameChangeInput.characterLimit = DataManager.PlayerNameMaxLength;
        nameChangeInput.onValueChanged.AddListener(OnNameChanged);

        editButton.onClick.AddListener(ClicEditButton);

        confirmButton.onClick.AddListener(ClickConfirmButton);

    }

    private void ConfigureNameInputVisual()
    {
        if (nameChangeInput == null || nameText == null)
        {
            return;
        }

        // Previously the editable field lived off-screen under a nearly transparent helper,
        // while a separate label displayed the name. The keyboard could open, but TMP drew
        // the caret at X=-10000. Put the real input in the label's responsive layout instead.
        var inputRect = nameChangeInput.transform as RectTransform;
        var labelRect = nameText.rectTransform;
        if (inputRect != null && labelRect.parent != null)
        {
            inputRect.SetParent(labelRect.parent, false);
            inputRect.anchorMin = labelRect.anchorMin;
            inputRect.anchorMax = labelRect.anchorMax;
            inputRect.pivot = labelRect.pivot;
            inputRect.anchoredPosition = labelRect.anchoredPosition;
            inputRect.sizeDelta = labelRect.sizeDelta;
            inputRect.localRotation = labelRect.localRotation;
            inputRect.localScale = labelRect.localScale;
            inputRect.SetSiblingIndex(labelRect.GetSiblingIndex() + 1);
        }

        var inputBackground = nameChangeInput.GetComponent<Image>();
        if (inputBackground != null)
        {
            var backgroundColor = inputBackground.color;
            backgroundColor.a = 0f;
            inputBackground.color = backgroundColor;
            inputBackground.raycastTarget = true;
        }

        var inputText = nameChangeInput.textComponent;
        if (inputText != null)
        {
            inputText.font = nameText.font;
            inputText.fontSharedMaterial = nameText.fontSharedMaterial;
            inputText.fontSize = nameText.fontSize;
            inputText.fontStyle = nameText.fontStyle;
            inputText.alignment = nameText.alignment;
            inputText.color = nameText.color;
            inputText.enableAutoSizing = nameText.enableAutoSizing;
            inputText.fontSizeMin = nameText.fontSizeMin;
            inputText.fontSizeMax = nameText.fontSizeMax;
            inputText.margin = nameText.margin;
        }

        var placeholderText = nameChangeInput.placeholder as TMP_Text;
        if (placeholderText != null)
        {
            var placeholderColor = nameText.color;
            placeholderColor.a = 0.55f;

            placeholderText.text = "Enter Name";
            placeholderText.font = nameText.font;
            placeholderText.fontSharedMaterial = nameText.fontSharedMaterial;
            placeholderText.fontStyle = nameText.fontStyle;
            placeholderText.alignment = nameText.alignment;
            placeholderText.color = placeholderColor;
            placeholderText.enableAutoSizing = true;
            placeholderText.fontSizeMin = 30f;
            placeholderText.fontSizeMax = Mathf.Min(nameText.fontSize, 40f);
            placeholderText.margin = nameText.margin;
        }

        nameChangeInput.customCaretColor = true;
        nameChangeInput.caretColor = nameText.color;
        nameChangeInput.caretWidth = 2;
        nameText.gameObject.SetActive(false);
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
        StartCoroutine(FocusNameInputNextFrame());
    }

    private IEnumerator FocusNameInputNextFrame()
    {
        // Let the popup show animation finish before selecting the field;
        // otherwise the animation can clear EventSystem focus on mobile.
        yield return new WaitForSecondsRealtime(0.35f);
        var eventSystem = UnityEngine.EventSystems.EventSystem.current;
        if (eventSystem != null)
        {
            nameChangeInput.OnPointerClick(new UnityEngine.EventSystems.PointerEventData(eventSystem));
        }
        else
        {
            DraftUtils.Utils.TMPInputFieldUtils.Focus(nameChangeInput);
        }
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
