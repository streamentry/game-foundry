# v1 batch 13: items 9, 46, 47 (ticked)

Date: 2026-11-27. Branch `feat/mosselora-v1-batch-13`, stacked on #140. Three Haiku agents worked in parallel on separate files, and the coordinator reviewed their work.

- **9 Restore that the player can trust.** The garden-file section opens with "Your garden is saved on this phone. Last saved today at 14:05." (or yesterday, or a date, in the player's locale format), placed above every copy, restore or replace control. `WorldStore.LastSavedLocal` is set on each successful save and read from the save file's time at launch. The kill-mid-move test is `SaveFuzzTests` (#96): every crash point loads the old or new garden. Open: the line itself is English only.
- **46 Things grow outside the garden.** `WildGrowth.Spread` lets flowers you planted by hand seed wild cells along the river. It adds one cell every 2 days played, at most 3 per flower planted and 24 in all, on free land only, never shrinking, and deterministic. It is drawn with the existing meadow flower sprites. **Review fix:** the save has no planted-at day, so spread is also capped by the flowers you've planted; a first flower planted late no longer arrives with a burst.
- **47 Build simple paths and bridges.** Stepping stones already existed (I330 decor). This batch adds undo: 20 steps per session, exact restore, refused safely if a planting now blocks the old layout. An "Undo" token sits in the Decorate row. A seeded 500-operation random walk proves every undo restores the exact prior state, and placement is always by grid cell. **Decision for the founder:** stones are land-only, so they don't yet cross water to join banks; allowing that changes the land-only decor rule.

## Verification

- `.NET` suite, run as CI does: 2,021,060 assertions pass.
- Unity 6.3 runtime compile: 0 errors with both the release and dev define sets.
- `verify_evergrove_project.py` and `check_evergrove_docs.py`: OK.
- **Not run:** editor tour and device.
