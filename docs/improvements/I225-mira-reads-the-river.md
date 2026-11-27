# I225 — Mira reads the river

## Player
- Even with the drops (I224), I wasn't sure what they meant. A neighbour saying
  it once, in plain words, would settle it.

## PM decision
- Mira keeps the trees by the water, so she speaks for the river. After the
  story, and only when it is true:
  - One drop left: "The mist downstream is thinning. One more thing tended and
    it will lift."
  - A stretch has just opened past the story's five (no drops yet toward the
    next): "The river runs further than it ever did. There's new bank down
    there, waiting for roots."
- Priority: a family gift waiting comes first, then a new arrival, then the
  river, then her count of tended trees. No clock, no pressure words; the
  banned-word test covers both lines.

## Dev
- Test-first (`NoticeTests`, red: Mira gave her count instead).
  `NeighbourNotice.Line` uses `GardenMap.TowardNext` and `ReachesFor`.
- The older count test now accepts the river line only where it's true: three
  of four, or a full row.

## Result
- Core tests pass (71,631 assertions at this step).
