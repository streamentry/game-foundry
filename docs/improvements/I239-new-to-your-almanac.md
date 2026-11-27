# I239 — New to your almanac

## Player
- In the nursery I can't tell which plants I already have in my almanac.

## PM decision
- A nursery plant your almanac doesn't have yet wears a small book on the
  token's shoulder: the same book as the almanac's menu token.
- No words and no counter. It quietly points the collector toward something
  new.

## Dev
- `EvergroveApp.NewToAlmanac(token)` is a cream disc with the journal glyph in
  leaf green. It's added in `NurseryTokens` when `!almanac.Has(plant)`.

## Result (b278)
- Tours complete in portrait, landscape and 150% text, 58 shots each.
  - `17k`: the nursery before buying. Slot one is the poplar, with the book
    mark.
  - `17l`: after buying, slot one has restocked with a silver birch, also
    marked new.
  - `17l2`: the almanac shows 5 of 50 trees, the poplar now in colour.
- Not yet tried by players aged 40+.
