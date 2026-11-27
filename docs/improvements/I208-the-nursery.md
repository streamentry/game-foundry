# I208 — The nursery: spend seeds on new plants

## Player
- I had seeds from tree puzzles (I204) and nothing to spend them on, while the
  nursery's rules (I206) existed only in code.

## PM decision
- The nursery lives in the basket tray, not on a new screen: the first token in
  the tray is a seed bag that switches between "your own kinds" and the nine
  nursery plants.
- Each nursery token shows the plant's painting and a small price chip. A plant
  you can't afford yet dims its price; a closed row is faded with a lock-like
  chip. Pick one, tap the grass: it's bought and planted there, with a chime.
  It stays picked while you can afford another.
- A bought plant is like anything else you grew: tap it to pick it up, move it,
  or play its own puzzle at its row's difficulty (flowers hold Pairs).

## Dev
- `EvergroveApp`: `Occupied()` (plantings + bought) feeds the map layout and
  `PlantedAt`, so nothing ever stands on a bought plant; bought plants are map
  items `2000+k` with a golden ring once solved; `OnMapItem/OnMapLand` handle
  picking up, moving and buying; `NurseryTokens` and `PriceChip` draw the tray;
  `OpenBoughtPlay` and the shared `OpenThingPuzzle` open the right puzzle kind.
- Tour: the nursery opens with the first plant picked, then a plant is bought
  beside the newest thing.

## Result
- `I208-nursery.jpg` (local only): the tray with 60 seeds and the poplar picked;
  after buying, the poplar stands beside the wisteria and the purse shows 10,
  with every 50-seed price now dimmed; 150%; sideways with the closed rows
  faded.
- Portrait, landscape and 150% tours: 35 shots each, complete (b247).
- Not bought by hand on the device yet; not tested with players 40+.
