# I255 — Choose today's puzzle

## Player
- My garden has eight kinds of puzzle now, but every thing wears the same
  little arrow. Some days I want sudoku, some days the stream. I'd have to open
  each thing to find out.

## PM decision
- The play mark shows which kind each thing holds, drawn small in white on the
  teal disc:
  - water: waves;
  - Pairs: two cards;
  - Mosaic: four tiles, one lifted;
  - planting by numbers: a planted row;
  - seed sudoku: a sprout;
  - garden plots: a field in three beds;
  - lead the stream: a drop;
  - garden tiles: a small terrace.
- Choosing what to play is part of the daily ritual: a reason to look around
  the garden, and real agency for the player.
- Built from shapes the game already uses. New painted kind icons were planned,
  but the Codex image tool reported the workspace is out of credits, so this
  uses no new art for now.

## Dev
- `GardenMapView.Placed.Kind` is `PuzzleKind + 1`; 0 keeps the plain arrow.
- `KindBadge` draws the glyphs and blocks.
- `EvergroveApp` sets the kind from `ThingPuzzle` for story things, nursery
  plants, and a relative's things when visiting.

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
