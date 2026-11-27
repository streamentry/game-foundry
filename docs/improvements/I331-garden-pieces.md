# I331 — Garden pieces

Idea 9 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after the seven-piece tangram of China, made square for big fingers.

## Player

"I like the puzzles where you fit shapes together. Something calm."

## PM decision

A new kind in the nursery's last edition of things built (Orin keeps it): fill a silhouette exactly with the pieces in the tray.
1. Tap a piece and turn it; at deeper tiers you can mirror it too.
2. Tap where it should lie, and it settles over that square if it fits. Any of its squares can be the one you tap.
3. Tap a laid piece to take it back.

Each piece has its own soft colour.

| Tier | Pieces |
| --- | --- |
| 0 | 3 |
| 1 | 5 |
| 2 | 7 |

## Dev

- Core (a Sonnet subagent, test-first, merged): `GardenPiecesGame`. Silhouettes are grown from the pieces, so they're always solvable, and kept compact (no holes, at least 55% fill). It has a hint that corrects a misplaced piece first.
- Unity: `PiecesBoard` has the silhouette, a tray of piece thumbnails, and turn and mirror tokens.
- Keeper, words, melody, voice, badge and flourish are its own.

## Result

A shape-fitting kind for those who like it. Not checked: whether the tray thumbnails are large enough at tier 2 on a small phone.
