# 3D art pipeline

## Coordinate and naming contract

- Author in Blender's native Z-up coordinates with metric units and one Blender
  unit per meter. Export FBX with `axis_forward=-Z`, `axis_up=Y`,
  `apply_unit_scale=true`, and `bake_space_transform=false`; the Unity contract is
  Y-up and +Z-forward.
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

The PR3D import probe's unscaled source maximum dimension is `1.695 m`. When
using Unity MCP `import_model_file`, pass `target_size=1.695`; otherwise that
tool intentionally normalizes the model to a one-meter maximum dimension and
changes the importer scale.

## Vertical-slice prefab promotion

PR3D review prefabs live under `Assets/_Projects/Art/PR3D/Prefabs/`. They are
visual-only nested prefabs: their roots have no MonoBehaviour or collider, and
color variants reuse shared meshes. Use `MaterialPropertyBlock` for runtime
container tinting; accessing `Renderer.materials` creates per-renderer material
instances and invalidates the shared-material performance budget.

The Level 301 environment composition is a review asset, not a gameplay
reference. Its wall and terracotta-floor children remain available as family
prefabs but are disabled in the portrait composition because they obscured the
HUD/puzzle safe area. Do not promote the composition or visual children into
shared gameplay prefabs until the vertical-slice review explicitly approves
rollout.
