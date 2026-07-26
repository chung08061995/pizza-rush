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

For the Level 301 slice, the procedural models use shared solid-color material
parameters and no bitmap texture dependencies, so an empty atlas would add
overhead without reducing texture state. Blender MCP audited every render mesh
for applied rotation/scale and at least one UV layer. The six authored families
total under 100k triangles including preview variants; runtime modules therefore
ship as LOD0-only for this review and must be reconsidered after a real rollout
composition is profiled.

## Export and rollback

Export FBX for Unity integration and GLB for review/archive when useful. Record scale, pivot, materials, textures, triangle count, and target prefab in `PR3D_manifest.json`. Keep prototype assets in a versioned art folder; rollback disables/removes the additive visual child or restores the prefab variant without changing gameplay files.

Run `python3 Art/PR3D/build_manifest.py` after any source/export change. It
writes identical, hashed manifests to the source art root and Unity art root.
Unity-side model validation must still use MCP; the manifest generator does not
replace import, pivot, component, Console, or Game-view checks.

Before promoting an FBX, import the asymmetric probe without target-size
normalization. A passing Unity result has importer scale 1, a 1×1×1 meter cube
centered at `(0, 0.5, 0)`, and the orange marker extending toward +Z. Do not
enable Unity `Bake Axis Conversion` for this preset.

## Level 301 visual integration

The vertical slice uses additive visual prefabs under
`Assets/_Projects/Art/PR3D/Prefabs/` plus derived static meshes under
`Assets/_Projects/Art/PR3D/Derived/`. Gameplay prefab roots, colliders,
MonoBehaviours, places, and serialized renderer lists stay intact.

- Container visuals replace only the mesh/material on the existing referenced
  renderer; the derived mesh compensates the legacy visual transform.
- Pizza and gate visuals reuse the existing serialized production/line
  renderers so runtime color assignment continues to work.
- Board and environment are collider-free children of `LevelRunner`.
- The environment backdrop is a collider-free quad with a procedural URP
  material: warm terracotta below the factory wall split and blue tiles above.

After any integration edit, compare the gameplay prefab MonoBehaviour/collider
YAML blocks to HEAD, verify the Level 301 JSON hash, run
`MyMenu > StartGame`, and inspect the runtime reference counts and Console.

Prefab override object references must use Unity's canonical two-line YAML:

```yaml
propertyPath: m_Mesh
value:
objectReference: {fileID: 4300000, guid: ..., type: 2}
```

Never concatenate `value:` and `objectReference:` on one line. Unity accepts the
file but resolves the override as null after reimport/restart. PR3D validation
must therefore include a clean-editor restart and non-zero renderer-bounds
audit, not only an in-session screenshot.

The Level 301 replay is reproducible with
`CoffeeRunMigration/Solutions/0301.json`; the most recent result is written to
`CoffeeRunMigration/Reports/runtime-replay/0301.json`. Final validation and
rollout review live in `docs/reviews/pr3d-level301-validation.md` and
`docs/reviews/pr3d-vertical-slice-review.md`.
