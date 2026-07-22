# 001 — Pizza 3D vertical slice

Status: Ready
Issue: Forge Plan Sync / Pizza Rush 3D reskin
Branch/worktree: TBD — use a dedicated art branch before prefab promotion.

## Objective

Create a polished pizza-factory 3D visual slice for Level 301 while preserving all gameplay and serialized contracts.

## Scope

Level 301 only for acceptance. The slice covers board, rails, connectors, color gates, pizza/topping variants, four container shapes, Ice, and reusable kitchen props. It does not change level JSON or gameplay code.

## Tasks

- [ ] [PR3D-001] Capture baseline Level 301
  Record Game view, hierarchy, camera, lights, materials, prefab roots, profiler frame time, memory, and a complete playthrough baseline. Concept reference: `docs/reference/pizza-factory-concept.png`; inspect it with `view_image` using `detail: "high"` or `"original"` only.

- [ ] [PR3D-002] Build Blender master scene
  Configure meters/Y-up/forward-Z, portrait camera, collections, naming, export settings, and concept reference at `docs/reference/pizza-factory-concept.png`. If inspecting the image, use `view_image` with `detail: "high"` or `"original"`; never use `"low"`.

- [ ] [PR3D-003] Model board and tile kit
  Build the 7×7 tray/frame and tile visual while preserving cell size, pivots, and grid positions. Use `docs/reference/pizza-factory-concept.png` as the visual reference and inspect it only with `view_image` detail `high` or `original`.

- [ ] [PR3D-004] Model procedural rail kit
  Build straight rail, mirrored 90-degree curve, supports, arrows, and board connectors aligned to existing production places. Refer to `docs/reference/pizza-factory-concept.png`; `view_image` detail must be `high` or `original`, never `low`.

- [ ] [PR3D-005] Model color gate system
  Build one gate mesh with ten shared material/emission variants without changing entry/exit transforms. Refer to `docs/reference/pizza-factory-concept.png` using `view_image` detail `high` or `original` only.

- [ ] [PR3D-006] Model pizza and topping variants
  Build one triangular pizza mesh with ten readable topping/color variants as materials, not duplicate meshes. Refer to `docs/reference/pizza-factory-concept.png`; use `view_image` detail `high` or `original` only.

- [ ] [PR3D-007] Reskin Level 301 containers
  Replace visuals for 1×1, 1×2, 1×3, T, and Ice while preserving roots, colliders, places, and component references.

- [ ] [PR3D-008] Build pizza-kitchen environment
  Create the oven and reusable wall/floor/counter, shelf, lamp, jar/bowl, basil, utensil, and ingredient-crate families. Concept: `docs/reference/pizza-factory-concept.png`; use `view_image` detail `high` or `original`, never `low`.

- [ ] [PR3D-009] Optimize and export assets
  Apply transforms, cleanup topology, UV/atlas, LOD, shared materials, FBX/GLB export, and `PR3D_manifest.json`.

- [ ] [PR3D-010] Integrate prefab visuals in Unity
  Add or swap visual children/materials only; do not change scripts, enums, JSON, colliders, or serialized gameplay roots.

- [ ] [PR3D-011] Validate Level 301
  Play the level through, test drag/Ice/pizza transfer/ray directions/gates, check three portrait ratios, console, frame time, memory, HUD, boosters, and ad area.

- [ ] [PR3D-012] Review the vertical slice and decide rollout
  Compare before/after evidence, list fixes, and create a separate plan for the remaining 319 levels only after approval.

## Acceptance criteria

- [ ] Level 301 still has 49 grid cells, 23 containers, 7 production lines, four shapes, Ice, and ten colors.
- [ ] Existing drag, production transfer, timer, win/lose, booster, and popup behavior is unchanged.
- [ ] Rails are continuous and gates align with current board/line anchors.
- [ ] Pizza color/topping changes are readable at phone scale without obscuring the puzzle.
- [ ] No more than +20% GPU frame time or +30% memory versus baseline on the same device.
- [ ] No critical Unity Console errors and no visual overflow at three portrait ratios.

## Verification

- Run `MyMenu > StartGame` in Unity.
- Load Level 301 and complete a playthrough.
- Capture Game view and Blender viewport screenshots.
- Inspect Unity Console and profiler.
- Confirm `PR3D_manifest.json` matches imported assets.

## Files/docs to update

- `docs/changelog.md`
- `docs/technical/3d-art-pipeline.md`
- `docs/decisions/` when a new consequential pipeline decision is made.
- This plan: check off only verified tasks and record handoff evidence.

## Handoff notes

The repository is the source of truth. Forge imports this plan by task ID; do not create duplicate task IDs in another plan file.
