#!/usr/bin/env python3
"""Solve and replay Coffee Run's forced Special 1 progression board.

Special 1 is not one of Pizza Rush's 100 normal level JSON files. Coffee Run
forces it after Level 10, so this source-only helper clears it without a skill
or booster and allows sequential capture to continue at Level 11.
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


def special_data() -> dict:
    # Source observations use L/C/R/G. Numeric values are local solver identities;
    # only equality and order matter to the grid model.
    colours = {"L": 1, "C": 2, "R": 3, "G": 4}
    lane_orders = {
        1: "LRGLGCR",
        3: "CGLGCRL",
        5: "RLGCRLC",
        7: "GCRLCGR",
    }
    containers = []
    for x, top_to_bottom in lane_orders.items():
        for y, colour in zip(range(10, 3, -1), top_to_bottom):
            containers.append(
                {
                    "position": {"x": x, "y": y},
                    "rotationType": 1,
                    "flipX": False,
                    "containerData": {
                        "containerShapeType": 100,
                        "containerMovementType": 100,
                        "containerMaterialType": 100,
                        "isStone": False,
                        "containerColorData": {
                            "colorType": colours[colour],
                            "colors": [colours[colour]],
                            "colorAmounts": [4],
                            "isLayerBox": False,
                            "isMultiColor": False,
                        },
                    },
                }
            )
    lines = []
    for x, colour in zip((1, 3, 5, 7), "LCRG"):
        lines.append(
            {
                "position": {"x": x, "y": 12},
                "rotationType": 1,
                "productionCollections": [
                    {"colorType": colours[colour], "Amount": 28}
                ],
            }
        )
    return {
        "levelIndex": 1001,
        "gridPositions": (
            [{"x": x, "y": y} for y in range(1, 4) for x in range(1, 8)]
            + [
                {"x": x, "y": y}
                for x in (1, 3, 5, 7)
                for y in range(4, 12)
            ]
        ),
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
    occupied = set()
    for index, other in enumerate(state.containers):
        if index != container_index and other.active:
            occupied.update(model.cells(index, other))
    available = model.grid - occupied
    queue = deque([current.anchor])
    previous = {current.anchor: None}
    while queue:
        anchor = queue.popleft()
        if anchor == target:
            break
        for dx, dy in solver.DIRECTIONS[100]:
            candidate = anchor[0] + dx, anchor[1] + dy
            if candidate not in previous and candidate in available:
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
    return round(540 + 80 * (cell[0] - 4)), round(912 - 72 * (cell[1] - 6))


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--adb", default=DEFAULT_ADB)
    parser.add_argument("--serial", default="127.0.0.1:5555")
    parser.add_argument("--trials", type=int, default=4000)
    parser.add_argument("--move-delay", type=float, default=0.025)
    parser.add_argument("--idle-delay", type=float, default=0.06)
    parser.add_argument("--feed-delay", type=float, default=1.35)
    parser.add_argument("--dry-run", action="store_true")
    parser.add_argument(
        "--capture-dir",
        type=Path,
        default=Path("/tmp/coffee-run-control/special/0001"),
    )
    args = parser.parse_args()

    model = solver.LevelModel(special_data())
    plan, attempts = solver.solve_stochastic(
        model, trials=args.trials, max_drags=500, seed=1001
    )
    if plan is None:
        raise RuntimeError(f"Special 1 unsolved after {attempts} stochastic actions")
    plan = solver.minimize_plan(model, plan)
    replayed = solver.replay_plan(model, plan)
    if replayed is None:
        raise RuntimeError("minimized Special 1 plan failed static replay")
    plan = replayed
    print(f"Special 1 static plan: {len(plan)} drags", flush=True)

    args.capture_dir.mkdir(parents=True, exist_ok=True)
    (args.capture_dir / "solution.json").write_text(
        json.dumps(
            {
                "level": "Special 1",
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
        with (args.capture_dir / "replay-start-2.png").open("wb") as output:
            run_adb(args.adb, args.serial, "exec-out", "screencap", "-p", stdout=output)
    for number, action in enumerate(plan, 1):
        route = cell_route(model, state, action.container, action.anchor)
        if not args.dry_run:
            sx, sy = screen_point(route[0])
            run_adb(args.adb, args.serial, "shell", "input", "motionevent", "DOWN", sx, sy)
            time.sleep(0.04)
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
            f"S1 {number:03d}/{len(plan)} {route[0]}->{action.anchor} "
            f"feed={actual.fed_amount}",
            flush=True,
        )
        if not args.dry_run:
            time.sleep(args.feed_delay if actual.fed_amount else args.idle_delay)
    if not model.solved(state):
        raise RuntimeError("Special 1 plan ended before every line emptied")
    if not args.dry_run:
        time.sleep(4)
        with (args.capture_dir / "replay-end-2.png").open("wb") as output:
            run_adb(args.adb, args.serial, "exec-out", "screencap", "-p", stdout=output)
    print("Special 1 static replay complete; inspect replay-end-2.png", flush=True)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
