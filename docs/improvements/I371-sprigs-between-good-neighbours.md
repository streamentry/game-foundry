# I371: Sprigs between good neighbours

Run 3, batch 7.

## PM decision

On the map, a small green sprig sits between each touching pair that makes a pairing. It's wordless and small, so the garden shows its own logic without a label.

## Dev

- `Pairings.Links(map, things)` returns cell pairs (a, b, a, b, ...).
- `GardenMapView.PairLinksNext` is set for home's map. The view draws a sprig at the midpoint of each pair, with no raycast.
- Tested: links match `Between`, with no duplicates.

## Result

You can see why the garden is "good" at a glance.
