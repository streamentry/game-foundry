# I355 — Windy days

Run 3, batch 4.

## PM decision

On a windy day:
- trees and flowers lean harder and faster;
- petals, leaves and snow are blown sideways;
- faint gusts sweep across the view;
- the wind is heard more often.

"A brisk wind today. Everything's leaning and whispering."

## Dev

- `DayWeather.WindStrength`: windy 2.6, rainy 1.3, snowy 1.2, hot 0.6, fair 1.
- `Sway` multiplies its lean and pace by it, and the season drift's breeze by its square.
- `WeatherLayer` (Windy) draws nine faint white streaks that sweep, fade in and fade out.
- Tour shot: `19w-windy-day` (autumn).

## Result

Wind you can see, in the spirit of I297.
