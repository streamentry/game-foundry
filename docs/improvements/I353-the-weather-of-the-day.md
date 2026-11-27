# I353 — The weather of the day

Run 3, batch 4. Founder: "should be raining days, snowing days, windy days, normal day, hot days..."

## Player

"Every day in the garden looks the same, apart from the season."

## PM decision

Each day has its own weather: fair, rainy, windy, hot (summer only) or snowy (winter only). It's the same all day, and a little different in each family member's garden. The chances follow the garden's season, with most days fair:

| Season | Fair | Rainy | Windy | Hot | Snowy |
| --- | --- | --- | --- | --- | --- |
| Spring | 55% | 25% | 20% | | |
| Summer | 50% | 12% | 13% | 25% | |
| Autumn | 45% | 30% | 25% | | |
| Winter | 40% | 13% | 12% | | 35% |

It's cosmetic, like the light (I272): it never changes a puzzle, a reward or progress, so a rainy week costs nobody anything. It follows the light switch: with the light off, every day is fair. It gives a gentle reason to look in each day, without streaks or gates.

## Dev

- `DayWeather` (Core, new):
  - `For(localDate, season, garden)` is a hash of the date and garden, so it's deterministic and needs no network;
  - `Remark`, `Rain`, `WindStrength` and `SnowfallCount`.
- `WeatherTests` ran red first (29 errors). Over a year of days per season it checks:
  - the same weather all day;
  - snow only in winter, heat only in summer;
  - mostly fair, with some of each other kind;
  - variety within two weeks;
  - each garden has its own weather;
  - calm remarks.
- `EvergroveApp.TodayWeather` uses the local date, with a tour override; tours pin a fair day.

## Result

Days differ. Not known: whether players notice, and whether a run of grey days feels gloomy. The weights are easy to tune.
