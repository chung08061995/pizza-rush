#!/usr/bin/env bash
set -euo pipefail

ADB="${ADB:-/Applications/BlueStacks.app/Contents/MacOS/hd-adb}"
LEVEL="${1:?usage: capture_level.sh LEVEL [screenshot|record]}"
MODE="${2:-screenshot}"
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
OUT="$ROOT/CoffeeRunMigration/Captures/$(printf '%04d' "$LEVEL")"
mkdir -p "$OUT"

case "$MODE" in
  screenshot)
    "$ADB" exec-out screencap -p > "$OUT/start.png"
    ;;
  record)
    "$ADB" shell screenrecord --time-limit 180 "/sdcard/coffee-run-$(printf '%04d' "$LEVEL").mp4"
    "$ADB" pull "/sdcard/coffee-run-$(printf '%04d' "$LEVEL").mp4" "$OUT/gameplay.mp4"
    "$ADB" shell rm "/sdcard/coffee-run-$(printf '%04d' "$LEVEL").mp4"
    ;;
  *)
    echo "unknown mode: $MODE" >&2
    exit 2
    ;;
esac

echo "captured level $LEVEL to $OUT"
