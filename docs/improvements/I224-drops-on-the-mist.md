# I224 — Drops on the mist show how close more river is

## Player
- I can see mist below my river, but I can't tell what clears it or how close
  it is. "Every fourth puzzle" lives in a document, not in my garden.

## PM decision
- A row of water drops sits on the next stretch's mist, made of the same
  filled and hollow drop glyphs as the board (I146), so it reads as the same
  water. Each step fills one drop, and a full row lifts the mist.
  - During the story, a step is a stream restored (3 drops).
  - After the story, a step is a first puzzle solve (4 drops). Replays don't
    count, as for seeds.
- The drop just earned swells once (with reduced motion it simply appears).
- No words, no timer, nothing lost by waiting. At the ceiling there's no mist,
  so there are no drops.

## Dev
- Test-first (`RiverGrowsTests`, red on the missing method):
  `GardenMap.TowardNext(restored, completed, puzzlesSolved, out have, out need)`
  returns false at the ceiling. The tests also check that the drops left are
  exactly what opens the next stretch, and that a negative count reads as zero.
- `GardenMapView.ShowDrops(have, need, swellNewest)` draws the row on the mist.
  `EvergroveApp.RiverDrops` uses it on home and on the story page's map.
- Tour: `QaHubToFoot` takes home to the river's foot; shot
  `17y-drops-on-the-mist` (three of four filled).

## Result
- b271, all three tours, 52 shots each. `17y` shows three filled drops and one
  hollow on the mist at the river's foot, readable at 150% text and sideways.
- Core tests pass (71,587 assertions at this step).
