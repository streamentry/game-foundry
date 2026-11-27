# I320 — Under the mossy stone

Idea 24 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after the secrets under stones in classic action-adventure games.

## Player

"The new stretches of river are pretty, but there's nothing to find there."

## PM decision

Every stretch of river after the first hides one mossy stone on its bank. Tap it and you find what was lost under it long ago: a cutting, or a little made thing from Orin's family. It goes to your basket, with a note from Fen. Each stone is found once, and there's one per stretch, so exploring the river is rewarded without a hunt. The stone is always in the same place; if you plant on its spot, it sits further along the bank. The find comes from a nursery row you already have open.

## Dev

- `RiverSecrets` (Core, new) has `CellFor(map, reach, occupied)` and `CuttingFor(world, reach)`. The test ran red first (6 errors). On the shipped mask it checks:
  - none on the first stretch;
  - land, inside its own stretch, on the bank;
  - always the same cell, and it moves along the bank when occupied;
  - one per stretch;
  - the find is from an open row.
- Unity:
  - Home places unfound stones as map things with index 3000 plus the stretch, using the painted `stone-spring` art.
  - `FindSecret` uses `ReceiveNeighbourCutting` (I315); a full basket keeps it under the stone.
  - "Found" is a per-garden flag on the device.

## Result

The river rewards a wander. Not known: whether players notice a stone is tappable. It has no mark, on purpose; watch for it in the playtest.
