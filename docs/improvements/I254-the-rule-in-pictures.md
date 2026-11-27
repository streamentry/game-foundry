# I254 — The rule, in pictures

## Player
- The hint shows a move, but not the rule. I'd like one look at how it works
  before I start, without reading a paragraph.

## PM decision
- The first three times you meet a kind, a small picture strip sits above its
  board. No words: numbers and pictures read in every language.
  - Planting by numbers: "3", arrow, three sprouts in a row, then empty soil.
  - Seed sudoku: four different plants in a row, arrow, a check.
  - Garden plots: "4", arrow, four squares; "2", arrow, two squares.
  - Lead the stream: spring, arrow, three water squares, arrow, the pond.
  - Garden tiles: two tiles with the same picture, arrow, a check.
- After three meetings it steps aside and the board gets the room. It's counted
  on the device, not in the save.

## Dev
- `BoardKit.FirstTimeRule(card, kind, parts)` returns the height it used, so
  each board fits beneath it.
- Helpers: `RulePicture`, `RuleSquare`, `RuleNumber`, `RuleArrow`.
- The tour resets each kind's count in `QaOpenKind`, so the strip is checked on
  every build.

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
