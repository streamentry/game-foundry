# I323 — A flourish for each kind

Idea 5 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after the win cascade of French and British patience games.

## Player

"Every puzzle ends the same way. A tiny celebration that fits each one would be lovely."

## PM decision

When a board is solved, a dozen of its own motif rise from it and fade within a second, just before it settles into its thing (I293):

| Kind | Motif |
| --- | --- |
| Water and lead the stream | Drops |
| Pairs | A flower of the season |
| Mosaic | Golden glints |
| Garden tiles | Mossy stones |
| Garden plots | Sprigs |
| The rest | Sprouts |

It's small and quiet, and appears with motion only.

## Dev

- `TreePlay.RiseFlourish` and `FlourishRise` rise, grow a little and fade, over the card below the top row. The app sets `Flourish` and `FlourishTint` per kind (`FlourishArt`).
- Purely visual, with no Core rule.

## Result

Each kind closes in its own way. The 1-second flourish runs before the settle, so the card's close time is unchanged.
