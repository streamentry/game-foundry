# I384: Their visitors too

Run 3, batch 10.

## PM decision

When you visit a family member's garden, you see its good-neighbour sprigs and the visitors its pairings bring: their heron, their owl at night. Each garden keeps its own season (I249) and you keep your own hour. It's a small way of sharing how a garden lives, and it might start a conversation ("you've got an owl!").

## Dev

- `SetVisitorsFor(garden, map, things)` is now shared by home and visits, and `VisitorSpot` takes the garden.
- The visit map gets `PairLinksNext` and the visitor cells from the visited garden's own `pairings` (which travel in the garden file, I368).
