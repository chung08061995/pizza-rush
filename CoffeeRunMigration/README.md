# Coffee Run → Pizza Rush level migration

This folder contains the repository-safe part of the 100-level migration. APKs, IL2CPP metadata, extracted Unity assets, screenshots and recordings are excluded by `.gitignore`.

## Implemented

- Source fingerprint for Coffee Run 3.20.0 (790), including SHA-256 for base and ARM64 split APK.
- IL2CPP source schema and active runtime variant (`Default`) documentation.
- Editor-only normalized models, strict converter, validator and comparison manifest.
- Deterministic `Straight`, `CurvedLeft` and `CurvedRight` production-line visual mapping. Converted levels cannot use random selection.
- AssetRipper importer for the decrypted `Default` Unity bundle, including the IL2CPP-recovered shape/anchor table and deterministic door-path visuals.
- Exact normalized and target JSON for Level 1–100 (`Exact=100`, `Mismatch=0`, `Unsupported=0`).
- Unity MCP runtime smoke-capture for Level 1–100 at 1080×1920; every level loaded its expected index without gameplay exceptions.
- Runtime mechanics for directional blocks, ice, LayerBox, two-color slot quotas and stone obstacles.
- A 14-key Pizza Rush palette so source color relationships remain one-to-one in high-color levels.
- A 100-level progress tracker and a local-only ADB capture helper.

## Unity workflow

1. Set `COFFEE_RUN_LEVEL_ASSET_DIR` to AssetRipper's local `encryptedasset/levels` folder.
2. Run `MyMenu > Coffee Run > Import AssetRipper level assets`.
3. Run `MyMenu > Coffee Run > Convert normalized levels`.
4. Inspect `CoffeeRunMigration/Reports/conversion-manifest.json` and run the 1–100 runtime audit.
5. Run the game using `MyMenu > StartGame` only.

The converter preserves an existing Pizza Rush `goldReward` (or uses 50 for a new level). It writes the destination only after every source field is mapped and validation returns `Exact`. Unsupported shapes, modifiers, custom production paths or visual keys return `Unsupported` and leave the destination untouched.

## Clone acceptance

Structured extraction, conversion and runtime start-state capture are complete.
Under the current clone-only scope, gameplay replay, Win/Lose checks, source
screenshot overlays and source-only Special stages are not approval gates. All
100 normal levels are accepted because conversion is `Exact`, production-line
visual selection is deterministic, and every level has a valid portrait
start-state capture. Run `python3 CoffeeRunMigration/Tools/audit_clone.py` to
recheck these gates.

APKs, decrypted bundles and AssetRipper output remain local-only and must not be
committed.

## Batch gates

| Batch | Levels | Gate |
|---|---:|---|
| Regression | 1–6 | Exact + valid start-state capture; re-run after runtime mechanic changes |
| Batch 2 | 7–20 | Exact + valid start-state capture |
| Batch 3 | 21–40 | Exact + valid start-state capture |
| Batch 4 | 41–60 | Exact + valid start-state capture |
| Batch 5 | 61–80 | Exact + valid start-state capture |
| Batch 6 | 81–100 | Exact + valid start-state capture |

The converter fails closed for unknown shapes, visual keys and modifiers. Current Level 1–100 source data uses only the mechanics now represented by the runtime and validator.
