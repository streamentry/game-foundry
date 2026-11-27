# I290 — Pairs deals the flowers of the season

Idea 3 of the [fifty from the world's games](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after hanafuda's card for each month's flower.

## Player

"The Pairs cards were just pictures from my garden. In winter I'd like winter things."

## PM decision

The Pairs table is dealt from flowers in bloom in the garden's season: snowdrops and camellias in winter, tulips and bluebells in spring. The season is the progress season (I248), never the clock. Each season has at least eight flowers, enough for the biggest table (8 pairs). We borrow the principle (a flower for a time of year), not hanafuda's cards, months or art.

## Dev

- `GardenCatalog.FlowersOf(Season)` tags each of the 50 catalogue flowers with one season, by when it flowers in a temperate garden: 13 spring, 21 summer, 8 autumn, 8 winter. Winter-flowering pansies, primroses and violets count as winter.
- `EvergroveApp.PairsPictures` deals in-season flowers first, starting at a place set by the thing. It falls back to the garden's own paintings if art is missing.
- Tests (written first, failing on the missing method): every flower has exactly one season, each season has at least 8, and spot checks (snowdrops are winter, sunflowers are summer).

## Result

Pairs now changes with the year. Not checked with players: whether adults 40+ notice or care that the cards follow the season.
