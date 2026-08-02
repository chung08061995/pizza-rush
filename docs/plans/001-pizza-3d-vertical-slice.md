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

- [ ] [PR3D-SEQ-001] Hoàn thiện vertical slice Pizza Rush 3D
  Thực hiện toàn bộ phần 3D còn lại như một task hybrid duy nhất để mọi thay đổi dùng chung một worktree và branch. Trước khi làm lại PR3D-002/003, kiểm tra các attempt đã archive và chỉ tái sử dụng/cherry-pick phần đã xác minh; không merge nguyên branch cũ một cách mù quáng.
  Mode: hybrid
  Subtasks:
    1. [PR3D-002] Tạo và xác minh Blender master scene
       Phase: 1
       Thiết lập đơn vị mét, trục Y-up/forward-Z, camera portrait, collection, naming, export preset và concept reference. Kiểm tra attempt PR3D-002 cũ, tái sử dụng asset hợp lệ rồi xác minh import thử bằng Unity MCP.
    2. [PR3D-003] Dựng và xác minh board cùng bộ tile
       Phase: 2
       Chỉ sở hữu `Assets/_Projects/Art/PR3D/Board/` và Blender source tương ứng. Dựng tray/frame 7×7 và tile visual nhưng giữ nguyên cell size, pivot và grid position. Kiểm tra attempt PR3D-003 cũ, tái sử dụng phần đạt yêu cầu và đối chiếu Level 301 trong Unity.
    3. [PR3D-004] Dựng bộ ray procedural
       Phase: 2
       Chỉ sở hữu `Assets/_Projects/Art/PR3D/Rails/` và Blender source tương ứng. Dựng ray thẳng, ray cong 90° có thể mirror/rotate, support, mũi tên và connector khớp production-line place hiện tại; xác minh anchor, hướng và khoảng cách bằng Unity MCP.
    4. [PR3D-005] Dựng hệ thống cổng màu
       Phase: 2
       Chỉ sở hữu `Assets/_Projects/Art/PR3D/Gates/` và Blender source tương ứng. Dựng một gate mesh với mười variant material/emission dùng chung, giữ nguyên entry/exit transform và kiểm tra màu, emission, vị trí trong Unity.
    5. [PR3D-006] Dựng pizza và variant topping
       Phase: 2
       Chỉ sở hữu `Assets/_Projects/Art/PR3D/Pizza/` và Blender source tương ứng. Dựng một pizza mesh với mười variant topping/màu dễ phân biệt ở kích thước điện thoại, không nhân bản mesh và kiểm tra readability trong Game view.
    6. [PR3D-007] Thay visual container Level 301
       Phase: 2
       Chỉ sở hữu `Assets/_Projects/Art/PR3D/Containers/` và Blender source tương ứng; chưa sửa prefab dùng chung ở phase này. Dựng visual cho shape 1×1, 1×2, 1×3, T và Ice, giữ nguyên yêu cầu root, collider, place, component reference, enum, JSON và drag behavior cho phase tích hợp.
    7. [PR3D-008] Dựng environment bếp pizza
       Phase: 2
       Chỉ sở hữu `Assets/_Projects/Art/PR3D/Environment/` và Blender source tương ứng. Tạo lò và các family wall/floor/counter, shelf, đèn, jar/bowl, basil, dụng cụ và ingredient crate có thể instance; kiểm tra camera portrait và safe area.
    8. [PR3D-009] Tối ưu và export asset
       Phase: 3
       Apply transform, cleanup topology, UV/atlas, LOD, shared material, export FBX/GLB và tạo `PR3D_manifest.json`; kiểm tra scale, pivot và material trong Unity.
    9. [PR3D-010] Tích hợp visual prefab vào Unity
       Phase: 4
       Chỉ thêm hoặc thay visual child/material; không đổi script, enum, JSON, collider hoặc serialized gameplay root. Apply prefab và kiểm tra reference trước khi lưu.
    10. [PR3D-011] Validate Level 301
        Phase: 5
        Chơi hết level; kiểm tra drag, Ice, pizza transfer, ray, gate, ba portrait ratio, Console, frame time, memory, HUD, booster và vùng quảng cáo.
    11. [PR3D-012] Review vertical slice và quyết định rollout
        Phase: 6
        So sánh evidence trước/sau, liệt kê lỗi cần sửa và chỉ tạo plan riêng cho 319 level còn lại sau khi được duyệt.
  Công cụ cho mọi bước: Blender MCP dùng để dựng/chỉnh/export asset; `unityMCP` tại `http://127.0.0.1:8080/mcp` dùng để kiểm tra Unity. Nếu MCP cần thiết không khả dụng, dừng đúng bước đang làm và báo blocker, không tự thay đổi gameplay contract.
  Image: docs/reference/pizza-factory-concept.png

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
