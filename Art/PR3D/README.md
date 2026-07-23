# PR3D art source

This folder contains versioned source and validation assets for the Level 301
pizza-factory vertical slice.

## PR3D-003 board and tile kit

- `Source/PR3D_PizzaFactory_Master.blend` contains the modeled board and tile.
- `Source/build_pr3d_003_board.py` deterministically rebuilds and exports the kit.
- `Exports/Board/*.glb` are review/archive exports.
- `Previews/PR3D_003_BoardTile.png` is the portrait review render.
- Unity-facing FBX files live in
  `Assets/_Projects/Art/PR3D/Board/Models/`.
- `PR3D_manifest.json` records dimensions, pivots, material dependencies, triangle
  counts, checksums, and Unity MCP evidence.

The kit uses the validated 1 m cell pitch from Level 301. The tile root stays at
the cell center on the gameplay plane and remains inside the existing 1×1 m
footprint. The board root is centered on the 7×7 grid and should be placed at
Unity world `(4.5, 0, 4.5)` for Level 301.

The source uses Blender-native Z-up authoring and the existing Unity export
conversion (`-Z` forward, `Y` up). Existing gameplay roots, colliders, grid
positions, level JSON, production anchors, and serialized child names are not
changed by this task.
