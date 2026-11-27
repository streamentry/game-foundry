# I286 — A bigger bloom

## Player
- When I clear the garden tiles (I247), the bed fills with the pictures I
  matched, but at six to a row they're small on a phone. The reward should be
  something to look at.

## PM decision
- Four to a row. With nine or twelve faces that's three rows of larger
  paintings, each blooming in turn.

## Dev
- `MahjongBoard.Lock`: `across = min(faces, 4)`.

## Result
- b301 tours complete in portrait, landscape and 150% text (89 shots each).
- Core tests: 349,144 assertions pass.
