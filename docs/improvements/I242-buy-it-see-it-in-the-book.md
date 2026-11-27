# I242 — Buy it, see it in the book

## Player
- I want to see the chain on screen: buy a plant, it fills its page in the
  almanac, and the nursery shows what's next.

## PM decision
- The tour now opens the almanac right after a nursery purchase, so every build
  checks that the bought poplar shows in colour, and the nursery shot shows the
  slot restocked with its next plant and the new-to-almanac book.

## Dev
- `DevQaTour`: shot `17l2-almanac-after-buying` (menu, then Almanac).

## Result (b278)
- Tours complete in portrait, landscape and 150% text, 58 shots each.
  - `17k`: the nursery before buying. Slot one is the poplar, with the book
    mark.
  - `17l`: after buying, slot one has restocked with a silver birch, also
    marked new.
  - `17l2`: the almanac shows 5 of 50 trees, the poplar now in colour.
- Not yet tried by players aged 40+.
