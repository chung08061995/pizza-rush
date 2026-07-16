#!/usr/bin/env python3
"""Audit the Coffee Run clone and Pizza Rush start-state evidence.

This check intentionally does not solve or replay levels. It verifies the
current clone-only acceptance criteria using repository artifacts.
"""

from __future__ import annotations

import hashlib
import json
import struct
import sys
from collections import Counter
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
NORMALIZED = ROOT / "CoffeeRunMigration/Normalized"
TARGET = ROOT / "Assets/_Projects/Resources/LevelData"
CAPTURES = ROOT / "CoffeeRunMigration/Captures/PizzaRush"
MANIFEST = ROOT / "CoffeeRunMigration/Reports/conversion-manifest.json"


def fail(message: str) -> None:
    raise RuntimeError(message)


def numbered_files(directory: Path, suffix: str = ".json") -> list[Path]:
    return [directory / f"{level:04d}{suffix}" for level in range(1, 101)]


def png_size(path: Path) -> tuple[int, int]:
    with path.open("rb") as stream:
        header = stream.read(24)
    if len(header) != 24 or header[:8] != b"\x89PNG\r\n\x1a\n":
        fail(f"{path}: invalid PNG header")
    if header[12:16] != b"IHDR":
        fail(f"{path}: missing PNG IHDR")
    return struct.unpack(">II", header[16:24])


def main() -> int:
    normalized = numbered_files(NORMALIZED)
    target = numbered_files(TARGET)
    missing = [str(path.relative_to(ROOT)) for path in normalized + target if not path.is_file()]
    if missing:
        fail("missing clone data: " + ", ".join(missing))

    manifest = json.loads(MANIFEST.read_text())
    levels = manifest.get("levels", [])
    if [entry.get("level") for entry in levels] != list(range(1, 101)):
        fail("conversion manifest does not contain levels 1-100 in sequence")
    statuses = Counter(entry.get("status") for entry in levels)
    if statuses != Counter({"Exact": 100}):
        fail(f"conversion manifest is not exact: {dict(statuses)}")

    visual_types: Counter[int] = Counter()
    for level, path in enumerate(target, 1):
        data = json.loads(path.read_text())
        if data.get("levelIndex") != level:
            fail(f"{path}: expected levelIndex {level}, got {data.get('levelIndex')}")
        for line in data.get("productionLines", []):
            visual_type = int(line.get("productionLineVisualType", 0))
            if visual_type not in (1, 2, 3):
                fail(f"{path}: unsupported or random production-line visual {visual_type}")
            visual_types[visual_type] += 1

    capture_hashes: set[str] = set()
    for level in range(1, 101):
        path = CAPTURES / f"{level:04d}/visual-start.png"
        if not path.is_file():
            fail(f"missing start-state capture: {path.relative_to(ROOT)}")
        size = png_size(path)
        if size != (1080, 1920):
            fail(f"{path}: expected 1080x1920, got {size[0]}x{size[1]}")
        capture_hashes.add(hashlib.sha256(path.read_bytes()).hexdigest())
    if len(capture_hashes) != 100:
        fail(f"expected 100 distinct start-state captures, got {len(capture_hashes)}")

    print("normalized records: 100/100")
    print("target level JSON: 100/100")
    print("conversion manifest: 100 Exact, 0 Mismatch, 0 Unsupported")
    print(f"production-line visuals: {dict(sorted(visual_types.items()))}; LegacyRandom=0")
    print("start-state captures: 100/100 valid, 1080x1920, 100 distinct")
    print("clone/start-state audit: PASS")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except (OSError, ValueError, RuntimeError, json.JSONDecodeError) as error:
        print(f"clone/start-state audit: FAIL: {error}", file=sys.stderr)
        raise SystemExit(1)
