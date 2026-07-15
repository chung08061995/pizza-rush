using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UsingAddTileSkillState : DraftUtils.IState
{
    private LevelRunner _levelRunner;

    public void SetLevelRunner(LevelRunner data)
    {
        _levelRunner = data;
    }

    public void FixedUpdate()
    {
    }

    public void OnEnter()
    {
        _levelRunner.LevelObjectSpawner.ShowAddTileAnchors();
    }

    public void OnExit()
    {
        _levelRunner.LevelObjectSpawner.HideAddTileAnchors();
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            AnchorPoint anchorUnderMouse = null;
            bool found = DraftUtils.Utils.Physic3DUtils.TryGetComponentUnderMouse(
                mouseScreenPosition: Input.mousePosition,
                camera: Camera.main,
                getComponentFunc: hit => hit.collider.GetComponentInParent<AnchorPoint>(),
                out anchorUnderMouse
            );

            if (found && anchorUnderMouse != null)
            {
                _levelRunner.LevelObjectSpawner.AddNewTile(anchorUnderMouse.CellPosition);
                DataManager.Instance.Using(ItemType.Skill_AddTile, -1);
                GameAnalytics.LogItemEvent(GameAnalytics.SkillUse, ItemType.Skill_AddTile);
                CancelSkill();
            }
        }
    }

    public void CancelSkill()
    {
        _levelRunner.GameplayStateMachine.ChangeToDragContainerState();
        PopupManager.Instance.HidePopupUsingSkill();
        PopupManager.Instance.GetPopupSkillGameplay();
    }
}
