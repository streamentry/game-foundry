# I382: The kingfisher

Run 3, batch 9.

## PM decision

Flowers by the water bring a kingfisher, which perches on a reed over the water beside the flower. It's out by day, except in winter when the river is frozen (I342). It's small and bright, and bobs now and then. It ties back to the river's own name, "Kingfisher Run" (I360).

## Dev

- `Visitor.Kingfisher`: `Welcomes` uses `HasPairing(Waterside)`; `Out` is daytime, not winter; `Why` explains it.
- `Pairings.SpotFor(..., Waterside)` gives the water square beside the flower.
- `GardenCreature` kind 3 is drawn from soft shapes: reed, blue back, orange breast, dark beak. It bobs when motion is on.
- The almanac's visitors page shows it in kingfisher blue.
- Tests cover the spot and the hours.

## Result

Seven visitors, each with a reason a gardener would recognise. Not checked on device yet. The tour garden doesn't yet hold the pairings that bring the new visitors, so a QA arrangement is a follow-up.
