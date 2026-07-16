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
        private static readonly BindingFlags InstancePrivate = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly FieldInfo DragStateField =
            typeof(GameplayStateMachine).GetField("dragContainerState", InstancePrivate);
        private static readonly MethodInfo BeginDragMethod =
            typeof(DragContainerState).GetMethod("BeginDrag", InstancePrivate);
        private static readonly MethodInfo EndDragMethod =
            typeof(DragContainerState).GetMethod("EndDrag", InstancePrivate);

        private static ReplayPhase _phase;
        private static Solution _solution;
        private static readonly Queue<int> PendingLevels = new();
        private static int _actionIndex;
        private static double _nextActionAt;
        private static string _lastMessage;

        public static bool IsRunning => _phase != ReplayPhase.Idle;
        public static string LastMessage => _lastMessage;

        [MenuItem("MyMenu/Coffee Run/Replay Level 46 runtime solution")]
        private static void ReplayStoneRegression() => Start(46);

        public static void Start(int level)
        {
            Cancel();
            BeginLevel(level);
        }

        public static void StartRange(int firstLevel, int lastLevel)
        {
            Cancel();
            if (firstLevel < 1 || lastLevel < firstLevel || lastLevel > 100)
            {
                Fail(0, 0, $"Invalid replay range {firstLevel}–{lastLevel}.");
                return;
            }
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
            SceneControllerExtensions.LoadGameplay();

            _phase = ReplayPhase.WaitingForLevel;
            _actionIndex = 0;
            _nextActionAt = EditorApplication.timeSinceStartup + 0.5d;
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
            PendingLevels.Clear();
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
                _nextActionAt = EditorApplication.timeSinceStartup + 0.25d;
                return;
            }

            runner.Timer.SetRemaining(100000f);
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

            _phase = ReplayPhase.Replaying;
            _nextActionAt = EditorApplication.timeSinceStartup + 0.25d;
            _lastMessage = $"Replaying {_solution.actions.Count} actions for Level {_solution.level:0000}.";
            Debug.Log($"[CoffeeRunRuntimeReplayer] {_lastMessage}");
        }

        private static void ReplayNextAction()
        {
            var runner = RequireRunner();
            if (_actionIndex >= _solution.actions.Count)
            {
                _phase = ReplayPhase.WaitingForWin;
                _nextActionAt = EditorApplication.timeSinceStartup + 2.5d;
                return;
            }

            if (!AllContainerMotionSettled(runner))
            {
                _nextActionAt = EditorApplication.timeSinceStartup + 0.05d;
                return;
            }

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
                _nextActionAt = EditorApplication.timeSinceStartup + 0.1d;
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
            _nextActionAt = EditorApplication.timeSinceStartup + (consumed > 0 ? 1.8d : 0.05d);
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
