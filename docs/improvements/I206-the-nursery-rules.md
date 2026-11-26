# I206 — The nursery's rules: seeds buy new plants, rows open in order

## Player
- I204 gave me seeds with nothing to spend them on.

## PM decision
- The nursery (depth spine, Layer 3): nine plant kinds in three rows, 50 / 100 /
  200 seeds. Row 1 is always open; a row opens once you own a plant from the row
  before (the founder's daughter's rule). Every bought plant holds its own
  puzzle at its row's difficulty.
- Bought plants are never taken back, only moved, so their puzzles keep stable
  identities.
- Founder decision recorded: the companion is offline noticing neighbours first;
  a real language model later in the plan (spine updated).

## Dev
- Built by a Sonnet subagent from a written brief, test-first (100 compile
  errors red, then green), reviewed here: `Core/Nursery.cs`,
  `WorldModel.Buy/MoveBought/RowOpen/BoughtSolved/SolveBought`,
  `WorldSnapshot.bought/boughtSolved`, garden-file keys with older files still
  accepted. Purchases are all-or-nothing; seeds never go negative.
- Kinds come from later gardens of the catalogue (poplar, garden gate, clover;
  pomegranate, covered bridge, peonies; aspen, forge, sunflowers).
- Finding: one garden's twelve first solves pay 280 seeds, enough for rows 1–2
  but not a row-3 plant. Kept as a long-term goal; noted in the spine for
  playtests.

## Result
- Core tests: 61,515 assertions pass (with I207's Pairs tests). No player-visible
  change yet: the nursery's screen comes next.
