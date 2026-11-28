# v1 batch 28: item 91 (ticked; the tour's touch scan finds nothing under 48 pt in either orientation)

Date: 2026-11-28. Branch `feat/mosselora-v1-batch-28`, stacked on #159. The coordinator built this item directly, following the founder's design calls of 2026-11-28: zoom for dense boards, and a side panel for landscape controls. It took four tour rounds.

## What changed

- **Dense boards zoom** (`TreePlay.Zoom.cs`, rule `Core/BoardZoomRule`):
  - When the card settles, its board grid is found (the parent of five or more live cells) and its smallest cell measured. Under 48 pt, the grid goes into a clipped view that opens whole, as a picture: the cells take no taps, and the picture does.
  - One tap zooms in around that spot, so every cell is at least 48 pt (×1.36 in portrait, ×2.59 in landscape for 9×9). The zoomed board pans by dragging.
  - A 52 pt "See the whole board" token zooms back out, and a solved board shows whole again. Only transforms move.
- **Sideways, the puzzle card splits** (`TreePlay.Sideways.cs`):
  - The board sits on the left. A 260 pt panel on the right holds the top row (the thing, hint, take turns, seeds), the trays, undo and the leaves chooser, at full size.
  - The board is built for the room it really has, since the top row's height is freed for it.
  - When the panel is still too tall, its lowest controls move under the board rather than the card shrinking.
  - The chain line joins the panel too.
  - `TreePlay.cs` became `partial` and did not grow: every hook sits on an existing line.
- **The family sheet scrolls its body** inside the card when the screen is short, instead of shrinking to 75% (which took its 52–56 pt controls under 48).
- **The scan counts only targets a finger can reach.** A raycast at each target's centre must land on it. Targets under something else are reported as `covered` but not counted: the quiet garden's full-screen "tap to come back" layer covers the lookout map's plots (22), and a zoomable board's picture covers its cells.

## The test: the automated scan in the tour

| Tour | Reachable targets under 48 pt | Covered |
| --- | --- | --- |
| Batch 23, portrait | 393 (94 kinds) | not measured |
| Batch 23, landscape | 1,001 (200 kinds) | not measured |
| **Batch 28, portrait** | **0** | 22 (lookout plots) |
| **Batch 28, landscape** | **0** | 22 (lookout plots) |

Zoom states (`MOSSELORA_ZOOM`): the doorstep, sudoku and nonogram boards are zoomable and toured whole, then zoomed in (`28-zoom-*-whole.png`, `28-zoom-*-in.png`). The scan runs at every shot, so both states are measured.

## Bugs the tours found and fixed

1. **Clipped boards.** Most boards sit in a "bed" placed by hand, with no layout group. The zoom view had only a `LayoutElement`, so it stayed Unity's default 100 × 100 and clipped the pieces, sowing and doorstep boards. It now takes the grid's own anchors, position and size, and measures the cells' real bounds after their layout has run.
2. **The sowing beds were missed.** Seven beds at 39–42 pt wide were under the old nine-cell threshold; five or more now make a board.
3. **The landscape card still shrank slightly** (43–47 pt controls) when the panel was too tall. Panel balancing fixed it.
4. **The family sheet** shrank its controls in landscape. It scrolls instead.

## Checks

- Core tests: 5,213,778 assertions pass (`BoardZoomTests`, and `TouchTargetTests` registers the zoom picture's tap handler).
- Unity compile check is clean with and without `DEVELOPMENT_BUILD`.
- Portrait and landscape tours: 183 shots each, `MOSSELORA_QA_TOUR_DONE`. Every board's shots were reviewed in both orientations.

## Open

- In landscape a whole 9×9 board shows small (cells about 18 pt) until zoomed. Whether 40+ players find tap-to-zoom natural is untested.
- Not covered: a scan at 150% text, and non-hub sheets the tour doesn't open.
