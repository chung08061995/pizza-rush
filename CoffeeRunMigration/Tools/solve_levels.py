#!/usr/bin/env python3
"""Find no-skill drag solutions for converted Coffee Run levels.

The model mirrors DragContainerState at grid level: a drag may end at any anchor
reachable by the container through free cells, then the first compatible adjacent
production line releases its consecutive head colour.  The generated plan is an
editor/test artifact; runtime remains independent from this script.
"""

from __future__ import annotations

import argparse
import functools
import heapq
import json
import random
from dataclasses import dataclass, replace
from pathlib import Path
from typing import Iterable, NamedTuple


SHAPES = {
    100: ((0, 0),),
    101: ((0, 0), (0, 1)),
    102: ((0, 0), (0, 1), (0, 2)),
    200: ((0, 0), (0, 1), (1, 1)),
    201: ((0, 0), (0, 1), (0, 2), (1, 2)),
    300: ((0, 0), (1, 0), (0, 1), (1, 1)),
    400: ((0, 0), (0, 1), (-1, 1), (1, 1)),
    500: ((0, 0), (0, 1), (-1, 1), (1, 1), (0, 2)),
}

# Names follow the project enum values. Horizontal intentionally moves on grid Y
# and Vertical on grid X, matching ContainerMovementTypeExtensions.
DIRECTIONS = {
    100: ((0, 1), (0, -1), (-1, 0), (1, 0)),
    200: ((0, 1), (0, -1)),
    300: ((-1, 0), (1, 0)),
    400: (),
}


def rotate(point: tuple[int, int], rotation: int) -> tuple[int, int]:
    """Apply RotationType's clockwise quarter turns (Rotate_0 is enum value 1)."""
    x, y = point
    turns = max(0, rotation - 1) % 4
    for _ in range(turns):
        x, y = -y, x
    return x, y


def transformed_parts(shape: int, rotation: int, flip_x: bool) -> tuple[tuple[int, int], ...]:
    points = []
    for x, y in SHAPES[shape]:
        if flip_x:
            x = -x
        points.append(rotate((x, y), rotation))
    return tuple(points)


def pos(value: dict) -> tuple[int, int]:
    return int(value["x"]), int(value["y"])


@dataclass(frozen=True)
class ContainerSpec:
    parts: tuple[tuple[int, int], ...]
    rotation: int
    flip_x: bool
    movement: int
    material: int
    stone: bool
    colours: tuple[int, ...]
    quotas: tuple[int, ...]
    layered: bool
    multi: bool
    ice_amount: int
    inner: dict | None


@dataclass(frozen=True)
class ContainerState:
    anchor: tuple[int, int]
    active: bool = True
    layer: int = 0
    fills: tuple[int, ...] = ()
    thawed: bool = False


@dataclass(frozen=True)
class State:
    containers: tuple[ContainerState, ...]
    lines: tuple[tuple[int, ...], ...]
    resolved: int = 0


class Action(NamedTuple):
    container: int
    anchor: tuple[int, int]
    fed_line: int
    fed_colour: int
    fed_amount: int


class LevelModel:
    def __init__(self, data: dict):
        self.level = int(data["levelIndex"])
        self.grid = frozenset(pos(p) for p in data["gridPositions"])
        self.specs: list[ContainerSpec] = []
        states: list[ContainerState] = []
        for raw in data["containers"]:
            spec = self._make_spec(raw["containerData"], raw["rotationType"], raw.get("flipX", False))
            self.specs.append(spec)
            states.append(ContainerState(pos(raw["position"]), fills=(0,) * len(spec.colours)))

        lines: list[tuple[int, ...]] = []
        intakes: list[tuple[int, int]] = []
        for raw in data["productionLines"]:
            queue = []
            for group in raw["productionCollections"]:
                queue.extend([int(group["colorType"])] * int(group["Amount"]))
            lines.append(tuple(queue))
            base = pos(raw["position"])
            offset = rotate((0, -1), int(raw["rotationType"]))
            intakes.append((base[0] + offset[0], base[1] + offset[1]))
        self.intakes = tuple(intakes)
        self.initial = self._apply_thaws(State(tuple(states), tuple(lines), 0))

    def _make_spec(self, raw: dict, rotation: int, flip_x: bool) -> ContainerSpec:
        colour = raw.get("containerColorData") or {}
        colours = tuple(int(x) for x in (colour.get("colors") or [colour.get("colorType", 0)]))
        quotas_raw = colour.get("colorAmounts") or []
        cell_capacity = len(SHAPES[int(raw["containerShapeType"])]) * 4
        if quotas_raw and len(quotas_raw) >= len(colours):
            quotas = tuple(int(x) for x in quotas_raw[: len(colours)])
        elif colour.get("isMultiColor"):
            each = cell_capacity // max(1, len(colours))
            quotas = tuple(each for _ in colours)
        else:
            quotas = tuple(cell_capacity for _ in colours)
        ice = raw.get("containerIceData") or {}
        return ContainerSpec(
            parts=transformed_parts(int(raw["containerShapeType"]), rotation, flip_x),
            rotation=int(rotation),
            flip_x=bool(flip_x),
            movement=int(raw.get("containerMovementType", 100)),
            material=int(raw.get("containerMaterialType", 100)),
            stone=bool(raw.get("isStone", False)),
            colours=colours,
            quotas=quotas,
            layered=bool(colour.get("isLayerBox", False)),
            multi=bool(colour.get("isMultiColor", False)),
            ice_amount=int(ice.get("iceAmount", 0)),
            inner=ice.get("innerContainerData"),
        )

    def spec_for(self, index: int, state: ContainerState) -> ContainerSpec:
        outer = self.specs[index]
        if state.thawed and outer.inner:
            return self._make_spec(outer.inner, outer.rotation, outer.flip_x)
        return outer

    def _apply_thaws(self, state: State) -> State:
        containers = list(state.containers)
        changed = False
        for i, current in enumerate(containers):
            spec = self.specs[i]
            if current.active and not current.thawed and spec.material == 300 and spec.inner and state.resolved >= spec.ice_amount:
                inner = self._make_spec(spec.inner, spec.rotation, spec.flip_x)
                containers[i] = replace(current, thawed=True, layer=0, fills=(0,) * len(inner.colours))
                changed = True
        return replace(state, containers=tuple(containers)) if changed else state

    @functools.lru_cache(maxsize=1_000_000)
    def cells(self, index: int, state: ContainerState) -> frozenset[tuple[int, int]]:
        spec = self.spec_for(index, state)
        ax, ay = state.anchor
        return frozenset((ax + x, ay + y) for x, y in spec.parts)

    def movable(self, index: int, state: ContainerState) -> bool:
        spec = self.spec_for(index, state)
        return state.active and spec.material != 300 and spec.movement != 400

    @functools.lru_cache(maxsize=500_000)
    def reachable(self, whole: State, index: int) -> frozenset[tuple[int, int]]:
        current = whole.containers[index]
        spec = self.spec_for(index, current)
        occupied: set[tuple[int, int]] = set()
        for j, other in enumerate(whole.containers):
            if j != index and other.active:
                occupied.update(self.cells(j, other))
        available = self.grid - occupied

        def valid(anchor: tuple[int, int]) -> bool:
            ax, ay = anchor
            return all((ax + dx, ay + dy) in available for dx, dy in spec.parts)

        if not valid(current.anchor):
            return frozenset()
        seen = {current.anchor}
        queue = [current.anchor]
        directions = DIRECTIONS.get(spec.movement, DIRECTIONS[100])
        for anchor in queue:
            for dx, dy in directions:
                nxt = anchor[0] + dx, anchor[1] + dy
                if nxt not in seen and valid(nxt):
                    seen.add(nxt)
                    queue.append(nxt)
        return frozenset(seen)

    def accepted(self, spec: ContainerSpec, current: ContainerState, colour: int) -> tuple[int, int] | None:
        # Stone boxes are movable blockers, not production receivers. Their
        # serialized color/shape describes the covered box but must not satisfy
        # production capacity until a supported stone-removal mechanic says so.
        if spec.stone:
            return None
        if spec.layered:
            slot = current.layer
            if slot < len(spec.colours) and spec.colours[slot] == colour:
                return slot, max(0, spec.quotas[slot] - current.fills[slot])
            return None
        if spec.multi:
            if colour in spec.colours:
                slot = spec.colours.index(colour)
                return slot, max(0, spec.quotas[slot] - current.fills[slot])
            return None
        if spec.colours and spec.colours[0] == colour:
            return 0, max(0, spec.quotas[0] - current.fills[0])
        return None

    def transition(self, state: State, index: int, anchor: tuple[int, int]) -> tuple[State, Action]:
        containers = list(state.containers)
        current = replace(containers[index], anchor=anchor)
        containers[index] = current
        lines = list(state.lines)
        spec = self.spec_for(index, current)
        occupied = self.cells(index, current)
        fed_line = -1
        fed_colour = 0
        fed_amount = 0
        resolved_delta = 0

        # TryGetProductionNearAndSamleColor returns the first compatible line.
        for line_index, queue in enumerate(lines):
            if not queue or self.intakes[line_index] not in occupied:
                continue
            accepted = self.accepted(spec, current, queue[0])
            if not accepted or accepted[1] <= 0:
                continue
            slot, room = accepted
            colour = queue[0]
            run = 0
            while run < len(queue) and queue[run] == colour:
                run += 1
            amount = min(room, run)
            fills = list(current.fills)
            fills[slot] += amount
            current = replace(current, fills=tuple(fills))
            containers[index] = current
            lines[line_index] = queue[amount:]
            fed_line, fed_colour, fed_amount = line_index, colour, amount

            full = False
            if spec.layered:
                full = current.fills[current.layer] >= spec.quotas[current.layer]
                if full and current.layer + 1 < len(spec.colours):
                    current = replace(current, layer=current.layer + 1, fills=(0,) * len(spec.colours))
                    containers[index] = current
                    resolved_delta += 1
                    full = False
            elif spec.multi:
                full = all(current.fills[i] >= spec.quotas[i] for i in range(len(spec.colours)))
            else:
                full = current.fills[0] >= spec.quotas[0]
            if full:
                containers[index] = replace(current, active=False)
                resolved_delta += 1
            break

        resolved = state.resolved + resolved_delta
        next_state = self._apply_thaws(State(tuple(containers), tuple(lines), resolved))
        return next_state, Action(index, anchor, fed_line, fed_colour, fed_amount)

    def actions(self, state: State) -> Iterable[tuple[State, Action]]:
        fed: list[tuple[State, Action]] = []
        moves: list[tuple[State, Action]] = []
        for i, current in enumerate(state.containers):
            if not self.movable(i, current):
                continue
            for anchor in self.reachable(state, i):
                nxt, action = self.transition(state, i, anchor)
                if nxt == state:
                    continue
                if action.fed_amount:
                    fed.append((nxt, action))
                elif anchor != current.anchor:
                    moves.append((nxt, action))
        # Product consumption is monotonic and strongly preferred, but blocker moves
        # remain available so levels such as 3 can open a route first.
        yield from fed
        yield from moves

    def feed_actions(self, state: State) -> list[tuple[State, Action]]:
        """Return only reachable drops that consume production.

        Deriving candidate anchors from intake/part pairs avoids enumerating every
        empty anchor on large boards when the next productive drag is available.
        """
        result: list[tuple[State, Action]] = []
        for i, current in enumerate(state.containers):
            if not self.movable(i, current):
                continue
            spec = self.spec_for(i, current)
            reachable = self.reachable(state, i)
            candidates: set[tuple[int, int]] = set()
            for line_index, queue in enumerate(state.lines):
                if not queue or not self.accepted(spec, current, queue[0]):
                    continue
                ix, iy = self.intakes[line_index]
                for dx, dy in spec.parts:
                    candidates.add((ix - dx, iy - dy))
            for anchor in candidates & reachable:
                nxt, action = self.transition(state, i, anchor)
                if action.fed_amount:
                    result.append((nxt, action))
        return result

    @functools.lru_cache(maxsize=500_000)
    def has_feed(self, state: State) -> bool:
        for i, current in enumerate(state.containers):
            if not self.movable(i, current):
                continue
            spec = self.spec_for(i, current)
            reachable = self.reachable(state, i)
            for line_index, queue in enumerate(state.lines):
                if not queue or not self.accepted(spec, current, queue[0]):
                    continue
                ix, iy = self.intakes[line_index]
                for dx, dy in spec.parts:
                    if (ix - dx, iy - dy) in reachable:
                        return True
        return False

    @functools.lru_cache(maxsize=500_000)
    def goal_blockers(self, state: State) -> frozenset[int]:
        """Find containers directly preventing a compatible box reaching a line."""
        occupied_by: dict[tuple[int, int], int] = {}
        for j, other in enumerate(state.containers):
            if other.active:
                for cell in self.cells(j, other):
                    occupied_by[cell] = j
        blockers: set[int] = set()
        for i, current in enumerate(state.containers):
            if not self.movable(i, current):
                continue
            spec = self.spec_for(i, current)
            reachable = self.reachable(state, i)
            directions = DIRECTIONS.get(spec.movement, DIRECTIONS[100])
            for line_index, queue in enumerate(state.lines):
                if not queue or not self.accepted(spec, current, queue[0]):
                    continue
                ix, iy = self.intakes[line_index]
                targets = {(ix - dx, iy - dy) for dx, dy in spec.parts}
                for target in targets:
                    tx, ty = target
                    target_cells = {(tx + dx, ty + dy) for dx, dy in spec.parts}
                    if not target_cells <= self.grid:
                        continue
                    for cell in target_cells:
                        owner = occupied_by.get(cell)
                        if owner is not None and owner != i:
                            blockers.add(owner)
                # If the mouth itself is free but disconnected, inspect the blocked
                # one-step frontier around all currently reachable anchors.
                for anchor in reachable:
                    for dx, dy in directions:
                        nxt = anchor[0] + dx, anchor[1] + dy
                        if nxt in reachable:
                            continue
                        nx, ny = nxt
                        placement = {(nx + px, ny + py) for px, py in spec.parts}
                        if not placement <= self.grid:
                            continue
                        for cell in placement:
                            owner = occupied_by.get(cell)
                            if owner is not None and owner != i:
                                blockers.add(owner)
        return frozenset(blockers)

    def focused_actions(self, state: State, limit: int = 40) -> list[tuple[State, Action]]:
        """Generate feeds, or a bounded set of moves that clear current goal routes."""
        feeds = self.feed_actions(state)
        if feeds:
            return feeds

        movable = [i for i, current in enumerate(state.containers) if self.movable(i, current)]
        blockers = [i for i in self.goal_blockers(state) if i in movable]
        candidates = blockers or movable
        ranked: list[tuple[tuple[int, int, int], State, Action]] = []
        intake_set = set(self.intakes)
        for i in candidates:
            current = state.containers[i]
            for anchor in self.reachable(state, i) - {current.anchor}:
                nxt, action = self.transition(state, i, anchor)
                immediate = 1 if self.has_feed(nxt) else 0
                cells = self.cells(i, nxt.containers[i])
                on_intake = len(cells & intake_set)
                parking_distance = min(
                    abs(anchor[0] - intake[0]) + abs(anchor[1] - intake[1])
                    for intake in self.intakes
                )
                ranked.append(((-immediate, on_intake, -parking_distance), nxt, action))
        ranked.sort(key=lambda item: item[0])
        return [(nxt, action) for _, nxt, action in ranked[:limit]]

    @staticmethod
    def solved(state: State) -> bool:
        return all(not line for line in state.lines)

    @staticmethod
    def remaining(state: State) -> int:
        return sum(len(line) for line in state.lines)

    @staticmethod
    def feed_lower_bound(state: State) -> int:
        runs = 0
        for line in state.lines:
            previous = None
            for colour in line:
                if colour != previous:
                    runs += 1
                    previous = colour
        return runs


def solve(model: LevelModel, max_nodes: int, beam: int) -> tuple[list[Action] | None, int]:
    initial = model.initial
    serial = 0
    # Cost prioritises consumed products first, then shorter plans and fewer pure moves.
    heap: list[tuple[tuple[int, int, int], int, State]] = [
        ((model.remaining(initial), 0, 0), serial, initial)
    ]
    best: dict[State, tuple[int, int]] = {initial: (0, 0)}
    parent: dict[State, tuple[State, Action]] = {}
    expanded = 0
    depth_buckets: dict[int, int] = {}

    while heap and expanded < max_nodes:
        (_, _, _), _, state = heapq.heappop(heap)
        depth, pure_moves = best[state]
        if model.solved(state):
            plan: list[Action] = []
            while state != initial:
                previous, action = parent[state]
                plan.append(action)
                state = previous
            plan.reverse()
            return plan, expanded
        expanded += 1
        for nxt, action in model.focused_actions(state):
            next_depth = depth + 1
            next_pure = pure_moves + (0 if action.fed_amount else 1)
            old = best.get(nxt)
            if old is not None and old <= (next_depth, next_pure):
                continue
            if beam > 0:
                used = depth_buckets.get(next_depth, 0)
                if used >= beam:
                    continue
                depth_buckets[next_depth] = used + 1
            best[nxt] = (next_depth, next_pure)
            parent[nxt] = (state, action)
            serial += 1
            priority = (model.remaining(nxt), next_pure, next_depth)
            heapq.heappush(heap, (priority, serial, nxt))
    return None, expanded


def solve_stochastic(
    model: LevelModel,
    trials: int,
    max_drags: int,
    seed: int,
) -> tuple[list[Action] | None, int]:
    """Low-memory greedy/random fallback for crowded, mostly open boards."""
    rng = random.Random(seed)
    attempts = 0
    best_remaining = model.remaining(model.initial)
    for trial in range(trials):
        if trial > 0 and trial % 10 == 0:
            LevelModel.cells.cache_clear()
            LevelModel.reachable.cache_clear()
            LevelModel.has_feed.cache_clear()
            LevelModel.goal_blockers.cache_clear()
        state = model.initial
        plan: list[Action] = []
        seen = {state}
        for _ in range(max_drags):
            attempts += 1
            if model.solved(state):
                return plan, attempts
            feeds = model.feed_actions(state)
            if feeds:
                # Prefer large releases/removals, with seeded tie variation so a
                # restart can choose a different colour/line ordering. Some layouts
                # require a smaller available feed first to avoid parking a partially
                # filled box in another route, so retain exploration across all feeds.
                largest = max(action.fed_amount for _, action in feeds)
                preferred = [item for item in feeds if item[1].fed_amount == largest]
                pool = preferred if rng.random() < 0.7 else feeds
                unseen = [item for item in pool if item[0] not in seen]
                nxt, action = rng.choice(unseen or pool)
            else:
                movable = [
                    i for i, current in enumerate(state.containers)
                    if model.movable(i, current)
                ]
                blockers = model.goal_blockers(state)
                focused = [i for i in movable if i in blockers]
                rng.shuffle(focused)
                rng.shuffle(movable)
                if focused and rng.random() < 0.7:
                    movable = focused + [i for i in movable if i not in blockers]
                choices: list[tuple[State, Action]] = []
                for i in movable:
                    current = state.containers[i]
                    anchors = list(model.reachable(state, i) - {current.anchor})
                    # Parking farther from all line mouths generally opens more
                    # routes and avoids immediately replacing one blocker with another.
                    rng.shuffle(anchors)
                    anchors.sort(
                        key=lambda a: min(abs(a[0] - p[0]) + abs(a[1] - p[1]) for p in model.intakes),
                        reverse=True,
                    )
                    parking = anchors[:8]
                    remainder = anchors[8:]
                    rng.shuffle(remainder)
                    for anchor in parking + remainder[:8]:
                        nxt, action = model.transition(state, i, anchor)
                        if nxt not in seen:
                            choices.append((nxt, action))
                    if choices:
                        break
                if not choices:
                    break
                # Keep relocations away from line mouths when possible.
                clear = [item for item in choices if not (model.cells(item[1].container, item[0].containers[item[1].container]) & set(model.intakes))]
                nxt, action = rng.choice(clear or choices)
            state = nxt
            plan.append(action)
            seen.add(state)
            best_remaining = min(best_remaining, model.remaining(state))
        if model.solved(state):
            return plan, attempts
        # Vary the deterministic stream substantially between restarts.
        rng.seed(seed + (trial + 1) * 1_000_003 + best_remaining)
    return None, attempts


def solve_layered_beam(
    model: LevelModel,
    width_per_progress: int,
    max_drags: int,
) -> tuple[list[Action] | None, int]:
    """Keep diverse position states at every production-progress milestone.

    A single global greedy queue discards the alternative feed order needed by
    crowded Ice/LayerBox boards. This beam retains a bounded number of states for
    each distinct remaining-product count at every drag depth.
    """
    initial = model.initial
    frontier: dict[State, tuple[int, int]] = {initial: (0, 0)}
    parent: dict[State, tuple[State, Action]] = {}
    expanded = 0
    best_remaining = model.remaining(initial)
    best_state = initial
    for depth in range(max_drags):
        candidates: dict[State, tuple[int, int, State, Action]] = {}
        for state, (_, pure_moves) in frontier.items():
            expanded += 1
            for nxt, action in model.focused_actions(state, limit=80):
                next_pure = pure_moves + (0 if action.fed_amount else 1)
                existing = candidates.get(nxt)
                if existing is None or next_pure < existing[1]:
                    candidates[nxt] = (depth + 1, next_pure, state, action)
        if not candidates:
            break
        candidate_best = min(candidates, key=model.remaining)
        candidate_remaining = model.remaining(candidate_best)
        if candidate_remaining < best_remaining:
            best_remaining = candidate_remaining
            best_state = candidate_best
        for state, (_, _, previous, action) in candidates.items():
            parent[state] = (previous, action)
            if model.solved(state):
                plan: list[Action] = []
                cursor = state
                while cursor != initial:
                    prior, step = parent[cursor]
                    plan.append(step)
                    cursor = prior
                plan.reverse()
                return plan, expanded

        buckets: dict[int, list[tuple[tuple[int, int, int, int], State, tuple[int, int]]]] = {}
        for state, (next_depth, next_pure, _, _) in candidates.items():
            remaining = model.remaining(state)
            active = sum(1 for container in state.containers if container.active)
            # Prefer states ready to feed, then fewer blocker moves, while the
            # per-progress buckets preserve less-greedy production histories.
            score = (
                0 if model.has_feed(state) else 1,
                next_pure,
                active,
                hash(state),
            )
            buckets.setdefault(remaining, []).append((score, state, (next_depth, next_pure)))

        frontier = {}
        for entries in buckets.values():
            entries.sort(key=lambda item: item[0])
            for _, state, costs in entries[:width_per_progress]:
                frontier[state] = costs
    heads = [line[0] if line else 0 for line in best_state.lines]
    print(f"{model.level:04d}: layered beam best remaining {best_remaining}; heads {heads}; resolved {best_state.resolved}")
    return None, expanded


def relaxed_feed_plan(model: LevelModel, initial: State | None = None) -> list[tuple[int, int]] | None:
    """Find a valid container/line feed order while temporarily ignoring geometry."""
    memo: set[State] = set()

    def visit(state: State) -> list[tuple[int, int]] | None:
        if model.solved(state):
            return []
        if state in memo:
            return None
        memo.add(state)
        options: list[tuple[int, int, State, Action]] = []
        for i, current in enumerate(state.containers):
            if not model.movable(i, current):
                continue
            spec = model.spec_for(i, current)
            for line_index, queue in enumerate(state.lines):
                if not queue or not model.accepted(spec, current, queue[0]):
                    continue
                ix, iy = model.intakes[line_index]
                for dx, dy in spec.parts:
                    nxt, action = model.transition(state, i, (ix - dx, iy - dy))
                    if action.fed_line == line_index and action.fed_amount:
                        options.append((i, line_index, nxt, action))
                        break
        options.sort(key=lambda item: -item[3].fed_amount)
        for i, line_index, nxt, _ in options:
            suffix = visit(nxt)
            if suffix is not None:
                return [(i, line_index)] + suffix
        return None

    return visit(initial or model.initial)


def target_feed_actions(
    model: LevelModel,
    state: State,
    container_index: int,
    line_index: int,
) -> list[tuple[State, Action]]:
    current = state.containers[container_index]
    if not model.movable(container_index, current) or not state.lines[line_index]:
        return []
    spec = model.spec_for(container_index, current)
    if not model.accepted(spec, current, state.lines[line_index][0]):
        return []
    reachable = model.reachable(state, container_index)
    ix, iy = model.intakes[line_index]
    result = []
    for dx, dy in spec.parts:
        anchor = (ix - dx, iy - dy)
        if anchor not in reachable:
            continue
        nxt, action = model.transition(state, container_index, anchor)
        if action.fed_line == line_index and action.fed_amount:
            result.append((nxt, action))
    return result


def target_blockers(
    model: LevelModel,
    state: State,
    container_index: int,
    line_index: int,
) -> set[int]:
    current = state.containers[container_index]
    spec = model.spec_for(container_index, current)
    occupied_by: dict[tuple[int, int], int] = {}
    for j, other in enumerate(state.containers):
        if other.active:
            for cell in model.cells(j, other):
                occupied_by[cell] = j
    blockers: set[int] = set()
    ix, iy = model.intakes[line_index]
    for px, py in spec.parts:
        anchor = (ix - px, iy - py)
        placement = {(anchor[0] + dx, anchor[1] + dy) for dx, dy in spec.parts}
        if not placement <= model.grid:
            continue
        for cell in placement:
            owner = occupied_by.get(cell)
            if owner is not None and owner != container_index:
                blockers.add(owner)
    reachable = model.reachable(state, container_index)
    directions = DIRECTIONS.get(spec.movement, DIRECTIONS[100])
    for anchor in reachable:
        for dx, dy in directions:
            nxt = (anchor[0] + dx, anchor[1] + dy)
            if nxt in reachable:
                continue
            placement = {(nxt[0] + px, nxt[1] + py) for px, py in spec.parts}
            if not placement <= model.grid:
                continue
            for cell in placement:
                owner = occupied_by.get(cell)
                if owner is not None and owner != container_index:
                    blockers.add(owner)
    return blockers


def route_to_feed(
    model: LevelModel,
    initial: State,
    container_index: int,
    line_index: int,
    max_nodes: int,
) -> tuple[State, list[Action], int] | None:
    serial = 0
    heap: list[tuple[tuple[int, int], int, State]] = [((0, 0), serial, initial)]
    depth: dict[State, int] = {initial: 0}
    parent: dict[State, tuple[State, Action]] = {}
    expanded = 0
    while heap and expanded < max_nodes:
        _, _, state = heapq.heappop(heap)
        target = target_feed_actions(model, state, container_index, line_index)
        if target:
            final, feed_action = target[0]
            route: list[Action] = [feed_action]
            cursor = state
            while cursor != initial:
                previous, action = parent[cursor]
                route.append(action)
                cursor = previous
            route.reverse()
            return final, route, expanded
        expanded += 1
        blockers = target_blockers(model, state, container_index, line_index)
        movable = [i for i, current in enumerate(state.containers) if model.movable(i, current)]
        preferred = [i for i in blockers if i in movable]
        order = preferred + [container_index] + [i for i in movable if i not in blockers and i != container_index]
        ranked: list[tuple[tuple[int, int, int], State, Action]] = []
        for rank, i in enumerate(order):
            current = state.containers[i]
            for anchor in model.reachable(state, i) - {current.anchor}:
                nxt, action = model.transition(state, i, anchor)
                if action.fed_amount:
                    continue
                next_blockers = len(target_blockers(model, nxt, container_index, line_index))
                ready = 0 if target_feed_actions(model, nxt, container_index, line_index) else 1
                ranked.append(((ready, next_blockers, rank), nxt, action))
        ranked.sort(key=lambda item: item[0])
        for _, nxt, action in ranked[:80]:
            next_depth = depth[state] + 1
            if next_depth >= depth.get(nxt, 1 << 30):
                continue
            depth[nxt] = next_depth
            parent[nxt] = (state, action)
            serial += 1
            heuristic = len(target_blockers(model, nxt, container_index, line_index))
            heapq.heappush(heap, ((heuristic, next_depth), serial, nxt))
    return None


def solve_guided(
    model: LevelModel,
    max_nodes_per_feed: int,
) -> tuple[list[Action] | None, int]:
    memo: set[State] = set()
    expanded = 0

    def visit(state: State, feed_depth: int) -> list[Action] | None:
        nonlocal expanded
        if model.solved(state):
            return []
        if state in memo:
            return None
        memo.add(state)

        immediate = model.feed_actions(state)
        immediate.sort(key=lambda item: -item[1].fed_amount)
        for nxt, action in immediate:
            if relaxed_feed_plan(model, nxt) is None:
                continue
            suffix = visit(nxt, feed_depth + 1)
            if suffix is not None:
                return [action] + suffix

        relaxed = relaxed_feed_plan(model, state)
        goals: list[tuple[int, int]] = []
        if relaxed:
            goals.append(relaxed[0])
        for i, current in enumerate(state.containers):
            if not model.movable(i, current):
                continue
            spec = model.spec_for(i, current)
            for line_index, queue in enumerate(state.lines):
                goal = (i, line_index)
                if queue and model.accepted(spec, current, queue[0]) and goal not in goals:
                    goals.append(goal)
        for container_index, line_index in goals:
            routed = route_to_feed(model, state, container_index, line_index, max_nodes_per_feed)
            if routed is None:
                continue
            nxt, actions, nodes = routed
            expanded += nodes
            if relaxed_feed_plan(model, nxt) is None:
                continue
            suffix = visit(nxt, feed_depth + 1)
            if suffix is not None:
                return actions + suffix
        return None

    plan = visit(model.initial, 0)
    if plan is None:
        print(f"{model.level:04d}: guided search exhausted {len(memo)} feed states")
    return plan, expanded


def routes_to_next_feeds(
    model: LevelModel,
    initial: State,
    max_nodes: int,
    extra_depth: int = 4,
) -> tuple[list[tuple[State, list[Action]]], int]:
    """BFS pure legal relocations until one or more production feeds open."""
    queue: list[State] = [initial]
    cursor = 0
    depth: dict[State, int] = {initial: 0}
    parent: dict[State, tuple[State, Action]] = {}
    first_feed_depth: int | None = None
    results: dict[State, list[Action]] = {}
    expanded = 0
    while cursor < len(queue) and expanded < max_nodes:
        state = queue[cursor]
        cursor += 1
        current_depth = depth[state]
        if first_feed_depth is not None and current_depth > first_feed_depth + extra_depth:
            break
        feeds = model.feed_actions(state)
        if feeds:
            if first_feed_depth is None:
                first_feed_depth = current_depth
            prefix: list[Action] = []
            trace = state
            while trace != initial:
                previous, action = parent[trace]
                prefix.append(action)
                trace = previous
            prefix.reverse()
            for nxt, feed_action in feeds:
                candidate = prefix + [feed_action]
                existing = results.get(nxt)
                if existing is None or len(candidate) < len(existing):
                    results[nxt] = candidate
        expanded += 1
        if first_feed_depth is not None and current_depth >= first_feed_depth + extra_depth:
            continue
        for i, current in enumerate(state.containers):
            if not model.movable(i, current):
                continue
            for anchor in model.reachable(state, i) - {current.anchor}:
                nxt, action = model.transition(state, i, anchor)
                if action.fed_amount or nxt in depth:
                    continue
                depth[nxt] = current_depth + 1
                parent[nxt] = (state, action)
                queue.append(nxt)
    return list(results.items()), expanded


def solve_macro(
    model: LevelModel,
    max_nodes_per_feed: int,
) -> tuple[list[Action] | None, int]:
    memo: set[State] = set()
    expanded = 0
    best_remaining = model.remaining(model.initial)
    best_history: list[tuple[int, int, int]] = []

    def visit(
        state: State,
        feed_depth: int = 0,
        history: list[tuple[int, int, int]] | None = None,
    ) -> list[Action] | None:
        nonlocal expanded, best_remaining, best_history
        history = history or []
        remaining = model.remaining(state)
        if remaining < best_remaining:
            best_remaining = remaining
            best_history = list(history)
        if model.solved(state):
            return []
        if state in memo:
            return None
        memo.add(state)
        macros, nodes = routes_to_next_feeds(model, state, max_nodes_per_feed)
        expanded += nodes
        relaxed = relaxed_feed_plan(model, state) or []
        relaxed_rank: dict[tuple[int, int], int] = {}
        for rank, goal in enumerate(relaxed):
            relaxed_rank.setdefault(goal, rank)
        viable = []
        for nxt, actions in macros:
            if relaxed_feed_plan(model, nxt) is None:
                continue
            feed = actions[-1]
            mobility = sum(
                max(0, len(model.reachable(nxt, i)) - 1)
                for i, current in enumerate(nxt.containers)
                if model.movable(i, current)
            )
            viable.append((
                (-(nxt.resolved - state.resolved),
                 -mobility,
                 relaxed_rank.get((feed.container, feed.fed_line), 1 << 20),
                 len(actions), -feed.fed_amount),
                nxt,
                actions,
            ))
        viable.sort(key=lambda item: item[0])
        for _, nxt, actions in viable[:20]:
            feed = actions[-1]
            suffix = visit(
                nxt,
                feed_depth + 1,
                history + [(feed.container, feed.fed_line, feed.fed_amount)],
            )
            if suffix is not None:
                return actions + suffix
        return None

    plan = visit(model.initial)
    if plan is None:
        print(f"{model.level:04d}: macro search exhausted {len(memo)} feed states; "
              f"best remaining {best_remaining}; feeds {best_history}")
    return plan, expanded


def solve_macro_beam(
    model: LevelModel,
    max_nodes_per_feed: int,
    width: int,
    max_feeds: int,
) -> tuple[list[Action] | None, int]:
    initial = model.initial
    frontier: dict[State, int] = {initial: 0}
    parent: dict[State, tuple[State, list[Action]]] = {}
    expanded = 0
    for _ in range(max_feeds):
        candidates: dict[State, tuple[int, State, list[Action]]] = {}
        for state, drag_count in frontier.items():
            macros, nodes = routes_to_next_feeds(
                model,
                state,
                max_nodes_per_feed,
                extra_depth=2,
            )
            expanded += nodes
            for nxt, actions in macros:
                next_drags = drag_count + len(actions)
                existing = candidates.get(nxt)
                if existing is None or next_drags < existing[0]:
                    candidates[nxt] = (next_drags, state, actions)
        if not candidates:
            break
        for state, (_, previous, actions) in candidates.items():
            parent[state] = (previous, actions)
            if model.solved(state):
                chunks: list[list[Action]] = []
                cursor = state
                while cursor != initial:
                    prior, steps = parent[cursor]
                    chunks.append(steps)
                    cursor = prior
                plan = [action for chunk in reversed(chunks) for action in chunk]
                return plan, expanded

        ranked = []
        for state, (drag_count, _, _) in candidates.items():
            mobility = sum(
                max(0, len(model.reachable(state, i)) - 1)
                for i, current in enumerate(state.containers)
                if model.movable(i, current)
            )
            locked_ice = sum(
                1 for i, current in enumerate(state.containers)
                if current.active and not current.thawed and model.specs[i].material == 300
            )
            ranked.append((
                (-state.resolved, locked_ice, model.remaining(state), -mobility, drag_count),
                state,
                drag_count,
            ))
        ranked.sort(key=lambda item: item[0])
        frontier = {state: drag_count for _, state, drag_count in ranked[:width]}
    best = min((model.remaining(state) for state in frontier), default=model.remaining(initial))
    print(f"{model.level:04d}: macro beam exhausted; best remaining {best}; expanded {expanded}")
    return None, expanded


def action_json(action: Action) -> dict:
    return {
        "container": action.container,
        "anchor": {"x": action.anchor[0], "y": action.anchor[1]},
        "fedLine": action.fed_line,
        "fedColor": action.fed_colour,
        "fedAmount": action.fed_amount,
    }


def replay_plan(model: LevelModel, plan: list[Action]) -> list[Action] | None:
    state = model.initial
    replayed: list[Action] = []
    for requested in plan:
        index = requested.container
        if index < 0 or index >= len(state.containers) or not model.movable(index, state.containers[index]):
            return None
        if requested.anchor not in model.reachable(state, index):
            return None
        state, actual = model.transition(state, index, requested.anchor)
        replayed.append(actual)
    return replayed if model.solved(state) else None


def minimize_plan(model: LevelModel, plan: list[Action]) -> list[Action]:
    """Delta-debug redundant relocations while preserving a legal solved replay."""
    current = plan
    granularity = 2
    while len(current) >= 2:
        chunk = max(1, (len(current) + granularity - 1) // granularity)
        reduced = False
        for start in range(0, len(current), chunk):
            candidate = current[:start] + current[start + chunk :]
            replayed = replay_plan(model, candidate)
            if replayed is not None:
                current = replayed
                granularity = max(2, granularity - 1)
                reduced = True
                break
        if not reduced:
            if granularity >= len(current):
                break
            granularity = min(len(current), granularity * 2)

    index = 0
    while index < len(current):
        replayed = replay_plan(model, current[:index] + current[index + 1 :])
        if replayed is not None:
            current = replayed
        else:
            index += 1
    return current


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("levels", nargs="+", type=int)
    parser.add_argument("--root", type=Path, default=Path("Assets/_Projects/Resources/LevelData"))
    parser.add_argument("--output", type=Path, default=Path("CoffeeRunMigration/Solutions"))
    parser.add_argument("--max-nodes", type=int, default=500_000)
    parser.add_argument("--beam", type=int, default=0, help="maximum accepted states per depth; 0 is unlimited")
    parser.add_argument("--strategy", choices=("search", "random", "layered-beam", "guided", "macro", "macro-beam"), default="search")
    parser.add_argument("--trials", type=int, default=2_000)
    parser.add_argument("--max-drags", type=int, default=400)
    parser.add_argument("--progress-width", type=int, default=20)
    parser.add_argument("--minimize-existing", action="store_true")
    args = parser.parse_args()
    args.output.mkdir(parents=True, exist_ok=True)
    failed = False
    for level in args.levels:
        source = args.root / f"{level:04d}.json"
        model = LevelModel(json.loads(source.read_text()))
        target = args.output / f"{level:04d}.json"
        if args.minimize_existing:
            existing = json.loads(target.read_text())
            raw_plan = [
                Action(
                    int(item["container"]),
                    pos(item["anchor"]),
                    int(item.get("fedLine", -1)),
                    int(item.get("fedColor", 0)),
                    int(item.get("fedAmount", 0)),
                )
                for item in existing["actions"]
            ]
            plan = minimize_plan(model, raw_plan)
            expanded = int(existing.get("expandedNodes", 0))
        elif args.strategy == "random":
            plan, expanded = solve_stochastic(model, args.trials, args.max_drags, level)
        elif args.strategy == "layered-beam":
            plan, expanded = solve_layered_beam(model, args.progress_width, args.max_drags)
        elif args.strategy == "guided":
            plan, expanded = solve_guided(model, args.max_nodes)
        elif args.strategy == "macro":
            plan, expanded = solve_macro(model, args.max_nodes)
        elif args.strategy == "macro-beam":
            plan, expanded = solve_macro_beam(
                model,
                args.max_nodes,
                args.progress_width,
                args.max_drags,
            )
        else:
            plan, expanded = solve(model, args.max_nodes, args.beam)
        if plan is None:
            print(f"{level:04d}: UNSOLVED ({expanded} nodes)")
            failed = True
            LevelModel.cells.cache_clear()
            LevelModel.reachable.cache_clear()
            LevelModel.has_feed.cache_clear()
            LevelModel.goal_blockers.cache_clear()
            continue
        payload = {
            "level": level,
            "solver": "grid-bfs-no-skill-v1",
            "expandedNodes": expanded,
            "dragCount": len(plan),
            "actions": [action_json(action) for action in plan],
        }
        target.write_text(json.dumps(payload, indent=2) + "\n")
        feeds = sum(1 for action in plan if action.fed_amount)
        print(f"{level:04d}: solved in {len(plan)} drags ({feeds} feeds, {expanded} nodes)")
        LevelModel.cells.cache_clear()
        LevelModel.reachable.cache_clear()
        LevelModel.has_feed.cache_clear()
        LevelModel.goal_blockers.cache_clear()
    return 1 if failed else 0


if __name__ == "__main__":
    raise SystemExit(main())
