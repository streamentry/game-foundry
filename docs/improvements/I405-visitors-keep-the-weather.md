# I405: Visitors keep the weather

Run 3, batch 14.

Visitors now keep the day's weather, like real ones:

- bees and butterflies stay in on rainy and snowy days;
- butterflies also stay out of the wind;
- the owl keeps to its tree on a rainy night;
- the heron and kingfisher don't mind rain.

**Only what is seen changes.** The economy never depends on the weather (I353), so pollination and season yields are unaffected. Weather is by date and players can't control it, so it must never cost them anything.

- `Visitors.OutIn(visitor, season, phase, weather)` wraps `Out`. It's used for the bees, the butterflies and every visitor spot. Tested.
