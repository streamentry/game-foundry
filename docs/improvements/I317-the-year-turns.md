# I317 — The year turns

Idea 25 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after farming life games, where festivals mark the year.

## Player

"I've been tending for weeks. Does the garden know a year has gone by?"

## PM decision

After the story, the seasons keep turning with your tending (I248), and each spring begins a new year. Through that spring, home keeps a festival:
- a garland of the catalogue's own paper lanterns hangs across the top of the view, swaying a little (still with reduced motion), and lit after dusk;
- Orin writes when the year turns: the lanterns are up, and that's a year (or n years) of your tending.

The festival follows progress, never the calendar. There's nothing to do, and nothing missed if you're away.

## Dev

- `GardenSeason` gains `FestivalAfterStory(solved)`, `YearAfterStory(solved)`, `Festival(world)` and `FestivalNote(year)`. The test was written first. It checks:
  - the festival is exactly the spring after the story;
  - never during the story;
  - the first spring begins year one;
  - the note passes the letterbox rules with no "!".
- `GardenMapView.FestivalGarland` draws the garland, and `ApplyDayLight` sets `FestivalNow`. `NoteSeasonTurn` adds Orin's note when the turn lands on a festival spring.

## Result

Years are marked. The garland covers some sky at the top of home; check that it never hides the menu or purse at 150% text.
