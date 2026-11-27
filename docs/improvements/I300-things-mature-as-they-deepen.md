# I300 — Things mature as they deepen

Idea 21 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after bonsai, where years of patience show in the tree.

## Player

"My favourite pear tree is harder to solve now, but it looks the same as the day I planted it."

## PM decision

A thing is drawn 7% larger for each tier its replays have deepened it, up to two tiers (14%). It never shrinks. A thing that began at the deepest tier has no further to deepen; its crown (I292) marks it instead. The growth is modest on purpose: it must never crowd a cell or cover a neighbour.

## Dev

- `ReplayBoards.Maturity(baseTier, replaysDone)` returns 1 plus 0.07 per tier deepened. It follows the tier of the thing's next board, the same depth its leaves show (I261).
- `GardenMapView.Placed.Grown` scales the drawn height.
- The test was run red first (it didn't compile). It checks 1.0 to 1.16, that size never shrinks, that a first-solve thing is its own size, and that one tier shows.

## Result

Long-tended favourites stand a little taller. Not known: whether players notice a 7% change without being told.
