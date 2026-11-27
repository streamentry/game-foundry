# I253 — A hint on every new board

## Player
- Garden plots: numbers in a grid, and I have no idea where to start. The water
  board teaches itself; the five new kinds don't.

## PM decision
- A lightbulb token in the card's top row, on the five new kinds.
  - First tap: the next sensible move glows (a cell, a bed, a pair, the next
    field).
  - Second tap: it's made for you.
- Never a penalty, never a counter, and the seeds are the same. A hint teaches
  the rule by showing it in play, and a stuck player always has a way forward.
- The hints come from the engines' tested hint functions, so every hint is a
  correct move:
  - nonogram: `HintCell`, a cell the line-solver can deduce;
  - sudoku: `HintCell`, a single;
  - plots: `HintBed`, a forced bed;
  - stream: `NextHint`;
  - tiles: `HintPair`.

## Dev
- `TreePlay.IPuzzleBoard.Hint()`; the token appears when a board is hosted.
- Each board keeps the shown move and makes it if the same hint is asked again.
- Tour: `18-<kind>-hint`.

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
