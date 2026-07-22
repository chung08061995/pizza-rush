# Game content pipeline

## Existing content

- Level JSON lives in `Assets/_Projects/Resources/LevelData/` and is loaded by `LevelFactory`.
- `TileMap3D` selects modular tile prefabs from adjacency.
- `ContainerFactory` selects shape/material prefabs.
- `ProductionLineFactory` selects straight or curved production-line prefabs.

## 3D slice

Blender assets are authored as reusable families, exported with stable roots/pivots, imported into Unity, and assigned only to visual children of existing prefabs. A manifest records source concept, Blender scene, export file, material dependencies, triangle count, and target prefab.

## Promotion

Prototype assets stay separate from current production assets until Level 301 passes review. Accepted assets can then be promoted into shared prefab variants and evaluated against later levels.
