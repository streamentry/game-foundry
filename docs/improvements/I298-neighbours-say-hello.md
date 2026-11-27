# I298 — Neighbours say hello

Idea 35 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after wordless companionship games, where presence says more than words.

## Player

"When I open the game, nobody notices I'm back."

## PM decision

When you open Mosselora or come back to it, Mira, Orin and Fen each give a small hop and tilt on the map, one after another: hello, without words. It happens once per return, not on every rebuild of home, and never with reduced motion. It's not a reward and not a guilt cue: it happens the same however long you were away.

## Dev

- `waveHello` is set at launch and in `OnApplicationPause(false)`, and cleared once home's map is built. `HelloHop` makes two hops of 0.42 s each, staggered by 0.28 s per neighbour.
- Known gap: if the app resumes with home already built, the hello waits for home's next rebuild.

## Result

Returning feels noticed. Not known: whether players read the hop as "hello".
