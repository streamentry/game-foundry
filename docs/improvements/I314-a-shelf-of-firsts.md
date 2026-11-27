# I314 — A shelf of firsts

Idea 34 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after unpacking games, where a life is told through the objects it keeps.

## Player

"I remember the day my sister first sent me a lantern. The game doesn't."

## PM decision

The almanac opens on a memory shelf of eight firsts:
- a letter;
- fruit from a tree you tended;
- a nursery plant;
- a puzzle played together;
- help from family;
- a lantern;
- new river past the story;
- the story told to its end.

What has happened is painted in colour; the rest are pencil silhouettes, like unfound almanac items. A tap says what it is, or "Not yet". It has no count, no reward and no hint of a checklist to finish.

## Dev

- `MemoryShelf.For(world)` (Core, new) reads each first straight from the save, so nothing extra is stored and nothing can be wrong. The test was written before the class. It checks that a new garden has kept nothing, that every keepsake has art and a calm label, and that one kept note puts exactly one keepsake on the shelf.
- `MemoryShelfRow` is four columns at the top of the almanac page.

## Result

The garden's history is visible at a glance. It shows that a first happened, not when, because the save keeps no dates (by design).
