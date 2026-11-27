# v1 batch 4: items 96, 53, 60 (ticked); 36, 52 (partial)

Date: 2026-11-27. Branch `feat/mosselora-v1-batch-4`. Three Haiku agents worked in parallel on separate files, and the coordinator reviewed their work.

## 96 Save is never lost (ticked, together with #123)

- **Visible "saved" mark** (`EvergroveApp.Saving.cs`): after a successful save, a small sprig with the word "Saved" appears under the menu on the hub. It fades by alpha only. With Motion off it simply appears and disappears. It shows at most once every 1.5 s, never over a board or sheet, and never blocks taps.
- **Export to Files:** "Save garden file…" opens the system share sheet with the same garden file text as "Copy garden file". iOS offers Save to Files, and Android hands it to Files or Drive. If sharing isn't available, it falls back to the clipboard.
- **Localization:** new keys `garden_save` and `saved` in English and all 29 catalogs (127 keys each), worded after each language's existing `garden_copy`. Native review is still owed (#86).
- The crash fuzz test is in PR #123, which merges first.

## 53 Missing days are made kind (ticked)

`DailyRhythmAuditTests` checks the "since you were last here" path (`DayWeather.SinceLastVisit`) for returns after 1, 3, 14 and 60 days. Every line describes weather or the garden looking after itself, and every owned counter is at least what it was before. A literal-wording scan of the return-related files allows only five neutral phrases, and any new loss-like word fails the build.

## 60 Come back for people, not prizes (ticked)

[return-reasons-v1.md](../../games/evergrove/docs/specs/return-reasons-v1.md) lists every return reason with its trigger, code location and offer, and confirms each one is tied to the garden or family and that none expires or punishes absence. The test parses that table, checks that every referenced file and symbol exists, and fails if a row is removed or turns punitive.

**Decision:** the daily replay cap (#80) is an earnings limit the game never presents as a reason to return, so it is listed under "not return reasons".

## 36 Technique names (partial)

`Core/SudokuTechnique` classifies a placement as "One plant left" (naked single), "Only place for it" (hidden single) or none, using only the game's public API. Across 1,000 boards and 17,324 placements it had 0 disagreements with the solver. The almanac doesn't show the names yet; that needs a hook in the sudoku board and almanac UI, which the run-3 session is actively changing.

## 52 No streak that can break (partial)

There is no streak field, and the audit proves that skipping 3 days costs nothing. The item also asks for a growing days-played total, which doesn't exist yet.

## Verification

- `.NET` suite: 1,475,928 assertions pass, run from the repo root as CI does.
- `tools/verify_evergrove_project.py`: OK, 127 keys.
- Unity 6.3 runtime compile: 0 errors with both the release and `DEVELOPMENT_BUILD` define sets.
- **Not run:** Unity editor tour (portrait, landscape, 150% text) and the device share sheet.
