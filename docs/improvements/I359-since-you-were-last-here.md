# I359 — Since you were last here

Run 3, batch 5.

## Player

"I came back after a week. Nobody noticed, and nothing said what I'd missed."

## PM decision

When you come back after two days or more, Mira's next words are:

> "Since you were last here: three rainy days and a windy one. The garden looked after itself."

It's true (the days' own weather, I353), never guilt (no "we missed you"), and reassuring (the garden looked after itself). It's said once, then she's back to her usual news. After more than a month away, it's simply "a good while has passed, with all sorts of weather."

## Dev

- `DayWeather.SinceLastVisit(lastDay, today, season, garden)` (Core). The test ran red first. It checks:
  - nothing for today, yesterday, or a clock set back;
  - a calm summary after a week, naming rain if it rained;
  - a long absence still reads calmly.
- `EvergroveApp.NoteReturn` keeps the last-seen day per garden on the device only (not in the save, never sent anywhere).
- The line is cleared when Mira's conversation closes, so rebuilds keep the same words.

## Result

Coming back is welcomed truthfully. Offline, nothing else changes while you're away, so nothing else is claimed.
