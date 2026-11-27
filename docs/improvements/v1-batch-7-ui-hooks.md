# v1 batch 7: UI for items 35, 36, 58, 74, 79 (ticked) and 52's display

Date: 2026-11-27. Branch `feat/mosselora-v1-batch-7-ui`, stacked on #132. Two Haiku agents built the UI and the coordinator reviewed it. The hook placement was agreed with the run-3 session, and 4 lines changed in `EvergroveApp.cs`.

## What players can now reach

- **#36 Technique names:** in seed sudoku, the first correct "One plant left" (naked single) or "Only place for it" (hidden single) shows a one-line note on the board. The almanac then lists the ways you've solved. Each note shows once per device, stored in PlayerPrefs.
- **#58 Weekly diary:** the hub stores a week-start save the first time it opens in each ISO week. The almanac shows this week's page from `WeeklyDiary.Write` (gains and neutral facts only). If the stored save can't be read, the diary resets quietly and play is never affected.
- **#35 Deeper boards:** the menu gains "This week's deeper boards" once the garden has a first solve. It opens a top-tier board per supported kind (Water, Sudoku, Plots, Stream, Sums, Doorstep) with "No timer, and it pays nothing." Next week's board simply replaces it.
- **#74 and #79 Stall and trades:** the menu gains "The stall". You can sell one fruit or one jar at the season's price, see today's remaining allowance, and trade with Orin, Fen or Mira. Refusals give a gentle reason ("The stall has sold enough fruit for today."). Deviations: goods are fruit, jam and seeds only (no wood or stone), and prices follow the garden's season, not the calendar.
- **#52 display:** the cottage card shows "Days in the garden: N".

## Review fixes

- The diary's week-start save is now taken on hub entry (`NotePlayedToday` → `EnsureWeekStart`), not on the first almanac visit, so the page covers the whole week.
- The two menu entries are added by one helper, `BuildStallEntries`.

## Not done or not verified

- New copy is English-only. It isn't in `GameLocalization`, so localization is still owed.
- The week-start save is a full garden export in PlayerPrefs; its size hasn't been checked on a device.
- **Not run:** Unity editor tour (portrait, landscape, 150% text) and device. These ticks mean built and tested in code, not checked in the editor or with 40+ players.

## Verification

- `.NET` suite, run as CI does: 1,619,731 assertions pass.
- Unity 6.3 runtime compile: 0 errors with both the release and dev define sets.
- `verify_evergrove_project.py` and `check_evergrove_docs.py`: OK.
