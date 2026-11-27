# v1 batch 9: item 40 (ticked), item 3 (built; its test needs players)

Date: 2026-11-27. Branch `feat/mosselora-v1-batch-9`, stacked on #136. Two Haiku agents did the work, and the coordinator reviewed it.

## 40 Mistakes shown kindly (ticked)

- **Off by default:** conflicts show only when the player asks. A lens token next to the hint token on the Doorstep, Sums and Sudoku boards (the only boards that mark conflicts) toggles "Show clashes" or "Hide clashes". The setting is per device and persists immediately (`evergrove:showClashes`, off by default).
- **Calm cue:** when on, a conflict is marked by a small ring in `EarthInk` (shape, not only colour, 4.9:1 contrast) on a warm sand tile. The old pale-pink Clash tile is gone.
- **No red:** the three remaining red error texts in `EvergroveApp.cs` (save warning, schedule error, garden-file error) now use `EarthInk`. The coordinator did this after the new test flagged them.
- **Enforced:** `KindMistakesTests` fails the build if a red-like colour appears near conflict or error code, if `Color.red` appears in the board files, or if any board marks conflicts without the setting gate.
- **Open:**
  - The lens uses the existing ring sprite as a stand-in icon; real art is still owed.
  - The top row may be tight at 320 pt and hasn't been checked in a tour.
  - The short "shiver" on a conflicting entry is motion, not colour, and is left in.

## 3 Name the garden (built, not ticked)

- **Core:** `WorldModel.SetGardenName` keeps an optional name of up to 24 text elements, measured with `StringInfo` so emoji and Indic text count correctly. Spaces are trimmed and collapsed, and control characters, markup and line breaks are refused. Restore validates fail-closed. `WorldModel.SuggestedName` gives a deterministic suggestion from `RiverNames`.
- **Postcards:** the text gains "From <name>" when a name is set, and is byte-identical when unset.
- **App:** a "Name your garden" menu entry opens a small sheet with the suggestion as the placeholder, and the cottage sign shows the name as its title.
- **Why unticked:** the item's test (the share of players who change the suggestion) needs real players.

## Verification

- `.NET` suite, run as CI does: 1,772,084 assertions pass.
- Unity 6.3 runtime compile: 0 errors with both the release and dev define sets.
- `verify_evergrove_project.py` and `check_evergrove_docs.py`: OK.
- **Not run:** editor tour of the lens and the name sheet at 320 pt, 150% text and landscape.
