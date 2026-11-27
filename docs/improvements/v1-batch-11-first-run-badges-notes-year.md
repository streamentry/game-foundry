# v1 batch 11: items 4, 16, 57, 59 (ticked)

Date: 2026-11-27. Branch `feat/mosselora-v1-batch-11`, stacked on #138. Three Haiku agents worked in parallel on separate files, and the coordinator reviewed their work.

- **4 Mira explains one thing, once.** A new player sees 20 words before the first puzzle, under the 40-word goal, and Mira speaks once (one bubble, two short sentences). No copy needed changing. `FirstMinutesAuditTests` counts the first-run text from source, with a canary per literal, so new or moved copy fails the test until the count is updated.
- **16 Badges that mean one thing.** Every badge is listed in [badges-v1.md](../../games/evergrove/docs/specs/badges-v1.md): what triggers it, what clears it, its colour, and that none shows a count. Fixes:
  - The menu dot never cleared when the menu was opened; now it does, and the book keeps its own dot until the book is opened.
  - The gift ribbon was red; it's now honey.
  - The hint level pips, which read as a count, are removed.
  
  A source audit fails on any new badge-like symbol that isn't listed.
- **57 Notifications only for real arrivals.** Notes now fire only for an unopened letter or cutting: opt-in, at most one a day, and they stop after three go unopened. Reminders about the river mist, unsolved things and favourite boards are no longer scheduled. `GardenNote.ForWeek` is the single pure plan, and `NotificationWeekTests` proves a 7-day plan has at most 7 entries, at most 1 per day, and only arrivals. `return-reasons-v1.md` and the notifications proposal are updated to match.
- **59 The year has a shape.** The first year's four seasons each open one existing gift (spring seedling, summer seeds, autumn fruit, winter jam), and the almanac says what each season brought. `YearShapeTests` walks a garden through the year and checks each new chapter is usable and stays open. After the story, seasons advance only through first solves of bought plants; this is proposed as a later decision, not changed.

## Verification

- `.NET` suite, run as CI does: 1,801,979 assertions pass.
- Unity 6.3 runtime compile: 0 errors with both the release and dev define sets.
- `verify_evergrove_project.py` and `check_evergrove_docs.py`: OK.
- **Not run:** editor tour and device. The improvement logs I273–I277 now describe notification kinds that were removed.
