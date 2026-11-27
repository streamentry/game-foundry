# I362 — Mira names the new river

Run 3, batch 5.

## PM decision

When tending opens new river, Mira's note (I271) names where it now runs: "The river runs on, down to Heron Pool." The rest of the note (the season it brought) is unchanged.

## Dev

- `NeighbourNotice.RiverNote(season, stretch)` (Core) wraps the existing note. Tested: the name is in it, and it's still a valid letter.
- The app passes `RiverNames.For(OpenReaches() - 1)`.

## Result

The letterbox, the map's planks and the mist all use the same names.
