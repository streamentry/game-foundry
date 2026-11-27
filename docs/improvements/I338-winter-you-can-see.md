# I338 — Winter you can see

Run 3, batch 1 ([player notes](../../games/evergrove/docs/playtest/run3-player-notes-2026-11-27.md)). The founder asked: "should there be snow? and what about autumn, spring, summer?"

## Player

"It says winter, but it's just paler. The wisteria is still blooming purple in the snow."

## PM decision

Winter has to look like winter, honestly:
- snow lies on the ground, as a soft white veil under every thing, so things stand out against it;
- every thing wears a snow cap;
- the cottage roofs are white;
- the snowfall is fuller (42 flakes, slow, never a blizzard).

Trees rest: a cool, quiet tint. Flowers rest too, smaller and grey-green, except winter flowers (snowdrops, crocuses, camellias...), which bloom on in the snow.

## Dev

- `SeasonLook` (Core, new) has `For(season, family, flowerIndex)` returning Full, Blossom, Turning or Resting; `SnowCaps`, `SnowLying`, `DriftCount` and `River`.
  - It reads the progress season (I248), never the clock, and is true to the catalogue's flower seasons (I290).
  - `SeasonLookTests` ran red first (37 errors).
- `GardenMapView` draws:
  - `DressForSeason` per thing: caps, tints and blossom;
  - the snow veil over the painting;
  - snow on the roof at each reach, mirrored ones included.
- `Placed.FlowerIndex` carries a flower's catalogue index (story and nursery). `ApplyDayLight` sets `SeasonNow`.

## Result

Winter reads as winter. The rest is procedural over the existing paintings, since the image tool has no credits; painted winter variants would be the step up.
