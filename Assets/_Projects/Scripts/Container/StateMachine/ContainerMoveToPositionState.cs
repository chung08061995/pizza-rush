

using System.Collections;
using DG.Tweening;
using UnityEngine;

[System.Serializable]
public class ContainerMoveToPositionState : DraftUtils.IState
{

    private DraftUtils.SmoothMover smoothMover = new();
    private Container _container;


    public DraftUtils.SmoothMover SmoothMover => smoothMover;
    public void SetContainer(Container container)
    {
        _container = container;
    }
    public void FixedUpdate()
    {

    }

    public void OnEnter()
    {

    }
    public void OnExit()
    {
        smoothMover.Pause();
    }

    public void Update()
    {
        smoothMover.Update(Time.deltaTime);
    }

    public IEnumerator WaitMoveCompletedCouroutin(float timeout)
    {
        while (timeout > 0)
        {
            if (smoothMover.IsImmediateSnapDistance())
            {
                break;
            }
            yield return null;
            timeout -= Time.deltaTime;
        }
    }
}