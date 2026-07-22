# 001 — Pizza 3D vertical slice

Status: Review blocked — rollout not approved
Issue: Forge Plan Sync / Pizza Rush 3D reskin
Branch/worktree: TBD — use a dedicated art branch before prefab promotion.

## Objective

Create a polished pizza-factory 3D visual slice for Level 301 while preserving all gameplay and serialized contracts.

## Scope

Level 301 only for acceptance. The slice covers board, rails, connectors, color gates, pizza/topping variants, four container shapes, Ice, and reusable kitchen props. It does not change level JSON or gameplay code.

## Tasks

- [ ] [PR3D-001] Capture baseline Level 301
  Record Game view, hierarchy, camera, lights, materials, prefab roots, profiler frame time, memory, and a complete playthrough baseline.

- [ ] [PR3D-002] Build Blender master scene
  Configure meters/Y-up/forward-Z, portrait camera, collections, naming, export settings, and concept reference.

- [ ] [PR3D-003] Model board and tile kit
  Build the 7×7 tray/frame and tile visual while preserving cell size, pivots, and grid positions.

- [ ] [PR3D-004] Model procedural rail kit
  Build straight rail, mirrored 90-degree curve, supports, arrows, and board connectors aligned to existing production places.

- [ ] [PR3D-005] Model color gate system
  Build one gate mesh with ten shared material/emission variants without changing entry/exit transforms.

- [ ] [PR3D-006] Model pizza and topping variants
  Build one triangular pizza mesh with ten readable topping/color variants as materials, not duplicate meshes.

- [ ] [PR3D-007] Reskin Level 301 containers
  Replace visuals for 1×1, 1×2, 1×3, T, and Ice while preserving roots, colliders, places, and component references.

- [ ] [PR3D-008] Build pizza-kitchen environment
  Create the oven and reusable wall/floor/counter, shelf, lamp, jar/bowl, basil, utensil, and ingredient-crate families.

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

## PR3D-012 review record (2026-07-22)

### Evidence reviewed

- Level 301 source data is present at `Assets/_Projects/Resources/LevelData/0301.json` and contains 49 grid positions, 23 containers, 7 production lines, and a 90-second duration. This verifies the source contract only; it is not runtime validation.
- No Level 301 Game view or Blender viewport before/after captures are present in the repository.
- No Unity Console export, profiler frame-time sample, memory sample, three-portrait-ratio capture, or completed-playthrough record is present.
- No `PR3D_manifest.json` or clearly identified PR3D art export folder is present. Existing model/material files are not sufficient evidence of Level 301 integration or prefab-contract preservation.
- `Assets/_Projects/UIReview/before_home.png`, `before_home_2.png`, `after_home.png`, and `after_splash.png` are 1080×1920 solid-color placeholders and have the same SHA-1 (`e150ea0ab499571de5114c98e521695febf399a4`); they do not evidence a 3D before/after comparison.

### Decision

**Not approved for rollout.** PR3D-012 remains unchecked. Do not create or execute a separate plan for the remaining 319 levels until the Level 301 review package is complete and an explicit approval is recorded.

### Required fixes before re-review

1. Capture matched baseline and after Game-view evidence for Level 301, plus Blender viewport screenshots, with device/build and portrait aspect ratio recorded.
2. Run the required `MyMenu > StartGame` Level 301 playthrough and record drag, Ice, pizza transfer, gate/ray directions, timer, win/lose, boosters, popups, and ad-area results.
3. Export Unity Console and profiler evidence; report baseline versus after GPU frame time and memory, demonstrating the +20%/+30% limits.
4. Verify the acceptance contract from runtime (49 cells, 23 containers, 7 lines, four shapes, Ice, ten colors) and attach evidence that rails/gates align and pizza variants remain readable without overflow at three portrait ratios.
5. Produce `PR3D_manifest.json` for imported assets and document roots, pivots, colliders, serialized references, materials, textures, triangle counts, and rollback path.
6. Re-run the review, list any residual defects with owners, and record explicit approval before drafting the 319-level rollout plan.
