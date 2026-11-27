# I307 — A shower after water

Idea 29 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after cosy Japanese life games, where weather is something you notice.

## Player

"I free the water in the puzzle, and then nothing in the garden shows it."

## PM decision

When you come home from solving a water puzzle, a short, light shower falls over that thing while its solved ring grows in: 16 thin streaks, under two seconds, then gone. It plays only with motion on, like the ring. It says "the water you freed reached it" without words.

## Dev

- `GardenMapView.Shower` draws streaks above the thing, falling and slanting slightly, and fades out over its last half second.
- It's shown when a placed thing has `BloomNow` (just solved, gentle motion) and `Kind` is Water.
- Purely visual, with no Core rule.

## Result

The water puzzle's result lands in the garden. The tour solves a water thing, but its home shot comes 3 s after the close, after the shower ends, so the shower itself is unverified in screenshots.
