# I187 — Neighbours gather around the newest thing you made

Date: 2026-11-26. Branch: feat/mosselora-i127-plus.

## Player (b205 tours, I186's note)

Neighbours who had nothing of their own waited at fixed cells in the valley, and on the smaller board-page maps those were often outside the window (Fen at the right edge, Orin's face cut by the top). It's an open, pannable map, but a neighbour cut in half looks broken, and a neighbour standing in an empty corner says nothing.

## PM decision

A neighbour with nothing of their own yet **comes to see the newest thing you made**. The map always opens on that newest thing, so they're in view, and they're gathered around what you just did. Each tries the spots around it in turn (left, right, a step up on either side, two steps out) and takes the first one free. **Neighbours who already have a thing of their own claim their spot first**, so newcomers fit around them.

## Dev

`EvergroveApp.NeighborsOnMap`: order owners first, then a candidate-spot search around `cells[newest]` with the same clash test as I184, falling back to the step-down.

## Result so far (b206, before the candidate search and ordering)

`I187-first-try.jpg` (gitignored): on the weathervane board, Mira and Orin stand either side of the new weathervane, as intended. On the birch board, Orin's spot collided with Mira's (she stands beside her own birch), so he was pushed below her and Fen fell off the bottom. That is what the candidate search and owner-first ordering in this commit fix.

**Not yet built:** this commit's code passes verify and the .NET tests (52,728), but the Unity build and tours for it run next. The result follows in I188's log.
