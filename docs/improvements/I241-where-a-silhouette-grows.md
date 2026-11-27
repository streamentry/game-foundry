# I241 — Where a silhouette grows

## Player
- The almanac's grey silhouettes are a mystery with no thread to pull. Where
  would I ever find a mulberry tree?

## PM decision
- Tap a silhouette: a small note says where it grows, and the name stays a
  surprise.
  - "In the nursery now": it's in stock today.
  - "Comes to the nursery in time": a later edition.
  - "In a garden yet to come": only a new garden grows it.

## Dev
- Test-first (`NurseryRestockTests`):
  - `Nursery.KindOf(GardenItem)` finds every nursery plant by its item;
  - every other catalogue item returns -1;
  - null is -1.
- `EvergroveApp.AlmanacPage`: unfound slots take taps and show the note
  (`RuleToast`).

## Result
- Core tests: 72,658 assertions pass.

## Result (b278)
- Tours complete in portrait, landscape and 150% text, 58 shots each.
  - `17k`: the nursery before buying. Slot one is the poplar, with the book
    mark.
  - `17l`: after buying, slot one has restocked with a silver birch, also
    marked new.
  - `17l2`: the almanac shows 5 of 50 trees, the poplar now in colour.
- Not yet tried by players aged 40+.
