# I370: The keepers hint at good neighbours

Run 3, batch 7.

## PM decision

Each keeper hints only at the pairing in their own line of work, and only until it's found. This uses the "AI companion in subtle ways" idea: the neighbours notice what you haven't tried yet.

- Mira (trees) talks about a grove.
- Orin (builds) talks about building by the water.
- Fen (flowers) talks about shelter and then a bed.

## Dev

- `NeighborSays`: after the day's weather line, a keeper whose pairing isn't found gives their hint (`PairingHint`).
- The hints are written in each neighbour's voice ("A grove, my grandmother called it."). Once their pairing is found, that hint stops.

## Result

Discovery comes through the neighbours, never a tutorial popup.
