# Pizza item model candidates

These assets are an uncommitted visual experiment for Level 301. Existing level JSON, gameplay colliders, production-line contracts, and container logic are unchanged.

| Candidate | Source | License | Intended option |
| --- | --- | --- | --- |
| Quaternius Pizza Slice | [Poly Pizza model](https://poly.pizza/m/CA4HtaaMJn) / [GLB](https://static.poly.pizza/b76298f9-9ff1-48ba-a000-c966ced03e90.glb) | Public Domain / CC0 | Individual conveyor item; strongest silhouette at small gameplay scale |
| Quaternius Pizza | [Poly Pizza model](https://poly.pizza/m/XmmG0uImLL) / [GLB](https://static.poly.pizza/39c42afe-d7d8-42d1-870f-175a479bde0c.glb) | Public Domain / CC0 | Whole pizza item; compare against slice for production-line readability |
| Kenney Food Kit (alternative) | [Kenney asset page](https://www.kenney.nl/assets/food-kit) | CC0 | Backup source if the Quaternius models do not read clearly in gameplay |
| Kenney Pizza | [Food Kit](https://www.kenney.nl/assets/food-kit) / [OpenGameArt mirror](https://opengameart.org/content/food-kit) | CC0 (credit optional) | Selected production-line item; FBX plus external colormap |

## Import notes

- Project: Unity 6000.4.10f1.
- Selected production asset: `Source/Kenney/FBX/pizza.fbx` with `Source/Kenney/FBX/Textures/colormap.png`; Unity's built-in FBX importer preserves the palette correctly.
- Source files are kept immutable under `Source/`. The earlier GLB/Quaternius experiments are not part of the production commit.
- No animation is required. The preview should use additive model children and preserve the existing gameplay prefab contracts.
- `Models/PizzaDepthVisual.mesh` is a deterministic Unity-cylinder mesh with three submeshes for contact shadow, raised crust, and cheese body. `KenneyProductionPizza.prefab` renders it through one additive visual child with shared URP materials; it has no collider and does not change the production pool contract.
- The candidate models were checked on 2026-08-01. The Fab marketplace pizza packs were not imported because their license terms were not sufficiently verifiable for this experiment.
