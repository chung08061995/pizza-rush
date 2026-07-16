# Coffee Run 3.20.0 level schema

The schema was recovered from ARM64 IL2CPP metadata. The APK contains an encrypted UnityFS bundle at `assets/EncryptedAsset/encrypted.encr`; after local decryption, AssetRipper exported the main `LevelConfig` plus versioned/tutorial/special variants. Runtime prefs and logs select `Default`. Extracted binaries, decryption material and Unity assets remain outside the repository.

## Source classes

- `LevelConfig`: `List<LevelSO> listLevelData`
- `LevelSO`: `level`, `grid`, `blocks`, `doors`, `time`, hard-level flags, solution steps and camera fields
- `Grid`: `row`, `col`, `strGrid`
- `Block`: tile count, position, `Direction`, `ShapeTypes`, ordered colors and `BlockSpecial`
- `BlockSpecial`: movement axis, ice, stone, color layer, key, cap, bomb, linked, barrier, ropes and boom
- `Door`: position, direction, ordered `PairColors`, path segments, locks, ice, caps and directional lock
- `PairColors`: color and quantity
- `PairDirections`: direction and length
- `LevelSolution`: ordered solution steps (`StepRecordInfo[]` on `LevelSO`)

## Source enums

- Direction: `Right=0`, `Left=1`, `Up=2`, `Down=3`
- Shape: `One=0`, `Two=1`, `Three=2`, `Square=3`, `LShort=4`, `T=5`, `L=6`, `LLeft=7`, `Cross=8`, `Z=9`, `ZReverse=10`, `U=11`

## Normalized contract

Normalized records live in `CoffeeRunMigration/Normalized/NNNN.json`. They carry source fingerprint, config variant, extraction method, explicit grid cells, timer, container shape/rotation/flip/movement/material/modifiers, ordered container colors and per-color capacity, production visual key/path and ordered production quantities. Coordinates are transformed into Pizza Rush grid space by the importer.

`strGrid=0` is excluded from the board; both `1` and `2` are playable cells. Native `GetFilledTilePos*` offsets are mapped to Pizza Rush shape/rotation/flip with anchor correction. Door paths map to `Straight` when all segments retain the root direction. Offset paths preserve turn chirality as `CurvedLeft` or `CurvedRight`; Pizza Rush mirrors the curved prefab for the left variant.

Within Level 1–100, a LayerBox gives each ordered color a full container capacity, a multi-color block divides its slots according to Coffee Run's shape-specific distribution, and a stone block contributes no production demand and cannot move.

Accepted extraction methods are `Il2CppAsset` and `AdbVisualFallback`. Unknown mechanics or visual keys produce `Unsupported`; the converter never guesses a substitute.
