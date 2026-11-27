# I238 — The almanac counts everything you grow

## Player
- I bought a poplar from the nursery, and Nan's clover cutting is planted by
  the creek. Both are things in the almanac's 50 trees and 50 flowers, but the
  almanac still shows them as grey silhouettes. It only counts the story's
  twelve.

## PM decision
- Nursery plants and planted family gifts are catalogue things too: they enter
  the almanac when they're planted.
- The collection becomes a long goal fed by everything you do: story gardens,
  the nursery, and family.

## Dev
- `EvergroveApp.RecordInAlmanac` also walks `world.Bought` (bought plants and
  planted gifts). It is called after `Buy` and `PlantGift`.

## Result (b278)
- Tours complete in portrait, landscape and 150% text, 58 shots each.
  - `17k`: the nursery before buying. Slot one is the poplar, with the book
    mark.
  - `17l`: after buying, slot one has restocked with a silver birch, also
    marked new.
  - `17l2`: the almanac shows 5 of 50 trees, the poplar now in colour.
- Not yet tried by players aged 40+.
