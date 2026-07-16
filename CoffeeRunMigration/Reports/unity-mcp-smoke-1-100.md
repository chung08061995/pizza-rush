# Unity MCP runtime smoke report

- Range: Level 0001–0100
- Entry flow: `MyMenu > StartGame`, then runtime level selection through `DataManager`/`LevelFactory`
- Capture: 100/100 Game View images at 1080×1920 under ignored `CoffeeRunMigration/Captures/PizzaRush/NNNN/visual-start.png`
- Runtime load: 100/100 levels returned the expected `levelIndex`, timer, container list and production-line list
- Normalized validation: `Exact=100`, `Mismatch=0`, `Unsupported=0`
- Runtime integrity audit: 100/100 exact
- Migration self-tests: passed, including LayerBox (31), stone (46), multi-color quota (61), strict mismatch and unsupported gates

Visual QA found and fixed production-line path chirality. Curved paths now map to deterministic `CurvedLeft` or `CurvedRight`; Level 2's bottom line was re-captured bending right like the Coffee Run source.

The Unity Console contained no gameplay exception during the sweep. Two existing package-import errors remained for the missing immutable-package meta file `Packages/com.draft.unitydraftutils/Runtime/MonoBehaviour/MonoBehaviourLifecycleCallbacks.cs.meta`.

This smoke run proves that every target JSON can instantiate and frame in portrait. It does not mark source screenshot overlay, no-skill solve, or approval as complete.

## Level 1 gameplay result

Level 1 was subsequently completed through the normal drag-state path without a skill or booster. Two legal drags moved the matching 1×3 containers to the top and bottom intake cells; both production lines emptied, both containers flew away, `WinState` advanced the player to Level 2, and the Win popup was captured locally as `0001/win-no-skill.png`. The previously captured 180-second timeout reached the Time Up flow. Visual overlay remains pending, so Level 1 is not yet approved.

## Level 2–6 gameplay results

The grid solver in `CoffeeRunMigration/Tools/solve_levels.py` models the same reachable-anchor BFS used by `DragContainerState`. Each generated action was replayed through the runtime `BeginDrag`/`EndDrag` path; before every drop Unity confirmed the target anchor was present in `ProgressDragContainerData.cachedConnectedCells`. No skill or booster state was invoked.

| Level | Legal drags | Remaining time | Result |
|---:|---:|---:|---|
| 002 | 2 | not recorded | production lines empty; advanced to Level 3 |
| 003 | 6 | 137 s | production lines empty; advanced to Level 4 |
| 004 | 9 | 152 s | production lines empty; advanced to Level 5 |
| 005 | 9 | 152 s | production lines empty; advanced to Level 6 |
| 006 | 3 | 172 s | production lines empty; advanced to Level 7 |

Start-state visual overlay remains pending, so these levels are not yet approved.

## Level 21 Ice regression

The first Ice level exposed a missing runtime payload: converted frozen containers had an `iceAmount` but a null `innerContainerData`. The converter now emits a complete non-Ice inner container, the validator rejects a missing or mismatched payload, color shuffle recurses into that payload, and replacement iterates over a stable container snapshot while preserving runtime identity.

Level 21 was replayed after a clean `MyMenu > StartGame` boot. Both frozen containers thawed at resolved-container counts 2 and 3, retained the shuffled color relationship, and the level completed through ten legal drag-state actions without a skill or booster. All production queues emptied, `WinState` advanced to Level 22, and 0.65 seconds remained on the source 60-second timer.

## Runtime replay automation and Level 7–20

`CoffeeRunRuntimeReplayer` now loads a generated solution in editor Play Mode,
names runtime containers by their source index, invokes the normal
`DragContainerState.BeginDrag`/`EndDrag` path, requires every target anchor to
be present in the runtime BFS result, and compares the production count consumed
by every action with the solver plan. It writes one evidence JSON per level and
never invokes a skill or booster.

On a clean `MyMenu > StartGame` session with the full 14-color palette loaded,
Levels 7–20 all passed. Every plan ended with zero production remaining; the
individual results are under `CoffeeRunMigration/Reports/runtime-replay/`.
Level 46 Stone, Level 31 LayerBox and Level 61 multi-color regressions also
passed through the same runner.

## Runtime no-skill completion: Level 1–100

All 100 Pizza Rush target levels now have a successful no-skill/no-booster
runtime completion. Levels 1–6 and 21 retain their earlier manual runtime
evidence; Levels 7–20 and 22–100 have machine-readable evidence under
`CoffeeRunMigration/Reports/runtime-replay/`. Every automated replay required
each requested anchor to be accepted by the live runtime BFS, matched the
expected production consumption per action, and ended with zero production
remaining. Stone containers are treated strictly as movable blockers rather
than production capacity; the Stone-safe plans, including Level 88, passed in
the live Unity runtime.

This completes the target `Solved` gate for Level 1–100. Visual screenshot
overlays are still pending, so no level is promoted to `Approved` by this run.
