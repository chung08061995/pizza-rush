#!/usr/bin/env python3
"""Replay a solved Coffee Run level on BlueStacks through ADB.

This is source-verification tooling only.  It reads the converted level and the
matching no-skill plan, reconstructs a collision-free cell-by-cell drag route,
and emits one continuous Android motion gesture per drag.  The game runtime is
still authoritative for whether the level reaches Win.
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
ROOT = TOOLS_DIR.parent.parent
sys.path.insert(0, str(TOOLS_DIR))

import solve_levels as solver  # noqa: E402


DEFAULT_ADB = "/Applications/BlueStacks.app/Contents/MacOS/hd-adb"


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
    occupied: set[tuple[int, int]] = set()
    for index, other in enumerate(state.containers):
        if index != container_index and other.active:
            occupied.update(model.cells(index, other))
    available = model.grid - occupied

    def valid(anchor: tuple[int, int]) -> bool:
        return all(
            (anchor[0] + dx, anchor[1] + dy) in available
            for dx, dy in spec.parts
        )

    queue = deque([current.anchor])
    previous: dict[tuple[int, int], tuple[int, int] | None] = {
        current.anchor: None
    }
    directions = solver.DIRECTIONS.get(spec.movement, solver.DIRECTIONS[100])
    while queue:
        anchor = queue.popleft()
        if anchor == target:
            break
        for dx, dy in directions:
            candidate = anchor[0] + dx, anchor[1] + dy
            if candidate not in previous and valid(candidate):
                previous[candidate] = anchor
                queue.append(candidate)

    if target not in previous:
        raise RuntimeError(
            f"container {container_index} cannot reach {target} from {current.anchor}"
        )

    route: list[tuple[int, int]] = []
    cursor: tuple[int, int] | None = target
    while cursor is not None:
        route.append(cursor)
        cursor = previous[cursor]
    route.reverse()
    return route


def estimate_screen_grid(data: dict) -> tuple[float, float, float, float]:
    """Estimate the stable Coffee Run portrait board projection.

    The board is centered at (540, 890) on the 1080x1920 BlueStacks profile.
    Small boards use the game's maximum zoom; larger boards are limited by the
    horizontal or vertical play area.  Touches target cell centers, so the
    estimate has ample tolerance even with rounded borders/perspective.
    """
    xs = [int(point["x"]) for point in data["gridPositions"]]
    ys = [int(point["y"]) for point in data["gridPositions"]]
    columns = max(xs) - min(xs) + 1
    rows = max(ys) - min(ys) + 1
    y_step = min(100.0, 870.0 / rows)
    x_step = min(114.0, 650.0 / columns, y_step * 1.11)
    # Wide boards are scaled by their horizontal fit. Coffee Run's portrait
    # projection renders their cells slightly shorter than they are wide
    # (7-column boards measure about 93×83 px on the 1080×1920 profile).
    if columns >= 7:
        y_step = min(y_step, x_step * 0.9)
    return 540.0, 890.0, x_step, y_step


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("level", type=int)
    parser.add_argument("--adb", default=DEFAULT_ADB)
    parser.add_argument("--serial", default="127.0.0.1:5555")
    parser.add_argument("--center-x", type=float)
    parser.add_argument("--center-y", type=float)
    parser.add_argument("--x-step", type=float)
    parser.add_argument("--y-step", type=float)
    parser.add_argument("--move-delay", type=float, default=0.075)
    parser.add_argument("--idle-delay", type=float, default=0.35)
    parser.add_argument("--feed-delay", type=float, default=2.2)
    parser.add_argument("--per-item-delay", type=float, default=0.055)
    parser.add_argument(
        "--capture-dir",
        type=Path,
        default=Path("/tmp/coffee-run-control/levels"),
    )
    parser.add_argument("--dry-run", action="store_true")
    args = parser.parse_args()

    level_path = ROOT / f"Assets/_Projects/Resources/LevelData/{args.level:04d}.json"
    solution_path = ROOT / f"CoffeeRunMigration/Solutions/{args.level:04d}.json"
    data = json.loads(level_path.read_text())
    solution = json.loads(solution_path.read_text())
    model = solver.LevelModel(data)
    state = model.initial

    center_x, center_y, x_step, y_step = estimate_screen_grid(data)
    center_x = args.center_x if args.center_x is not None else center_x
    center_y = args.center_y if args.center_y is not None else center_y
    x_step = args.x_step if args.x_step is not None else x_step
    y_step = args.y_step if args.y_step is not None else y_step
    xs = [int(point["x"]) for point in data["gridPositions"]]
    ys = [int(point["y"]) for point in data["gridPositions"]]
    mid_x = (min(xs) + max(xs)) / 2.0
    mid_y = (min(ys) + max(ys)) / 2.0

    def screen_point(cell: tuple[int, int]) -> tuple[int, int]:
        return (
            round(center_x + x_step * (cell[0] - mid_x)),
            round(center_y - y_step * (cell[1] - mid_y)),
        )

    capture_dir = args.capture_dir / f"{args.level:04d}"
    capture_dir.mkdir(parents=True, exist_ok=True)
    if not args.dry_run:
        with (capture_dir / "replay-start.png").open("wb") as output:
            run_adb(args.adb, args.serial, "exec-out", "screencap", "-p", stdout=output)

    for number, raw in enumerate(solution["actions"], 1):
        index = int(raw["container"])
        target = int(raw["anchor"]["x"]), int(raw["anchor"]["y"])
        route = cell_route(model, state, index, target)
        if not args.dry_run:
            start_x, start_y = screen_point(route[0])
            run_adb(
                args.adb,
                args.serial,
                "shell",
                "input",
                "motionevent",
                "DOWN",
                start_x,
                start_y,
            )
            time.sleep(0.06)
            for cell in route[1:]:
                x, y = screen_point(cell)
                run_adb(
                    args.adb,
                    args.serial,
                    "shell",
                    "input",
                    "motionevent",
                    "MOVE",
                    x,
                    y,
                )
                time.sleep(args.move_delay)
            end_x, end_y = screen_point(route[-1])
            run_adb(
                args.adb,
                args.serial,
                "shell",
                "input",
                "motionevent",
                "UP",
                end_x,
                end_y,
            )

        state, actual = model.transition(state, index, target)
        expected = int(raw.get("fedLine", -1)), int(raw.get("fedAmount", 0))
        observed = actual.fed_line, actual.fed_amount
        if observed != expected:
            raise RuntimeError(
                f"action {number} static replay mismatch: expected {expected}, got {observed}"
            )
        print(
            f"{args.level:04d} {number:03d}/{len(solution['actions'])}: "
            f"container {index} {route[0]} -> {target}, feed={actual.fed_amount}",
            flush=True,
        )
        if not args.dry_run:
            delay = (
                args.feed_delay + args.per_item_delay * actual.fed_amount
                if actual.fed_amount
                else args.idle_delay
            )
            time.sleep(delay)

    if not model.solved(state):
        raise RuntimeError("plan ended without emptying every production line")
    if not args.dry_run:
        time.sleep(2.0)
        with (capture_dir / "replay-end.png").open("wb") as output:
            run_adb(args.adb, args.serial, "exec-out", "screencap", "-p", stdout=output)
    print(
        f"{args.level:04d}: static replay complete; inspect replay-end.png for source Win",
        flush=True,
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
