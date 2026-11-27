# I257 — Neighbours speak true to the puzzle

## Player (reviewer pass)
- Since I243 a later tree may hold a nonogram or a stream, but Mira still says
  "You freed the water under 2 of your trees". Help lines had the same flaw:
  "Nan came by and freed the water under one of your trees". The vision's
  companion speaks only true facts, so this line could be false.

## PM decision
- Neighbours speak for kinds of puzzle, not families of things:
  - Mira: water, lead the stream, planting by numbers;
  - Orin: mosaic, garden plots, garden tiles;
  - Fen: Pairs, seed sudoku.
- Each names the kind you've solved most of theirs, in words true to it. For
  example:
  - "You have led the stream through 3 meadows. I can hear them from my door."
  - "You have cleared 2 tables of garden tiles. My mother played those on
    winter evenings."
- A relative's help is told by the keeper of that puzzle's kind, in its words:
  "Nan came by and staked out a field of plots for you."

## Dev
- Test-first (`TendedKindTests`, red on the missing API):
  - `NeighbourNotice.OwnerOf(kind)`, `SolvedOf(world, kind)` and
    `KindWord(kind)`;
  - `Tended(kind, n)` and `HelpedWith(kind)`;
  - every later story thing's solve is named by its keeper and by no one else;
  - no false "freed the water" for other kinds;
  - per-kind counts add up to the solves.
- `FamilyHelpTests` now expects the kind's words, not "tree".

## Result
- Core tests: 343,541 assertions pass.

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
