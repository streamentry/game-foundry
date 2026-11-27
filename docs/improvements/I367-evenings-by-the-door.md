# I367 — Evenings by the door

Run 3, batch 6.

## PM decision

From dusk, Mira, Orin and Fen gather on the path below the lamplit cottage door (I296), instead of standing by their things. It's a warm evening scene, a little community, wordless. By day they're back by their things.

## Dev

- `DayLight.NeighboursGather(phase)` (Core). Tested: true at dusk and night, false by day.
- `NeighborsOnMap` sends each neighbour to a spot below the door (row 2 and 3 cells by the path), on home only; the usual clash spacing still applies.
- The tour's dusk and night shots show it.

## Result

The garden has an evening routine. Not checked: whether the gathering spot overlaps the lanterns (I310) on gardens that have them.
