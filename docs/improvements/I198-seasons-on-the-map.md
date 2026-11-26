# I198 — The garden wears the season

## Player
- The walk stones ahead are coloured summer, autumn and winter, and the village
  strip used to change with them, but the garden map behind every board was the
  same bright June green at the start, mid-autumn and deep winter. The year
  moving on was something I was told about, never something I saw.

## PM decision
- The map wears the season the walk has reached (seasons come with progress,
  never the clock, vision 4.2): summer, then autumn amber, then winter frost; a
  finished garden is summer again for the festival.
- No new painting: grade our own map master. A tint alone could not do winter
  (multiplying green only gives darker green), so the two grades are baked once
  in the art pipeline. Winter is partly desaturated, cooled and frosted.
- Things you've grown keep their summer paintings for now.

## Dev
- `tools/evergrove_art_pipeline.py`: `season_maps(plot)` writes
  `garden-map-autumn` and `garden-map-winter` (1024×1536, same import settings,
  stable GUIDs) whenever the pipeline writes `garden-map`.
- `EvergroveApp.MapPainting()`: picks the painting by
  `PaintedVillage.SeasonFor(LandscapePhase, Completed)`, falling back to summer.
  All four map views use it (behind boards, home, memory card, storybook page).

## Result
- Summer / autumn / winter behind the board, and winter sideways:
  `I198-seasons.jpg` (local only). Boards stay readable on both grades.
- Portrait, landscape and 150% tours: all complete (build b230).
- Seen, not fixed: on the chapter 2 board a pale paper band covers the lower
  third of the garden.
- Not tested on a real device or with players 40+.
