# Forge plans

Forge Desktop scans Markdown files below this directory. Only unchecked tasks under an exact `## Tasks` heading are candidates for import.

## Task format

```markdown
- [ ] [PR3D-001] Task title
  Indented description becomes the Forge task description.
```

IDs are case-insensitive and must be unique across every file in `docs/plans`. Acceptance checkboxes belong under another heading so Forge does not import them as tasks.

## Sequential task format

Use one sequential parent when steps depend on each other or modify overlapping assets:

```markdown
- [ ] [PR3D-SEQ-001] Complete vertical slice
  Mode: sequential
  Subtasks:
    1. [PR3D-002] Build the Blender master scene
    2. [PR3D-003] Build the board and tiles
    3. [PR3D-004] Build the rail kit
```

Forge imports this as one card and one execution context. Numbered sub-tasks run in order in the same attempt, worktree, and branch. Do not create separate top-level task cards for those same dependent steps.

## Hybrid task format

Use a hybrid parent when some steps are independent and have non-overlapping file or asset ownership:

```markdown
- [ ] [PR3D-HYBRID-001] Complete 3D art slice
  Mode: hybrid
  Subtasks:
    1. [PR3D-001] Set up shared source
       Phase: 1
    2. [PR3D-002] Model board
       Phase: 2
       Own Art/Board only.
    3. [PR3D-003] Model environment
       Phase: 2
       Own Art/Environment only.
    4. [PR3D-004] Integrate
       Phase: 3
```

Phases run in numeric order. Sub-tasks in the same phase are delegated to sub-agents in parallel, and the coordinator waits for all of them before advancing. Phase numbers must start at 1, be contiguous, and include at least one phase with multiple sub-tasks. All agents still share one Forge task, attempt, worktree, and branch.
