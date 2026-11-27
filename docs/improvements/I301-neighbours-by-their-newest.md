# I301 — Neighbours stand by their newest

Idea 22 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after life simulations, where people live in the world among the things they care about.

## Player

"I bought Fen three new flowers. She still stands by the old one from the story."

## PM decision

Each neighbour stands beside the newest thing of their own kind: Mira by trees, Orin by bridges, Fen by flowers. A nursery plant of theirs comes before a story thing. It's a small truth, but the companions only ever say and do true things.

## Dev

- `NeighbourNotice.StandBy(world, villager, out fromNursery)` returns the newest of the villager's family among bought plants, else among story things, else -1.
- The test was run red first. It covers a grown garden (each stands by the newest story thing of their kind) and a restored garden with nursery plants (Fen by the newest flower bought, Mira by the tree, Orin with none bought by his story thing).
- `NeighborsOnMap` uses it; the plant's cell must be land, otherwise the story thing is used as before.

## Result

Buying a plant now visibly matters to its neighbour.
