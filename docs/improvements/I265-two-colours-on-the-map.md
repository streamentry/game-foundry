# I265 — Two colours on the map

## Player
- I'd like to see which flower bed my granddaughter and I did together.

## PM decision
- A thing solved together wears two small overlapping discs, sage and honey,
  the two players' colours, at its other shoulder (the play mark sits on the
  right).
- It's a quiet keepsake on the map: no counter, no badge.

## Dev
- `GardenMapView.Placed.Together` draws the pair.
- `EvergroveApp.MapItems` uses `world.SolvedTogether(i)` and
  `SolvedTogether(BoughtIndex + k)`.
- Tour: `17j2-solved-together-mark`.

## Result (b289)
- Tours complete in portrait, landscape and 150% text (82 shots each).
  - `17i2`: the take-turns token in sage.
  - `17j2`: the forget-me-nots wear the two-colour mark.
  - `17j3`: Fen says "The two of you matched the flower beds together. My
    bees approve."
- Core tests: 344,155 assertions pass.
- Not yet tried with two real people on one device. That's the next family
  playtest.
