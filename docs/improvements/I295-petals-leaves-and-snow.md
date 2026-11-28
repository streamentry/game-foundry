# I295 — Petals, leaves and snow drift over home

Idea 27 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after hanami and momijigari, the Japanese customs of watching blossom and autumn leaves fall.

## Player

"The seasons change the painting, but nothing moves."

## PM decision

By day, the season's own drift crosses home:
- pink petals in spring;
- russet leaves in autumn;
- a few flakes of snow in winter.

Summer is still, and its evenings have the fireflies (I279). There are 10 to 18 pieces, falling slowly with a light breeze: weather, not confetti. Nothing drifts at dusk or at night. With reduced motion nothing drifts at all, since a petal frozen in the air reads as a stain. The season is the progress season (I248). If the light switch is off, it counts as day.

## Dev

- `GardenSeason.DriftOf(Season, DayPhase)` returns the `Drift` (None, Petals, Leaves, Snow). The test was written before the method; the run that proved it red was skipped, so it was only seen green.
- `GardenMapView.SeasonDrift` lays the pieces over the view, above the map, below the controls. It uses a fixed seed, so the same garden looks the same each time. `ApplyDayLight` sets `DriftNow`.

## Result

The seasons now move. To check with players: whether the drift is calming or distracting, and whether it gets in the way of reading the map.

## Change, 2026-11-28 (founder)

Falling snow is weather, so with the day's weather shown it now falls only on a snowy winter day. On a fair, rainy or windy winter day the snow still lies on the ground (and keeps its footprints), but no flakes fall. Before this, a fair winter day showed flakes while a neighbour might say "Upstream it's raining today". Petals and leaves still drift whatever the sky. With the light off no weather is shown, so winter keeps its light snowfall there.

- `GardenSeason.DriftOf(Season, DayPhase, Weather)` gives the drift with the weather shown. `ApplyDayLight` uses it when the light is on, and the two-argument rule when it is off. Tested in `WeatherTests` (red first, then green).
