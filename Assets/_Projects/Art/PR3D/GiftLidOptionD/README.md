# Pizza gift lid — Option D

Production decoration for completed pizza-container covers.

## Visual contract

- Keeps every existing cover prefab root, pivot, shell mesh, collider setup, and fly-away tween unchanged.
- Adds a burgundy ribbon with a warm-ivory center stripe and a gold low-poly bow.
- Ribbon geometry follows the occupied shape for straight, square, L, T, and plus containers.
- Added renderers do not cast or receive shadows.
- The decoration contains no colliders or scripts.

## Asset provenance

- Bow source: `present-a-rectangle.fbx` from Kenney Holiday Kit 2.0.
- Creator page: https://www.kenney.nl/assets/holiday-kit
- License: Creative Commons CC0; the original license is preserved under `Source/`.
- The production `KenneyBow.asset` contains only the bow geometry extracted from the source lid mesh. Its pivot is normalized to the bottom of the bow for stable placement on the ribbon.

## Unity normalization

- Unity version: 6000.4.10f1
- Render pipeline: URP
- Source convention: meters, Y-up
- Runtime decoration is a visual child of each existing cover prefab.
