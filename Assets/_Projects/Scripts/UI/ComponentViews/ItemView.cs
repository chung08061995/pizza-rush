using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemView : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] private Transform root;
    [SerializeField] private DraftUtils.OptionalImageGroup icon = new();
    [SerializeField] private DraftUtils.OptionalTMPTextGroup nameText = new();
    [SerializeField] private DraftUtils.OptionalTMPTextGroup descriptionText = new();
    [SerializeField] private DraftUtils.OptionalTMPTextGroup descriptionNewItemText = new();
    [SerializeField] private DraftUtils.OptionalTMPTextGroup remaningText = new();
    [SerializeField] private DraftUtils.OptionalTMPTextGroup skillTutorialText = new();
    [SerializeField] private DraftUtils.OptionalTMPTextGroup costText = new();
    [SerializeField] private DraftUtils.OptionalTMPTextGroup levelUnlockText = new();
    [SerializeField] private DraftUtils.OptionalButtonGroup button = new();

    [ShowInInspector] [ReadOnly] private ItemType _data;
    public ItemType Data => _data;
    public Transform Root => root;

    public DraftUtils.OptionalTMPTextGroup RemaningText => remaningText;
    public DraftUtils.OptionalButtonGroup Button => button;

    private void Start()
    {
        button.RegisterClickEvents();
    }

    public void SetData(ItemType data)
    {
        TryFirstRegisterRemaningChangedEvent(data);
        _data = data;
        SetIcon();
        SetNameText();
        SetDescriptionText();
        SetRemaningText();
        SetSkillTutorialText();
        SetCostText();
        SetLevelUnlockText();
        SetDescriptionNewItemText();
    }
    private void TryFirstRegisterRemaningChangedEvent(ItemType data)
    {
        if (_data != ItemType.None)
        {
            return;
        }
        if (DataManager.Instance.remainningItems.TryGetValue(data, out var persistentValue))
        {
            persistentValue.Notifier.AddListener(SetRemaningText);
        }
    }
    private void TryRemoveRegisterRemaningChangedEvent()
    {
        if (_data == ItemType.None)
        {
            return;
        }
        if (DataManager.Instance.remainningItems.TryGetValue(_data, out var persistentValue))
        {
            persistentValue.Notifier.RemoveListener(SetRemaningText);
        }
    }
    private void OnDestroy()
    {
        TryRemoveRegisterRemaningChangedEvent();
    }
    private void SetRemaningText()
    {
        if (DataManager.Instance.remainningItems.TryGetValue(_data, out var persistentValue))
        {
            remaningText.SetText(persistentValue.Value);
            SetRemaningTextActive();
        }
    }

    public void SetRemaningTextActive()
    {
        remaningText.SetActive(false);

        if (DataManager.Instance.remainningItems.TryGetValue(_data, out var persistentValue))
        {
            bool hasRemaining = persistentValue.Value > 0;

            remaningText.SetActive(hasRemaining);
        }
    }

    private void SetNameText()
    {
        if (DataManager.Instance.nameItems.TryGetValue(_data, out var nameItem))
        {
            nameText.SetText(nameItem);
        }
    }

    private void SetDescriptionText()
    {
        if (DataManager.Instance.descriptionItems.TryGetValue(_data, out var description))
        {
            descriptionText.SetText(description);
        }
    }

    private void SetDescriptionNewItemText()
    {
        if (DataManager.Instance.descriptionNewItems.TryGetValue(_data, out var description))
        {
            descriptionNewItemText.SetText(description);
        }
    }
    private void SetSkillTutorialText()
    {
        if (DataManager.Instance.skillTutorialItems.TryGetValue(_data, out var tutorial))
        {
            skillTutorialText.SetText(tutorial);
        }
    }
    private void SetCostText()
    {
        if (DataManager.Instance.costItems.TryGetValue(_data, out var cost))
        {
            costText.SetText(cost);
        }
    }

    private void SetIcon()
    {
        if(!DataManager.Instance.iconItemsSO.TryGetValue(_data, out var iconSprite))
        {
            return;
        }
        icon.SetSprite(iconSprite);
    }

    private void SetLevelUnlockText()
    {
        if (DataManager.Instance.levelUnlockItems.TryGetValue(_data, out var unlockLevel))
        {
            levelUnlockText.SetActive(true);
            levelUnlockText.SetText(string.Format("Lv.{0}", unlockLevel));
        }
        else
        {
            levelUnlockText.SetActive(false);
        }
    }
}
