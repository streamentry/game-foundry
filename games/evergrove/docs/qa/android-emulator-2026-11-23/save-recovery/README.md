# Save recovery — Android emulator evidence (2026-11-23)

> **Superseded:** these fixtures went to the internal `files/` directory, which the game does not read on Android. See [`../save-recovery-external/`](../save-recovery-external/README.md) for the corrected B/C results.

**Scope: EMULATOR ONLY. Not device evidence.** Save path: `/data/user/0/com.streamentry.evergrove.playtest/files/evergrove-world-v2.json` (+ `.bak`, `.tmp`).

## Results

| Scenario | Result | Reason |
| --- | --- | --- |
| A. Clean relaunch | INCONCLUSIVE | A1 and A2 are byte-identical (sha256 7db39bdb…). No save file is created on fresh launch. |
| B. Write chapter-2 snapshot, force-stop, relaunch | INCONCLUSIVE | The iOS fixture (sha256 f4466fea…) was pushed and left unchanged. But B1 shows stream 1 "First Bend", not stream 2 with the bridge landmark. No save-related log lines were found. |
| C1. Truncated primary, valid backup | INCONCLUSIVE | Setup is correct (primary f3b2eafa…, backup f4466fea…). The screen matches every other capture, so load behaviour is not shown. |
| C2. Primary and backup corrupt | INCONCLUSIVE | No "Progress could not be saved" banner visible. Both files unchanged. No crash (no FATAL/AndroidRuntime). |
| D. Background via HOME, resume | INCONCLUSIVE | `KEYCODE_HOME`, then `am start` brought the task back. Save hashes unchanged. The screen matches every other capture. |

## Why INCONCLUSIVE
All eight screenshots (A1–D3) are byte-identical, and the live screen right now has the same hash. The captures therefore do not reflect the state under test. Either the app is not reading the injected saves on Android, or screencap returns a stale frame. This was not diagnosed. Hash-unchanged and no-crash are the only confirmed results.

## Limits
- Emulator is not a device. `am force-stop` approximates force-close, not power loss or fsync.
- Fixtures were copied from the iOS QA snapshot, not written by this Android build.
- No logcat line from WorldStore/SaveJournal was captured, so the load path is unobserved.

## Files
- `A/`, `B/`, `C/`, `D/`: screenshots and sha256 logs per scenario (`hashes-*.txt`, `sha256-*.txt`).
