# Pizza Rush 3D — Level 301 vertical-slice review

Date: 2026-07-24
Scope: PR3D-002 through PR3D-012
Decision: **Level 301 integration preview applied; rollout held pending explicit approval**

## Outcome

The Level 301 pizza-factory visual slice is complete as an additive review
package. Blender sources, exports, Unity models, shared materials, 44 visual
prefabs, three portrait captures, and validation evidence are present. No
shared container, production-line, production-pizza, and LevelRunner prefabs as
visual children only. Scripts, enums, level JSON, gameplay colliders, places,
and serialized gameplay roots remain unchanged.

The Unity visual library contains:

- board and ceramic tile prefabs;
- five container families including Ice;
- straight/curved rails, connector, and support;
- ten gate material variants on one shared gate mesh;
- ten pizza topping/color variants on one shared pizza mesh;
- eleven reusable kitchen environment modules;
- one Level 301 portrait environment composition.

Prefab audit: 44 prefabs, zero colliders, zero MonoBehaviours, zero missing
components, and one shared Unity mesh across all ten pizza variants.

## Runtime validation

Unity `6000.4.10f1` compiled cleanly on Android after switching away from the
Standalone target whose project defines excluded Google Mobile Ads types.
Neither Ads nor gameplay code was modified.

`MyMenu > StartGame` loaded Level 301 with the expected 49 cells, 23 containers,
seven production lines, four shape families, ten colors, and one T-shaped Ice
container. A generated no-skill solution was used only as temporary test input
and was not added to the repository.

The optimized PR3D composition was attached before the first action and the
level completed with:

- 293 drag actions;
- 26 production feeds;
- zero skill or booster use;
- production remaining `0`;
- Ice reduced from `12` to `0`;
- normal post-win scene transition;
- Unity Console: zero errors and zero warnings.

HUD, booster buttons, and ad/continue popup area remained usable. The pre-existing
side `Button`/`Next` debug controls are visible in Game View and were not changed
by this art task.

## Portrait evidence

- `Assets/_Projects/Art/PR3D/Evidence/PR3D_Level301_Integrated_9x16_final_v2.png`
  — 1080×1920.
- `Assets/_Projects/Art/PR3D/Evidence/PR3D_Level301_Integrated_9x18.png`
  — 1080×2160.
- `Assets/_Projects/Art/PR3D/Evidence/PR3D_Level301_Integrated_1440x2960.png`
  — 1440×2960.

The environment wall and terracotta-floor modules initially obscured the
puzzle/HUD in portrait. They remain available as standalone family prefabs but
are disabled in the Level 301 composition. The final three captures have no
PR3D safe-area overflow.

## Same-session A/B

The original baseline was captured in a different Unity instance, so the final
budget decision uses a controlled A/B in the same worktree editor session.

| Metric | Legacy Level 301 | PR3D composition | Delta |
|---|---:|---:|---:|
| GPU frame-time median (7 samples) | 7.611 ms | 6.615 ms | -13.1% |
| CPU frame-time median (7 samples) | 16.888 ms | 17.208 ms | +1.9% |
| Allocated memory | 697,135,782 B | 718,189,900 B | +3.0% |
| Reserved memory | 1,577,631,744 B | 1,577,631,744 B | 0% |

Container tinting uses `MaterialPropertyBlock`. The rejected intermediate
approach used `Renderer.materials`, created per-renderer material instances, and
exceeded the GPU budget; it is not part of the final integration guidance.

## Rollout decision

The Level 301 preview is bound into shared gameplay prefabs for stakeholder
review. Do not roll it out to the remaining 319 levels yet. After explicit
approval, create a separate rollout plan covering level-scoped binding, device
profiling, content batching, and rollback.
