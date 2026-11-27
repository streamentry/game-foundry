# I363 — Wild flowers come back

Run 3, batch 6 ([player notes](../../games/evergrove/docs/playtest/run3-player-notes-2026-11-27.md), P8).

## Player

"Early on, the garden is mostly empty grass and river. It takes a while to feel like mine."

## PM decision

As the streams are restored, wild flowers come back to the banks: two small tufts for each stream, up to 24. They're scattered along the whole river, small and behind your own things, and never where something stands. From the first stream on, the valley visibly comes alive. They rest in winter like the rest of the flowers (I338).

## Dev

- `MeadowLife` (Core, new) has `Count(streams)` and `Cells(map, count, occupied)`. Cells are bank land (next to water, diagonals included), in a fixed hashed scatter.
- `LivingValleyTests` ran red first. It checks:
  - more tufts with each stream, never a carpet;
  - on land, one per cell, as many as the count;
  - never on an occupied cell, and always the same banks.
- `GardenMapView` draws catalogue flowers at 55% of a cell. They're home's alone, kept per view so zooming and the lookout keep them.

## Result

The first minutes show life returning, not just grass.
