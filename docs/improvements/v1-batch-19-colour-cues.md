# v1 batch 19: item 92 (ticked, verified with colour-blind simulations of tour shots)

Date: 2026-11-28. Branch `feat/mosselora-v1-batch-19`, stacked on #149. One Haiku agent audited the code; the coordinator built the app, ran the QA tour on the iOS Simulator and simulated colour-vision deficiency on the screenshots.

**92 A colour-blind pass.** Every state that was carried by colour alone now also has a shape, mark or picture. [colour-cues-v1.md](../../games/evergrove/docs/specs/colour-cues-v1.md) lists 40 cue sites: 26 had only colour before, and 14 already had another cue. What was added:
- **Settings toggles** show a check when on: Haptics, Day light, Garden notes and Speak less.
- **The clash lens** shows a check when on.
- **A focus ring** marks selected or hinted cells on every board.
- **Garden Pieces:** snug fits get a ring, and placed pieces are numbered.
- **Plots:** each bed gets its own picture.
- **Checks** mark ready Sow beds, matched Pairs, Mosaic tiles in place and the chosen Leaves level.
- **Sudoku** marks given digits.

`ColourCueTests` guards 34 sites and the settings toggles.

**Test:** deuteranopia and protanopia simulations (Machado 2009 matrices) of tour shots, saved in `/Volumes/SSD/mosselora-builds/v1/cvd-b19/`. Reviewed: the water board, pieces, plots, settings, pairs, mahjong, sudoku, stream and sums. The on/off and selected states stay distinguishable by shape.

**Follow-up:** a few selection tokens (journal tabs, nursery and gift pickers, family sheet choices) differ by luminance but have no check mark yet.
