# I91 — Hints point without hiding the water

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

## Problem (found by using the hint live on stream 1, b67)

"Guide me" has three requested steps: notice a row, focus a tile, then turn that tile. Two of them misled the player, and two left stale marks.

1. **Step 1 repainted the row's tiles honey-sand.** That erased the blue "water reaches here" colour on tiles that already flow. In the b67 shot, the middle row's left tile, which carries water, turned sand, so the hint hid the progress the player had made.
2. **Step 2 filled the target tile with honey and relied on a breathing ring that sat under the teal goal glow.** When the target was the garden, as on stream 1, no honey ring was visible at all.
3. **A move ends the hint in the core (`ClearHint`), but the tap refresh path never removed the rings.** The hint button's two progress dots also stayed until the next full rebuild.

## Changes (b68–b70)

- **Colours:** `MosseloraTheme.TileFill` no longer lets a hint override tile colour. Tiles always show dry, wet, solved or rooted.
- **Step 1:** thin honey rings (75% alpha) around each tile of the hinted row, drawn over the tiles. Water colours stay visible.
- **Step 2:** a bolder honey ring on the target tile, still breathing only with gentle motion. A hinted tile no longer gets the teal goal glow, so the hint is never hidden.
- **Cleanup:** row rings, the focus ring and the hint button's level dots are collected in `hintMarkers`. The first refresh after a move, when `HintStep == 0`, hides them all.
- **Unchanged:** the core hint logic, the planner, the spoken clues ("Notice row 2…", "Row 2, column 2: compare the open ends.") and step 3, which turns the tile.

## Evidence (live, iOS Simulator, stream 1)

`I91-honest-hints.png`, left to right:

1. **b67 step 1:** the row is painted sand, and the wet left tile lost its blue.
2. **b69 step 1:** honey rings on the row, and the left tile is still blue.
3. **b69 step 2:** a honey ring on the garden tile, with no teal glow.
4. **b70 after a move:** no rings, and the hint button is back to a plain bulb.

The verifier passes, and the b68, b69 and b70 QA tours each captured all 19 screens.

## Still unverified

- Hint step 3, which turns the tile, was not re-run; its code is unchanged.
- No players aged 40+ have used the hints (m3-session-kit P1–P9).
