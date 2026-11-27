# I246 — Lead the stream

## Player
- The water puzzles turn pipes. This one lets me draw the stream myself.

## PM decision
- From the spring (waves), tap the next field to lead the water there, around
  the stones, until every field is watered and the stream reaches the pond.
- Tap any watered field to draw the stream back to it.
- The pond takes the water only once every field has some. Too soon, the pond
  shivers.
- The water shows as channels joining cell to cell, the river's own teal.

## Dev
- `StreamBoard`: water bars per cell edge. The head marker hides once the
  stream reaches the pond.

## Result
- Core tests: 343,066 assertions pass (the five engines and `ThingPuzzle`).
- b280 tours complete in portrait, landscape and 150% text (68 shots each).
  Every new kind is shot unsolved and solved (`18-<kind>`, `18-<kind>-solved`).
- Found in b279 and tuned in b280:
  - the sudoku tray was too small;
  - neighbouring plots shared colours;
  - a cleared mahjong table was an empty bed.
- Not yet played by adults 40+. Difficulty per tier is unmeasured.
