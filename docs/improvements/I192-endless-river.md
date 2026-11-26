# I192 — One endless river: new land opens downstream as you restore streams

Date: 2026-11-26. Branch: feat/mosselora-world-first.

## Founder decision (2026-11-26)

> "Should the river be infinity vertical (a fictional world) and people just live around the river?" … "Yes, let's go with this option."

The open world is **one endless river running down the screen**. Players only swipe up and down: no getting lost in a 2D world. Upstream is where you've been; new land opens downstream as you restore streams; neighbours' cottages line the banks.

(Planned as I193 in I189's plan; done before the icon settings because the founder chose it mid-run.)

## Player (b216)

The garden map was one fixed painting. After a few streams it filled up, and with a 200-item catalogue there was nowhere left to grow. The lower valley was dry straw that slowly greened, but the land itself never grew.

## PM decision

- The map is now **reaches of river**: the painting, then the painting mirrored top to bottom, then the painting again, and so on. Mirroring means the creek, the banks and the footpath join with no seam; where two mirrored cottages meet, a little hamlet of two thatched houses appears: the neighbours live along the river.
- **A new reach opens every three streams** (1 at the start, 5 once the garden is finished). Each stream's tree, flower or building grows by default in the reach its stream opened, so the garden grows down the river with the year. Things you placed yourself never move, and opening a new reach never moves anything.
- **Below the open river, the next reach waits in a morning mist**, dry and not yet plantable: a promise of more land, not a locked door.
- The old dry-straw wash is retired in the living garden (new land arrives as a reach instead).

## Dev

- `Core/GardenMap`: `River(columns, reachRows, reachMask, reaches)` stacks the mask, mirroring odd reaches; `ReachRows`, `Reaches`, `ReachesFor(restored, completed)` (`StreamsPerReach` 3, `MaxReaches` 5). `Score`'s band puts chapter *c* in reach *c / 3* when the map is a river.
- Tests (`RiverMapTests`): stacking, mirroring, the third reach repeating the first, the unlock schedule, refusing zero reaches, every item of a 12-stream garden in its own stream's reach, and items not moving when the river grows.
- `GardenMapView`: one `RawImage` per reach (odd ones flipped by UV) under a transparent tap catcher; `teaser` draws the top 55% of the next reach (flipped as it would be) with a straw wash and a fade to cream. Width covers the window over all reaches.
- `EvergroveApp`: `LoadGardenMap` builds the river for the garden's progress and rebuilds when a reach opens; every map view passes `RiverGoesOn(map)`; `GreenedShare` is 1 in the living garden; the hub map is 1.3× wide (less sideways panning).

## Result (b217; portrait, landscape and 150% tours, 24 each, all complete)

- `I192-endless-river.jpg`: the finished garden on home scrolled to the wisteria in reach 5, with the hamlet of two cottages on the right bank; stream 2 over the garden with the mist below the board; autumn and winter boards over the river; the memory card.
- `I192-fresh-and-150.jpg`: a fresh garden on home: one reach, green, with the next reach in mist under the play token; the 150% home and board.

.NET tests pass (52,764 assertions + the new river tests). Project verify passes.

**Next:** watch a new reach open (the mist lifts after the choice that unlocks it); free planting of what you've unlocked; pinch to zoom.
