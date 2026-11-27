# SC07: The player check, prepared

Smart companion, iteration 7 of 7 ([spec](../../games/evergrove/docs/specs/proposals/smart-companion-2026-11-27.md), step 7).

## Player note

Nothing in the game changes. This iteration prepares the one test that can tell us whether the companion works for people.

## PM decision

The spec's falsification test is a week with the bank for 5–10 adults aged 40+, then asking what the neighbours said. The [companion week kit](../../games/evergrove/docs/playtest/companion-week-kit.md) turns that into a protocol:

- bands 40–49, 50–64 and 65+, reported separately;
- a 20-minute uncoached day-1 session;
- six days at home with no reminders;
- a day-7 call asking for recall, speaker, truth, repetition and pressure, plus an old-versus-new card.

The spec's three stop rules are written into the results table: no recall, repetitive within the week, or the old lines preferred. Any report of pressure or surveillance removes or softens that kind. Thresholds are `TO PREDECLARE` and must be fixed in the decision log before anyone is recruited.

**I can't run the human test, and haven't.** Age fit is `not validated`.

## Dev

- `games/evergrove/docs/playtest/companion-week-kit.md`, linked from the playtest `README.md`.
- A decision-log entry, "Mosselora smart companion SC01–SC07", with its preflight table and what never to claim.

## Result

- `tools/check_evergrove_docs.py`: OK.

## Not verified

- The test itself: no participants, no data.
- The kit's own limit: recall can't yet be checked against what was actually said, because the heard-list stays on the phone and nothing exports it. A consented, dev-only export is a follow-up.
