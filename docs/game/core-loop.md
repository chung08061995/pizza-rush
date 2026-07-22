# Core loop

1. Init shows loading and bootstraps Main.
2. Player starts a level and optionally selects boosters.
3. LevelRunner loads JSON and spawns a grid, containers, and production lines.
4. Player drags containers onto valid grid positions.
5. Production pieces travel along line places and are consumed by matching containers.
6. The state machine reaches Win or Lose; the player can retry, quit, or continue.

## 3D visual invariants

- Board cell occupancy and container shape positions remain data-driven.
- Rail direction, place order, and production color remain gameplay data.
- Ice remains a material/state overlay, not a new gameplay type.
