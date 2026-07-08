using Sirenix.OdinInspector;
using UnityEngine;

public enum LevelCreaterStateType
{
    None = 0,
    DrawBackgroundState = 1,
    CreateElementsState = 2,
    WritePositionState = 3,
}
[System.Serializable]
public class LevelCreatorStateMachine
{
    private DrawBackgroundState drawBackgroundState = new();
    private CreateElementsState createElementsState = new();
    private WritePositionState writePositionState = new();

    private DraftUtils.StateNode drawBackgroundNode = new();
    private DraftUtils.StateNode createElementsNode = new();
    private DraftUtils.StateNode writePositionNode = new();
    private DraftUtils.StateMachine stateMachine = new();
    public DraftUtils.StateMachine StateMachine => stateMachine;
    public DrawBackgroundState DrawBackgroundState => drawBackgroundState;


    private LevelCreator _data;
    public void SetData(LevelCreator data)
    {
        _data = data;

        drawBackgroundState.SetLevelCreator(data);
        createElementsState.SetLevelCreator(data);
        writePositionState.SetLevelCreator(data);


        drawBackgroundNode.State = drawBackgroundState;
        createElementsNode.State = createElementsState;
        writePositionNode.State = writePositionState;

        stateMachine.AddNode(LevelCreaterStateType.DrawBackgroundState, drawBackgroundNode);
        stateMachine.AddNode(LevelCreaterStateType.CreateElementsState, createElementsNode);
        stateMachine.AddNode(LevelCreaterStateType.WritePositionState, writePositionNode);



        stateMachine.StartAt(LevelCreaterStateType.CreateElementsState);
    }

    [Button]
    private void StartDrawBackgroundState()
    {
        stateMachine.ChangeStateByKey(LevelCreaterStateType.DrawBackgroundState);
    }
    [Button]
    private void StartCreateElementsState()
    {
        stateMachine.ChangeStateByKey(LevelCreaterStateType.CreateElementsState);
    }
    [Button]
    private void StartWritePositionState()
    {
        stateMachine.ChangeStateByKey(LevelCreaterStateType.WritePositionState);
    }
}
