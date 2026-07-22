# 001 — Pizza 3D vertical slice

Status: Ready
Issue: Forge Plan Sync / Pizza Rush 3D reskin
Branch/worktree: TBD — use a dedicated art branch before prefab promotion.

## Objective

Create a polished pizza-factory 3D visual slice for Level 301 while preserving all gameplay and serialized contracts.

## Scope

Level 301 only for acceptance. The slice covers board, rails, connectors, color gates, pizza/topping variants, four container shapes, Ice, and reusable kitchen props. It does not change level JSON or gameplay code.

### Quy tắc thực thi Forge

Mỗi task phải ưu tiên dùng **Unity MCP** để mở scene, thao tác prefab, chạy Play Mode, chụp Game view và đọc Console/Profiler; dùng **Blender MCP** để dựng, chỉnh và export asset 3D. Nếu Unity MCP hoặc Blender MCP không khả dụng, task phải ghi rõ blocker trong handoff và không tự thay đổi gameplay contract, level JSON, collider hay serialized root.

## Tasks

- [ ] [PR3D-001] Chụp baseline Level 301
  Ghi lại Game view, hierarchy, camera, ánh sáng, material, prefab root, frame time, memory và một lượt chơi hoàn chỉnh trước khi thay 3D. Dùng Unity MCP để mở scene, chụp ảnh và đọc profiler; concept nằm ở `docs/reference/pizza-factory-concept.png`.
  Công cụ: dùng Unity MCP để kiểm tra Unity và Blender MCP cho asset; nếu MCP không khả dụng, ghi rõ và không tự sửa gameplay contract.
  Image: docs/reference/pizza-factory-concept.png

- [ ] [PR3D-002] Tạo Blender master scene
  Thiết lập đơn vị mét, trục Y-up/forward-Z, camera portrait, collection, quy tắc đặt tên, preset export và concept `docs/reference/pizza-factory-concept.png`. Dùng Blender MCP để dựng scene; dùng Unity MCP để kiểm tra import thử trong Unity.
  Công cụ: dùng Unity MCP để kiểm tra Unity và Blender MCP cho asset; nếu MCP không khả dụng, ghi rõ và không tự sửa gameplay contract.
  Image: docs/reference/pizza-factory-concept.png

- [ ] [PR3D-003] Dựng board và bộ tile
  Dựng tray/frame 7×7 và visual tile theo concept nhưng giữ nguyên cell size, pivot và vị trí grid. Dùng Blender MCP để model và Unity MCP để đối chiếu Level 301 trong Unity.
  Công cụ: dùng Unity MCP để kiểm tra Unity và Blender MCP cho asset; nếu MCP không khả dụng, ghi rõ và không tự sửa gameplay contract.
  Image: docs/reference/pizza-factory-concept.png

- [ ] [PR3D-004] Dựng bộ ray procedural
  Dựng ray thẳng, ray cong 90° có thể mirror/rotate, support, mũi tên và connector board khớp production place hiện tại. Dùng Blender MCP để tạo kit và Unity MCP để kiểm tra anchor, hướng ray và khoảng cách.
  Công cụ: dùng Unity MCP để kiểm tra Unity và Blender MCP cho asset; nếu MCP không khả dụng, ghi rõ và không tự sửa gameplay contract.
  Image: docs/reference/pizza-factory-concept.png

- [ ] [PR3D-005] Dựng hệ thống cổng màu
  Dựng một gate mesh với mười variant material/emission dùng chung, không đổi entry/exit transform. Dùng Blender MCP để tạo mesh/material và Unity MCP để kiểm tra màu, emission và vị trí cổng.
  Công cụ: dùng Unity MCP để kiểm tra Unity và Blender MCP cho asset; nếu MCP không khả dụng, ghi rõ và không tự sửa gameplay contract.
  Image: docs/reference/pizza-factory-concept.png

- [ ] [PR3D-006] Dựng pizza và variant topping
  Dựng một mesh pizza tam giác với mười variant topping/màu dễ phân biệt ở kích thước điện thoại bằng material, không nhân bản mesh. Dùng Blender MCP để tạo asset và Unity MCP để kiểm tra readability trong Game view.
  Công cụ: dùng Unity MCP để kiểm tra Unity và Blender MCP cho asset; nếu MCP không khả dụng, ghi rõ và không tự sửa gameplay contract.
  Image: docs/reference/pizza-factory-concept.png

- [ ] [PR3D-007] Thay visual container Level 301
  Thay visual cho shape 1×1, 1×2, 1×3, T và Ice nhưng giữ nguyên root, collider, place và component reference. Dùng Unity MCP để kiểm tra prefab/runtime; không sửa enum, JSON, drag hoặc collider.
  Công cụ: dùng Unity MCP để kiểm tra Unity và Blender MCP cho asset; nếu MCP không khả dụng, ghi rõ và không tự sửa gameplay contract.
  Image: docs/reference/pizza-factory-concept.png

- [ ] [PR3D-008] Dựng environment bếp pizza
  Tạo lò và các family wall/floor/counter, shelf, đèn, jar/bowl, basil, dụng cụ và crate nguyên liệu có thể instance. Dùng Blender MCP cho model/procedural asset, Unity MCP để kiểm tra ánh sáng, camera portrait và safe area.
  Công cụ: dùng Unity MCP để kiểm tra Unity và Blender MCP cho asset; nếu MCP không khả dụng, ghi rõ và không tự sửa gameplay contract.
  Image: docs/reference/pizza-factory-concept.png

- [ ] [PR3D-009] Tối ưu và export asset
  Apply transform, cleanup topology, UV/atlas, LOD, shared material, export FBX/GLB và tạo `PR3D_manifest.json`. Dùng Blender MCP để export; dùng Unity MCP kiểm tra import, scale, pivot và material.
  Công cụ: dùng Unity MCP để kiểm tra Unity và Blender MCP cho asset; nếu MCP không khả dụng, ghi rõ và không tự sửa gameplay contract.
  Image: docs/reference/pizza-factory-concept.png

- [ ] [PR3D-010] Tích hợp visual prefab vào Unity
  Chỉ thêm hoặc thay visual child/material; không đổi script, enum, JSON, collider hay serialized gameplay root. Dùng Unity MCP để thao tác prefab, apply thay đổi và kiểm tra reference trước khi lưu.
  Công cụ: dùng Unity MCP để kiểm tra Unity và Blender MCP cho asset; nếu MCP không khả dụng, ghi rõ và không tự sửa gameplay contract.
  Image: docs/reference/pizza-factory-concept.png

- [ ] [PR3D-011] Validate Level 301
  Chơi hết level và kiểm tra drag, Ice, pizza transfer, hướng ray, gate, ba portrait ratio, console, frame time, memory, HUD, booster và vùng quảng cáo. Dùng Unity MCP để chạy test, chụp evidence và đọc profiler.
  Công cụ: dùng Unity MCP để kiểm tra Unity và Blender MCP cho asset; nếu MCP không khả dụng, ghi rõ và không tự sửa gameplay contract.
  Image: docs/reference/pizza-factory-concept.png

- [ ] [PR3D-012] Review vertical slice và quyết định rollout
  So sánh evidence trước/sau, liệt kê lỗi cần sửa và chỉ tạo plan riêng cho 319 level còn lại sau khi được duyệt. Dùng Unity MCP để mở evidence/runtime và Blender MCP để review asset source.
  Công cụ: dùng Unity MCP để kiểm tra Unity và Blender MCP cho asset; nếu MCP không khả dụng, ghi rõ và không tự sửa gameplay contract.
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
