# 3D art pipeline

## Coordinate and naming contract

- Blender units are meters, Y-up, forward Z; apply transforms before export.
- Root names use `PR3D_<family>_<variant>`; child names describe render purpose (`Visual`, `GateGlow`, `Topping`).
- Pivots sit at the gameplay attachment point: tile center, rail origin, gate centerline, or container grid anchor.
- Existing gameplay roots and serialized child names are not renamed.

## Asset families

Gameplay families: board tray, tile surface, straight rail, curved rail, connector, gate, pizza slice, four Level 301 container shapes, and Ice overlay. Environment families: oven, wall/floor/counter, shelf, lamp, jars/bowls, plants, utensils, and ingredient crates.

## Materials and budgets

- URP-compatible metallic/roughness materials with restrained emission for gates and oven fire.
- Shared materials and property variants are preferred over duplicated meshes/materials.
- Target at most two 2048² environment atlases and one 1024² pizza/gate atlas.
- Visible gameplay geometry target is ≤300k triangles; core asset target 2k–10k triangles; oven ≤20k.
- Compare GPU frame time and memory to Level 301 baseline; allow no more than +20% frame time or +30% memory.

## Export and rollback

Export FBX for Unity integration and GLB for review/archive when useful. Record scale, pivot, materials, textures, triangle count, and target prefab in `PR3D_manifest.json`. Keep prototype assets in a versioned art folder; rollback disables/removes the additive visual child or restores the prefab variant without changing gameplay files.
