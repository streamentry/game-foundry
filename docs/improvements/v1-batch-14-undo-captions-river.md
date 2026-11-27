# v1 batch 14: items 19, 39 (ticked); 41 (partial, founder decision)

Date: 2026-11-27. Branch `feat/mosselora-v1-batch-14`, stacked on #141. Four Haiku agents did the work (three in parallel, then one follow-up), and the coordinator reviewed it.

- **39 Undo everywhere, without limit.** `Core/BoardHistory` records each move and undoes by rebuilding the board from its seed and tier and replaying, so every private counter is restored exactly. Redo replays the undone move, and a new move clears redo. Every board kind has Undo, a step count and Redo: a row under the board for the 11 non-water kinds, and the water board's existing control row. Water undo is unlimited in session; the save keeps the newest 10, as before, to keep the save small. `UndoEverywhereTests` runs a 300-move random walk with 30% undo and 10% redo for every kind and tier, and proves exact restore at every step. A latent `PlotsGame` bug (a refused draw reset `LastReplaced`) was fixed.
- **19 Long-press explains anything.** `LongPressCaption` shows a control's caption after a 0.6 s hold, suppresses the following click, and forwards drags so scrolling and page turns still work. It is attached in the shared `Button` and `IconButton` helpers and at the 15 controls built outside them. `LongPressCaptionTests` audits every interactive target in the sources: 127 controls, 9 allowlisted non-controls (veils, backdrops, the map surface), 0 uncaptioned. Any new uncaptioned control fails the build. Captions on board cells and the new Redo are English literals, so localization is still owed.
- **41 Scroll beyond the mist (partial, not ticked).** The fixed 20-stretch ceiling is gone. Stretches open by play with no gameplay cap; a safety bound of 100,000 guards arrays and ints. Names continue past 20 ("Cottage Reach II"), the old-well validator scales with open stretches, and layout is faster (cached water distance; 200 stretches lay out in under 200 ms). **But the save can't yet represent unlimited stretches:** progress comes from distinct first solves, which are capped at 12 grown things plus 200 bought plants, so at most 212 solves, or 58 stretches. **Founder decision:** lift the 200-plant nursery cap, or let replays or days open the river. Rendering is still eager (one image per stretch), which is item 50's work.

## Verification

- `.NET` suite, run as CI does: 2,134,390 assertions pass.
- Unity 6.3 runtime compile: 0 errors with both the release and dev define sets.
- `verify_evergrove_project.py` and `check_evergrove_docs.py`: OK.
- **Not run:** editor tour and device. Check especially the undo row on small phones and the landscape tool rail, which grew from 156 to about 258 pt.
