# v1 batch 5: items 52 (ticked), 58 and 35 (rules and tests, UI to do)

Date: 2026-11-27. Branch `feat/mosselora-v1-batch-5`. Three Haiku agents worked in parallel on separate new files, and the coordinator reviewed their work.

## 52 No streak that can break (ticked)

`WorldModel.MarkPlayed(day)` counts days played as a total that only grows. A later calendar day adds one. The same day, a day set back, day 0 and puzzle-only mode change nothing. The new save fields `daysPlayed` and `lastPlayedDay` are optional, validated fail-closed (including real calendar dates) and omitted from JSON while 0. `CottageSummary.DaysPlayed` exposes the total. The app marks the day on entering the hub (`GardenMapOnHub` → `NotePlayedToday`) and saves only when a new day was counted. Tests show that skipping 3 days costs nothing owned. The total isn't displayed yet; where to show it is a design choice.

## 58 A diary page per week (rules only, not ticked)

`Core/WeeklyDiary.Write(weekStart, now, isoWeek, year)` writes up to 8 plain lines from the difference between two saves, covering gains and neutral facts only. An empty week gives "A quiet week in the garden." Seed lines use the ledger only when it fully covers the week; otherwise they give an honest net figure. The page is byte-identical after an Export/TryRestore round trip and after a garden-file round trip. Still to do: store a snapshot at the first launch of each ISO week and show the page in the almanac.

## 35 A weekly deeper board (rules only, not ticked)

`Core/WeeklyBoard` gives a deterministic top-tier seed per garden, kind and ISO week. Uniqueness is proven for 6 kinds over 52 weeks and 3 gardens (936 boards): Water, Sudoku, Sums, Plots, StreamPath and Doorstep. Six kinds are excluded, with reasons given in the code: Pairs, Mosaic, Mahjong, Nonogram, Sowing and Pieces. There's no timer and no penalty; next week's board simply replaces this one. Still to do: the UI entry point ("this week's deeper board" per favourite kind).

## Verification

- `.NET` suite, run as CI does: 1,416,116 assertions pass. The weekly proof takes about 3.4 s.
- Unity 6.3 runtime compile: 0 errors with both the release and dev define sets.
- `verify_evergrove_project.py` and `check_evergrove_docs.py`: OK.
- **Not run:** Unity editor tour, device.
