#!/usr/bin/env python3
"""Replay Special 2 by observing each live production-line head colour.

The source level changes colour along its twelve long production queues.  This
helper keeps the normalized board geometry authoritative while reading only the
currently visible queue heads from Coffee Run.  It therefore does not guess or
hard-code the hidden production order.
"""

from __future__ import annotations

import argparse
import io
import json
import subprocess
import time
from dataclasses import replace
from pathlib import Path

from PIL import Image

import replay_special2 as special
import solve_levels as solver


LINE_X = (261, 323, 509, 571, 757, 819)


def pixel_colour(pixel: tuple[int, int, int]) -> int | None:
    red, green, blue = pixel
    if red > 180 and green > 110 and blue < 120:
        return special.YELLOW
    if blue > 150 and red < 180 and green < 120:
        return special.PURPLE
    return None


def screenshot(adb: str, serial: str) -> Image.Image:
    result = subprocess.run(
        [adb, "-s", serial, "exec-out", "screencap", "-p"],
        check=True,
        stdout=subprocess.PIPE,
    )
    return Image.open(io.BytesIO(result.stdout)).convert("RGB")


def line_heads(image: Image.Image) -> tuple[int | None, ...]:
    top = tuple(pixel_colour(image.getpixel((x, 343))) for x in LINE_X)
    bottom = tuple(pixel_colour(image.getpixel((x, 1500))) for x in LINE_X)
    return top + bottom


def container_present(
    image: Image.Image,
    model: solver.LevelModel,
    state: solver.ContainerState,
    index: int,
    colour: int,
) -> bool:
    cells = model.cells(index, state)
    matches = 0
    for cell in cells:
        x, y = special.screen_point(cell)
        # The center can contain a dimple; inspect a small cross around it.
        samples = (
            image.getpixel((x, y)),
            image.getpixel((x - 12, y)),
            image.getpixel((x + 12, y)),
            image.getpixel((x, y - 12)),
            image.getpixel((x, y + 12)),
        )
        if sum(pixel_colour(pixel) == colour for pixel in samples) >= 2:
            matches += 1
    return matches >= max(1, len(cells) // 2)


def candidates(
    model: solver.LevelModel,
    state: solver.State,
    heads: tuple[int | None, ...],
    attempts: list[int],
) -> list[tuple[tuple[int, int, int, int], int, int, tuple[int, int]]]:
    result = []
    for index, current in enumerate(state.containers):
        if not current.active:
            continue
        spec = model.spec_for(index, current)
        colour = spec.colours[0]
        reachable = model.reachable(state, index)
        for line_index, head in enumerate(heads):
            if head != colour:
                continue
            intake = model.intakes[line_index]
            for part_x, part_y in spec.parts:
                target = intake[0] - part_x, intake[1] - part_y
                if target not in reachable:
                    continue
                route = special.cell_route(model, state, index, target)
                score = (
                    0 if attempts[index] else 1,
                    len(spec.parts),
                    len(route),
                    line_index,
                )
                result.append((score, index, line_index, target))
    result.sort(key=lambda item: item[0])
    return result


def drag(
    adb: str,
    serial: str,
    route: list[tuple[int, int]],
    move_delay: float,
) -> None:
    start_x, start_y = special.screen_point(route[0])
    special.run_adb(adb, serial, "shell", "input", "motionevent", "DOWN", start_x, start_y)
    time.sleep(0.035)
    for cell in route[1:]:
        x, y = special.screen_point(cell)
        special.run_adb(adb, serial, "shell", "input", "motionevent", "MOVE", x, y)
        time.sleep(move_delay)
    end_x, end_y = special.screen_point(route[-1])
    special.run_adb(adb, serial, "shell", "input", "motionevent", "UP", end_x, end_y)


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--adb", default=special.DEFAULT_ADB)
    parser.add_argument("--serial", default="127.0.0.1:5555")
    parser.add_argument("--move-delay", type=float, default=0.018)
    parser.add_argument("--feed-delay", type=float, default=1.5)
    parser.add_argument("--max-drags", type=int, default=500)
    parser.add_argument(
        "--capture-dir",
        type=Path,
        default=Path("/tmp/coffee-run-control/special/0002-adaptive"),
    )
    args = parser.parse_args()

    model = solver.LevelModel(special.special_data())
    state = replace(model.initial, lines=tuple(() for _ in model.initial.lines))
    attempts = [0] * len(state.containers)
    evidence = []
    args.capture_dir.mkdir(parents=True, exist_ok=True)
    screenshot(args.adb, args.serial).save(args.capture_dir / "start.png")

    for drag_number in range(1, args.max_drags + 1):
        before = screenshot(args.adb, args.serial)
        heads = line_heads(before)
        options = candidates(model, state, heads, attempts)
        active_before = sum(container.active for container in state.containers)
        if active_before == 0:
            break
        if not options:
            before.save(args.capture_dir / f"blocked-{drag_number:03d}.png")
            raise RuntimeError(
                f"Special 2 adaptive replay blocked with {active_before} containers; "
                f"heads={heads}"
            )

        _, index, line_index, target = options[0]
        current = state.containers[index]
        spec = model.spec_for(index, current)
        colour = spec.colours[0]
        route = special.cell_route(model, state, index, target)
        drag(args.adb, args.serial, route, args.move_delay)
        time.sleep(args.feed_delay)
        after = screenshot(args.adb, args.serial)
        moved = replace(current, anchor=target)
        removed = not container_present(after, model, moved, index, colour)
        containers = list(state.containers)
        containers[index] = replace(moved, active=not removed)
        state = replace(state, containers=tuple(containers))
        attempts[index] += 1
        active_after = sum(container.active for container in state.containers)
        evidence.append(
            {
                "drag": drag_number,
                "container": index,
                "line": line_index,
                "colour": colour,
                "from": {"x": current.anchor[0], "y": current.anchor[1]},
                "to": {"x": target[0], "y": target[1]},
                "heads": list(heads),
                "removed": removed,
                "activeAfter": active_after,
            }
        )
        print(
            f"S2A {drag_number:03d}: container {index} {current.anchor}->{target} "
            f"line={line_index} colour={colour} removed={removed} active={active_after}",
            flush=True,
        )
        if drag_number % 20 == 0:
            after.save(args.capture_dir / f"progress-{drag_number:03d}.png")
            (args.capture_dir / "evidence.json").write_text(json.dumps(evidence, indent=2) + "\n")

    active = sum(container.active for container in state.containers)
    time.sleep(5)
    end = screenshot(args.adb, args.serial)
    end.save(args.capture_dir / "end.png")
    (args.capture_dir / "evidence.json").write_text(json.dumps(evidence, indent=2) + "\n")
    if active:
        raise RuntimeError(f"Special 2 adaptive replay ended with {active} containers")
    print("Special 2 adaptive replay removed every normalized container; inspect end.png", flush=True)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
