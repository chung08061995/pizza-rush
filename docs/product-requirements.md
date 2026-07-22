# Product requirements

## Goal

Make Pizza Rush feel like a polished pizza-factory puzzle game while preserving the existing puzzle rules and progression. The first milestone is a visual vertical slice of Level 301 that is safe to compare against the current runtime.

## Users and stakeholders

- Players need a clear board, readable moving items, and a warm pizza identity on a portrait phone screen.
- Designers need the existing level authoring and JSON workflow to remain valid.
- Artists and AI agents need repeatable Blender/Unity asset contracts.
- Developers need Forge-importable tasks and reversible changes.

## Scope

In scope: Level 301 3D environment, board tray, tile surface, straight/curved rails, connectors, color gates, pizza variants, four Level 301 container shapes, Ice, reusable kitchen props, shared materials, documentation, and Forge Plan Sync tasks.

Non-goals: gameplay-rule, timer, drag, collider, enum, production-line data, or level-JSON redesign; rollout to all 320 levels before review; mandatory Meshy dependency.

## Acceptance criteria

- Level 301 retains 49 grid cells, 23 containers, 7 production lines, four shapes, Ice, and ten colors.
- The board and rails match the supplied pizza-factory concept while keeping the board and HUD readable.
- A complete Level 301 playthrough works with existing drag, Ice, production transfer, win/lose, timer, and booster behavior.
- Art remains within the mobile budgets documented in `docs/technical/3d-art-pipeline.md`.

## Assumptions and TBDs

- Target profile is the current Android portrait build; minimum device and final compression settings are TBD.
- The concept is a style/composition reference, not a pixel-perfect asset sheet.
- Forge is the task review/import surface; the repository remains the source of truth.
