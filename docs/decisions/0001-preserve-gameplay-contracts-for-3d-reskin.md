# 0001 — Preserve gameplay contracts for the 3D reskin

Status: Accepted
Date: 2026-07-22

## Context

The pizza-factory concept changes board, rails, gates, pizza pieces, and environment, while the existing LevelRunner already supports Level 301 topology and behaviors.

## Decision

Implement the first slice as an additive visual reskin. Preserve level JSON, enums, grid sizes, colliders, drag behavior, production places, entry/exit transforms, timers, and serialized prefab contracts.

## Alternatives considered

- Redesign ray/board data now: rejected because it increases gameplay risk before art direction is validated.
- Replace every model across all levels: rejected until Level 301 review.

## Consequences

The first asset kit must fit current anchors and pivots. Some concept geometry is represented as visual children/materials instead of changing gameplay topology.
