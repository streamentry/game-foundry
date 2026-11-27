# v1 batch 23: item 91's automated scan (built, run; item not ticked) and an audit of item 38

Date: 2026-11-28. Branch `feat/mosselora-v1-batch-23`, stacked on #154. The coordinator built the scan; a Haiku agent audited item 38 (read-only). Nothing is ticked: the scan works, and what it measured shows that item 91 needs design decisions.

## 91 Touch targets of at least 48 pt: the scan

The runtime check `TouchTargets.Undersized` already existed, but nothing called it.
- `DevQaTour.Touch.cs` now runs it on every root canvas at every tour shot. Each element under 48 pt (with a 0.05 tolerance for layout float noise) goes to `touch-scan.tsv` beside the shots.
- The tour's last lines report `MOSSELORA_TOUCH_SCAN small=<rows> kinds=<names>`.
- `tools/evergrove_touch_scan_summary.py` groups the rows by kind, with size ranges and an example shot.
- `TouchTargetTests` checks that every shot is scanned and the total is reported.
- Sizes are canvas units, the project's design points: the canvas scales a 414 × 896 reference, so on a 402 pt-wide iPhone 1 unit is about 0.97 pt.

**What it found (iPhone simulator, 144-shot tours, before the tolerance was added):**

| Where | Element | Short side |
| --- | --- | --- |
| Portrait | Doorstep 9×9 squares | 44 pt |
| Portrait | Nonogram beds | 47 pt |
| Portrait | Sowing basket beds | 42 pt |
| Portrait | Lookout map plots (zoomed out) | 15–26 pt |
| Portrait | "Seeds you have", "Your name" | 48 pt (float noise; now excluded) |
| Landscape | Doorstep squares | 18–23 pt |
| Landscape | Beds | 15–21 pt |
| Landscape | Stream-path fields | 26–27 pt |
| Landscape | Mahjong tiles | 25 pt |
| Landscape | Tree-puzzle tiles, trays, pair cards | 29–44 pt |
| Landscape | Tree-puzzle top buttons, close, turn and mirror, family buttons | 32–48 pt |
| Landscape | Lookout map plots | 7–12 pt |

A 150% text tour in portrait measured the same as portrait at 100%.

**Why the item stays unticked:**
- **Dense grids can't reach 48 pt on a phone.** A 9×9 board needs 432 pt for 48 pt cells, wider than any phone held upright, and landscape gives a board only the screen's height. Fixing this means a design change: zoom or pan on large boards, a bigger cell with a scrolling board, or accepting a documented exception for 9×9 grids.
- **Landscape controls shrink** because the landscape puzzle card is scaled to fit on screen (`KeepCardOnScreen`, PR #148). Fixing them is a landscape layout redesign: a side panel for the controls rather than scaling the card.
- **The zoomed-out lookout map** is a view, not a place to tap plots. Whether its plots should take taps at all is a design question.

## 38 Chains between puzzles: audit only

The Haiku audit, which the coordinator spot-checked against the files, found:
- Neither named link exists. Solving a stream path waters nothing (no plot state in `WorldModel`; "water" there means the pipe board). Solving plots does not feed the nursery; the nursery restocks only by purchase (`Nursery.InStock`, `WorldModel.Buy`).
- What does show within 2 s of a solve is the seed or fruit pill ("+N") on the card, before it closes at 2.9 s. The bloom, mist lift and fruit flight follow after the close.

Building the links means new mechanics: watered-plot state (a save change) and free nursery stock (an economy change against `economy-v1.md`). That is a design decision, not something to invent, so nothing was built.

## Decisions needed

1. **91, dense boards:** zoom or pan, bigger cells with scrolling, or a documented exception for 9×9 grids?
2. **91, landscape controls:** redesign the landscape puzzle card so its controls stay at 48 pt?
3. **38:** what "waters plots" and "feeds the nursery" should do in play, and whether free nursery stock fits the economy.
