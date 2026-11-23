# Save recovery: iOS Simulator evidence (2026-11-23)

**Scope: SIMULATOR ONLY. Not physical-device evidence.** BLD-006 and SAVE-001/002 still require device QA.

- Device: "Mosselora QA iPhone", UDID 938A516A-5AD8-459E-A727-503402F3D304
- App: com.streamentry.evergrove.playtest (MosseloraPlaytest.app, Debug-iphonesimulator build)
- Save path: `<container>/Documents/evergrove-world-v2.json` (+ `.bak`, `.tmp`), container `71F927E9-537B-4FA7-9A2F-B371DB207C95`
- No game code was modified. Save files were written only inside the simulator app container.

## Results

| Scenario | Result | Reason |
| --- | --- | --- |
| A. Clean reinstall, relaunch | PASS (UI only) | A1 and A2 screenshots are byte-identical (sha256 9293682b…). Stream 1 shown both times. No save file is created on a fresh launch, so there is no save JSON to validate. |
| B. Chapter 2 Bridge state persists; force-close and relaunch | PASS, with fixture caveat | Hand-built v4 snapshot (round 1, choices [1], boardVariant 2, rotations from `stream-02-bridge` with L/E tiles turned, anchor tile 0 untouched). Attempt 1 rotated I tiles and was rejected (B1: stream 1 plus "Unreadable save retained"). Attempt 2 loads stream 2 "Bridge foundation". File hash f4466fea… unchanged after launch and after `simctl terminate` plus relaunch. B4 matches B3. |
| C1. Primary truncated, backup valid | PASS | Stream 2 restored from `.bak` (C1 screenshot). The truncated primary was not overwritten on load, which matches SAVE_AND_MIGRATION.md. A `.bak` was created by copying the valid primary, because the app had never written one. |
| C2. Primary and backup both corrupt | PASS | No crash. Shows "Progress could not be saved" banner, stream 1, and dev console "Unreadable Evergrove save retained. Saving is disabled until an explicit reset." Both corrupt files kept (sha256 f3b2eafa…). |
| D. Background via Settings, resume | PASS | Stream 2 state; D3 byte-identical to B3 (sha256 cc656d09…). Save hash unchanged. |

## Limits
- Simulator is not a device. Filesystem, memory pressure and OS suspension differ.
- `simctl terminate` approximates force-close. It is not power loss and does not test fsync.
- Rotations were not made by touch. No tap automation was used; state came from fixtures written into the container.
- The C-scenario `.bak` was created by a test copy, not by app writes. The app's own backup rotation was not exercised.
- Fixture rejection in Scenario B attempt 1 was not diagnosed beyond the observed symptom; the likely cause is invalid rotations for I-tiles.
- Recovered-progress notice and the actual reset flow were not tested.

## Files
- `A/`: A1, A2 screenshots and sha256-screenshots.txt
- `B/`: snapshot-v4-chapter2-bridge.json (attempt 2 fixture), attempt1-rejected/, B1–B4, sha256 logs
- `C/`: C1, C2, sha256 before/after logs
- `D/`: D1–D3, sha256 before/after logs
