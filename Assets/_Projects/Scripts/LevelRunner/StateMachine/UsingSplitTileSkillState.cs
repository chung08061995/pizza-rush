using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UsingSplitTileSkillState : DraftUtils.IState
{
    private LevelRunner _levelRunner;
    private bool _isSplitting = false;

    public void SetLevelRunner(LevelRunner data)
    {
        _levelRunner = data;
    }

    public void FixedUpdate()
    {
    }

    public void OnEnter()
    {
        _isSplitting = false;
    }

    public void OnExit()
    {
    }

    public void Update()
    {
        if (_isSplitting)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Container containerUnderMouse = null;
            bool found = DraftUtils.Utils.Physic3DUtils.TryGetComponentUnderMouse(
                mouseScreenPosition: Input.mousePosition,
                camera: Camera.main,
                getComponentFunc: hit => hit.collider.GetComponentInParent<Container>(),
                out containerUnderMouse
            );

            if (found && containerUnderMouse != null)
            {
                var splitDatas = ContainerDataUtils.Split(containerUnderMouse.Data.containerData);
                if (splitDatas.Count > 0)
                {
                    _levelRunner.StartCoroutine(SplitCoroutine(containerUnderMouse, splitDatas));
                }
            }
        }
    }

    private IEnumerator SplitCoroutine(Container container, List<SplitContainerData> splitDatas)
    {
        _isSplitting = true;
        container.ShowSplitObject(true);

        yield return new WaitForSeconds(0.5f);

        if (container != null && container.gameObject.activeSelf)
        {
            _levelRunner.LevelObjectSpawner.SplitAndReplaceContainer(container, splitDatas);
        }

        DataManager.Instance.Using(ItemType.Skill_SplitContainer, -1);
        CancelSkill();
        _isSplitting = false;
    }

    public void CancelSkill()
    {
        _levelRunner.GameplayStateMachine.ChangeToDragContainerState();
        PopupManager.Instance.HidePopupUsingSkill();
        PopupManager.Instance.GetPopupSkillGameplay();
    }
}
