# Coffee Run migration tracker

Source is locked to Coffee Run Puzzle 3.20.0 (version code 790), runtime config `Default`.

## Clone-only acceptance (current scope)

Per the updated acceptance criteria, gameplay replay, Win/Lose and source-only
Special stages are not release gates. A normal level is accepted for the clone
pass when its normalized source record converts as `Exact`, its production-line
visual type is deterministic, and Pizza Rush has a valid portrait start-state
capture.

- Normalized source records: **100/100**.
- Target level JSON files: **100/100**.
- Conversion manifest: **100 Exact, 0 Mismatch, 0 Unsupported**.
- Pizza Rush start-state captures: **100/100** at 1080×1920.
- Start-state capture audit: **100 valid, 100 distinct layouts**.
- Clone-pass result: **100/100 accepted**.

The detailed table below preserves the earlier gameplay-oriented fields as
historical evidence. `Solved` and source screenshot overlays are no longer
required for the current clone pass.

Status definitions:

- **Extracted**: normalized record produced from IL2CPP asset or documented ADB fallback.
- **Converted**: converter produced target JSON and normalized validation is `Exact`.
- **Visual Verified**: a valid Pizza Rush start-state capture exists and its rendered layout passed the clone audit.
- **Solved**: retained only as historical evidence; gameplay completion is not required for the clone pass.
- **Approved**: normalized conversion is `Exact`, the visual mapping is deterministic, and the start-state capture passed.

| Level | Extracted | Converted | Visual Verified | Solved | Approved |
|---:|---|---|---|---|---|
| 001 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 002 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 003 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 004 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 005 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 006 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 007 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 008 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 009 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 010 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 011 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 012 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 013 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 014 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 015 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 016 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 017 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 018 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 019 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 020 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 021 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 022 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 023 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 024 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 025 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 026 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 027 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 028 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 029 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 030 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 031 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 032 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 033 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 034 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 035 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 036 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 037 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 038 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 039 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 040 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 041 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 042 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 043 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 044 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 045 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 046 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 047 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 048 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 049 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 050 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 051 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 052 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 053 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 054 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 055 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 056 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 057 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 058 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 059 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 060 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 061 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 062 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 063 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 064 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 065 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 066 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 067 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 068 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 069 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 070 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 071 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 072 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 073 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 074 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 075 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 076 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 077 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 078 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 079 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 080 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 081 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 082 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 083 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 084 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 085 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 086 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 087 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 088 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 089 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 090 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 091 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 092 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 093 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 094 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 095 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 096 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 097 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 098 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 099 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
| 100 | Yes — Il2CppAsset | Yes — Exact | Yes — start capture | N/A — not required | Yes — clone pass |
