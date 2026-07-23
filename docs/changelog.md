# Changelog

## 2026-07-23

- Added the active Google Play `no_ads` one-time product at USD 1.99.
- Connected the No Ads purchase popup to Unity IAP and persisted/restored the
  entitlement so banner, interstitial, and rewarded ads remain disabled after
  purchase.
- Generated `GooglePlayTangle` from the Play Console licensing key and added
  Android receipt validation before granting the No Ads entitlement.
- Removed No Ads buttons and the `no_ads` Shop product after ownership is granted
  or restored.
- Added persisted Music/Vibrate toggle synchronization across Main and Settings UI.
- Added Android/iOS vibration feedback for toggle confirmation, pizza placement,
  container completion, win, and lose events.
- Added the PR3D Blender master scene, reproducible setup script, portrait
  preview, Unity import probe, and asset manifest.
- Verified meter scale, root pivot, Y-up, and +Z-forward import through
  Blender MCP and Unity MCP without changing gameplay contracts.

## 2026-07-22

- Added durable project documentation and Forge Plan Sync conventions.
- Added the Level 301 3D pizza-factory vertical-slice plan.
- Recorded preservation of gameplay contracts as the art-pipeline decision.
