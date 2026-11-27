# I223 — The river opens on screen when tending earns it

## Player
- After the story ends, I solved four of my things' puzzles. The rules say that
  opens another stretch of river (I220), but nothing happened that I could see.
  The map was just a bit longer the next time I scrolled. A reward I don't notice
  isn't a reward.

## PM decision
- The solve that opens a new stretch gets the same moment the story's streams
  get (I195): back home, the view frames where the old river meets the new
  stretch, its mist lifts over two seconds, and the chime plays. With reduced
  motion it's the chime and the opened land, with no lift.
- It's still derived from the save. The app compares the open stretches before
  and after the solve; nothing new is stored.

## Dev
- `EvergroveApp.OpenThingPuzzle` wraps the solve: it compares `OpenReaches()`
  before and after, and sets `riverOpened`. `GardenMapOnHub` then calls
  `view.LiftMist(map.Reaches - 1)` and `PlayChime()`.
- Tour: `QaHoldUnsolved` picks an unsolved tree for the fourth first solve.
  Shots `17w-river-opens` (mid-lift) and `17x-river-opened` (after).

## Result
- b268 tour: the log shows `solves before 3`. `17w` shows the new stretch
  half-veiled at the foot of home, and `17x` shows it clear, with Orin, Mira and
  Fen standing on the new bank.
- First seen on screen; the I220 rule had only been checked by tests.
