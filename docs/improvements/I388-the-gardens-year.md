# I388: The garden's year

Run 3, batch 11 ("the garden's year").

## PM decision

A real garden gives different things through the year, and that's a reason to come back each season, not only each day. Once per season, a good neighbour standing in the garden gives what that season gives (I389–I391). It's collected on coming home, with a picture toast (I387).

Rules, all logical and bounded:

- **Once per season turn.** A season turn is the story's own season during the story, then each turn after it (I251).
- **The needed pairing must have been found** (I368) **and must stand in the garden now.** Move the trees apart and the grove gives nothing.
- **A full basket or a cap keeps the yield waiting** rather than wasting it.
- **One word per return.** If a pairing toast is showing, the season's gift waits for the next return.

## Dev

- `Core/SeasonalYields.cs`: `Key(world)`, `Needs(season)`, `Gives(season)` and `Why(season)`.
- `WorldModel.TakeSeasonYield(standing)`.
- New additive save field `yields` (the last turn taken, plus one), plumbed through WorldModel and GardenTransfer, and validated on read.
- `EvergroveApp.NoteSeasonYield(map)` runs after `NotePairings` when home loads.
- `SeasonalYieldsTests` cover:
  - each season;
  - found-but-not-standing and standing-but-not-found both give nothing;
  - once per season;
  - the field travels in the garden file, and isn't written while 0.
