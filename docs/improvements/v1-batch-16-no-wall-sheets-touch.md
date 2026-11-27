# v1 batch 16: items 8, 17 (ticked); 91 (built, tour scan to run)

Date: 2026-11-27. Branch `feat/mosselora-v1-batch-16`, stacked on #143. Three Haiku agents worked in parallel on separate files, and the coordinator reviewed their work and finished the touch-target fixes.

- **8 No sign-up wall, ever, for solo play.** `FamilyOffer.Visible` shows family features from day 3, or straight away if the garden already has family activity, such as a parcel from a relative. Every entry point is gated: the family sheet, the fan menu token, the journal tab, family keepsakes on the shelf, and "send this board". A one-time gentle offer appears on the hub. `NoSignupWallTests` checks the rule, a day-1 solo play through Core, and a source audit that no non-dev path uses the network, sign-in or accounts. Side effect: the dev QA family tour opens nothing on a fresh garden.
- **17 Sheets slide, never pop.** `SheetMotion` slides each sheet in from its edge, using position and opacity only, over 0.28 s (`SheetEdge` table: bottom for most, the right for the journal, the top for letters). With Gentle motion off, it's a 0.18 s fade with no movement. `SheetMotionTests` audits that only transform and opacity are animated and that every sheet goes through it. It also fixed two bugs: the name sheet never opened, and the dates sheet never fitted.
- **91 Touch targets of at least 48 pt (built, not ticked).**
  - 13 controls were raised to 48 pt, the visitor tap area grew from 24 to 48 pt, and the almanac grid drops to 6 columns when 7 would be under 48.
  - **Coordinator:** the sudoku plant tray wraps into two rows when one row would be under 48 pt. `BoardFit.MinTouch` went from 44 to 48, so sideways board tiles keep 48 pt; the smallest landscape phones may need the board to scroll.
  - `TouchTargets.Undersized` is a runtime scan for the tour, and `TouchTargetTests` statically checks 160 builders with 6 audited exceptions (board cells sized at run time, the Sums tray, text fields and map items).
  - Not ticked: the item's test is the scan in the tour, which hasn't run.

## Verification

- `.NET` suite, run as CI does: 2,139,078 assertions pass.
- Unity 6.3 runtime compile: 0 errors with both the release and dev define sets.
- `verify_evergrove_project.py` and `check_evergrove_docs.py`: OK.
- **Not run:** editor tour (the sheet slides, the two-row tray, landscape board at 48 pt) and device.
