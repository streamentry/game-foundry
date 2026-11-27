# I342 — The river through the year

Run 3, batch 1.

## Player

"The river never changes. Everything else does now."

## PM decision

The river follows the year:
- **winter:** frosty ice along it;
- **autumn:** leaves afloat, bobbing gently;
- **summer:** sun glints twinkling on the water;
- **spring:** it simply runs, full and clear.

Marks go on the real water cells, only some of them, never cluttering the stream.

## Dev

- `SeasonLook.River(season)` returns IceEdged, LeavesAfloat, Sparkling or Running (tested).
- `GardenMapView` marks at most 70 water cells, chosen from the map mask, using:
  - frosty glows for ice;
  - `Afloat` leaves;
  - `Twinkle` glints.
- They move only with motion on.

## Result

The stream is part of the season. Not yet checked on very long rivers, where 70 marks spread thinly.
