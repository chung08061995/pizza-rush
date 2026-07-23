# 3D art pipeline

## Coordinate and naming contract

- Blender units are meters (1 Blender unit = 1 meter). Author in Blender's
  native Z-up space; the FBX preset uses `axis_forward=-Z`, `axis_up=Y`,
  `apply_unit_scale=true`, and `bake_space_transform=false` so Unity receives
  Y-up, +Z-forward geometry.
- Root names use `PR3D_<family>_<variant>`; child names describe render purpose (`Visual`, `GateGlow`, `Topping`).
- Pivots sit at the gameplay attachment point: tile center, rail origin, gate centerline, or container grid anchor.
- Existing gameplay roots and serialized child names are not renamed.

The versioned master scene lives at
`Art/PR3D/Source/PR3D_PizzaFactory_Master.blend`. It contains:

- a 1080×1920 portrait camera;
- `00_REFERENCE`, `10_GAMEPLAY_VISUALS`, `20_ENVIRONMENT`,
  `80_CAMERAS_LIGHTS`, `90_EXPORT`, and `99_GUIDES` collections;
- the portable concept reference
  `docs/reference/pizza-factory-concept.png`;
- embedded naming and FBX-preset text blocks;
- a 7×7 meter guide and a meter-scale asymmetric Unity import probe.

Keep only validated roots under `90_EXPORT`. The rebuild source,
preview, probe export, checksums, and Unity import evidence are recorded in
`Art/PR3D/PR3D_manifest.json`.

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

Before promoting an FBX, import the asymmetric probe without target-size
normalization. A passing Unity result has importer scale 1, a 1×1×1 meter cube
centered at `(0, 0.5, 0)`, and the orange marker extending toward +Z. Do not
enable Unity `Bake Axis Conversion` for this preset.
