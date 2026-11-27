# I207 — Flowers hold Pairs

## Player
- Every thing's puzzle was the same water board. The founder asked for other
  gentle puzzles that adults 40+ already love, in our themes. Card-matching
  memory games are familiar, wordless and calm.

## PM decision
- Puzzle kind follows the family (depth spine): flowers hold **Pairs**. Turn
  two cards; a match stays up; a miss stays up until your next tap so you can
  study it. No timer, no turn limit, no penalty.
- The cards are paintings from your own garden's catalogue, so each table is
  a little album of things you know. Sizes by tier: 4, 6 or 8 pairs.
- Same card, same seed rewards, same close-by-itself finish as the water board.

## Dev
- `Core/PairsGame.cs`: deals from a seed (its own xorshift shuffle, the same on
  every device); `Flip` returns First / Match / Miss / Ignored; counts turns.
  Written before its tests this time (not test-first); `Tests/PairsTests.cs`
  covers each picture exactly twice, the same deal per flower, varied deals,
  a miss showing until the next tap, kept cards locked, and a perfect game
  solved in one turn per pair.
- `TreePlay.OpenPairs` and `BuildPairs/TurnCard/PaintPairs`; the solve path is
  shared (`Finish`). Sideways the card size follows the screen height.
- `EvergroveApp.OpenTreePlay` routes flowers to Pairs; `PairsPictures` picks
  distinct catalogue paintings starting from the flower's place.
- Tour: a flower is picked up, its table opened, solved by memory.

## Result
- `I207-pairs.jpg` (face down, solved with +40, 150%, sideways) and
  `I207-pairs-sideways.jpg` (after the fit fix), local only.
- Found and fixed: sideways, four rows of cards ran off the screen.
- Portrait, landscape and 150% tours: 33 shots each, complete (b245). Core
  tests: 61,515 assertions pass.
- Not played by hand yet (the tour solves it by code). Not tested on a real
  device or with players 40+.
