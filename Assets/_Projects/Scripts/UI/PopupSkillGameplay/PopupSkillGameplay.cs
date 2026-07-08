using System.Collections.Generic;
using UnityEngine;

public class PopupSkillGameplay : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] public DraftUtils.Popup popup;
    [SerializeField] private FreezeSkillRemainingButton freezeTimeSkillItem;
    [SerializeField] private SkillRemainingDataView splitContainerItem;
    [SerializeField] private SkillRemainingDataView destroyContainerContainerItem;
    [SerializeField] private SkillRemainingDataView addTileItemsItem;

    private void Start()
    {
        if (freezeTimeSkillItem?.SkillRemainingDataView?.Button != null)
        {
            freezeTimeSkillItem.SkillRemainingDataView.Button.OnClickAction = ClickFreeTimeSkill;
        }

        if (splitContainerItem?.Button != null)
        {
            splitContainerItem.Button.OnClickAction = ClickSplitContainerSkill;
        }

        if (destroyContainerContainerItem?.Button != null)
        {
            destroyContainerContainerItem.Button.OnClickAction = ClickDestroyContainerSkill;
        }

        if (addTileItemsItem?.Button != null)
        {
            addTileItemsItem.Button.OnClickAction = ClickAddTileSkill;
        }
    }

    private void ClickAddTileSkill()
    {
        if (DataManager.Instance != null && DataManager.Instance.IsRemaning(ItemType.Skill_AddTile))
        {
            var popupUsingSkill = PopupManager.Instance?.GetPopupUsingSkill();
            popupUsingSkill?.SetData(ItemType.Skill_AddTile);
            LevelFactory.Instance?.LevelRunner?.GameplayStateMachine?.ChangeToUsingAddTileSkillState();
            popup?.Hide();
        }
        else
        {
            var popupBuyItem = PopupManager.Instance?.GetPopupBuyItem();
            popupBuyItem?.SetData(ItemType.Skill_AddTile);
        }
    }

    private void ClickDestroyContainerSkill()
    {
        if (DataManager.Instance != null && DataManager.Instance.IsRemaning(ItemType.Skill_DestroyContainer))
        {
            var popupUsingSkill = PopupManager.Instance?.GetPopupUsingSkill();
            popupUsingSkill?.SetData(ItemType.Skill_DestroyContainer);
            LevelFactory.Instance?.LevelRunner?.GameplayStateMachine?.ChangeToUsingDestroySkillState();
            popup?.Hide();
        }
        else
        {
            var popupBuyItem = PopupManager.Instance?.GetPopupBuyItem();
            popupBuyItem?.SetData(ItemType.Skill_DestroyContainer);
        }
    }

    private void ClickSplitContainerSkill()
    {
        if (DataManager.Instance != null && DataManager.Instance.IsRemaning(ItemType.Skill_SplitContainer))
        {
            var popupUsingSkill = PopupManager.Instance?.GetPopupUsingSkill();
            popupUsingSkill?.SetData(ItemType.Skill_SplitContainer);
            LevelFactory.Instance?.LevelRunner?.GameplayStateMachine?.ChangeToUsingSplitTileSkillState();
            popup?.Hide();
        }
        else
        {
            var popupBuyItem = PopupManager.Instance?.GetPopupBuyItem();
            popupBuyItem?.SetData(ItemType.Skill_SplitContainer);
        }
    }

    private void ClickFreeTimeSkill()
    {
        if (LevelFactory.Instance?.LevelRunner != null && LevelFactory.Instance.LevelRunner.IsFreezeTime)
        {
            return;
        }

        if (DataManager.Instance != null && DataManager.Instance.IsRemaning(ItemType.Skill_FreezeTime))
        {
            DataManager.Instance.Using(ItemType.Skill_FreezeTime, -1);
            if (freezeTimeSkillItem?.SkillRemainingDataView != null)
            {
                freezeTimeSkillItem.SkillRemainingDataView.SetData(ItemType.Skill_FreezeTime);
            }

            LevelFactory.Instance?.LevelRunner?.StartFreezeTime(10f);
            SetFreezeTimeFill(1f);
        }
        else
        {
            var popupBuyItem = PopupManager.Instance?.GetPopupBuyItem();
            popupBuyItem?.SetData(ItemType.Skill_FreezeTime);
        }
    }

    public void SetData()
    {
        if (freezeTimeSkillItem?.SkillRemainingDataView != null)
        {
            freezeTimeSkillItem.SkillRemainingDataView.SetData(ItemType.Skill_FreezeTime);
        }

        splitContainerItem?.SetData(ItemType.Skill_SplitContainer);
        destroyContainerContainerItem?.SetData(ItemType.Skill_DestroyContainer);
        addTileItemsItem?.SetData(ItemType.Skill_AddTile);
    }

    public void SetFreezeTimeFill(float ratio)
    {
        freezeTimeSkillItem.SetFill(ratio);
    }
}
