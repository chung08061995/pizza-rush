

using System.Collections;
using DG.Tweening;
using UnityEngine;

[System.Serializable]
public class ContainerFlyAwayState : DraftUtils.IState
{

    private Container _container;

    public void SetContainer(Container container)
    {
        _container = container;
    }
    private const float FULL_CONTAINER_HEIGHT = 2f;
    public void FixedUpdate()
    {

    }

    public void OnEnter()
    {
        _container.IsFlyingAway = true;
        VibrationManager.Vibrate(VibrationType.Completion);
        _container.transform.DOKill();
        var startPos = _container.transform.position;
        var endPosUp = new Vector3(startPos.x, FULL_CONTAINER_HEIGHT, startPos.z);

        GameObject coverInstance = null;
        Sequence sequence = DOTween.Sequence();

        var moveUpDuration = DataManager.Instance.ParametterGameConfigSO.ContainerFlyAwayDuration * 0.5f;

        if (LevelFactory.Instance != null && LevelFactory.Instance.LevelRunner != null)
        {
            var spawner = LevelFactory.Instance.LevelRunner.LevelObjectSpawner;
            if (spawner != null && spawner.CoverFactory != null)
            {
                if (spawner.CoverFactory.TryGetPrefab(_container.ShapeType, out var coverPrefab))
                {
                    Transform parent = _container.transform;
                    coverInstance = Object.Instantiate(coverPrefab, parent);
                    coverInstance.transform.localPosition = new Vector3(0f, 4f, 0f);
                    if (_container.Data != null)
                    {
                        float angle = RotationTypeExtensions.ConvertToAngle(_container.Data.rotationType);
                        coverInstance.transform.localEulerAngles = new Vector3(0f, angle, 0f);
                    }
                    else
                    {
                        coverInstance.transform.localRotation = Quaternion.identity;
                    }
                    PizzaGiftTwineSealVisual.ApplyTo(coverInstance);
                    coverInstance.transform.localScale = Vector3.zero;

                    var targetScale = _container.Data != null && _container.Data.flipX
                        ? new Vector3(-1f, 1f, 1f)
                        : Vector3.one;
                    coverInstance.transform.DOScale(targetScale, moveUpDuration).SetEase(Ease.OutBack);
                    coverInstance.transform.DOLocalMoveY(0f, moveUpDuration).SetEase(Ease.OutBounce);
                }
            }
        }

        Tween tweenMoveUp = _container.transform
            .DOMove(endPosUp, moveUpDuration)
            .SetEase(DataManager.Instance.ParametterGameConfigSO.ContainerFlyAwayUpEase);
        sequence.Append(tweenMoveUp);

        sequence.AppendInterval(0.15f);

        var endPosOffScreen = new Vector3(endPosUp.x + 20, endPosUp.y, endPosUp.z);
        Tween tweenMoveOffScreen = _container.transform
            .DOMove(endPosOffScreen, DataManager.Instance.ParametterGameConfigSO.ContainerFlyAwayDuration * 0.5f)
            .SetEase(DataManager.Instance.ParametterGameConfigSO.ContainerFlyAwayOffScreenEase);

        sequence.Append(tweenMoveOffScreen);

        sequence.OnComplete(() =>
        {
            if (coverInstance != null)
            {
                Object.Destroy(coverInstance);
            }
            if (_container != null && LevelFactory.Instance.LevelRunner != null)
            {
                _container.isAnimating = false;
                var levelRunner = LevelFactory.Instance.LevelRunner;
                levelRunner.LevelObjectSpawner.ContainerPooler.Despawn(_container);
                levelRunner.LevelTracking.resolvedContainer.SetValue(levelRunner.LevelTracking.resolvedContainer.Value + 1);
                levelRunner.LevelTracking.resolvedContainer.Notifier.Notify();
            }
        });

        sequence.Play();
    }



    public void OnExit()
    {
        _container.transform.DOKill();
    }

    public void Update()
    {

    }
}
