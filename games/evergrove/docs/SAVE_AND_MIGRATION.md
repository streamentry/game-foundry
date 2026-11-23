# Evergrove — save contract, corruption handling and historical compatibility

**Source of truth:** [WorldModel.cs](../Assets/Scripts/Core/WorldModel.cs), [SaveJournal.cs](../Assets/Scripts/Core/SaveJournal.cs) and [WorldStore.cs](../Assets/Scripts/Unity/WorldStore.cs). **Current snapshot schema: v4**. Filename `evergrove-world-v2.json` intentionally persists for compatibility.

## 1. What is saved

```json
{
  "version": 4,
  "mode": "living",
  "round": 1,
  "solved": false,
  "rotations": [1, 2, 3, 4, 1, 2, 3, 4, 1],
  "previous": [],
  "choices": [0],
  "hintsUsed": 0,
  "boardVariant": 1,
  "lastSolvedVariant": 0,
  "hintStep": 0,
  "hintTile": -1
}
```

**The JSON above illustrates the field shape only, NOT a validated example board**; rotation values must match the selected chapter's cell count, pipe orientation domains and protected tile constraint or `TryRestore` rejects it. Never paste that illustrative object into a real saved-game fixture.

| Field | Meaning | Important invariant |
| --- | --- | --- |
| `version` | Snapshot schema version | Current **4**; v1/v2/v3 migrations recognized |
| `mode` | `living` or `puzzle` | Strict match |
| `round` | Zero-based index of current chapter | 0..12; 12 means completed |
| `solved` | Current puzzle completed | Must equal independent `PipeRules.IsComplete` validation |
| `rotations` | Current puzzle orientation per tile | Length = current board cells (9/16), range and anchor valid; empty at completion |
| `previous` | One-step undo board snapshot | Empty or same valid grid size; empty at completion |
| `choices` | Ordered `ShadeTree=0, Bridge=1, Flowers=2` | Living: one choice per advanced chapter, length = round; PuzzleOnly: empty |
| `hintsUsed` | Number of actual helped tile corrections | First two clue stages do not increment it; nonnegative and bounded |
| `hintStep` | 0 idle; 1 row notice; 2 tile focus | Both preview stages preserve tile rotations and are saved |
| `hintTile` | Target tile index, -1 at idle | Must be currently wrong and not anchored whenever stage is 1 or 2 |
| `boardVariant` | Active puzzle geometry from prior choice | 0 base/legacy; 1 tree; 2 bridge; 3 flowers; 0 on first and completed chapter |
| `lastSolvedVariant` | Variant of previous completed board | Restores exact board on `ReconsiderLatestChoice` |

**Critical distinction:** `round` indexes 0..11 active content; player-facing UI shows 1..12. Completed `round=12` has no grid or active puzzle, but the history stays.

## 2. Example valid state transitions (conceptual)

```text
New Living game: round=0, variant=0, choices=[]
Solve first stream: round=0, solved=true, variant=0, choices=[]
Choose tree: round=1, solved=false, variant=1, lastSolvedVariant=0, choices=[0]
Solve second stream: round=1, solved=true, variant=1, choices=[0]
Choose flowers: round=2, solved=false, variant=3, lastSolvedVariant=1, choices=[0,2]
Reconsider on memory interlude:
  round=1, solved=true, variant=1, choices=[0] (same prior puzzle)
Choose bridge instead:
  round=2, solved=false, variant=2, choices=[0,1]
Final Living choice:
  round=12, solved=false, variant=0, rotations=[], choices.length=12
```

PuzzleOnly advances after a win without a choice and always uses `boardVariant=0`. The on-screen memory interlude is **not** represented by an extra persisted enum: a kill/restart will resume chapter state, not necessarily replay the interlude.

## 3. Historical migrations and file path

| Original format | Load behavior | Non-negotiable |
| --- | --- | --- |
| v1 six-chapter PlayerPrefs prototype | Imports from `evergrove:unity:world:v1` if no valid JSON primary or backup; finished chapter 6 becomes the new 4×4 chapter 7, preserving six choices | Legacy key deleted only when new file saved successfully |
| v2 twelve-chapter local JSON | Reads active board **unchanged** as variant 0; after next committed Living choice, subsequent board uses new branch variant | No retroactive rotation of an in-progress puzzle |
| v3 historical JSON | Active ecological board is preserved exactly, with staged hint reset to idle | No retroactive reorientation |
| v4 current JSON | Persists current board, last choice, hint stage and target | Reject inconsistent, anchored or already-solved hint targets |
| Unknown/faulty data | Validated primary → `.bak` → `.tmp` fallback; if all existing slots fail validation, play is in-memory only until explicit reset | Never overwrite unreadable user files automatically; platform recovery remains unverified |

The save file lives under `Application.persistentDataPath/evergrove-world-v2.json`. `.bak` contains a previously valid primary and `.tmp` is a staged candidate, which can be recovered if both main slots are unusable. The schema is **v4**; renaming the historic filename without migration would orphan existing progress.

## 4. Progressive hint durability and visible save failures

`WorldModel.AdvanceGentleHint()` persists `hintStep` and `hintTile` in schema v4. Step 1 identifies a row; step 2 focuses an unanchored, incorrect tile; step 3 aligns that tile, increases `hintsUsed` and resets the staged hint. Player rotation, Undo, Restart and chapter advancement clear the hint. Migrating v1/v2/v3 retains the active board and resets staged help to idle. The Unity UI now shows a localized warning when `WorldStore.Save` returns false; this does **not** make all device writes crash-atomic or guarantee recovery.

## 5. Write, recovery and failure sequence

**The new source-only recovery design** uses [SaveJournal.cs](../Assets/Scripts/Core/SaveJournal.cs):

1. Reject invalid v4 JSON and refuse any write while `WorldStore.RecoveryBlocked` is set.
2. Write a complete `.tmp` candidate and revalidate it before publishing anything.
3. If the existing primary is valid, copy it to `.bak` and confirm the backup matches before replacing primary. **If neither primary nor backup validates but `.tmp` does, copy `.tmp` to backup before writing a new stage.** Never rotate corrupt bytes over the last valid copy.
4. Publish the validated stage to primary and revalidate it. Keep `.tmp` if publishing fails; delete it only after success.
5. On load, prefer valid primary, then valid backup, then valid `.tmp`. Do **not** copy recovered backup bytes over the primary during loading.
6. If files exist but none can be validated, leave them untouched; show the localized save warning, block future writes, and require an explicit two-step reset. The Unity UI now pins the warning **outside** scrollable content, reserves a safe-area viewport strip and updates it immediately on both failed and successful writes, rather than hiding the warning offscreen or leaving it visible after recovery. The pure-C# journal also rejects such a commit even if a caller bypasses the Unity guard. `WorldStore.Clear()` returns success/failure and UI does not claim success after a partial deletion.

**Visible reset edge case:** if a broken newer save is present but an older valid PlayerPrefs game imports at chapter 2 or later, the blocked-reset option is now available at **any** chapter, rather than only chapter one. Whether this is comprehensible and operable on an actual phone is still unverified.

**Limitations:** This is best-effort fault recovery, not proven mobile flash durability. `File.Copy` / `File.WriteAllText` cannot guarantee atomicity or a fully durable fsync across power loss. Recovered backup may omit the most recent move. A distinct in-game notice for recovered-but-older progress remains to be designed and verified. Saves are not encrypted or synchronized.

**Test scope:** [SaveJournalTests.cs](../Tests/SaveJournalTests.cs) adds pure-C# backup and staged recovery, 100 deterministic interruption/truncation simulations and 100 stage-only-last-valid recovery simulations. These are **not** 100 actual Android/iOS force-closes, filesystem permission/disk-full tests or `SAVE-001/002` verification; two-device testing remains mandatory.

## 6. Invariants and extra QA

The loader checks: recognized version, mode, chapter bounds, arrays present, choice history length, per-choice enumerations, current/previous orientations, solved/unsolved truth and variant consistency with the most recent chapter choice. This is basic **data integrity**, not malicious tamper resistance.

Manual fault-injection matrix:

| Scenario | Expected behavior / evidence |
| --- | --- |
| Kill after rotating tile | Resume same board variant and orientation, or identify a genuine write failure |
| Kill immediately after choosing a tree | Choice exists once; next chapter variant 1 is stable |
| Kill during “memory moment” | History still saved; modal may not reappear (documented limitation) |
| Corrupt primary but valid backup | Recover backup without poisoning it; log potential loss of the most recent update; dedicated in-game recovered-progress notice remains pending |
| Corrupt all save slots | No automatic overwrite: block saving, retain unreadable files, warn visibly and require explicit reset |
| Simulate disk full/permission denied | Save failure surfaces in UI; verify actual behavior on phone and record failed attempts |
| Upgrade v1 completed prototype | Six choices preserved; unlocked chapter 7 is playable |
| Upgrade v2 mid-chapter | Exact in-progress base board remains unchanged until next choice |
| Reconsider variant 2 | Reopen prior solved variant board, not the unrotated source |
| PuzzleOnly after full run | Completed at round=12 without fabricated village choices |
| Change language | Must not mutate `WorldSnapshot` or `LocalPlaytest`, and vice versa |
| Use anchored tile | Mutator rejects tap; solver and restored snapshot honor fixed orientation |

## 7. Evolution policy

1. Only introduce a new `WorldSnapshot` version when a real schema/rule change requires it.
2. Keep explicit migration cases for every shipped historical version, including an old *partially solved* board.
3. Generate a unique test for new puzzle variants before persisting them.
4. Add round-trip, corruption, undo, reconsider, backup and power-loss regression tests; inspect platform storage behavior.
5. Document forward/backward compatibility of game upgrades and rollback; older binaries may **not** recognize v4 saves.
6. Never silently turn an old garden into a new topology merely because the code became more expressive.

## 8. Settings vs progress

World data persists to `Application.persistentDataPath`. Separate PlayerPrefs hold language override `evergrove:language-choice:v1`, sound preference, 2D/2.5D view, optional motion and the unconnected Cloudflare demo capability. Deleting/recreating a world should not be described as revoking that demo token or erasing all OS preferences. For privacy disclosures, note local-only world state and opt-in clipboard report separately.

## 9. Player-exportable garden file (format v1)

**Source of truth:** [GardenTransfer.cs](../Assets/Scripts/Core/GardenTransfer.cs), tested in [GardenTransferTests.cs](../Tests/GardenTransferTests.cs). The core only builds and parses text; it never reads the clock. The caller supplies `exportedAtUtc`.

### Envelope

```json
{"format":"mosselora-garden","formatVersion":1,"exportedAtUtc":"2026-11-23T09:00:00Z","checksumSha256":"<64 lowercase hex>","snapshot":{ ...WorldSnapshot... }}
```

| Field | Meaning |
| --- | --- |
| `format` | Must be exactly `mosselora-garden`. |
| `formatVersion` | Envelope version. Current **1**. Integer only. |
| `exportedAtUtc` | Caller-supplied timestamp string, at most 64 characters. Informational; not covered by the checksum. |
| `checksumSha256` | Lowercase SHA-256 hex of the UTF-8 bytes of the canonical `snapshot` text. |
| `snapshot` | The existing `WorldSnapshot` schema, field order identical to Unity `JsonUtility.ToJson`. |

The canonical snapshot text is produced by hand-written string building, so the same bytes come out in Unity and in the .NET 8 test project. Import parses the envelope with a strict minimal reader (no duplicate keys, no trailing content, depth limit 16).

### Import

`GardenTransfer.TryImport(text, out world, out error)` returns one of:

| Error | Cause |
| --- | --- |
| `Empty` | Null, empty or whitespace-only input. |
| `TooLarge` | More than **1 MB** (1,048,576 UTF-8 bytes). |
| `Malformed` | Not JSON, not an object, duplicate key, or missing `exportedAtUtc`/`checksumSha256`. |
| `WrongFormat` | Missing or different `format`; `formatVersion` missing, non-integer or below 1. |
| `UnsupportedFutureVersion` | `formatVersion` greater than 1. |
| `ChecksumMismatch` | Recomputed checksum differs from the stored one. |
| `InvalidSnapshot` | Snapshot missing, wrong JSON type, non-integer number, unknown key, or failing `WorldModel.TryRestore`. |

Errors are checked in that order. Import always goes through `WorldModel.TryRestore`, so every snapshot version that `TryRestore` accepts (v1, v2, v3, v4) imports and migrates to the current version on the next export.

### Versioning and compatibility promise

- Envelope `formatVersion` changes only when the envelope itself changes. A new reader must keep accepting every older envelope version.
- Snapshot versions follow the save-contract rules in sections 1 to 7. A snapshot that `TryRestore` rejects is never silently repaired.
- Older binaries that do not know a future `formatVersion` must refuse the file with `UnsupportedFutureVersion` rather than guessing.

### What the checksum does and does NOT do

- **Does:** detect accidental corruption, such as a truncated copy, a bit flip or a partial paste, of the snapshot text.
- **Does NOT:** provide tamper-proofing, authentication or anti-cheat. The algorithm is public, so anyone can edit a garden and recompute the checksum. A checksum-valid snapshot is still checked by `TryRestore`, so forged progress must also satisfy the puzzle rules, but it is not prevented.
- **Does NOT cover** `exportedAtUtc` or any other envelope field. Editing the timestamp still imports.

### Not implemented yet

**UI is NOT implemented.** There is no share sheet, file picker, clipboard action or in-app import button. Unity files in `Assets/Scripts/Unity/*` are untouched by this change. The pure core only exports and imports text. Platform file handling, storage permissions, large-file behavior on device, and the player-facing wording are unverified and still need a design and device pass.

### Evidence

The core test suite adds deterministic checks for round trips at start, mid-puzzle, hint focus, each chapter boundary, the completed 12-chapter run and PuzzleOnly. It also covers every error case, tampering, future versions, historical snapshot versions, the 1 MB boundary, and continued play after import matching the original. These are **pure C# tests only**, not Unity JsonUtility, device or file-system evidence.
