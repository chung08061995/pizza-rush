# Power-up 3D asset manifest

| Asset | Intended preview use | Creator | Source | Price | License | Commercial | Attribution | Modified | Checked |
|---|---|---|---|---:|---|---|---|---|---|
| Hammer_Double | Destroy Container | Quaternius | https://quaternius.com/packs/ultimaterpg.html | Free | CC0 1.0 | Yes | No | Unity import normalization only | 2026-07-30 |
| Snowflake2 | Freeze Time | Quaternius | https://quaternius.com/packs/ultimaterpg.html | Free | CC0 1.0 | Yes | No | Unity import normalization only | 2026-07-30 |
| Axe_Double | Split Container option | Quaternius | https://quaternius.com/packs/ultimaterpg.html | Free | CC0 1.0 | Yes | No | Unity import normalization only | 2026-07-30 |
| Star | Add Tile option | Quaternius | https://quaternius.com/packs/ultimaterpg.html | Free | CC0 1.0 | Yes | No | Unity import normalization only | 2026-07-30 |
| Crystal1 | Magic booster primary option | Quaternius | https://quaternius.com/packs/ultimaterpg.html | Free | CC0 1.0 | Yes | No | Unity import normalization only | 2026-07-30 |
| Crystal4 | Magic booster color alternative | Quaternius | https://quaternius.com/packs/ultimaterpg.html | Free | CC0 1.0 | Yes | No | Unity import normalization only | 2026-07-30 |
| coffee | Coffee Time option | Existing Pizza Rush project asset | Project-local | Existing | Existing project asset | Existing project use | Existing | Preview material/pose only | 2026-07-30 |
| cup-coffee | Coffee Time candidate (rejected after preview) | Kenney | https://kenney.nl/assets/food-kit | Free | CC0 1.0 | Yes | No | Unity import normalization and preview poses only | 2026-07-30 |
| Powerup_Split_Saw | Split Container Option B | Pizza Rush generated | Project-local | N/A | Project-owned | Yes | No | Built additively from Unity primitives | 2026-07-30 |
| Powerup_AddTile | Add Tile Option B | Pizza Rush generated | Project-local | N/A | Project-owned | Yes | No | Built additively from Unity primitives | 2026-07-30 |

## Selection notes

- Primary direction: stylized low-poly, readable at mobile icon size, bold silhouettes.
- The Quaternius pack contains 106 FBX, OBJ, Blend, and PNG item assets.
- The source page explicitly allows personal and commercial use under CC0.
- Axe and Star are semantic alternatives for Split Container and Add Tile; they
  are preview candidates, not approved final replacements.
- Option B replaces those two candidates with a project-owned hand saw and a
  stacked tile with a plus sign, improving semantic readability at icon size.
- Kenney's `cup-coffee` model is commercially usable under CC0 but was rejected
  visually because the plain cup body does not read as Coffee Time at icon size.
  Option B therefore retains the existing project coffee cup.
- Existing gameplay prefabs and SpriteItemSO references remain unchanged until
  visual approval.

## Import notes

- Unity 6000.4 URP.
- Imported FBX target size: 1.5 Unity units.
- No rig or animation.
- Original source files stay under the `original` folder.
- Option B preview geometry is additive under `Generated/Prefabs`; it does not
  replace or modify a runtime power-up prefab.
- Runtime icons under `RuntimeIcons/` are transparent 640 px sprite renders.
  Gameplay skill icons are alpha-cropped for the approximately 79 px square
  slot, with a subtle cool rim for contrast on dark UI backgrounds.
- The six primary `SpriteItemSO` assets use the Option B runtime icons. The four
  skill tutorial/preview `SpriteItemSO` assets share the corresponding gameplay
  sprites so the visual language stays consistent.
