# I336 — Honest difficulty

Idea 13 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after Nikoli in Japan: a puzzle's difficulty comes from the logic it needs, not its size.

## Player

"Three leaves said it was hard, but it fell out in a minute. Another with two leaves took me ages."

## PM decision

For the kinds whose reasoning the game can measure, the leaves above a board (I261) now show how much reasoning that board needs, not just its size:

| Kind | Measure |
| --- | --- |
| Seed sums | Rounds of deduction until nothing more follows |
| Planting by numbers | Sweeps over the rows and columns |
| Seed sudoku | Solving rounds; any hidden single counts as two leaves at least |
| Doorstep pattern | How many squares a nearby given would mislead |

The other kinds keep leaves by tier until they can measure themselves.

## Dev

- Core (a Sonnet subagent, test-first, merged): `Leaves` (1 to 3) on `SeedSumsGame`, `NonogramGame`, `SeedSudokuGame` and `DoorstepGame`, with the raw measures exposed. Cut points come from 300 seeds per tier, and `DifficultyTests` prints the distributions.
  - Leaves differ within at least one tier for every engine, so they aren't just the tier.
  - Every leaves-3 board measures above every leaves-1 board.
  - The doorstep's first idea (distance from a given to its mirrors) turned out to be the same for every board of a tier. The subagent measured decoys instead and documented why.
- Unity: `PuzzleCard` records `boardLeaves`, and the card shows them when set.

## Result

The leaves are honest where they can be. The bigger boards still skew harder, which is true of their logic as well as their size.
