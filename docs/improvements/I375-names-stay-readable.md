# I375: River names stay readable

Run 3, batch 8.

## Player note (P15, b315 tour)

On the first map shot, Orin stood by his thing right on the "Kingfisher Run" plank (I360) and hid half its name.

## PM decision

A neighbour never stands on a river's name. If they would, they step aside, toward whichever side of the plank has more room.

## Dev

- `GardenMapView.KeepOffSigns(standing, width, reachHeight, reaches)` runs after the neighbours are laid out. It uses the same plank geometry as the signs. Any neighbour whose footprint overlaps a plank moves to its left or right, whichever stays on screen.

## Result

The fix is in place; the next tour will confirm it.
