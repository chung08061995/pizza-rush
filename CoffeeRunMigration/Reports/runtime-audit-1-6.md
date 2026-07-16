# Existing Pizza Rush level audit

Audit command: `MyMenu > Coffee Run > Audit existing Pizza Rush levels 1-6` (Unity 6000.4.10f1 batchmode).

| Level | Result | Findings |
|---:|---|---|
| 1 | Exact runtime integrity | — |
| 2 | Exact runtime integrity | Deterministic visual key repaired from line rotation. |
| 3 | Exact runtime integrity | Deterministic visual key repaired from line rotation. |
| 4 | Exact runtime integrity | Timer set to 180 seconds; duplicate grid cell removed; deterministic visual key repaired. |
| 5 | Exact runtime integrity | Timer set to 180 seconds; deterministic visual key repaired. |
| 6 | Exact runtime integrity | Timer set to 180 seconds; deterministic visual key repaired. |

`levelIndex: 100` in levels 4–6 is tolerated by the audit because `LevelData.Load` replaces it from the zero-padded filename at runtime. The final audit is now exact for all six levels; visual and no-skill gameplay QA remain separate gates.
