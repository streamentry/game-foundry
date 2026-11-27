# I325 — Bees at the flowers

Ideas 17 and 18 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after Japanese cat-collecting games, where visitors come to what you put out, and British creature-garden games, where each visitor has a logical reason.

## Player

"I've planted a lot of flowers. It would be nice if something came to them."

## PM decision

Once three flowers grow in the garden (story or nursery), bees come. They buzz in small loops at up to three flower things, by day in spring and summer. Butterflies (the hub's existing ones, I157) now follow the same logic: they come to any flower, in spring and summer, by day. Before this they flew in every garden, any season. The story's home keeps its butterflies as before. All visitors appear with motion only.

## Dev

- `Visitors` (Core, new) has `Welcomes(world, visitor)` (read from the save), `Out(visitor, season, phase)` and `Why(visitor)`. The test was written first. It checks:
  - a bare garden has no visitors;
  - four story flowers bring bees;
  - birds wait for trees tended;
  - each visitor is out only at the right seasons and hours;
  - every reason is calm.
- `GardenMapView.BeeBuzz` draws a small bee from three rounded shapes (body, stripe, wings), with no painted art, so it stays tiny and abstract. `ApplyDayLight` sets `BeesOut`.

## Result

Flowers draw life. The bee is drawn, not painted. If it reads as clip-art next to the paintings, it needs painted art when the image tool has credits.
