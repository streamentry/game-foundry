# I245 — Garden plots

## Player
- Dividing a field into beds is something gardeners actually do.

## PM decision
- Split the field into rectangular beds. Each bed holds one marker, and the
  marker says how many squares the bed covers.
- Tap one corner, then the opposite corner, to stake a bed. There's no dragging,
  so it's easy on any hand.
- Tap a staked bed to lift it. An impossible bed gives a small shiver and
  nothing else.
- Neighbouring beds never share a colour (b280 tune: in b279, two yellow beds
  blurred together).

## Dev
- `PlotsBoard` with greedy bed colouring (`Tones` and `Touch`).

## Result
- Core tests: 343,066 assertions pass (the five engines and `ThingPuzzle`).
- b280 tours complete in portrait, landscape and 150% text (68 shots each).
  Every new kind is shot unsolved and solved (`18-<kind>`, `18-<kind>-solved`).
- Found in b279 and tuned in b280:
  - the sudoku tray was too small;
  - neighbouring plots shared colours;
  - a cleared mahjong table was an empty bed.
- Not yet played by adults 40+. Difficulty per tier is unmeasured.
