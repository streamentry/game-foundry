# I299 — The cottage at a glance

Idea 36 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after the inn in classic Japanese role-playing games: a calm place where you see where you stand.

## Player

"I come back after a few days and don't know what's waiting."

## PM decision

Tap your cottage (the house at the top of your garden) with nothing in hand, and a small card shows, in pictures and counts:
- the season, as its stone;
- letters in your box;
- cuttings in your basket;
- things holding a new puzzle;
- the drops toward new river.

It lists only what's true, with no "you should". When nothing is waiting, it says "All tended", and that replays are there whenever you like. A tap outside closes it. Seeds and fruit stay on the purse, not here.

## Dev

- `GardenMap.IsCottage(cell)` returns true for the built block in the first two rows of the first reach: 6 cells on the shipped mask. The built path below it and later reaches don't count.
- `CottageSummary.For(world, unreadLetters)` counts the things waiting, cuttings and drops (via `GardenMap.TowardNext`), seeds, fruit and season.
- Tests ran red first: `IsCottage` on the real mask, then the summary on a new and a grown garden, including that a solve leaves one fewer waiting.
- `OnMapLand` opens it only with nothing in hand and the basket closed. Tour shot 17j5.

## Result

There's one calm place to see the garden's state. Not known: whether players think to tap the house. It has no mark, on purpose; the tour will show if it needs one.
