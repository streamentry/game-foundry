# I240 — The nursery restocks

## Player
- Once I've bought the poplar, slot one has nothing new. Nine plants and the
  nursery is done.

## PM decision
- Each of the nine slots restocks with its next plant when you buy from it:
  five editions per slot, 45 plants in all, all different catalogue items. The
  price follows the row, as always.
- No clock: the stock is worked out from what you've bought in this garden.
  Today's nine plants are edition 0, so saves and parcels are unchanged.
- Buying now feeds the almanac (I238) with something new each time, and later
  editions will bring new kinds of puzzle (I243 on).

## Dev
- Rules and tests by a Sonnet subagent, reviewed by me
  (`Tests/NurseryRestockTests.cs`, test-first).
  - `Nursery.Slots = 9`, `Editions = 5`, `Kinds = 45`.
  - `Edition(kind)`, `Slot(kind)`, `InStock(slot, bought)`.
  - `Item(kind)` uses garden `row + 1 + edition * 3`. Edition 0 is unchanged,
    and the items are distinct within each family.
  - `WorldModel.NurseryStock(slot)`; `Buy` accepts only the kind in stock.
- `EvergroveApp`: the nursery shows one token per slot, with its current stock.
  After a purchase, the picked token moves to the slot's next plant.

## Result
- Core tests: 72,611 assertions at this step.

## Result (b278)
- Tours complete in portrait, landscape and 150% text, 58 shots each.
  - `17k`: the nursery before buying. Slot one is the poplar, with the book
    mark.
  - `17l`: after buying, slot one has restocked with a silver birch, also
    marked new.
  - `17l2`: the almanac shows 5 of 50 trees, the poplar now in colour.
- Not yet tried by players aged 40+.
