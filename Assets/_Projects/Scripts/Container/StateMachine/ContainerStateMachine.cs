using System;
using Sirenix.OdinInspector;
using UnityEngine;

public enum ContainerState
{
    None = 0,
    MoveToPosition = 1,
    FlyAway = 2,
}
[System.Serializable]
public class ContainerStateMachine
{
    private ContainerFlyAwayState flyAwayState = new();
    private ContainerMoveToPositionState moveToPositionState = new();

    private DraftUtils.StateNode flyAwayNode = new();
    private DraftUtils.StateNode moveToPositionNode = new();
    private DraftUtils.StateMachine stateMachine = new();


    private Container _data;

    public DraftUtils.StateMachine StateMachine => stateMachine;
    public ContainerMoveToPositionState MoveToPositionState => moveToPositionState;
    public void SetData(Container data)
    {
        _data = data;

        flyAwayState.SetContainer(data);
        moveToPositionState.SetContainer(data);



        flyAwayNode.State = flyAwayState;


        moveToPositionNode.State = moveToPositionState;

        stateMachine.AddNode(ContainerState.MoveToPosition, moveToPositionNode);
        stateMachine.AddNode(ContainerState.FlyAway, flyAwayNode);

        stateMachine.finalStateKeys.Add(ContainerState.FlyAway);

        stateMachine.StartAt(ContainerState.MoveToPosition);
    }

    [Button]
    public void ChangeToFlyAwayState()
    {
        stateMachine.ChangeStateByKey(ContainerState.FlyAway);
    }
}
