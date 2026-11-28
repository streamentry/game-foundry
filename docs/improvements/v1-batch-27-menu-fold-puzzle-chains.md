# v1 batch 27: items 11 and 38 (ticked, verified in simulator tours)

Date: 2026-11-28. Branch `feat/mosselora-v1-batch-27`, stacked on #158. Both items follow the founder's design calls of 2026-11-28 (`docs/research/decision-log.md`). Two Haiku agents built them in parallel. The coordinator added a missing cue, hooked the nursery mark, and fixed three bugs the tours found.

## 11 One menu button

The letterbox and the basket moved into the corner button's fan, alongside a new "Seed book" token. The seed count stays on home as a read-only label.

**Hub at rest (logged):** `MOSSELORA_REST controls 2 : Open menu, Play`, in portrait and landscape. The target was 2 at most.

**Moved into the fan:**
- **Letters:** shows the painted letterbox, with a sealed envelope while a letter is unread.
- **Basket:** opens the tray as the old corner button did. The open tray keeps its close control.
- **Seed book:** what tapping the purse used to open.

**Signals kept on home:**
- A letter's arrival now flies into the menu button.
- An unread letter puts a honey dot on it (badge B4).
- A cutting waiting in the basket puts the same dot there (new badge B5b, `MenuGiftCue`, cleared when planted). The coordinator added this: without it, a waiting gift had no cue on home once the basket left.

**Fixed in review:**
- **Landscape:** with seven tokens, the fan ran off the bottom of the screen and Settings could not be reached. It now fills top to bottom and wraps into more columns when the screen is short (`FanOrder.RowsThatFit`), with the last-used token still first. The tour logs `MOSSELORA_FAN_FIT tokens 7 offscreen 0` in both orientations.
- The letterbox token rendered as a black silhouette, because the token tints icons as glyphs. Its painted colours are kept now.

**Unchanged on purpose:** the neighbours' lines mention "your letterbox". The Letters token is the painted letterbox, so the lines still point at something the player sees, and those founder-reviewed lines were not rewritten.

## 38 Chains between puzzles (visible only)

- **The two links:**
  - Solving a stream path, when the garden has a thing holding plots, shows "The stream runs on and waters the garden plots."
  - Solving plots in a living garden shows "The plots send a seedling to the nursery."
- **On the card:** the line appears on your own boards' solved card with a drifting drop or sprout. With reduced motion it appears without the drift.
- **Same-day touches:**
  - Things holding plots wear a wet ring and a drop on the hub.
  - The nursery bag in the basket tray wears a small sprout. The coordinator hooked this in `EvergroveApp.MapActions.cs`.
- **The economy is untouched:** no seeds, no stock, no save change. A test checks the new files.

**Test, the cause on screen within 2 s:** the tour solves a stream path (chapter 4) and plots (chapter 6) and times the line's first visible frame from the solve: `MOSSELORA_CHAIN StreamPath shown 0.37` and `MOSSELORA_CHAIN Plots shown 0.37`, in portrait and landscape. The 1.5 s shots show both lines on the card.

**Fixed in review:** the agent's tour step opened the puzzles without home on screen, so nothing opened (logged `none`). It now opens them from the hub.

**Not confirmed visually:** in `27-chain-watered.png` the watered ring sits under an unrelated pollen toast. The ring's drawing is covered only by source audit.

## Checks

- Core tests: 5,213,501 assertions pass (`MenuFoldTests`, `PuzzleChainTests`, `FanOrderTests` extended).
- Unity compile check is clean with and without `DEVELOPMENT_BUILD`; project and docs checks pass.
- Portrait and landscape tours: 178 shots each, `MOSSELORA_QA_TOUR_DONE`.

## Open

- Whether 40+ players find the letterbox and basket in the fan is untested. The decision log names this as a stop condition.
- The cause lines are English only.
