# PR3D art source

This folder contains versioned source and validation assets for the Level 301
pizza-factory vertical slice.

## Master scene

- `Source/PR3D_PizzaFactory_Master.blend` is the Blender 5.2 LTS master scene.
- `Source/setup_master.py` rebuilds the scene through Blender or Blender MCP.
- `Source/validate_master.py` checks board count, bounds, UVs, transforms, and
  triangle count without modifying the scene.
- `Exports/PR3D_ImportProbe.fbx` is an asymmetric meter-scale probe for Unity
  scale, pivot, Y-up, and +Z-forward validation.
- `Exports/Board/` contains the 7×7 tray plus reusable center, edge, and corner
  tile FBX modules.
- `Previews/PR3D_MasterScene.png` is the portrait-camera preview.
- `PR3D_manifest.json` records the contract, checksums, and import evidence.

The master scene uses meters and Blender-native Z-up authoring. Its FBX preset
uses the standard Blender-to-Unity conversion (`-Z` forward, `Y` up), producing
Y-up and +Z-forward geometry in Unity. Do not enable Blender's experimental
`bake_space_transform` option for this static pipeline.

## Collections

- `00_REFERENCE`: concept images; not exported.
- `10_GAMEPLAY_VISUALS`: Board, Rails, Gates, Pizza, Containers, and Ice.
- `20_ENVIRONMENT`: Architecture and reusable Props.
- `80_CAMERAS_LIGHTS`: portrait camera and look-development lights.
- `90_EXPORT`: validated export roots only.
- `99_GUIDES`: origin and 7×7 meter guide; not exported.

Root objects use `PR3D_<Family>_<Variant>`. Render-only children use descriptive
names such as `Visual`, `GateGlow`, and `Topping`. Existing Unity gameplay roots,
colliders, anchors, JSON, enums, and serialized child names remain unchanged.

## Board contract

The Level 301 board uses 1-meter cells. `PR3D_Board_7x7_Root` is centered at
the floor-plane origin and contains 49 tile visuals on exact 1-meter spacing.
Its overall visual bounds are 7.6×0.42×7.6 meters after Unity import.

The center module remains inside the cell at 0.92×0.12×0.92 meters. Edge and
corner modules add visual trim outside the cream tile surface and are intended
to be rotated around the same cell-center pivot.
