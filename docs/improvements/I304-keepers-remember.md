# I304 — Keepers remember

Idea 33 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after the museum in village life games, where every exhibit you bring tells you something.

## Player

"The almanac is a grid of pictures. Tapping one only tells me its name."

## PM decision

Tap a found item in the almanac and you see its name, then a line from its keeper: Mira for trees, Orin for things built, Fen for flowers. "Fen: I remember the snowdrops you grew. They bloom in winter." Only true things are said: what you made, and, for flowers, the season they bloom in (from I290). An unfound item still says only where it grows (I241).

## Dev

- `Almanac.KeeperLine(item)` builds the line from the item's own memory phrase and, for a flower, its season. It was run red first (it didn't compile). Every one of the 200 catalogue items is checked: the right keeper, the memory included, calm and short (no "!", at most 110 characters), and the right season for flowers.
- `AlmanacPage` adds the line to the toast under the name. It's English only, like the catalogue names.

## Result

The almanac remembers with you. Untranslated, like the rest of the catalogue: consumer builds keep English here until native review.
