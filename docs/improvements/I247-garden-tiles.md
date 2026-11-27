# I247 — Garden tiles

## Player
- My mother played mahjong solitaire every evening. Painted garden tiles would
  feel like ours.

## PM decision
- Painted tiles lie in a small terrace of up to three layers.
- A tile is free when nothing rests on it and one side is open. Blocked tiles
  sit a little shaded, so you can see which are free.
- Tap two free tiles with the same picture to lift them. A different picture
  just moves your choice: no "wrong".
- If no pair is free, the tiles quietly reshuffle.
- A cleared table shows the garden's pictures blooming in the bed, one of each
  (b280 tune: b279 ended on an empty grey bed).
- Known limit: in rare tier-3 cases a table could reach a state no reshuffle
  can free. Thousands of random playouts never reached one. If it happens, the
  bed counts as tended rather than leaving the player stranded.

## Dev
- `MahjongBoard`: layers are drawn up and left; draw order is layer, then row,
  then column.

## Result
- Core tests: 343,066 assertions pass (the five engines and `ThingPuzzle`).
- b280 tours complete in portrait, landscape and 150% text (68 shots each).
  Every new kind is shot unsolved and solved (`18-<kind>`, `18-<kind>-solved`).
- Found in b279 and tuned in b280:
  - the sudoku tray was too small;
  - neighbouring plots shared colours;
  - a cleared mahjong table was an empty bed.
- Not yet played by adults 40+. Difficulty per tier is unmeasured.
