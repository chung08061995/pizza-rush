#!/usr/bin/env python3
"""Solve and replay Coffee Run's forced Special 2 progression board.

Special 2 is a source-only 12x18 board forced after normal Level 30.  The
layout below was normalized from the live Coffee Run 3.20.0 board capture.  It
is deliberately kept out of Pizza Rush's 100 normal level JSON files.
"""

from __future__ import annotations

import argparse
import json
import subprocess
import sys
import time
from collections import deque
from pathlib import Path


TOOLS_DIR = Path(__file__).resolve().parent
sys.path.insert(0, str(TOOLS_DIR))

import solve_levels as solver  # noqa: E402


DEFAULT_ADB = "/Applications/BlueStacks.app/Contents/MacOS/hd-adb"
YELLOW = 1
PURPLE = 2


def special_rectangles() -> list[tuple[int, int, int, int, int]]:
    """Return (screen column, screen row, width, height, colour)."""
    result: list[tuple[int, int, int, int, int]] = []

    def add(colour: int, *rectangles: tuple[int, int, int, int]) -> None:
        result.extend((*rectangle, colour) for rectangle in rectangles)

    # Rows 1-4.
    add(YELLOW, (1, 1, 2, 2), (3, 1, 2, 2))
    add(YELLOW, *((column, row, 1, 1) for row in (3, 4) for column in range(1, 5)))
    add(PURPLE, (5, 1, 2, 2), (5, 3, 2, 2))
    add(PURPLE, *((column, row, 1, 1) for row in range(1, 5) for column in (7, 8)))
    add(YELLOW, (9, 1, 2, 2), (11, 2, 2, 2))
    add(YELLOW, (11, 1, 1, 1), (12, 1, 1, 1), (9, 3, 1, 1), (10, 3, 1, 1))
    add(YELLOW, *((column, 4, 1, 1) for column in range(9, 13)))

    # Rows 5-7.
    add(PURPLE, (1, 5, 2, 2), (3, 5, 2, 2))
    add(PURPLE, *((column, 7, 1, 1) for column in range(1, 5)))
    add(YELLOW, (6, 5, 2, 2), (7, 7, 2, 1))
    add(YELLOW, (5, 5, 1, 1), (8, 5, 1, 1), (5, 6, 1, 1), (8, 6, 1, 1))
    add(YELLOW, (5, 7, 1, 1), (6, 7, 1, 1))
    add(PURPLE, (9, 5, 2, 2), (11, 6, 2, 2))
    add(PURPLE, (11, 5, 1, 1), (12, 5, 1, 1), (9, 7, 1, 1), (10, 7, 1, 1))

    # Rows 8-11.
    add(YELLOW, (1, 8, 2, 2), (3, 8, 2, 2), (1, 10, 2, 2), (3, 10, 2, 2))
    add(PURPLE, *((column, 8, 1, 1) for column in range(5, 9)))
    add(PURPLE, (6, 9, 2, 2))
    add(PURPLE, (5, 9, 1, 1), (8, 9, 1, 1), (5, 10, 1, 1), (8, 10, 1, 1))
    add(PURPLE, *((column, 11, 1, 1) for column in range(5, 9)))
    add(YELLOW, (11, 8, 2, 2), (9, 9, 2, 2))
    add(YELLOW, (9, 8, 1, 1), (10, 8, 1, 1), (11, 10, 1, 1), (12, 10, 1, 1))
    add(YELLOW, *((column, 11, 1, 1) for column in range(9, 13)))

    # Rows 12-14.
    add(PURPLE, *((column, 12, 1, 1) for column in range(1, 5)))
    add(PURPLE, (3, 13, 2, 2), (1, 14, 2, 1))
    add(PURPLE, (1, 13, 1, 1), (2, 13, 1, 1))
    add(YELLOW, (6, 12, 2, 2), (5, 14, 2, 1))
    add(YELLOW, (5, 12, 1, 1), (8, 12, 1, 1), (5, 13, 1, 1), (8, 13, 1, 1))
    add(YELLOW, (7, 14, 1, 1), (8, 14, 1, 1))
    add(PURPLE, *((column, 12, 1, 1) for column in range(9, 13)))
    add(PURPLE, (9, 13, 2, 2))
    add(PURPLE, *((column, row, 1, 1) for row in (13, 14) for column in (11, 12)))

    # Rows 15-18.
    add(YELLOW, (2, 15, 2, 2), (2, 17, 2, 2))
    add(YELLOW, *((column, row, 1, 1) for row in range(15, 19) for column in (1, 4)))
    add(PURPLE, *((column, 15, 1, 1) for column in range(5, 9)))
    add(PURPLE, (5, 16, 2, 2))
    add(PURPLE, *((column, row, 1, 1) for row in (16, 17) for column in (7, 8)))
    add(PURPLE, *((column, 18, 1, 1) for column in range(5, 9)))
    add(YELLOW, (9, 15, 2, 2), (11, 15, 2, 2))
    add(YELLOW, *((column, row, 1, 1) for row in (17, 18) for column in range(9, 13)))
    return result


def special_data() -> dict:
    rectangles = special_rectangles()
    occupied: dict[tuple[int, int], int] = {}
    containers = []
    for index, (column, row, width, height, colour) in enumerate(rectangles):
        cells = {
            (x, y)
            for x in range(column, column + width)
            for y in range(row, row + height)
        }
        overlap = cells & occupied.keys()
        if overlap:
            raise RuntimeError(f"Special 2 rectangle {index} overlaps {sorted(overlap)}")
        for cell in cells:
            occupied[cell] = index

        # Solver Y increases upwards; source screen rows increase downwards.
        anchor_y = 19 - (row + height - 1)
        if (width, height) == (1, 1):
            shape, rotation, anchor_x = 100, 1, column
        elif (width, height) == (1, 2):
            shape, rotation, anchor_x = 101, 1, column
        elif (width, height) == (2, 1):
            shape, rotation, anchor_x = 101, 4, column
        elif (width, height) == (2, 2):
            shape, rotation, anchor_x = 300, 1, column
        else:
            raise RuntimeError(f"unsupported Special 2 rectangle {width}x{height}")
        containers.append(
            {
                "position": {"x": anchor_x, "y": anchor_y},
                "rotationType": rotation,
                "flipX": False,
                "containerData": {
                    "containerShapeType": shape,
                    "containerMovementType": 100,
                    "containerMaterialType": 100,
                    "isStone": False,
                    "containerColorData": {
                        "colorType": colour,
                        "colors": [colour],
                        "colorAmounts": [width * height * 4],
                        "isLayerBox": False,
                        "isMultiColor": False,
                    },
                },
            }
        )

    expected = {(x, row) for x in range(1, 13) for row in range(1, 19)}
    missing = expected - occupied.keys()
    extra = occupied.keys() - expected
    if missing or extra:
        raise RuntimeError(
            f"Special 2 normalization mismatch: missing={sorted(missing)}, extra={sorted(extra)}"
        )

    lines = []
    # Eight yellow and four purple lines split the 480/384 source items evenly.
    for top in (True, False):
        for column, colour, amount in (
            (2, YELLOW, 60),
            (3, YELLOW, 60),
            (6, PURPLE, 96),
            (7, PURPLE, 96),
            (10, YELLOW, 60),
            (11, YELLOW, 60),
        ):
            lines.append(
                {
                    "position": {"x": column, "y": 19 if top else 0},
                    "rotationType": 1 if top else 3,
                    "productionCollections": [{"colorType": colour, "Amount": amount}],
                }
            )
    return {
        "levelIndex": 1002,
        "gridPositions": [{"x": x, "y": y} for y in range(1, 19) for x in range(1, 13)],
        "containers": containers,
        "productionLines": lines,
    }


def run_adb(adb: str, serial: str, *args: object, stdout=None) -> None:
    subprocess.run(
        [adb, "-s", serial, *(str(arg) for arg in args)],
        check=True,
        stdout=stdout if stdout is not None else subprocess.DEVNULL,
    )


def cell_route(
    model: solver.LevelModel,
    state: solver.State,
    container_index: int,
    target: tuple[int, int],
) -> list[tuple[int, int]]:
    current = state.containers[container_index]
    spec = model.spec_for(container_index, current)
    occupied = set()
    for index, other in enumerate(state.containers):
        if index != container_index and other.active:
            occupied.update(model.cells(index, other))
    available = model.grid - occupied

    def valid(anchor: tuple[int, int]) -> bool:
        return all((anchor[0] + dx, anchor[1] + dy) in available for dx, dy in spec.parts)

    queue = deque([current.anchor])
    previous = {current.anchor: None}
    while queue:
        anchor = queue.popleft()
        if anchor == target:
            break
        for dx, dy in solver.DIRECTIONS[100]:
            candidate = anchor[0] + dx, anchor[1] + dy
            if candidate not in previous and valid(candidate):
                previous[candidate] = anchor
                queue.append(candidate)
    if target not in previous:
        raise RuntimeError(f"container {container_index} cannot reach {target}")
    route = []
    cursor = target
    while cursor is not None:
        route.append(cursor)
        cursor = previous[cursor]
    return list(reversed(route))


def screen_point(cell: tuple[int, int]) -> tuple[int, int]:
    return round(197 + 62.2 * (cell[0] - 1)), round(400 + 61.2 * (18 - cell[1]))


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--adb", default=DEFAULT_ADB)
    parser.add_argument("--serial", default="127.0.0.1:5555")
    parser.add_argument("--trials", type=int, default=8000)
    parser.add_argument("--move-delay", type=float, default=0.018)
    parser.add_argument("--idle-delay", type=float, default=0.04)
    parser.add_argument("--feed-delay", type=float, default=1.15)
    parser.add_argument("--dry-run", action="store_true")
    parser.add_argument(
        "--capture-dir",
        type=Path,
        default=Path("/tmp/coffee-run-control/special/0002"),
    )
    args = parser.parse_args()

    model = solver.LevelModel(special_data())
    plan, attempts = solver.solve_stochastic(
        model, trials=args.trials, max_drags=900, seed=1002
    )
    if plan is None:
        raise RuntimeError(f"Special 2 unsolved after {attempts} stochastic actions")
    plan = solver.minimize_plan(model, plan)
    replayed = solver.replay_plan(model, plan)
    if replayed is None:
        raise RuntimeError("minimized Special 2 plan failed static replay")
    plan = replayed
    print(f"Special 2 static plan: {len(plan)} drags", flush=True)

    args.capture_dir.mkdir(parents=True, exist_ok=True)
    (args.capture_dir / "solution.json").write_text(
        json.dumps(
            {
                "level": "Special 2",
                "solver": "grid-bfs-no-skill-v1",
                "dragCount": len(plan),
                "actions": [solver.action_json(action) for action in plan],
            },
            indent=2,
        )
        + "\n"
    )
    state = model.initial
    if not args.dry_run:
        with (args.capture_dir / "replay-start.png").open("wb") as output:
            run_adb(args.adb, args.serial, "exec-out", "screencap", "-p", stdout=output)
    for number, action in enumerate(plan, 1):
        route = cell_route(model, state, action.container, action.anchor)
        if not args.dry_run:
            sx, sy = screen_point(route[0])
            run_adb(args.adb, args.serial, "shell", "input", "motionevent", "DOWN", sx, sy)
            time.sleep(0.035)
            for cell in route[1:]:
                x, y = screen_point(cell)
                run_adb(args.adb, args.serial, "shell", "input", "motionevent", "MOVE", x, y)
                time.sleep(args.move_delay)
            ex, ey = screen_point(route[-1])
            run_adb(args.adb, args.serial, "shell", "input", "motionevent", "UP", ex, ey)
        state, actual = model.transition(state, action.container, action.anchor)
        if (actual.fed_line, actual.fed_amount) != (action.fed_line, action.fed_amount):
            raise RuntimeError(f"static mismatch at action {number}")
        print(
            f"S2 {number:03d}/{len(plan)} {route[0]}->{action.anchor} feed={actual.fed_amount}",
            flush=True,
        )
        if not args.dry_run:
            time.sleep(args.feed_delay if actual.fed_amount else args.idle_delay)
    if not model.solved(state):
        raise RuntimeError("Special 2 plan ended before every line emptied")
    if not args.dry_run:
        time.sleep(5)
        with (args.capture_dir / "replay-end.png").open("wb") as output:
            run_adb(args.adb, args.serial, "exec-out", "screencap", "-p", stdout=output)
    print("Special 2 static replay complete; inspect replay-end.png", flush=True)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
