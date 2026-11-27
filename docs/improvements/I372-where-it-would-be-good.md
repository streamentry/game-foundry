# I372: Where it would be good

Run 3, batch 7.

## PM decision

While you hold something, whether moving a thing, planting from the basket or buying from the nursery, the free spots where it would make a pairing you haven't found yet glow softly green. This turns the rule into something you can feel. It isn't shown when every pairing is found, so a finished garden isn't cluttered.

## Dev

- `Pairings.NewAt(map, things, moving, cell, foundMask)` gives the new, not-yet-found pairings that moving one thing to a cell would make. It is pure and tested.
- `EvergroveApp.PairingSpots`:
  - finds the held thing (`mapInHand`, `heldPlanting`, `heldBought`, or a basket or nursery kind not yet placed, added as a thing with no cell);
  - checks free land cells, up to 40.
- `GardenMapView.PairSpotsNext` draws a SoftGlow under each spot. It breathes when motion is on.

## Result

Players can find good neighbours by playing, not reading. Not checked: how many spots glow on a crowded late garden. The 40 cap bounds it.
