# Pizza Rush Game Completion Tracker

Muc tieu: hoan thanh game truoc, store upload sau. Moi muc player-facing lon chi xoa khi da duoc owner chot trong phien play review.

## 1. Play Review Decisions

Status key: `Keep`, `Remove`, `Defer`, `Review`.

| Area | Current Surface | Decision | Notes |
| --- | --- | --- | --- |
| Main navigation | Home | Review | Core entry to gameplay. |
| Main navigation | Shop | Review | IAP and economy surface. |
| Main navigation | Ranking | Review | Candidate for removal if no backend/live ranking. |
| Main navigation | LevelUp | Review | Currently reached through screenshot nav button in `PopupMain`. |
| Main navigation | Setting | Review | Keep core toggles; review gift code/contact/privacy/restore. |
| Home | Daily Challenge | Review | Candidate for removal if release needs simpler loop. |
| Home | Starter Detail | Review | Depends on kept IAP package list. |
| Home | No Ads Detail | Review | Depends on kept IAP package list. |
| Home | Select Booster before play | Review | Candidate for simplification. |
| Gameplay | Freeze Time skill | Review | Skill inventory and tutorial surface. |
| Gameplay | Split Container skill | Review | Skill inventory and tutorial surface. |
| Gameplay | Destroy Container skill | Review | Skill inventory and tutorial surface. |
| Gameplay | Add Tile skill | Review | Skill inventory and tutorial surface. |
| Gameplay | Play On after lose | Review | Uses rewarded ad or `Booter_PlayOn`. |
| Economy | Lives/hearts | Review | More Lives popup and unlimited hearts state. |
| Economy | Gold | Review | Required if shop/skills remain paid by gold. |
| Meta | Avatar/profile | Review | Candidate for removal if no store value. |
| Meta | Gift Code | Review | Candidate for removal unless backend/code system exists. |
| Meta | Level Up rewards | Review | Candidate for defer if progression should be simple. |

## 2. IAP Product Audit

Current code source: `Assets/_Projects/Scripts/Models/IAP/IAPData.cs`.
Runtime service: `Assets/_Projects/Scripts/Core/Iap/UnityIAPService.cs` now uses Unity Purchasing v5 `StoreController`, pending order confirmation, product fetch events, restore transactions, and localized price lookup.

| Product | Product ID | Type | Decision | Notes |
| --- | --- | --- | --- | --- |
| SingleIAPData_1_000_Coin | test.1000gold | Consumable | Review | Coin pack. |
| SingleIAPData_5_000_Coin | test.5000gold | Consumable | Review | Coin pack. |
| SingleIAPData_10_000_Coin | test.10000gold | Consumable | Review | Coin pack. |
| SingleIAPData_25_000_Coin | test.25000gold | Consumable | Review | Coin pack. |
| SingleIAPData_50_000_Coin | test.50000gold | Consumable | Review | Coin pack. |
| SingleIAPData_100_000_Coin | test.100000gold | Consumable | Review | Coin pack. |
| MultipleIAPData_NoAds | test.noads | Non-consumable | Review | Removes ads features. |
| MultipleIAPData_NoAdsBundle | test.noadsbundle | Non-consumable | Review | No ads plus skills/gold. |
| MultipleIAPData_SmallBundle | test.smallbundle | Consumable | Review | Boosters, skills, gold. |
| MultipleIAPData_MediumBundle | test.mediumbundle | Consumable | Review | Boosters, skills, gold. |
| MultipleIAPData_LargeBundle | test.largebundle | Consumable | Review | Boosters, skills, gold. |
| MultipleIAPData_Starter | test.starter | Consumable | Review | Starter bundle. |

Removal checklist after owner approval:
- Remove product from `IAPData.productIds`.
- Remove data field/list entry from shop generation and `CoffeeRunIAPProductProvider`.
- Remove UI detail popup entry if product-specific.
- Remove icon/name/description ScriptableObject references if no longer used.
- Compile Unity and smoke test shop.

Store setup still needed:
- Replace `test.*` product IDs with final Google Play Console / App Store Connect IDs.
- Confirm which coin packs and bundles stay in release v1.
- Test purchase, cancel/fail, restore, and pending redelivery on device.

## 3. Core Gameplay Release Scope

Keep until explicitly changed:
- Boot through `MyMenu > StartGame`.
- Main to LevelRunner flow.
- Drag container gameplay.
- Timer, win, lose, retry/next.
- Level data `0001.json` to `0006.json`.
- Basic rewards and persistent progress through `DataManager`.

Review after play session:
- Number of skills kept in release v1.
- Whether boosters appear before level start.
- Whether lives/hearts are needed for v1.
- Whether ranking, daily challenge, avatar/profile, level-up rewards remain.

## 4. 3D Art Pipeline

Tool: Blender MCP.

Priority assets:
1. Container.
2. Production line.
3. Pizza/tile/item.
4. Gameplay background/environment props.
5. Win/lose presentation props if needed for screenshots.

Acceptance:
- Unity-friendly export, preferably FBX or glTF.
- Correct pivot and scale for existing prefabs.
- Materials readable in URP.
- Works in portrait camera framing.

## 5. 2D Art Replacement Pipeline

Waiting on owner-provided 2D art pack.

Replacement targets:
- Sprite atlas under `Assets/_Projects/SpriteAtlas`.
- UI sprites under `Assets/_Projects/Images`.
- Item icons under `Assets/_Projects/ScriptableObjects/ItemType/Icon`.
- Skill preview icons under `Assets/_Projects/ScriptableObjects/ItemType/Skill/PreviewIcon`.
- Store/shop art under `Assets/_Projects/Images/Store` and IAP prefabs.

Use Unity MCP if available; otherwise require Unity Editor connection before mass prefab/reference edits.

## 6. Firebase Integration Scope

Firebase package not present yet. Planned minimum:
- Firebase Analytics.
- Firebase Crashlytics.

Required config before implementation:
- Android `google-services.json`.
- iOS `GoogleService-Info.plist`.
- Final Android package name and iOS bundle ID.

Event list:
- `app_start`
- `level_start`
- `level_win`
- `level_lose`
- `level_retry`
- `booster_use`
- `skill_use`
- `rewarded_ad_show`
- `rewarded_ad_complete`
- `iap_purchase_success`
- `iap_purchase_fail`

Implementation target:
- Add one analytics facade so gameplay/UI code does not depend directly on Firebase SDK.
- Route IAP and ads callbacks through the facade.
- Keep no-op behavior in Editor if Firebase is unavailable.

## 7. Polish And Store Screenshots

Polish pass:
- Remove debug/dev-only UI.
- Fix text overflow in portrait.
- Improve button states and animation feedback.
- Verify sound/music toggles.
- Verify no placeholder art remains.

Screenshot pass:
- Capture after final art, Firebase, and IAP are stable.
- Use portrait shots only.
- Avoid debug UI, placeholder sprites, and removed features.
