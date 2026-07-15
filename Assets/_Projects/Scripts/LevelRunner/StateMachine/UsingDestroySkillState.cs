using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[System.Serializable]
public class UsingDestroySkillState : DraftUtils.IState
{
    private LevelRunner _levelRunner;
    private bool _isDestroying = false;

    public void SetLevelRunner(LevelRunner data)
    {
        _levelRunner = data;
    }

    public void FixedUpdate()
    {
    }

    public void OnEnter()
    {
        _isDestroying = false;
    }

    public void OnExit()
    {
    }

    public void Update()
    {
        if (_isDestroying)
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
                _isDestroying = true;
                containerUnderMouse.ShowDestroyObject(true);

                _levelRunner.StartCoroutine(DestroyCoroutine(containerUnderMouse));
            }
        }
    }

    private IEnumerator DestroyCoroutine(Container containerUnderMouse)
    {
        var colliders = containerUnderMouse.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }

        yield return new WaitForSeconds(1f);

        var transform = containerUnderMouse.transform;
        var originalScale = transform.localScale;

        DG.Tweening.Sequence seq = DG.Tweening.DOTween.Sequence();
        seq.Append(transform.DOScale(originalScale * 1.2f, 0.15f));
        seq.Append(transform.DOScale(Vector3.zero, 0.2f).SetEase(DG.Tweening.Ease.InBack));
        seq.OnComplete(() =>
        {
            transform.localScale = originalScale;
            foreach (var col in colliders)
            {
                if (col != null) col.enabled = true;
            }
            _levelRunner.LevelObjectSpawner.DestroyContainer(containerUnderMouse);

            DataManager.Instance.Using(ItemType.Skill_DestroyContainer, -1);
            GameAnalytics.LogItemEvent(GameAnalytics.SkillUse, ItemType.Skill_DestroyContainer);
            CancelSkill();
            _isDestroying = false;
        });
    }

    public void CancelSkill()
    {
        _levelRunner.GameplayStateMachine.ChangeToDragContainerState();
        PopupManager.Instance.HidePopupUsingSkill();
        PopupManager.Instance.GetPopupSkillGameplay();
    }
}
