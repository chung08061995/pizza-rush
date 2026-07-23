# Architecture

## System boundary

Pizza Rush is a Unity 6000.4.10f1 URP mobile game. Init bootstraps Main and LevelRunner. Gameplay data is loaded from `Resources/LevelData/*.json`, then factories instantiate tile, container, and production-line prefabs.

## Runtime components

- `DataManager` owns persistent player state and configuration.
- `LevelFactory` loads JSON and creates the LevelRunner.
- `LevelObjectSpawner` creates tiles, containers, production lines, and anchors.
- `TileMap3D` selects modular tile prefabs from grid adjacency.
- `ContainerFactory` and `ProductionLineFactory` select visual prefabs while preserving data-driven placement.
- `PopupManager` owns gameplay UI and remains independent of the 3D art layer.
- `IAPManager` registers store products through `CoffeeRunIAPProductProvider`. The
  permanent No Ads entitlement uses the Google Play product ID `no_ads`.
- `AdsManager` persists the No Ads entitlement locally and reconciles it with the
  store purchase history after Unity IAP initialization. It suppresses banner and
  interstitial ads while leaving opt-in rewarded ads available.
- `VibrationManager` provides guarded mobile haptic feedback. It reads the persisted
  `DataManager.vibrate` setting and is called for pizza placement, container
  completion, win, lose, and toggle-on confirmation events.

## 3D visual boundary

The vertical slice adds or swaps visual children below existing prefab roots. Root transforms, colliders, spawn-point children, serialized scripts, and factory contracts remain unchanged. Materials may be shared or color-instanced; gameplay code does not inspect the new meshes.

## Data flow

`0301.json` → `LevelFactory` → `LevelObjectSpawner` → existing tile/container/production prefabs → additive 3D visual children/materials.

Blender master scene → FBX/GLB + textures/manifest → Unity import settings → prefab visual child/material assignment → Level 301 review.

## External tools

Blender MCP is the procedural authoring surface. Hyper3D Rodin/Hunyuan may generate prop candidates, but Blender cleanup and Unity validation are mandatory. Meshy is an optional fallback only.

## Constraints

Unity has no CLI build or automated gameplay suite; editor play and screenshots are required. Minimum device and final Forge export behavior remain TBD.
