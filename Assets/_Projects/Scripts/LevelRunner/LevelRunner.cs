using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using DG.Tweening;
using System.Collections;
using System;

public class LevelRunner : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] private LevelObjectSpawner levelObjectSpawner;

    private DraftUtils.TimeCountdown _timer = new();

    [ShowInInspector][ReadOnly] private LevelTracking _levelTracking = new();
    public LevelTracking LevelTracking => _levelTracking;

    private GameplayStateMachine gameplayStateMachine = new();

    public LevelObjectSpawner LevelObjectSpawner => levelObjectSpawner;

    public DraftUtils.TimeCountdown Timer => _timer;
    private LevelData _levelData;
    public LevelData LevelData => _levelData;
    public GameplayStateMachine GameplayStateMachine => gameplayStateMachine;

    [ShowInInspector] [ReadOnly] private DraftUtils.TimeCountdown _freezeTimer = new();
    public DraftUtils.TimeCountdown FreezeTimer => _freezeTimer;
    private bool _isFreezeTime = false;
    public bool IsFreezeTime => _isFreezeTime;

    public void StartFreezeTime(float duration)
    {
        _isFreezeTime = true;
        _freezeTimer.SetDuration(duration);
        _freezeTimer.ResetCountdown();
        _freezeTimer.StartCountdown();
    }

    private void Start()
    {

        _levelTracking.dragContainerTimes.Value = 0;
        _levelTracking.resolvedContainer.Value = 0;

        _levelTracking.dragContainerTimes.Notifier.AddListener(LevelTracking_DragContainerTimes_OnChanged);
        _levelTracking.resolvedContainer.Notifier.AddListener(LevelTracking_ResolvedContainer_OnChanged);


        _freezeTimer = new DraftUtils.TimeCountdown();
        _freezeTimer.AddOnTickListener(FreezeTimer_OnTick);
        _freezeTimer.AddOnFinishedListener(FreezeTimer_OnFinished);
    }

    private void FreezeTimer_OnFinished()
    {
        _isFreezeTime = false;
    }

    private void FreezeTimer_OnTick(float arg0)
    {
        if (PopupManager.Instance.popupSkillGameplayReference.instance == null) 
        {
            return;
        }
        PopupManager.Instance.popupSkillGameplayReference.instance.SetFreezeTimeFill(_freezeTimer.GetRatio());
    }

    private void LevelTracking_ResolvedContainer_OnChanged()
    {
        foreach (var container in levelObjectSpawner.ContainerPooler.ActiveItems.ToList())
        {
            if (!container.ContainerView.ContainerIceDataView.isPresent)
            {
                continue;
            }
            container.ContainerView.ContainerIceDataView.value.UpdateAmountText(_levelTracking.resolvedContainer.Value);
            int remainingAmount = container.Data.containerData.containerIceData.iceAmount - _levelTracking.resolvedContainer.Value;
            if (remainingAmount <= 0)
            {
                var inner = container.Data.containerData.containerIceData.innerContainerData;
                if (inner != null)
                {
                    LevelFactory.Instance.LevelRunner.LevelObjectSpawner.ReplaceContainer(container, inner);
                }
            }

        }
    }

    private void LevelTracking_DragContainerTimes_OnChanged()
    {
        bool isLose = false;
        foreach (var container in levelObjectSpawner.ContainerPooler.ActiveItems)
        {
            if (!container.ContainerView.ContainerBoombDataView.isPresent)
            {
                continue;
            }
            container.ContainerView.ContainerBoombDataView.value.UpdateAmountText(_levelTracking.dragContainerTimes.Value);

            if (container.Data.containerData.containerBoombData != null && container.Data.containerData.containerBoombData.boombAmount > 0)
            {
                int remainingAmount = container.Data.containerData.containerBoombData.boombAmount - _levelTracking.dragContainerTimes.Value;
                if (remainingAmount <= 0)
                {
                    isLose = true;
                }
            }
        }

        if (isLose)
        {
            EndGame();
        }
    }

    private void Update()
    {
        gameplayStateMachine.StateMachine.Update();
        
        if (_isFreezeTime)
        {
            _freezeTimer.Update(Time.deltaTime);
        }
        else
        {
            _timer.Update(Time.deltaTime);
        }
    }

    public void AddTime(float extraSeconds)
    {
        if (extraSeconds <= 0f || _timer == null)
        {
            return;
        }

        var nextDuration = _timer.Duration + extraSeconds;
        var nextRemaining = _timer.Remaining + extraSeconds;
        _timer.SetDuration(nextDuration, false);
        _timer.SetRemaining(nextRemaining);
    }

    internal void SetData(LevelData levelData)
    {
        GameAnalytics.LogLevelEvent(GameAnalytics.LevelStart);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBackgroundGame(transform);
        }

        _levelData = levelData;
        levelObjectSpawner.SetData(levelData, transform);
        gameplayStateMachine.SetData(this);

        float levelDuration = levelData.duration > 0f
            ? levelData.duration
            : DataManager.Instance.ParametterGameConfigSO.DefaultLevelTime;

        _timer.SetDuration(levelDuration);
        _timer.ResetCountdown();
        _timer.AddTickListener(PopupManager.Instance.popupGameplayReference.instance.SetData);
        _timer.AddOnFinishedListener(EndGame);
        _timer.StartCountdown();

        CenterCameraOnLevel(levelData);
    }

    private void CenterCameraOnLevel(LevelData levelData)
    {
        if (levelData == null || levelData.gridPositions == null || levelData.gridPositions.Count == 0)
        {
            return;
        }

        float minX = float.MaxValue, maxX = float.MinValue;
        float minZ = float.MaxValue, maxZ = float.MinValue;
        
        foreach (var pos in levelData.gridPositions)
        {
            Vector3 worldPos = levelObjectSpawner.Grid.CellToWorld(pos.ToVector2Int());
            if (worldPos.x < minX) minX = worldPos.x;
            if (worldPos.x > maxX) maxX = worldPos.x;
            if (worldPos.z < minZ) minZ = worldPos.z;
            if (worldPos.z > maxZ) maxZ = worldPos.z;
        }

        Vector3 centerWorldPos = new Vector3((minX + maxX) / 2f, 0f, (minZ + maxZ) / 2f);

        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            // Set camera size based on grid width
            int minGridX = int.MaxValue;
            int maxGridX = int.MinValue;
            foreach (var pos in levelData.gridPositions)
            {
                if (pos.x < minGridX) minGridX = pos.x;
                if (pos.x > maxGridX) maxGridX = pos.x;
            }
            int gridWidth = (maxGridX - minGridX) + 1;

            if (DataManager.Instance != null && DataManager.Instance.cameraSize != null)
            {
                int targetSize = gridWidth + 2; // default fallback formula
                if (DataManager.Instance.cameraSize.TryGetValue(gridWidth, out int sizeFromDict))
                {
                    targetSize = sizeFromDict;
                }
                mainCam.orthographicSize = targetSize;
            }

            Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            if (plane.Raycast(ray, out float enter))
            {
                Vector3 currentLookAt = ray.GetPoint(enter);
                Vector3 offset = centerWorldPos - currentLookAt;
                mainCam.transform.position += new Vector3(offset.x, 0f, offset.z);
            }
            else
            {
                mainCam.transform.position = new Vector3(centerWorldPos.x, mainCam.transform.position.y, centerWorldPos.z - 10f);
            }
        }
    }

    private void EndGame()
    {
        gameplayStateMachine.ChangeToLoseState();
    }

    public static class PolygonUtils
    {
        // Kiểm tra point có nằm trong đa giác (mặt phẳng XZ - dùng cho game top-down/3D mặt đất)
        public static bool IsPointInPolygon(Vector3 point, List<Vector3> polygon)
        {
            int n = polygon.Count;
            if (n < 3) return false;

            bool inside = false;
            int j = n - 1;

            for (int i = 0; i < n; i++)
            {
                float xi = polygon[i].x, zi = polygon[i].z;
                float xj = polygon[j].x, zj = polygon[j].z;

                if (((zi > point.z) != (zj > point.z)) &&
                    (point.x < (xj - xi) * (point.z - zi) / (zj - zi) + xi))
                {
                    inside = !inside;
                }

                j = i;
            }

            return inside;
        }

        // Bản dùng cho Vector2 (mặt phẳng XY - dùng cho game 2D)
        public static bool IsPointInPolygon(Vector2 point, List<Vector2> polygon)
        {
            int n = polygon.Count;
            if (n < 3) return false;

            bool inside = false;
            int j = n - 1;

            for (int i = 0; i < n; i++)
            {
                float xi = polygon[i].x, yi = polygon[i].y;
                float xj = polygon[j].x, yj = polygon[j].y;

                if (((yi > point.y) != (yj > point.y)) &&
                    (point.x < (xj - xi) * (point.y - yi) / (yj - yi) + xi))
                {
                    inside = !inside;
                }

                j = i;
            }

            return inside;
        }
    }
}
