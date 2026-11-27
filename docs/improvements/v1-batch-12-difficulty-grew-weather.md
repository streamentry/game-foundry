# v1 batch 12: items 33, 34, 48 (ticked)

Date: 2026-11-27. Branch `feat/mosselora-v1-batch-12`, stacked on #139. Three Haiku agents worked in parallel on separate files, and the coordinator reviewed their work.

- **34 Difficulty the player chooses, honestly labelled.** Every one of the 12 puzzle kinds has a gentle level. Level n maps to generator tier n−1, and for the four logic kinds (Nonogram, Sudoku, Sums, Doorstep) the board is chosen so its measured leaves equal the level. The chooser says "Leaves" for logic kinds and "Size" for the rest (honesty rule I336), and is remembered per kind on the device. `DifficultyChoiceTests` proves every kind's gentle level gives valid boards that are gentler by the kind's own measure. **Review fix:** the re-deal applies only to your own things; a relative's shared board, the old well and the weekly deeper board stay exactly as dealt.
- **33 Garden layouts that remember.** "How my garden grew" (or "How <name> grew") is an almanac card listing each found pairing in plain words, with a count. It is byte-identical after an Export/TryRestore round trip and after a garden-file round trip. Limit: the save records which pairings were found, not where, so the lines describe the pairing, not the exact layout.
- **48 Weather moves down the river.** `RiverWeather` reads upstream rain off tomorrow's home weather, so the forecast is true by construction and no existing day's weather changed (36 frozen values checked). On those days Orin says "Rain on the hills upstream today. It'll reach the garden by tomorrow." Same seed, same forecast, over 1,096 days × 6 gardens.

## Verification

- `.NET` suite, run as CI does: 2,003,352 assertions pass.
- Unity 6.3 runtime compile: 0 errors with both the release and dev define sets.
- `verify_evergrove_project.py` and `check_evergrove_docs.py`: OK.
- **Not run:** editor tour of the leaves chooser and the almanac card, and device.
