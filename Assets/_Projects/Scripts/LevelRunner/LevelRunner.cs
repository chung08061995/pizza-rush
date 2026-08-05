using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using DG.Tweening;
using System.Collections;
using System;
using UnityEngine.Rendering;

public class LevelRunner : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] private LevelObjectSpawner levelObjectSpawner;
    [SerializeField] private GameplayVisualConfigSO gameplayVisualConfig;

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
    private float _lastVisualAspect = -1f;
    private int _totalProgressContainers;
    private float _idleHintElapsed;
    private const float IdleHintDelay = 6f;
    private const float HintPulseDuration = 1.15f;
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
        UpdateGameplayProgress();
        foreach (var container in levelObjectSpawner.ContainerPooler.ActiveItems.ToList())
        {
            var containerData = container.Data?.containerData;
            if (containerData == null || containerData.containerMaterialType != ContainerMaterialType.Ice ||
                containerData.containerIceData == null || containerData.containerIceData.iceAmount <= 0)
            {
                continue;
            }

            if (container.ContainerView.ContainerIceDataView.isPresent)
            {
                container.ContainerView.ContainerIceDataView.value.UpdateAmountText(_levelTracking.resolvedContainer.Value);
            }

            int remainingAmount = containerData.containerIceData.iceAmount - _levelTracking.resolvedContainer.Value;
            if (remainingAmount <= 0)
            {
                var inner = containerData.containerIceData.innerContainerData;
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
        UpdateIdleHint();

        if (_isFreezeTime)
        {
            _freezeTimer.Update(Time.deltaTime);
        }
        else
        {
#if UNITY_EDITOR
            if (!EditorDebugSettings.InfiniteTime)
            {
                _timer.Update(Time.deltaTime);
            }
#else
            _timer.Update(Time.deltaTime);
#endif
        }

        RefreshVisualsWhenAspectChanges();
    }

    private void UpdateIdleHint()
    {
        if (Input.GetMouseButton(0) || Input.GetMouseButtonDown(0) || Input.touchCount > 0)
        {
            _idleHintElapsed = 0f;
            return;
        }

        _idleHintElapsed += Time.unscaledDeltaTime;
        if (_idleHintElapsed < IdleHintDelay)
        {
            return;
        }
        _idleHintElapsed = 0f;

        if (!gameplayStateMachine.TryGetHintCandidate(out Container container) || container == null)
        {
            return;
        }

        container.GetComponentInChildren<PizzaContainerThemeVisual>()?.ShowHintPulse();
        ColorType color = container.Data?.containerData?.containerColorData?.colorType ?? ColorType.None;
        if (color != ColorType.None)
        {
            PizzaProductionLineThemeVisual.ShowHintColor(color, HintPulseDuration);
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
        _totalProgressContainers = CountProgressContainers();
        UpdateGameplayProgress();
        gameplayStateMachine.SetData(this);

        float levelDuration = levelData.duration > 0f
            ? levelData.duration
            : DataManager.Instance.ParametterGameConfigSO.DefaultLevelTime;

        _timer.SetDuration(levelDuration);
        _timer.ResetCountdown();
        _timer.AddTickListener(PopupManager.Instance.popupGameplayReference.instance.SetData);
        _timer.AddOnFinishedListener(EndGame);
        _timer.StartCountdown();

#if UNITY_EDITOR
        RefreshEditorInfiniteTime();
#endif
        ApplyGameplayVisuals(levelData);
    }

    private int CountProgressContainers()
    {
        if (levelObjectSpawner?.ContainerPooler?.ActiveItems == null)
        {
            return 0;
        }

        int total = 0;
        foreach (Container container in levelObjectSpawner.ContainerPooler.ActiveItems)
        {
            ContainerData containerData = container?.Data?.containerData;
            if (containerData == null || containerData.isStone)
            {
                continue;
            }

            if (containerData.containerMaterialType == ContainerMaterialType.Ice &&
                containerData.containerIceData?.innerContainerData != null)
            {
                containerData = containerData.containerIceData.innerContainerData;
            }

            var colorData = containerData.containerColorData;
            total += colorData != null && colorData.isLayerBox && colorData.colors != null
                ? Mathf.Max(1, colorData.colors.Count)
                : 1;
        }
        return total;
    }

    private void UpdateGameplayProgress()
    {
        PopupGameplay popup = PopupManager.Instance?.popupGameplayReference?.instance;
        if (popup == null)
        {
            return;
        }

        popup.SetContainerProgress(
            _levelTracking.resolvedContainer.Value,
            _totalProgressContainers);
    }

#if UNITY_EDITOR
    public void RefreshEditorInfiniteTime()
    {
        if (_timer == null)
        {
            return;
        }

        if (EditorDebugSettings.InfiniteTime)
        {
            _timer.Pause();
        }
        else
        {
            _timer.Resume();
        }
    }
#endif

    private void ApplyGameplayVisuals(LevelData levelData)
    {
        if (levelData == null || levelData.gridPositions == null || levelData.gridPositions.Count == 0)
        {
            return;
        }

        if (gameplayVisualConfig == null)
        {
            gameplayVisualConfig = Resources.Load<GameplayVisualConfigSO>(
                "Gameplay/GameplayVisualConfig");
        }

        Camera mainCamera = ResolveGameplayCamera();
        EnableAuthoritativeCamera(mainCamera);
        DisableNonAuthoritativeCameras(mainCamera);
        Light mainLight = FindObjectsByType<Light>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None)
            .FirstOrDefault(light => light.type == LightType.Directional);
        Volume globalVolume = FindObjectsByType<Volume>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None)
            .FirstOrDefault(volume => volume.isGlobal);

        if (gameplayVisualConfig == null)
        {
            Debug.LogError("GameplayVisualConfig could not be loaded from Resources/Gameplay.");
            return;
        }

        gameplayVisualConfig.ApplyRenderRig(mainCamera, mainLight, globalVolume);
        gameplayVisualConfig.ApplyBoardGrid(levelObjectSpawner);
        Bounds visualBounds = CalculateVisualBounds(levelData, out float cellSize);
        gameplayVisualConfig.AlignKitchenBackground(visualBounds, cellSize);
        gameplayVisualConfig.FrameCamera(mainCamera, visualBounds, cellSize);
        _lastVisualAspect = mainCamera != null ? mainCamera.aspect : -1f;
    }

    private void RefreshVisualsWhenAspectChanges()
    {
        if (_levelData == null || Camera.main == null)
        {
            return;
        }

        if (Mathf.Abs(Camera.main.aspect - _lastVisualAspect) > 0.001f)
        {
            ApplyGameplayVisuals(_levelData);
        }
    }

    private Bounds CalculateVisualBounds(LevelData levelData, out float cellSize)
    {
        Vector3 firstCell = levelObjectSpawner.Grid.CellToWorld(
            levelData.gridPositions[0].ToVector2Int());
        Vector3 adjacentX = levelObjectSpawner.Grid.CellToWorld(
            levelData.gridPositions[0].ToVector2Int() + Vector2Int.right);
        Vector3 adjacentZ = levelObjectSpawner.Grid.CellToWorld(
            levelData.gridPositions[0].ToVector2Int() + Vector2Int.up);
        float cellSizeX = Mathf.Max(0.01f, Vector3.Distance(firstCell, adjacentX));
        float cellSizeZ = Mathf.Max(0.01f, Vector3.Distance(firstCell, adjacentZ));
        cellSize = Mathf.Max(cellSizeX, cellSizeZ);

        Bounds gridBounds = new(firstCell, new Vector3(cellSizeX, 0f, cellSizeZ));
        foreach (SerializableVector2Int position in levelData.gridPositions)
        {
            Vector3 worldPosition = levelObjectSpawner.Grid.CellToWorld(position.ToVector2Int());
            gridBounds.Encapsulate(worldPosition - new Vector3(cellSizeX * 0.5f, 0f, cellSizeZ * 0.5f));
            gridBounds.Encapsulate(worldPosition + new Vector3(cellSizeX * 0.5f, 0f, cellSizeZ * 0.5f));
        }

        Bounds bounds = gridBounds;
        float powerupReserve = Mathf.Max(0f, gameplayVisualConfig.PowerupReserveCells);
        bounds.Expand(new Vector3(
            cellSizeX * powerupReserve * 2f,
            0f,
            0f));

        return bounds;
    }

    private static void DisableNonAuthoritativeCameras(Camera authoritativeCamera)
    {
        if (authoritativeCamera == null)
        {
            Debug.LogError("Gameplay camera could not be resolved. Keeping existing cameras enabled.");
            return;
        }

        foreach (Camera camera in FindObjectsByType<Camera>(
                     FindObjectsInactive.Include,
                     FindObjectsSortMode.None))
        {
            if (camera != authoritativeCamera)
            {
                camera.enabled = false;
            }
        }
    }

    private static void EnableAuthoritativeCamera(Camera authoritativeCamera)
    {
        if (authoritativeCamera == null)
        {
            return;
        }

        authoritativeCamera.gameObject.SetActive(true);
        authoritativeCamera.enabled = true;
        authoritativeCamera.targetTexture = null;
        authoritativeCamera.targetDisplay = 0;
        authoritativeCamera.rect = new Rect(0f, 0f, 1f, 1f);
    }

    private Camera ResolveGameplayCamera()
    {
        Camera sceneCamera = FindObjectsByType<Camera>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None)
            .FirstOrDefault(camera =>
                camera.gameObject.scene == gameObject.scene &&
                camera.CompareTag("MainCamera"));

        return sceneCamera != null ? sceneCamera : Camera.main;
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
