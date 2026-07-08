using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum GameplayStateType
{
    None = 0,
    UsingDestroySkill = 1,
    DraggingContainer = 2,
    Win = 3,
    Lose = 4,
    UsingAddTileSkill = 5,
    UsingSplitTileSkill = 6,
}

public class GameplayStateMachine
{
    private WinState winState = new();
    private LoseState loseState = new();
    private DragContainerState dragContainerState = new();
    private UsingDestroySkillState usingDestroySkillState = new();
    private UsingAddTileSkillState usingAddTileSkillState = new();
    private UsingSplitTileSkillState usingSplitTileSkillState = new();
    private DraftUtils.StateNode winNode = new();
    private DraftUtils.StateNode loseNode = new();
    private DraftUtils.StateNode dragContainerNode = new();
    private DraftUtils.StateNode usingDestroySkillNode = new();
    private DraftUtils.StateNode usingAddTileSkillNode = new();
    private DraftUtils.StateNode usingSplitTileSkillNode = new();

    private DraftUtils.FuncPredicate winPredicate = new();
    private DraftUtils.StateMachine stateMachine = new();
    public DraftUtils.StateMachine StateMachine => stateMachine;


    private LevelRunner _data;
    public void SetData(LevelRunner data)
    {
        _data = data;

        winState.SetLevelRunner(data);
        loseState.SetLevelRunner(data);
        dragContainerState.SetLevelRunner(data);
        usingDestroySkillState.SetLevelRunner(data);
        usingAddTileSkillState.SetLevelRunner(data);
        usingSplitTileSkillState.SetLevelRunner(data);

        winNode.State = winState;
        loseNode.State = loseState;
        dragContainerNode.State = dragContainerState;
        usingDestroySkillNode.State = usingDestroySkillState;
        usingAddTileSkillNode.State = usingAddTileSkillState;
        usingSplitTileSkillNode.State = usingSplitTileSkillState;

        stateMachine.AddNode(GameplayStateType.Win, winNode);
        stateMachine.AddNode(GameplayStateType.Lose, loseNode);
        stateMachine.AddNode(GameplayStateType.DraggingContainer, dragContainerNode);
        stateMachine.AddNode(GameplayStateType.UsingDestroySkill, usingDestroySkillNode);
        stateMachine.AddNode(GameplayStateType.UsingAddTileSkill, usingAddTileSkillNode);
        stateMachine.AddNode(GameplayStateType.UsingSplitTileSkill, usingSplitTileSkillNode);

        winPredicate = new(WinPredicate);
        stateMachine.AnyTransitions.AddAnyTransition(winNode, winPredicate);

        stateMachine.finalStateKeys.Add(GameplayStateType.Win);
        stateMachine.finalStateKeys.Add(GameplayStateType.Lose);

        stateMachine.StartAt(GameplayStateType.DraggingContainer);
    }

    public void ChangeToDragContainerState()
    {
        stateMachine.ChangeStateByKey(GameplayStateType.DraggingContainer);
    }

    public void ChangeToUsingDestroySkillState()
    {
        stateMachine.ChangeStateByKey(GameplayStateType.UsingDestroySkill);
    }

    public void ChangeToUsingAddTileSkillState()
    {
        stateMachine.ChangeStateByKey(GameplayStateType.UsingAddTileSkill);
    }

    public void ChangeToUsingSplitTileSkillState()
    {
        stateMachine.ChangeStateByKey(GameplayStateType.UsingSplitTileSkill);
    }

    private bool WinPredicate()
    {
        var spawner = _data.LevelObjectSpawner;
        if (spawner == null) return false;

        var productionLines = spawner.ProductionLinePooler.ActiveItems;
        if (productionLines == null || productionLines.Count == 0)
        {
            return false;
        }
        
        for (int i = 0; i < productionLines.Count; i++)
        {
            var line = productionLines[i];
            if (line != null && line.ProductionPooler != null && line.ProductionPooler.ActiveItems.Count > 0)
            {
                return false;
            }
        }
        
        return true;
    }

    public void ChangeToLoseState()
    {
        stateMachine.ChangeStateByKey(GameplayStateType.Lose);
    }
}
