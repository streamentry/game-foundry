# I244 — Seed sudoku

## Player
- I know sudoku, but numbers feel like homework. Plants feel like a garden.

## PM decision
- 4×4 (2×2 boxes) or 6×6 (2×3 boxes), drawn with plant pictures. Every row,
  column and box holds each plant once.
- Plants already growing sit in warm soil and can't move.
- How you play:
  - Tap a bed, then a plant from the tray.
  - The next empty bed is chosen for you, which means fewer taps for a shaky
    hand.
  - Tapping the plant a bed already holds lifts it, so there's no separate
    eraser and the plants get bigger tokens.
  - A plant that repeats glows rose: a hint, never a penalty.

## Dev
- `SudokuBoard`. The tray tokens are about 46 points (b280 tune; b279 had 40-point
  tokens and an eraser).

## Result
- Core tests: 343,066 assertions pass (the five engines and `ThingPuzzle`).
- b280 tours complete in portrait, landscape and 150% text (68 shots each).
  Every new kind is shot unsolved and solved (`18-<kind>`, `18-<kind>-solved`).
- Found in b279 and tuned in b280:
  - the sudoku tray was too small;
  - neighbouring plots shared colours;
  - a cleared mahjong table was an empty bed.
- Not yet played by adults 40+. Difficulty per tier is unmeasured.
