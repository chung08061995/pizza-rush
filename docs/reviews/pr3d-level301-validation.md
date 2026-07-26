# PR3D Level 301 validation

Date: 2026-07-26

Unity: `6000.4.10f1`

Scope: Level 301 vertical slice only

## Result

Level 301 passed the runtime replay and visual acceptance checks after repairing
malformed prefab override YAML introduced during PR3D-010. The repair only
split `value:` and `objectReference:` back into Unity's canonical two-line
format. The referenced GUIDs/file IDs did not change.

- The follow-up concept polish rotates the two derived gate meshes into the
  cross-rail frame without changing their transforms or BoxColliders. All seven
  runtime gates now measure either `1.104 × 0.490 × 0.284 m` or the rotated
  `0.284 × 0.490 × 1.104 m`, matching the direction of their production line.
- The shared derived pizza footprint is 78% of its previous X/Z size. The two
  food shaders now use a low-cost stylized normal ramp, preserving the single
  shared mesh and runtime color assignment while improving spacing and depth
  at phone scale.
- The procedural kitchen backdrop now uses a deeper navy wall and muted
  terracotta floor. This is a material/shader palette-only change: the backdrop
  still has zero Colliders and zero MonoBehaviours, and its mesh/transform are
  unchanged.
- Runtime replay: `Passed`, 108/108 drag actions, 27 feed actions, 148 total
  productions transferred, zero remaining production, Win reached, no skill or
  booster used.
- Ice/T container: unlocked and moved at replay actions 81, 88, 94, 97, 100,
  104 and 107; both inner batches of eight productions were transferred.
- Runtime structure: 49 cells, 23 active containers, 23 visible container
  renderers, 7 production lines, 0 null places, 128 initial productions and
  0 null production skin/mesh references.
- Level data SHA-256 remained
  `7115923d4205df433c54d12e59c64ba5726db82f394bdc0001a89e64c5482faa`.
- Every MonoBehaviour and Collider YAML document in the seven repaired
  container/production/line prefabs is byte-identical to the PR3D-010 commit.

Reproducible replay inputs and output:

- `CoffeeRunMigration/Solutions/0301.json`
- `CoffeeRunMigration/Reports/runtime-replay/0301.json`

## Portrait and UI checks

The Game view used exact fixed resolutions, not Free Aspect:

- `Assets/_Projects/Art/PR3D/Evidence/Phase5/PR3D_Level301_1080x1920.png`
- `Assets/_Projects/Art/PR3D/Evidence/Phase5/PR3D_Level301_1080x2340.png`
- `Assets/_Projects/Art/PR3D/Evidence/Phase5/PR3D_Level301_768x1024.png`

The concept-polish comparison set was captured from a fresh play session after
the shader import completed:

- `Assets/_Projects/Art/PR3D/Evidence/Polish/PR3D_Level301_Polish_1080x1920.png`
- `Assets/_Projects/Art/PR3D/Evidence/Polish/PR3D_Level301_Polish_1080x2340.png`
- `Assets/_Projects/Art/PR3D/Evidence/Polish/PR3D_Level301_Polish_768x1024.png`

The board, rails, gates, HUD, skill buttons and reserved ad region remain inside
all three frames. The Home START and Select Booster PLAY flow was exercised.
The editor stub had no live banner creative; the reserved region remained
present and did not overlap the puzzle. The timeout popup and Play On/Give Up
actions were also observed.

## Performance

The final sample was taken after a clean Unity restart at 1080×1920:

- allocated memory: 574.44 MB versus 514.74 MB baseline, +11.6%;
- memory gate: pass (limit +30%);
- GPU A/B proxy in the same scene and session:
  - additive PR3D board/environment disabled: median 8.525 ms (10 samples);
  - additive PR3D board/environment enabled: median 8.820 ms (10 samples);
  - delta: +3.5%;
- GPU gate: pass (limit +20%).

The original PR3D-001 capture did not expose a valid GPU counter, so the
same-session additive-root A/B is the comparable GPU gate. The clean full-art
sample reported 7.815 ms GPU and 2.188 ms CPU main-thread time.

The latest follow-up sample in the Unity Editor reported 14.129 ms total CPU
frame time, 2.552 ms on the main thread and 8.772 ms GPU time. Gfx used memory
was 492,903,356 bytes and mesh memory was 8,041,108 bytes. This remains an
Editor-only sample; a device build is required before any rollout decision.

## Console

No PR3D or gameplay errors were present after the clean run. Unity repeated one
instance of the known immutable-package diagnostic:

`Packages/com.draft.unitydraftutils/Runtime/MonoBehaviour/MonoBehaviourLifecycleCallbacks.cs has no meta file`

This package-owned message predates the slice and cannot be repaired from the
project worktree.
