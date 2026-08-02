# AGENTS.md

This file provides guidance to Codex (Codex.ai/code) when working with code in this repository.

## Project

Pizza Rush is a mobile (portrait) casual puzzle game built in **Unity 6000.4.10f1** with the **Universal Render Pipeline (URP)**. Players drag colored containers onto production lines against a countdown timer, using consumable skills and boosters. Monetization is via ads (AdMob / AppLovin MAX / Unity Ads) and IAP (Unity Purchasing).

Note: much of the inline documentation and comments are written in Vietnamese.

## Building and Running

There is no CLI build. Development happens inside the Unity Editor.

- **Play from the correct entry point:** Use the editor menu `MyMenu > StartGame` (defined in `Assets/_Projects/Scripts/Editor/ToolMenuEditor.cs`). This opens `Init.unity` and enters Play mode. Pressing Play on any other scene skips bootstrapping and will not run correctly.
- **Player builds:** `File > Build Settings` / `Build Profiles`, targeting Android or iOS.
- There is no unit test setup (no Unity Test Framework assemblies or `.asmdef` files); all scripts compile into the default `Assembly-CSharp`. The `.csproj` and `.sln`/`.slnx` files are Unity-generated — do not hand-edit them.

## Scene Flow

All gameplay code lives under `Assets/_Projects/`. Scenes load additively/singly via `SceneControllerSingleton`:

1. **Init** — `InitProgress.cs` bootstraps: shows loading popup, then loads Main after `ParametterGameConfigSO.InitDelay`.
2. **Main** — home/menu hub. `PopupMain` + `HomeContentsController` drive navigation between Home / Shop / Ranking / LevelUp / Setting content panels.
3. **LevelRunner** — the actual gameplay scene.
4. **LevelCreator** — an in-editor authoring scene for building levels (not shipped to players).

Scene names are constants in `GameConstain.SenceName`. Use `SceneControllerExtensions.LoadMain()` / `LoadGameplay()` to transition rather than calling `SceneManager` directly.

## Architecture

### DraftUtils dependency
The project depends on an external Git package `com.draft.unitydraftutils` (see `Packages/manifest.json`), namespaced `DraftUtils`. It provides the core building blocks used everywhere:
- `SingletonDontDestroyOnLoadMonoBehaviour<T>` — the singleton base for managers (`DataManager`, `PopupManager`, `SoundManager`, `SceneControllerSingleton`).
- `DraftMonoBehaviour` — the base MonoBehaviour most gameplay components derive from.
- `PersistentValue<T>` — an observable value backed by persistent storage (PlayerPrefs-style) with a `Notifier` for change listeners. This is the primary reactive/state pattern; UI views subscribe to these rather than polling.
- `StateMachine` / `StateNode` / `FuncPredicate` — the state machine primitives.
- `Pooler<T>`, `TimeCountdown`, `ComponentReference<T>`, `PopupFactory`, `FormattedLogger`.

### Central state: DataManager
`Assets/_Projects/Scripts/Common/DataManager/DataManager.cs` is the single source of truth for player state — level, gold, avatar, settings (music/sfx/vibrate), consumable inventory (`remainningItems` dictionary keyed by `ItemType`), lives/hearts state, and daily-challenge state. All persistent player data flows through its `PersistentValue<T>` fields. It also holds references to the game's ScriptableObject config (`ParametterGameConfigSO`, color/reward/sprite SOs). Read/write player state here rather than touching PlayerPrefs directly; the PlayerPrefs keys live in `GameConstain.PlayerPrefsKey`.

### UI: PopupManager
`PopupManager` (singleton) owns every popup as a `ComponentReference<T>` and instantiates them on demand via `PopupFactory` (`GetPopupX()` / `HidePopupX()` methods). Popups live under `Assets/_Projects/Scripts/UI/Popup*/`. To show UI, go through `PopupManager` rather than instantiating prefabs directly.

### State machines
Gameplay and level authoring are driven by `DraftUtils.StateMachine`. Each machine has a set of state classes plus matching `StateNode`s:
- **Gameplay** (`LevelRunner/StateMachine/`): `GameplayStateMachine` with states for dragging containers, using each skill (Destroy / AddTile / SplitTile / Freeze), Win, and Lose. `LevelRunner` orchestrates the timer, `LevelTracking`, and this machine.
- **Container** (`Container/StateMachine/`): movement states (move-to-position, fly-away).
- **Level authoring** (`LevelCreator/StateMachine/`): draw-background, create-elements, write-position states.
When adding gameplay behavior, add a state class + node and wire transitions in the relevant `*StateMachine.SetData(...)`, following the existing pattern.

### Data-driven design
- **Levels** are JSON files under `Assets/_Projects/Resources/LevelData/` named `0001.json`, `0002.json`, … (`GameConstain.StringFormats.LevelDataFileNameFormat`, zero-padded to 4 digits). Loaded via `Resources.Load` using `LevelDataPath`. `LevelCreator` authors and writes these; `LevelFactory` / `LevelObjectSpawner` build the runtime scene from a `LevelData`.
- **Config & content** are ScriptableObjects under `Assets/_Projects/ScriptableObjects/` (colors, rewards, sprite/audio/string item sets, `ParametterGameConfigSO` for tunable game parameters). Prefer adding tunables to a config SO over hardcoding.
- **`ItemType`** (`Models/Skill/ItemType.cs`) is the central enum tying together currencies, skills, boosters, avatars, and IAP products — inventory, rewards, and shop all key off it.

### Services (Ads / IAP)
Ads and IAP are behind interfaces with swappable implementations and stubs for editor testing:
- **Ads** (`Core/Ads/`): `IAdsService` with `AdMobService` / `AppLovinMAXService` / `UnityAdsService` impls and a `StubAdsService`. `AdsManager` selects by `AdSDKType`; config in `AdConfigSO`.
- **IAP** (`Core/Iap/`): `IIAPService` (`UnityIAPService` / `StubIAPService`) and `IIAPProductProvider` (`CoffeeRunIAPProductProvider`), coordinated by `IAPManager`.

## Key Libraries
- **Odin Inspector (Sirenix)** — `[Button]`, `[ShowInInspector]`, `[ReadOnly]` used heavily for editor tooling; keep using it for inspector-exposed debug/actions.
- **DOTween / DOTween Pro** — all tweening/animation.
- **FancyScrollView** and **EnhancedScroller v2** — scroll lists (rankings, level-up, shop). Custom cells under `Core/Implement*`.
- **Unity Input System** — actions in `Assets/InputSystem_Actions.inputactions`.

## Conventions
- New gameplay scripts go under `Assets/_Projects/Scripts/<Feature>/`. `Assets/_Recovery/` and root `Assets/` sample/plugin folders are not game code.
- Managers are `DontDestroyOnLoad` singletons accessed via `.Instance`; don't create parallel instances.
- Reactive UI: subscribe views to `PersistentValue.Notifier` change events instead of per-frame polling.

## Project memory and Forge plans

- Treat `README.md`, `docs/`, and this file as durable project memory; update the relevant document when behavior, architecture, art contracts, or delivery workflow changes.
- Forge Desktop v1.0.19 scans `docs/plans/**/*.md`. Only unchecked tasks under an exact `## Tasks` heading are imported. Every task must use a unique ID such as `- [ ] [PR3D-001] Title`.
- Keep task descriptions indented beneath their task line. Acceptance checkboxes belong under `## Acceptance criteria`, not under `## Tasks`.
- When dependent steps touch overlapping files or assets, define one parent with `Mode: sequential` and numbered items below `Subtasks:`. Forge must run them in one attempt/worktree/branch; do not create separate top-level cards for the same steps.
- When independent steps have disjoint ownership, use `Mode: hybrid` and a positive `Phase:` per sub-task. Tasks in the same phase run via sub-agents in parallel; phases run in order. State exclusive file/asset ownership in each parallel sub-task.
- Do not expose secrets, API keys, keystores, or local absolute paths in Markdown.

## 3D vertical slice rules

- The visual experiment targets Level 301 and must preserve its level JSON, grid, collider, drag behavior, production-line places, entry/exit transforms, timer, and serialized prefab contracts.
- Additive visual children/materials are preferred. Do not change `LevelData`, container/production enums, or the 320 JSON files for an art-only task.
- Blender uses meters, Y-up, and forward Z. Unity imports with the existing project scale; every exported asset needs a named root, stable pivot, applied transforms, UVs, and documented material/texture dependencies.
- Before accepting a 3D change, run `MyMenu > StartGame`, test Level 301 in the Game view, inspect three portrait aspect ratios, and check the Unity Console for errors.
- Keep generated Blender/AI assets in a clearly named art folder with a manifest. Do not replace existing models destructively until the vertical-slice review is accepted.

## Agent tool-call guardrails

- `view_image` accepts only `detail: "high"` or `detail: "original"`; omit `detail` for the default. Never send `detail: "low"` or `detail: "auto"`.
- The concept reference is `docs/reference/pizza-factory-concept.png`. When an agent needs to inspect it, use `view_image` with `detail: "high"` or `"original"`.
