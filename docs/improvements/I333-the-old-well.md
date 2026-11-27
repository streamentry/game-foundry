# I333 — The old well

Idea 11 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after Japanese puzzle-adventure games, where puzzles are found in the world rather than in a menu.

## Player

"I want a real challenge now and then, not just my own things' boards."

## PM decision

An old well stands at the head of the river, painted from the catalogue's own village well. It holds the deepest boards, one for each stretch of river opened, a different kind each time. Each time, the well wears a play mark with that kind. Solving one pays two fruit, once. When the well is still, a tap says "New river will fill it again."

The rewards are bounded by progress, so the economy stays logical, and there's no timer or daily gate.

## Dev

- `OldWell` (Core, new) has `Waiting`, `KindFor`, `SeedFor` and `Tier`. `WorldModel` gains `WellSolved` and `TendWell` (fruit through `PickFruit`, capped as always).
- The save gains `well`, written only when non-zero, so every older file is byte-identical and a stray `"well":0` fails the checksum, as with `jam`. The import is validated: 0 to the river ceiling, living gardens only.
- The test ran red first (16 errors). It checks:
  - one board in a new garden;
  - two fruit, once, then waiting for river;
  - older files unchanged;
  - the count travels in the garden file;
  - every kind in turn;
  - always the deepest tier;
  - living gardens only.
- Unity: the well is a map thing (index 5000) on the first free land at the river's head. `OpenWell` uses the shared puzzle card. Tour shots 17j8 onward.

## Result

There's a harder board to seek out, found in the garden itself. Not known: whether "deepest" is welcome or daunting for 40+ players. The head start (I322) and hints still apply.
