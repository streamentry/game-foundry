# I220 — After the story, the river keeps going

## Player
- After the twelfth stream the river stopped at five stretches, with the mist
  gone below the last one. The garden I'd keep coming back to had an edge.
- Founder requirement (2026-11-26): the map is vertically infinite and
  extendable; not all at once, but later, without breaking the game.

## PM decision
- The story still opens one stretch every three streams, five in all.
- After the story, the river keeps opening downstream as you **tend** what
  you've grown: every 4 things whose puzzle you've solved for the first time
  (grown or bought from the nursery) open one more stretch, up to 20 for now.
  The mist stays below the last open stretch while more can come.
- So the daily loop has a horizon: solve → seeds and new plants → more river,
  more land to plant. No clock, nothing to lose.
- Future-proof by design: the number of stretches is derived from the save
  (story progress and puzzles solved), never stored. Raising the ceiling later
  opens more river in every garden with no migration; saves already accept map
  cells up to 10,000 (about 66 stretches).

## Dev
- Test-first (`Tests/RiverGrowsTests.cs`, red on the missing overload):
  `GardenMap.ReachesFor(restored, completed, puzzlesSolved)`,
  `SolvesPerReach = 4`, `ReachesCeiling = 20`; `WorldModel.PuzzlesSolved`.
  Tests: the story rule unchanged before completion, one stretch per four
  solves, the ceiling, every story thing's default place unchanged as the river
  grows, new stretches have land, replays don't count.
- `EvergroveApp`: the home, board, memory and visit maps use it; the misty
  teaser shows until the ceiling.

## Result
- Core tests: 71,289 assertions pass. Tours complete (b266, b267); the extra stretches only open past the twelve-stream story, which the tour garden reaches without first solves, so the new rule is checked by tests, not yet on screen.
