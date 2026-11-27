# I357 — Snowy days, and what the neighbours say

Run 3, batch 4.

## PM decision

**Snowy days:** on a snowy winter day it snows properly:
- 90 flakes rather than winter's usual 42 (I338);
- deeper snow on the ground;
- the wind heard now and then.

**What the neighbours say:** when nothing newer and truer is waiting (letters, solves, gifts), a neighbour you talk to mentions the day's weather. That comes before the light remark (I281). On fair days they say nothing about the weather.

## Dev

- `SeasonDrift` uses the larger of the season's drift and the snowy day's snowfall.
- The snow veil is 0.12 stronger on snowy days.
- `NeighborSays` puts `DayWeather.Remark` after real news and before the hour remark.
- Tour shot: `19w-snowy-day` (winter).

## Result

Snow days are an event; the neighbours notice the sky with you. The remark is the same whoever you talk to; per-neighbour weather lines would be a nice follow-up.
