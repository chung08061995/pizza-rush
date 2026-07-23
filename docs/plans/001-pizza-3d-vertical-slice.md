# 001 — Pizza 3D vertical slice

Status: Ready
Issue: Forge Plan Sync / Pizza Rush 3D reskin
Branch/worktree: TBD — use a dedicated art branch before prefab promotion.

## Objective

Create a polished pizza-factory 3D visual slice for Level 301 while preserving all gameplay and serialized contracts.

## Scope

Level 301 only for acceptance. The slice covers board, rails, connectors, color gates, pizza/topping variants, four container shapes, Ice, and reusable kitchen props. It does not change level JSON or gameplay code.

### Quy tắc thực thi Forge

Mỗi task phải ưu tiên gọi server **`unityMCP` tại `http://127.0.0.1:8080/mcp`** để mở scene, thao tác prefab, chạy Play Mode, chụp Game view và đọc Console/Profiler; dùng **Blender MCP** để dựng, chỉnh và export asset 3D. Nếu MCP không khả dụng, task phải ghi rõ blocker trong handoff và không tự thay đổi gameplay contract, level JSON, collider hay serialized root.

## Tasks

- [x] [PR3D-001] Chụp baseline Level 301
  Ghi lại Game view, hierarchy, camera, ánh sáng, material, prefab root, frame time, memory và một lượt chơi hoàn chỉnh trước khi thay 3D. Dùng Unity MCP để mở scene, chụp ảnh và đọc profiler; concept nằm ở `docs/reference/pizza-factory-concept.png`.
  Công cụ: gọi server `unityMCP` (`http://127.0.0.1:8080/mcp`) để kiểm tra Unity và Blender MCP cho asset; nếu MCP không khả dụng, ghi rõ và không tự sửa gameplay contract.
  Image: docs/reference/pizza-factory-concept.png
  Evidence: `docs/baseline/level-301-baseline.md`, `Assets/_Baseline/Level301_GameView.png`, `Assets/_Baseline/Level301_EndState.png`, `Assets/_Baseline/Level301_FullRun_Complete.mp4` (Unity Recorder, 1080x1920/30 FPS, continuous timer `01:22` to `Time Up`).

- [x] [PR3D-002] Tạo Blender master scene
  Thiết lập đơn vị mét, trục Y-up/forward-Z, camera portrait, collection, quy tắc đặt tên, preset export và concept `docs/reference/pizza-factory-concept.png`. Dùng Blender MCP để dựng scene; dùng Unity MCP để kiểm tra import thử trong Unity.
  Công cụ: gọi server `unityMCP` (`http://127.0.0.1:8080/mcp`) để kiểm tra Unity và Blender MCP cho asset; nếu MCP không khả dụng, ghi rõ và không tự sửa gameplay contract.
  Image: docs/reference/pizza-factory-concept.png
  Evidence: reused only the verified `Art/PR3D/` paths from archived commit `b6fd5aa`; hashes match its manifest. Blender MCP reconfirmed metric units, 1080×1920 orthographic camera, collections, naming roots, and concept reference. Unity MCP reconfirmed scale `1`, a `1×1×1 m` meter cube at `(0, 0.5, 0)`, +Z marker, root pivot at origin, no collider/MonoBehaviour, and zero PR3D Console errors; the temporary Unity import was deleted.

- [x] [PR3D-003] Dựng board và bộ tile
  Dựng tray/frame 7×7 và visual tile theo concept nhưng giữ nguyên cell size, pivot và vị trí grid. Dùng Blender MCP để model và Unity MCP để đối chiếu Level 301 trong Unity.
  Công cụ: gọi server `unityMCP` (`http://127.0.0.1:8080/mcp`) để kiểm tra Unity và Blender MCP cho asset; nếu MCP không khả dụng, ghi rõ và không tự sửa gameplay contract.
  Image: docs/reference/pizza-factory-concept.png
  Evidence: Blender/Unity MCP pass; board `7.78×0.36×7.78 m`, tile `0.9×0.118×0.9 m`, 49 centers at 1 m pitch, UVs present, zero collider/MonoBehaviour; Level 301 remained 49 cells/23 containers/7 lines. Temp probe deleted.

- [x] [PR3D-004] Dựng bộ ray procedural
  Dựng ray thẳng, ray cong 90° có thể mirror/rotate, support, mũi tên và connector board khớp production place hiện tại. Dùng Blender MCP để tạo kit và Unity MCP để kiểm tra anchor, hướng ray và khoảng cách.
  Công cụ: gọi server `unityMCP` (`http://127.0.0.1:8080/mcp`) để kiểm tra Unity và Blender MCP cho asset; nếu MCP không khả dụng, ghi rõ và không tự sửa gameplay contract.
  Image: docs/reference/pizza-factory-concept.png
  Evidence: Blender/Unity MCP pass; straight 5.28 m with 11 anchors at 0.48 m, curves radius 1.89 m, connector 0.24 m, +Z-forward and Y-up bounds confirmed. Temp imports deleted; gameplay contract unchanged.

- [x] [PR3D-005] Dựng hệ thống cổng màu
  Dựng một gate mesh với mười variant material/emission dùng chung, không đổi entry/exit transform. Dùng Blender MCP để tạo mesh/material và Unity MCP để kiểm tra màu, emission và vị trí cổng.
  Công cụ: gọi server `unityMCP` (`http://127.0.0.1:8080/mcp`) để kiểm tra Unity và Blender MCP cho asset; nếu MCP không khả dụng, ghi rõ và không tự sửa gameplay contract.
  Image: docs/reference/pizza-factory-concept.png
  Evidence: shared gate mesh (1,662 verts/4 submeshes), bounds `1.000×0.716×0.286 m`, scale 1, symmetric Entry/Exit ±0.226 m with Exit rotated 180°, ten URP emission materials, and Unity temp import pass; temp deleted.

- [x] [PR3D-006] Dựng pizza và variant topping
  Dựng một mesh pizza tam giác với mười variant topping/màu dễ phân biệt ở kích thước điện thoại bằng material, không nhân bản mesh. Dùng Blender MCP để tạo asset và Unity MCP để kiểm tra readability trong Game view.
  Công cụ: gọi server `unityMCP` (`http://127.0.0.1:8080/mcp`) để kiểm tra Unity và Blender MCP cho asset; nếu MCP không khả dụng, ghi rõ và không tự sửa gameplay contract.
  Image: docs/reference/pizza-factory-concept.png
  Evidence: Blender MCP verified one shared mesh datablock with 11 users, metric units, applied scales, ten named variants; UVs repaired and re-exported through Blender MCP. Unity MCP import probe reports 324 UVs/184 triangles, zero colliders, and temp cleanup.

- [x] [PR3D-007] Thay visual container Level 301
  Thay visual cho shape 1×1, 1×2, 1×3, T và Ice nhưng giữ nguyên root, collider, place và component reference. Dùng Unity MCP để kiểm tra prefab/runtime; không sửa enum, JSON, drag hoặc collider.
  Công cụ: gọi server `unityMCP` (`http://127.0.0.1:8080/mcp`) để kiểm tra Unity và Blender MCP cho asset; nếu MCP không khả dụng, ghi rõ và không tự sửa gameplay contract.
  Image: docs/reference/pizza-factory-concept.png
  Evidence: Blender MCP verified five roots, metric units, zero non-unit scales, all UVs present; T cell offsets and Ice bounds are documented in `PR3D_Containers_Contract.json`. No shared prefab or gameplay file changed.

- [x] [PR3D-008] Dựng environment bếp pizza
  Tạo lò và các family wall/floor/counter, shelf, đèn, jar/bowl, basil, dụng cụ và crate nguyên liệu có thể instance. Dùng Blender MCP cho model/procedural asset, Unity MCP để kiểm tra ánh sáng, camera portrait và safe area.
  Công cụ: gọi server `unityMCP` (`http://127.0.0.1:8080/mcp`) để kiểm tra Unity và Blender MCP cho asset; nếu MCP không khả dụng, ghi rõ và không tự sửa gameplay contract.
  Image: docs/reference/pizza-factory-concept.png
  Evidence: Blender MCP verified 11 named roots (oven, wall, floor, counter, shelf, pendant, jar, bowl, basil, utensils, crate), metric units, zero non-unit scales, all UVs present; Unity oven probe had UVs and zero colliders. No gameplay files changed.

- [x] [PR3D-009] Tối ưu và export asset
  Apply transform, cleanup topology, UV/atlas, LOD, shared material, export FBX/GLB và tạo `PR3D_manifest.json`. Dùng Blender MCP để export; dùng Unity MCP kiểm tra import, scale, pivot và material.
  Công cụ: gọi server `unityMCP` (`http://127.0.0.1:8080/mcp`) để kiểm tra Unity và Blender MCP cho asset; nếu MCP không khả dụng, ghi rõ và không tự sửa gameplay contract.
  Image: docs/reference/pizza-factory-concept.png
  Evidence: all Blender sources use metric units and applied mesh scales; final FBX probes have UVs (Pizza repaired to 324 UVs/184 triangles), no probe collider/MonoBehaviour, and Unity generated worktree-local `.meta` files. `Art/PR3D/PR3D_manifest.json` schema 2 lists every family and contract. Generated `.blend1`/bytecode artifacts were removed.

- [x] [PR3D-010] Tích hợp visual prefab vào Unity
  Chỉ thêm hoặc thay visual child/material; không đổi script, enum, JSON, collider hay serialized gameplay root. Dùng Unity MCP để thao tác prefab, apply thay đổi và kiểm tra reference trước khi lưu.
  Công cụ: gọi server `unityMCP` (`http://127.0.0.1:8080/mcp`) để kiểm tra Unity và Blender MCP cho asset; nếu MCP không khả dụng, ghi rõ và không tự sửa gameplay contract.
  Image: docs/reference/pizza-factory-concept.png
  Evidence: Unity MCP created 44 additive visual prefabs under `Assets/_Projects/Art/PR3D/Prefabs/`: two board, five container, five rail, ten shared-mesh gate variants, ten shared-mesh pizza variants, eleven environment modules, and one Level 301 environment composition. Audit: zero colliders, zero MonoBehaviours, zero missing components; all ten pizza variants reference one shared Unity mesh. Existing gameplay prefabs, scripts, scenes, enums, colliders, and level JSON were not modified. Promotion into shared gameplay prefabs remains intentionally held for review.

- [x] [PR3D-011] Validate Level 301
  Chơi hết level và kiểm tra drag, Ice, pizza transfer, hướng ray, gate, ba portrait ratio, console, frame time, memory, HUD, booster và vùng quảng cáo. Dùng Unity MCP để chạy test, chụp evidence và đọc profiler.
  Công cụ: gọi server `unityMCP` (`http://127.0.0.1:8080/mcp`) để kiểm tra Unity và Blender MCP cho asset; nếu MCP không khả dụng, ghi rõ và không tự sửa gameplay contract.
  Image: docs/reference/pizza-factory-concept.png
  Evidence: Android compile passed. `MyMenu > StartGame` loaded Level 301 with 49 cells, 23 containers, seven lines, four shape families, ten colors, and one T-shaped Ice container. A generated no-skill solution completed twice with the PR3D runtime composition attached before the first action: 293 drags, 26 feeds, production remaining `0`, Ice `12 → 0`, and normal post-win scene transition. Final Console: zero errors and zero warnings. Three Game View captures passed at 1080×1920, 1080×2160, and 1440×2960. Same-session A/B medians: GPU `7.611 → 6.615 ms`, CPU `16.888 → 17.208 ms`; allocated memory `697,135,782 → 718,189,900 bytes` (+3.0%), reserved memory unchanged.

- [x] [PR3D-012] Review vertical slice và quyết định rollout
  So sánh evidence trước/sau, liệt kê lỗi cần sửa và chỉ tạo plan riêng cho 319 level còn lại sau khi được duyệt. Dùng Unity MCP để mở evidence/runtime và Blender MCP để review asset source.
  Công cụ: gọi server `unityMCP` (`http://127.0.0.1:8080/mcp`) để kiểm tra Unity và Blender MCP cho asset; nếu MCP không khả dụng, ghi rõ và không tự sửa gameplay contract.
  Image: docs/reference/pizza-factory-concept.png
  Evidence: review recorded in `docs/reviews/pr3d-level301-vertical-slice.md`. Decision: the Level 301 visual slice is ready for stakeholder review, but rollout/promotion to shared gameplay prefabs and the other 319 levels is on hold until explicit approval. No rollout plan was created.

## Acceptance criteria

- [x] Level 301 still has 49 grid cells, 23 containers, 7 production lines, four shapes, Ice, and ten colors.
- [x] Existing drag, production transfer, timer, win/lose, booster, and popup behavior is unchanged.
- [x] Rails are continuous and gates align with current board/line anchors.
- [x] Pizza color/topping changes are readable at phone scale without obscuring the puzzle.
- [x] No more than +20% GPU frame time or +30% memory versus baseline on the same device.
- [x] No critical Unity Console errors and no visual overflow at three portrait ratios.

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

2026-07-23 resolved blocker: the missing AdMob types occurred only while the
worktree editor was on Standalone, where the project's `GOOGLE_ADMOB` define is
not active. Switching the Unity MCP worktree instance to Android restored a
clean compile without changing Ads or gameplay code. PR3D-010 through PR3D-012
then completed with the evidence above.
