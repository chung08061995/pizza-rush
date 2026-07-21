#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace CoffeeRunMigration
{
    /// <summary>
    /// Replays a generated no-skill plan through DragContainerState in Play Mode.
    /// This is editor-only evidence tooling; the shipped runtime never reads the
    /// source records or solver output.
    /// </summary>
    public static class CoffeeRunRuntimeReplayer
    {
        [Serializable]
        private sealed class Solution
        {
            public int level;
            public List<SolutionAction> actions = new();
        }

        [Serializable]
        private sealed class SolutionAction
        {
            public int container;
            public SerializableVector2Int anchor;
            public int fedLine = -1;
            public int fedAmount;
        }

        [Serializable]
        private sealed class ReplayEvidence
        {
            public int level;
            public string result;
            public int completedActions;
            public int dragCount;
            public int remainingProduction;
            public bool usedSkillOrBooster;
            public string utc;
            public string message;
            public string startCapture;
            public List<MovementAuditEntry> movementAudit = new();
        }

        [Serializable]
        private sealed class MovementAuditEntry
        {
            public int container;
            public string movement;
            public bool beginDragSucceeded;
            public bool hasAlternativeCell;
            public int reachableCellCount;
            public string result;
            public string message;
        }

        private enum ReplayPhase
        {
            Idle,
            WaitingForLevel,
            Replaying,
            WaitingForWin,
        }

        private const string SolutionDirectory = "CoffeeRunMigration/Solutions";
        private const string EvidenceDirectory = "CoffeeRunMigration/Reports/runtime-replay";
        private const float ReplayTimeScale = 32f;
        private const double LevelLoadDelay = 0.2d;
        private const double SettlePollDelay = 0.01d;
        private const double FeedDelay = 0.04d;
        private const double WinDelay = 0.2d;
        private const double MotionTimeout = 5d;
        private const double VisualStartHold = 5d;
        private const string StartCaptureDirectory = "CoffeeRunMigration/Captures/PizzaRush";
        private static readonly BindingFlags InstancePrivate = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly FieldInfo DragStateField =
            typeof(GameplayStateMachine).GetField("dragContainerState", InstancePrivate);
        private static readonly MethodInfo BeginDragMethod =
            typeof(DragContainerState).GetMethod("BeginDrag", InstancePrivate);
        private static readonly MethodInfo EndDragMethod =
            typeof(DragContainerState).GetMethod("EndDrag", InstancePrivate);
        private static readonly MethodInfo GetConnectedCellsMethod =
            typeof(DragContainerState).GetMethod("GetConnectedValidBaseCells", InstancePrivate);

        private static ReplayPhase _phase;
        private static Solution _solution;
        private static readonly Queue<int> PendingLevels = new();
        private static int _actionIndex;
        private static double _nextActionAt;
        private static string _lastMessage;
        private static double _motionWaitStartedAt;
        private static List<MovementAuditEntry> _movementAudit = new();
        private static float _previousTimeScale = 1f;
        private static bool _ownsTimeScale;
        private static bool _visualAuditMode;
        private static bool _visualHoldActive;
        private static bool _captureOnlyMode;

        public static bool IsRunning => _phase != ReplayPhase.Idle;
        public static string LastMessage => _lastMessage;

        [MenuItem("MyMenu/Coffee Run/Replay Level 46 runtime solution")]
        private static void ReplayStoneRegression() => Start(46);

        public static void Start(int level)
        {
            Cancel();
            _visualAuditMode = false;
            EnableFastReplay();
            BeginLevel(level);
        }

        public static void StartRange(int firstLevel, int lastLevel)
        {
            StartRangeInternal(firstLevel, lastLevel, false, false);
        }

        /// <summary>
        /// Run a human-reviewable range. Each level is captured and held at its
        /// start state before any replay input is sent.
        /// </summary>
        public static void StartVisualAuditRange(int firstLevel, int lastLevel)
        {
            StartRangeInternal(firstLevel, lastLevel, true, false);
        }

        /// <summary>Refresh start screenshots using the source timer without replaying actions.</summary>
        public static void StartVisualCaptureRange(int firstLevel, int lastLevel)
        {
            StartRangeInternal(firstLevel, lastLevel, true, true);
        }

        private static void StartRangeInternal(int firstLevel, int lastLevel, bool visualAudit, bool captureOnly)
        {
            Cancel();
            if (firstLevel < 1 || lastLevel < firstLevel || lastLevel > 100)
            {
                Fail(0, 0, $"Invalid replay range {firstLevel}–{lastLevel}.");
                return;
            }
            _visualAuditMode = visualAudit;
            _captureOnlyMode = captureOnly;
            EnableFastReplay();
            for (var level = firstLevel + 1; level <= lastLevel; level++)
            {
                PendingLevels.Enqueue(level);
            }
            BeginLevel(firstLevel);
        }

        private static void BeginLevel(int level)
        {
            if (!EditorApplication.isPlaying)
            {
                Fail(level, 0, "Enter Play Mode through MyMenu > StartGame before replaying.");
                return;
            }

            var path = $"{SolutionDirectory}/{level:0000}.json";
            if (!File.Exists(path))
            {
                Fail(level, 0, $"Missing solution: {path}");
                return;
            }

            _solution = JsonConvert.DeserializeObject<Solution>(File.ReadAllText(path));
            if (_solution == null || _solution.level != level || _solution.actions == null)
            {
                Fail(level, 0, $"Invalid solution payload: {path}");
                return;
            }
            if (DataManager.Instance == null)
            {
                Fail(level, 0, "DataManager is not initialized.");
                return;
            }

            DataManager.Instance.Level.SetValue(level);
            DataManager.Instance.Level.Notifier.Notify();
            if (RuntimeStorage.Instance != null)
            {
                RuntimeStorage.Instance.Set(
                    GameConstain.RuntimeStorage.StartBooterItems,
                    new List<ItemType>());
            }
            PopupManager.Instance?.HideAllPopupInGameplay();
            SceneControllerExtensions.LoadGameplay();

            _phase = ReplayPhase.WaitingForLevel;
            _actionIndex = 0;
            _nextActionAt = EditorApplication.timeSinceStartup + LevelLoadDelay;
            _lastMessage = $"Waiting for Pizza Rush Level {level:0000}.";
            EditorApplication.update += Tick;
            Debug.Log($"[CoffeeRunRuntimeReplayer] {_lastMessage}");
        }

        public static void Cancel()
        {
            EditorApplication.update -= Tick;
            _phase = ReplayPhase.Idle;
            _solution = null;
            _actionIndex = 0;
            _movementAudit.Clear();
            PendingLevels.Clear();
            _visualHoldActive = false;
            _captureOnlyMode = false;
            if (_ownsTimeScale)
            {
                Time.timeScale = _previousTimeScale;
                _ownsTimeScale = false;
            }
        }

        private static void EnableFastReplay()
        {
            if (!EditorApplication.isPlaying || _ownsTimeScale)
            {
                return;
            }

            _previousTimeScale = Time.timeScale;
            Time.timeScale = _visualAuditMode ? 1f : ReplayTimeScale;
            _ownsTimeScale = true;
        }

        private static void Tick()
        {
            if (!EditorApplication.isPlaying)
            {
                Cancel();
                return;
            }
            if (EditorApplication.timeSinceStartup < _nextActionAt)
            {
                return;
            }

            try
            {
                if (_phase == ReplayPhase.WaitingForLevel)
                {
                    TryInitializeLevel();
                }
                else if (_phase == ReplayPhase.Replaying)
                {
                    ReplayNextAction();
                }
                else if (_phase == ReplayPhase.WaitingForWin)
                {
                    VerifyWin();
                }
            }
            catch (Exception exception)
            {
                Fail(_solution?.level ?? 0, _actionIndex, exception.ToString());
            }
        }

        private static void TryInitializeLevel()
        {
            var runner = LevelFactory.Instance != null ? LevelFactory.Instance.LevelRunner : null;
            if (runner == null || runner.LevelData == null || runner.LevelData.levelIndex != _solution.level)
            {
                _nextActionAt = EditorApplication.timeSinceStartup + SettlePollDelay;
                return;
            }

            var active = runner.LevelObjectSpawner.ContainerPooler.ActiveItems
                .Where(container => container != null && container.gameObject.activeInHierarchy)
                .ToList();
            if (active.Count != runner.LevelData.containers.Count)
            {
                throw new InvalidOperationException(
                    $"Expected {runner.LevelData.containers.Count} active containers, found {active.Count}.");
            }

            var grid = runner.LevelObjectSpawner.Grid;
            for (var index = 0; index < runner.LevelData.containers.Count; index++)
            {
                var expected = runner.LevelData.containers[index].position.ToVector2Int();
                var matches = active.Where(container => grid.WorldToCell(container.transform.position) == expected).ToList();
                if (matches.Count != 1)
                {
                    throw new InvalidOperationException(
                        $"Container {index} at {expected} matched {matches.Count} runtime objects.");
                }
                matches[0].name = RuntimeContainerName(index);
            }

            if (_visualAuditMode)
            {
                Time.timeScale = 0f;
                runner.Timer.SetRemaining(runner.Timer.Duration);
            }
            else
            {
                runner.Timer.SetRemaining(100000f);
            }

            CaptureStartState(runner);
            RunMovementAudit(runner, active);

            // Freeze the actual game clock while the start-state screenshot is
            // reviewed. EditorApplication.timeSinceStartup remains live, so
            // the audit advances after the hold without animating containers or
            // allowing a block to visually overlap another block mid-motion.
            if (_visualAuditMode)
            {
                Time.timeScale = 0f;
                _visualHoldActive = true;
            }

            _phase = ReplayPhase.Replaying;
            _motionWaitStartedAt = EditorApplication.timeSinceStartup;
            _nextActionAt = EditorApplication.timeSinceStartup +
                (_visualAuditMode ? VisualStartHold : SettlePollDelay);
            _lastMessage = $"Replaying {_solution.actions.Count} actions for Level {_solution.level:0000}.";
            Debug.Log($"[CoffeeRunRuntimeReplayer] {_lastMessage}");
        }

        private static void CaptureStartState(LevelRunner runner)
        {
            var directory = Path.Combine(StartCaptureDirectory, $"{runner.LevelData.levelIndex:0000}");
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, "visual-start.png");
            ScreenCapture.CaptureScreenshot(path);
            Debug.Log($"[CoffeeRunRuntimeReplayer] Capturing start state: {path}");
        }

        private static void RunMovementAudit(LevelRunner runner, List<Container> active)
        {
            var dragState = DragStateField?.GetValue(runner.GameplayStateMachine) as DragContainerState;
            if (dragState == null || GetConnectedCellsMethod == null)
            {
                throw new MissingMemberException("Movement audit reflection members are unavailable.");
            }

            var audit = new List<MovementAuditEntry>(active.Count);
            foreach (var container in active.OrderBy(item => item.name))
            {
                var entry = new MovementAuditEntry
                {
                    container = ParseRuntimeContainerIndex(container.name),
                    movement = container.Data.containerData.containerMovementType.ToString(),
                };
                try
                {
                    var occupied = new HashSet<Vector2Int>();
                    foreach (var other in active)
                    {
                        if (other == container)
                        {
                            continue;
                        }
                        var otherAnchor = runner.LevelObjectSpawner.Grid.WorldToCell(other.transform.position);
                        foreach (var part in other.GetPartPositions())
                        {
                            occupied.Add(otherAnchor + part);
                        }
                    }
                    var available = runner.LevelData.gridPositions
                        .Select(position => position.ToVector2Int())
                        .Where(cell => !occupied.Contains(cell))
                        .ToHashSet();
                    var connected = GetConnectedCellsMethod.Invoke(
                        dragState,
                        new object[]
                        {
                            container,
                            runner.LevelObjectSpawner.Grid,
                            container.transform.position,
                            available,
                        }) as HashSet<Vector2Int>;
                    entry.beginDragSucceeded = connected != null;
                    entry.reachableCellCount = connected?.Count ?? 0;
                    var start = runner.LevelObjectSpawner.Grid.WorldToCell(container.transform.position);
                    entry.hasAlternativeCell = connected != null && connected.Any(cell => cell != start);
                    entry.result = entry.hasAlternativeCell ? "Pass" : "NoAlternativeCell";
                }
                catch (Exception exception)
                {
                    entry.result = "Fail";
                    entry.message = exception.GetBaseException().Message;
                }
                audit.Add(entry);
            }

            _movementAudit = audit;
            var failed = audit.Where(entry => entry.result == "Fail").ToList();
            if (failed.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Movement audit failed for {failed.Count} container(s): " +
                    string.Join(", ", failed.Select(entry => entry.container)));
            }
        }

        private static int ParseRuntimeContainerIndex(string name)
        {
            return int.TryParse(name.Replace("CoffeeRunContainer_", string.Empty), out var index) ? index : -1;
        }

        private static void ReplayNextAction()
        {
            if (_visualHoldActive)
            {
                Time.timeScale = 1f;
                _visualHoldActive = false;
                if (_captureOnlyMode)
                {
                    if (PendingLevels.Count == 0)
                    {
                        Cancel();
                        return;
                    }
                    var nextLevel = PendingLevels.Dequeue();
                    EditorApplication.update -= Tick;
                    _phase = ReplayPhase.Idle;
                    _solution = null;
                    _actionIndex = 0;
                    EditorApplication.delayCall += () => BeginLevel(nextLevel);
                    return;
                }
                _nextActionAt = EditorApplication.timeSinceStartup + SettlePollDelay;
                return;
            }

            var runner = RequireRunner();
            if (_actionIndex >= _solution.actions.Count)
            {
                _phase = ReplayPhase.WaitingForWin;
                _nextActionAt = EditorApplication.timeSinceStartup + WinDelay;
                return;
            }

            if (!AllContainerMotionSettled(runner))
            {
                if (EditorApplication.timeSinceStartup - _motionWaitStartedAt > MotionTimeout)
                {
                    throw new TimeoutException(
                        $"Action {_actionIndex + 1}: container animation did not settle within {MotionTimeout:0.#} seconds.");
                }
                _nextActionAt = EditorApplication.timeSinceStartup + SettlePollDelay;
                return;
            }

            _motionWaitStartedAt = EditorApplication.timeSinceStartup;

            var action = _solution.actions[_actionIndex];
            var container = Resources.FindObjectsOfTypeAll<Container>()
                .FirstOrDefault(item =>
                    item != null && item.gameObject.activeInHierarchy &&
                    item.name == RuntimeContainerName(action.container));
            if (container == null)
            {
                throw new InvalidOperationException(
                    $"Action {_actionIndex + 1}: active container {action.container} was not found.");
            }
            if (container.isAnimating)
            {
                _nextActionAt = EditorApplication.timeSinceStartup + SettlePollDelay;
                return;
            }

            var before = RemainingProduction(runner);
            var dragState = DragStateField?.GetValue(runner.GameplayStateMachine) as DragContainerState;
            if (dragState == null || BeginDragMethod == null || EndDragMethod == null)
            {
                throw new MissingMemberException("DragContainerState replay reflection members are unavailable.");
            }

            var progress = new ProgressDragContainerData();
            BeginDragMethod.Invoke(dragState, new object[] { progress, container });
            var target = action.anchor.ToVector2Int();
            if (!progress.cachedConnectedCells.Contains(target))
            {
                throw new InvalidOperationException(
                    $"Action {_actionIndex + 1}: target {target} is not runtime-reachable for container {action.container}.");
            }
            progress.targetMoverPos = runner.LevelObjectSpawner.Grid.CellToWorld(target);
            EndDragMethod.Invoke(dragState, new object[] { progress, runner });

            var after = RemainingProduction(runner);
            var consumed = before - after;
            if (consumed != action.fedAmount)
            {
                throw new InvalidOperationException(
                    $"Action {_actionIndex + 1}: expected feed {action.fedAmount}, runtime consumed {consumed}.");
            }

            _actionIndex++;
            _lastMessage = $"Level {_solution.level:0000}: action {_actionIndex}/{_solution.actions.Count}, feed={consumed}.";
            Debug.Log($"[CoffeeRunRuntimeReplayer] {_lastMessage}");
            _nextActionAt = EditorApplication.timeSinceStartup + (consumed > 0 ? FeedDelay : SettlePollDelay);
        }

        private static bool AllContainerMotionSettled(LevelRunner runner)
        {
            foreach (var container in runner.LevelObjectSpawner.ContainerPooler.ActiveItems)
            {
                if (container == null || !container.gameObject.activeInHierarchy)
                {
                    continue;
                }
                if (container.isAnimating ||
                    !container.StateMachine.MoveToPositionState.SmoothMover.IsImmediateSnapDistance())
                {
                    return false;
                }
            }
            return true;
        }

        private static void VerifyWin()
        {
            var runner = RequireRunner();
            var remaining = RemainingProduction(runner);
            if (remaining != 0)
            {
                throw new InvalidOperationException($"Plan ended with {remaining} productions remaining.");
            }

            var message = $"Level {_solution.level:0000} runtime replay passed without skill or booster.";
            WriteEvidence(new ReplayEvidence
            {
                level = _solution.level,
                result = "Passed",
                completedActions = _actionIndex,
                dragCount = runner.LevelTracking.dragContainerTimes.Value,
                remainingProduction = remaining,
                usedSkillOrBooster = false,
                utc = DateTime.UtcNow.ToString("O"),
                message = message,
                startCapture = $"{StartCaptureDirectory}/{_solution.level:0000}/visual-start.png",
                movementAudit = _movementAudit,
            });
            _lastMessage = message;
            Debug.Log($"[CoffeeRunRuntimeReplayer] {message}");
            if (PendingLevels.Count == 0)
            {
                Cancel();
                return;
            }

            var nextLevel = PendingLevels.Dequeue();
            EditorApplication.update -= Tick;
            _phase = ReplayPhase.Idle;
            _solution = null;
            _actionIndex = 0;
            EditorApplication.delayCall += () => BeginLevel(nextLevel);
        }

        private static LevelRunner RequireRunner()
        {
            var runner = LevelFactory.Instance != null ? LevelFactory.Instance.LevelRunner : null;
            if (runner == null || runner.LevelData == null || runner.LevelData.levelIndex != _solution.level)
            {
                throw new InvalidOperationException("The expected LevelRunner is no longer active.");
            }
            return runner;
        }

        private static int RemainingProduction(LevelRunner runner) =>
            runner.LevelObjectSpawner.ProductionLinePooler.ActiveItems
                .Where(line => line != null)
                .Sum(line => line.Data?.productionColors?.Count ?? 0);

        private static string RuntimeContainerName(int index) => $"CoffeeRunContainer_{index:000}";

        private static void Fail(int level, int completedActions, string message)
        {
            _lastMessage = message;
            Debug.LogError($"[CoffeeRunRuntimeReplayer] {message}");
            if (level > 0)
            {
                var remaining = 0;
                var runner = LevelFactory.Instance != null ? LevelFactory.Instance.LevelRunner : null;
                if (runner != null)
                {
                    remaining = RemainingProduction(runner);
                }
                WriteEvidence(new ReplayEvidence
                {
                    level = level,
                    result = "Failed",
                    completedActions = completedActions,
                    dragCount = runner != null ? runner.LevelTracking.dragContainerTimes.Value : 0,
                    remainingProduction = remaining,
                    usedSkillOrBooster = false,
                    utc = DateTime.UtcNow.ToString("O"),
                    message = message,
                    startCapture = $"{StartCaptureDirectory}/{level:0000}/visual-start.png",
                    movementAudit = _movementAudit,
                });
            }
            Cancel();
        }

        private static void WriteEvidence(ReplayEvidence evidence)
        {
            Directory.CreateDirectory(EvidenceDirectory);
            File.WriteAllText(
                $"{EvidenceDirectory}/{evidence.level:0000}.json",
                JsonConvert.SerializeObject(evidence, Formatting.Indented) + Environment.NewLine);
        }
    }
}
#endif
