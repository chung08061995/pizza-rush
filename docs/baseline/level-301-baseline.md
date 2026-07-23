# Level 301 3D baseline (PR3D-001)

Captured: 2026-07-23, Unity `6000.4.10f1`, instance `pizza-rush@f5afa6b68a604228`.

## MCP availability

- Unity MCP: available at `http://127.0.0.1:8080/mcp` (server `mcp-for-unity-server` 3.4.4).
- Blender MCP: not exposed/available in this session; no Blender asset inspection was performed.
- `MyMenu/StartGame` was invoked. After the initial Unity play-mode transition issue, Level 301 was opened manually and captured live in Play mode. No gameplay or serialized gameplay contract was modified.

## LevelRunner scene snapshot

Scene loaded: `Assets/_Projects/Scenes/LevelRunner.unity`.

Authored hierarchy roots (5):

| Root | Components / notes |
|---|---|
| Main Camera | `Camera`, `UniversalAdditionalCameraData`; orthographic, FOV 60, size 7, aspect 0.5625, position `(5, 11, -3.03)`, rotation `(60.71, 0, 0)`, HDR/MSAA on, post-processing on |
| Directional Light | intensity 1, white, shadows hard (`shadowStrength=1`), rotation `(50, 330, 0)`, pipeline light data present |
| Plane | default Unity plane, scale `(10,10,10)`, position y `-0.01`, `MeshRenderer`, `MeshCollider`, material `Assets/_Projects/Materials/Floor.mat` |
| LevelFactory | `LevelFactory` component; no children in authored scene snapshot |
| Global Volume | `Volume` component |

Runtime hierarchy roots (6): the five roots above plus `BANNER(Clone)`. `LevelFactory` contained a runtime `LevelRunner(Clone)` tree with 5,331 transforms and 2,475 renderers. Major runtime groups included `TileMap3D`, `Container Pool`, `ProductionLine Pool`, container clones for 1x1/1x2/1x3/T shapes, Ice visuals, production spawn points, bridges and belt segments.

The runtime objects were pool/spawner-created clones and did not resolve to prefab source paths through `PrefabUtility.GetCorrespondingObjectFromSource`. Treat `LevelFactory -> LevelRunner(Clone)` as the runtime visual root; preserve its scripts, colliders, places and serialized references.

Observed material dependencies included `TileMap3D/Border.mat`, `Container/Ice.mat`, `Production/Production_White.mat`, `Tile/ground.mat`, `Tile/tile.mat`, `belt.mat`, `belt_border.mat`, `Models/Pizza/pizza.mat`, `Tile/border.mat`, `Floor.mat`, `pizza.mat` and `Pizza1.mat`, plus runtime material instances.

## Rendering/profiler sample

Unity MCP `mcpforunity://rendering/stats` reported render textures 28 (`199,935,740` bytes), render-target changes 3 and 256 visible skinned meshes. Its draw-call/triangle counters returned zero and should not be used as a valid GPU baseline.

Runtime Level 301 samples:

- Gameplay at timer `00:03`: frame delta `5.534 ms`; allocated memory `539,746,238` bytes; reserved memory `1,105,035,264` bytes; Mono used `1,403,756,544` bytes. The in-game overlay displayed 40 FPS.
- Lose popup at timer `00:00`: frame delta `19.435 ms`; allocated memory `512,360,450` bytes; reserved memory `1,081,950,208` bytes. The transition-frame overlay displayed 3 FPS.

## Screenshots and complete-play status

- Gameplay Game View: `Assets/_Baseline/Level301_GameView.png`.
- Terminal lose state (`PopupLose(Clone)`): `Assets/_Baseline/Level301_EndState.png`.
- Complete Unity Game View recording: `Assets/_Baseline/Level301_FullRun_Complete.mp4` (1080x1920, 30 FPS, 98.17 seconds).

Concept reference inspected: `docs/reference/pizza-factory-concept.png` (portrait factory layout with oven, colored production lines, central grid, and tiled/material-rich environment).

The final Unity Recorder evidence runs continuously from timer `01:22` through `00:00` and holds the terminal `PopupLose(Clone)` / `Time Up` state. This is a complete passive lose run (no container input), suitable as the pre-3D visual/performance baseline. Gameplay contracts, level JSON, prefabs and scene data were left unchanged.
