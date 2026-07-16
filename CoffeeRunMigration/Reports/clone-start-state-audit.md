# Coffee Run clone/start-state audit

Scope: normal levels 0001–0100 from Coffee Run Puzzle 3.20.0, runtime config
`Default`. Source-only Special stages and gameplay Win/Lose replay are outside
this audit.

## Result

- 100 normalized records exist under `CoffeeRunMigration/Normalized`.
- 100 target files exist under `Assets/_Projects/Resources/LevelData`.
- `conversion-manifest.json` contains levels 1–100 in sequence with 100
  `Exact`, 0 `Mismatch`, and 0 `Unsupported` results.
- 100 Pizza Rush start-state captures exist under
  `CoffeeRunMigration/Captures/PizzaRush/NNNN/visual-start.png`.
- Every capture decodes successfully at 1080×1920, has non-flat image content,
  and all 100 downsampled images are distinct.
- Production-line visuals use the saved `SafeStraight`, `SafeCurvedRight`, or
  `SafeCurvedLeft` key. The factory only uses random selection for legacy data;
  converted levels contain no `LegacyRandom` production line.
- Unity Editor pipeline self-tests passed through Unity MCP, including the
  positive `Exact` conversion and the negative `Mismatch`/`Unsupported` gates.
- Unity Editor runtime integrity audit passed levels 0001–0100 as `Exact`; the
  per-level output is stored in `runtime-audit-1-100.md`.

Clone/start-state acceptance: **100/100 passed**.

Re-run the audit from the repository root with:

```bash
python3 CoffeeRunMigration/Tools/audit_clone.py
```

## Evidence boundary

This result proves that the extracted normalized layouts are represented by the
target JSON and render as valid, distinct portrait start states. It does not
claim pixel-for-pixel equality of Pizza Rush art/UI with Coffee Run, and it does
not require completing the levels during gameplay.
