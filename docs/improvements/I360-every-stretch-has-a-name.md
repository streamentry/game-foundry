# I360 — Every stretch has a name

Run 3, batch 5 (the open world).

## Player

"Stretch four, stretch five... it's all just more river."

## PM decision

Every stretch of the river has its own name, as real rivers' reaches do:
- Cottage Reach (home's);
- Willow Bend, Heron Pool, Mill Stream, Kingfisher Run...
- Far Reach.

A painted wooden plank at the head of each stretch carries its name, so the way down the valley becomes a journey of places.

## Dev

- `RiverNames.For(reach)` (Core) has twenty names, one per possible stretch (the river's ceiling). Tested: distinct, at most 18 characters, stable, and out of range keeps to the ends. "Kingfisher Shallows" was too long for the plank and became "Kingfisher Run".
- `GardenMapView` places the `stream-sign` plank per reach. The lettering uses the game's font (`SignFont`) and shrinks to fit.

## Result

The open world has places. The names are English only; they need translation with the rest of the new text.
