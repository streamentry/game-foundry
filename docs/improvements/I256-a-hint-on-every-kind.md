# I256 — A hint on every kind

## Player
- The new boards have a lightbulb (I253), but my water tree and my Pairs table
  don't. Same card, different rules.

## PM decision
- All eight kinds get the same two-tap hint in the same place.
  - Water: a tile turned the wrong way glows, then turns home.
  - Pairs: two cards that match glow face down (memory stays memory), then
    turn over.
  - Mosaic: a tile out of place and the slot holding its piece glow, then swap.
- One rule everywhere: lightbulb once to see, twice to do. No penalty.

## Dev
- `TreePlay.Hint()` handles the classic three (`hintA` and `hintB`) and passes
  the newer kinds to their boards. The token is now always present.
- Tour: `17f2-water-hint`.

## Result (b286)
- Tours complete in portrait, landscape and 150% text (78 shots each).
  - `18-<kind>-hint`: a hint shown on each new kind.
  - The picture rule strips are above each new board.
  - `17f2`: a water hint.
  - Home's play marks show each thing's kind.
  - `17z`: Orin's help line is true to the mosaic.
- Found and fixed: since I243 the tour's "tree puzzle", Pairs and Mosaic shots
  had been opening later things that hold new kinds. The tour now picks the
  classic things from the first four chapters, so all eight kinds are covered.
- Core tests: 343,541 assertions pass.
- Not yet tried by players aged 40+.
