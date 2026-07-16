# Migration task status

Updated: 2026-07-16

| ID | Status | Result / next gate |
|---|---|---|
| CR-001 | Done | Fingerprint stored in `source-fingerprint.json`; APKs excluded. |
| CR-002 | Done | IL2CPP schema documented in `schema.md`. |
| CR-003 | Done | Decrypted packaged `encrypted.encr`; AssetRipper exported Default plus V72/V100/V110/V120/V211, tutorial, daily and special assets. |
| CR-004 | Done | Runtime prefs/log select `Default`; `LevelConfig` order and Level 1 were checked against BlueStacks. |
| CR-005 | Ready fallback | ADB capture helper remains available for visual/gameplay QA; structured assets were successfully extracted. |
| CR-006 | Done | Editor-only normalized record covers grid, timer, container, shape, rotation, flip, movement, material, modifiers, lines and color order. |
| CR-007 | Done | Strict converter targets `0001.json`–`0100.json`. |
| CR-008 | Done | Stable per-level source-color relationship mapping; Pizza Rush palette remains the renderer. |
| CR-009 | Done | Deterministic `Straight`/`CurvedLeft`/`CurvedRight` keys; unknown keys fail. |
| CR-010 | Done | Validator covers index/name, duplicate/outer/overlap cells, shape/rotation, capacity, order and visual mapping. |
| CR-011 | Done | Manifest reports `Exact`, `Mismatch` or `Unsupported`. |
| CR-012 | Done | `TRACKER.md` contains all 100 levels and five approval stages. |
| L001-01 | Done | Level 1 imported from the selected Default Unity asset and cross-checked with BlueStacks. |
| L001-02 | Done | 4×5, 20 cells, two free 1×3 containers, two opposing lines, 12 per color, 180 seconds. |
| L001-03 | Done | Core JSON matched; deterministic line visual fields were added. |
| L001-04 | Done | Top uses `ProductionLine_Straing`; bottom uses `ProductionLine_Belt`. |
| L001-05 | Done | `MyMenu > StartGame` portrait start-state capture exists at 1080×1920. |
| L001-06 | Not required | Gameplay solve is outside the current clone-only acceptance scope. |
| L001-07 | Not required | Lose-path timing is outside the current clone-only acceptance scope. |
| L001-08 | Done | Pizza Rush start state captured; source screenshot overlay is not required by the current scope. |
| L001-09 | Done | Exact conversion, deterministic visual mapping and start-state capture passed. |

## Conversion result

All 100 normalized records were imported from the Default runtime bundle. The strict conversion manifest reports `Exact=100`, `Mismatch=0`, `Unsupported=0`; the independent runtime integrity audit also reports 100 exact files. This replaces the earlier hand-cloned Level 1–6 data where source fields differed.

Runtime support covers directional movement, ice, LayerBox, two-color slot
distributions and stone obstacles. The clone-only acceptance gate is complete:
100 exact conversions and 100 valid portrait start-state captures. Gameplay
replay, Win/Lose and source screenshot overlays are retained only as historical
QA evidence and are not current approval gates.
