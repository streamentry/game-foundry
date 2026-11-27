# v1 economy, batch 2: items 78 (visible), 76 (partial), 72 (partial), fruit cap

Date: 2026-11-27. Branch `feat/mosselora-v1-economy-batch-2`, stacked on batch 1 (PR #117). Implementation by two Haiku agents working on separate files at the same time, then reviewed.

## What changed

- **78 (ticked):** tapping the seed purse opens a **seed book**, a sheet built with the existing `BuildSheet`/`SheetClose` pattern (dim veil, close button, tap the veil to dismiss). It lists the last 20 seed movements, newest first, each with an icon, a plain label and a signed amount. Gains are in the forest accent, spending in ink, and nothing is red. Rows are at least 48 pt and scale with text size. The purse now has a spoken caption ("Seeds you have, N. Opens the seed book.").
- **Fruit follows the day's full replays** (fixes economy finding 2, part of #80): a replay picks fruit only if it's one of the day's first 3 full-paid replays. First solves always pick. The other session gated its pollination fruit on the same signal.
- **76, "never loses items silently" (partial, not ticked):** a fruit or jam parcel that won't fit in full is now refused with `ParcelError.LarderFull`. Nothing changes, and it can be opened later. Before, the overflow was silently dropped. The growable larder (shelves) isn't built, so 76 stays open.
- **72, price reason (partial, not ticked):** each nursery token's spoken caption gives the row's reason ("Small and quick to grow", "Takes a season to grow", "Large and slow to grow"). The shelf has no room for a visible second line at 150% text. The item's test ("players predict price order") needs players.
- `SeedReason.Season = 8` is reserved for the run-3 summer-seeds yield, which will route through `ChangeSeeds`.

## Review fixes

- Item citations corrected (#80, not #76, for the fruit rule). The seed book names `Season` instead of the generic fallback.

## Verification

- `.NET` domain suite: 1,276,248 assertions pass. New fruit-cap, LarderFull and Season tests; old clamp tests replaced by refusal tests.
- `tools/verify_evergrove_project.py`: OK.
- Unity 6.3 runtime compile: 0 errors with `DEVELOPMENT_BUILD`. The release define fails only on main's pre-#118 `qaRainbow` error until main is merged in.
- **Not run:** Unity editor, QA tour screenshots (seed book at 150% text and in landscape), device. `age fit not validated`.
