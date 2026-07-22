# Pizza Rush

Pizza Rush is a portrait casual puzzle game in Unity 6000.4.10f1 where players drag colored containers onto production lines under a countdown timer.

## Status

The project is in active UI/gameplay iteration. The current 3D reskin is being developed as a Level 301 vertical slice before rollout to the remaining level data.

## Quick start

- Open the project in Unity 6000.4.10f1 with the Android build profile.
- Use `MyMenu > StartGame` to run the boot flow; playing another scene directly skips initialization.
- Read [`AGENTS.md`](AGENTS.md) before changing gameplay, prefabs, level data, or generated assets.

## Repository map

- `Assets/_Projects/Scripts/` — gameplay, state machines, services, and data models.
- `Assets/_Projects/Prefabs/` — reusable gameplay/UI prefabs.
- `Assets/_Projects/Resources/LevelData/` — 320 JSON level definitions.
- `Assets/_Projects/Models/` and `Assets/_Projects/Materials/` — current 3D content.
- `docs/` — durable product, architecture, game, art-pipeline, decision, and Forge plan documentation.

## Canonical documentation

- [Product requirements](docs/product-requirements.md)
- [Architecture](docs/architecture.md)
- [Game loop and systems](docs/game/core-loop.md)
- [3D content pipeline](docs/technical/3d-art-pipeline.md)
- [Forge vertical-slice plan](docs/plans/001-pizza-3d-vertical-slice.md)

## Known limitations

- There is no Unity CLI build or automated gameplay test suite; visual verification happens in the Unity Editor.
- The Forge task importer watches unchecked tasks under `docs/plans/**/## Tasks` and requires unique bracketed IDs.
- 3D reskin rollout beyond Level 301 is intentionally deferred until the vertical slice review.
