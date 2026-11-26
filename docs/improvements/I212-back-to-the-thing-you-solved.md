# I212 — After a puzzle, home shows the thing you solved

## Player
- After solving a tree's puzzle the card closed and home showed wherever the
  view had been. The golden ring under the solved tree (I204) happened
  off-screen, so the reward for solving was a number in the corner and nothing
  in the garden.

## PM decision
- When a thing's puzzle is solved, home comes back framed on that thing and its
  golden ring widens in from nothing, then settles. Closing a puzzle without
  solving it leaves the view as it was.

## Dev
- `EvergroveApp.OpenThingPuzzle`: wraps the solve callback to note a real
  solve; on close, `justSolved` (the thing's map index) and `RefreshMapsFresh`
  (rebuild without keeping the old view). `MapItems` marks it `Newest` (the
  map frames it) and `BloomNow`.
- `GardenMapView.RingGrow`: the ring scales from 0 with a small overshoot over
  0.9 s (gentle motion only; with motion off the ring is simply there).

## Result
- `I212-back-to-it.jpg` (local only): home after a solve, before (b249: the
  solved cherry off-screen) and after (b251), 150% and sideways.
- Portrait, landscape and 150% tours: 38 shots each, complete (b251). This is also the first time the I204 golden ring is seen on screen.
