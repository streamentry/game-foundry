# I184 — Neighbours talk about the very thing you made, and where

Date: 2026-11-26. Branch: feat/mosselora-i127-plus.

## Player (Haiku cold read of b195, as a 64-year-old: "what would bring you back tomorrow?")

"Seeing my birch still there, with Mira or Orin commenting that it has grown."

Until now the neighbours' lines came from a small set of authored family lines ("Mira remembers the tree you planted"), which never named *your* silver birch or *where* you put it.

## PM decision

When you tap a neighbour, they speak about **the newest thing of their own family, by name, and where it stands on your map**, all read from the save (the vision's "remembers truthfully" rule):
- Mira: "Your **silver birch by the cottage** is taking root nicely."
- Orin: "I walked past your **windmill by the creek** this morning. Good, sturdy work."
- Fen: "The bees found your **foxgloves in the meadow** before I did."
- From the third of a kind: "That makes 3 now."
- Places come from the map: by the creek (next to water), by the cottage (top right), down the valley (bottom rows), or in the meadow.
- Before they have a thing of their own, or outside English, their authored lines are used as before. The "has news" mark follows the new line.
- Neighbours never stand on each other: a later one steps down a row and a half until clear.

## Dev

`EvergroveApp.NeighborSays(villager)` replaces the three `LocalizedVillage.Speak` call sites (the conversation, the `HasNews` comparison and the heard-line record). `NeighborsOnMap` adds a clash check against already-placed neighbours.

## Result (b198 → b199, iPhone 17 simulator, hands-on)

- I solved stream 1, chose the silver birch and tapped the meadow. The birch planted there; tapping the creek first was refused, as it should be.
- After a full relaunch, **the birch was still where I planted it** (`I184-birch-kept.jpg`), and Mira stood beside it.
- Tapping Mira: **"Your silver birch by the cottage is taking root nicely."** (`I184-mira-remembers.png`; both images gitignored)
- b198 showed Mira overlapping Orin's waiting spot. b199 adds the step-aside, but my attempt to restore the planted save onto the b199 install didn't carry over (the save lives in files, not settings), so the clash fix is code-reviewed only.
- Portrait, landscape and 150% tours pass (23 each). Verify passes.
