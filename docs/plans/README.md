# Forge plans

Forge Desktop v1.0.13 scans Markdown files below this directory. Only unchecked tasks under an exact `## Tasks` heading are candidates for import.

## Task format

```markdown
- [ ] [PR3D-001] Task title
  Indented description becomes the Forge task description.
```

IDs are case-insensitive and must be unique across every file in `docs/plans`. Acceptance checkboxes belong under another heading so Forge does not import them as tasks.
