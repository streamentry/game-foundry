# I297 — Wind you can see

Idea 30 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after the visible wind in hand-drawn Japanese animated film.

## Player

"It looks like a painting. Lovely, but nothing is alive."

## PM decision

Trees and flowers lean gently in the breeze, by up to about two degrees, and the breeze rises and falls. Each one moves a little out of step with its neighbours. Bridges and other built things stay put. With reduced motion, everything is still.

## Dev

- `GardenMapView.Sway` rotates a thing about its foot (the item pivot is already at the base). Its phase comes from the cell, so neighbours never move in lockstep. It isn't added to the thing in hand, which bobs (I179).
- Purely visual. No Core rule, so no test. Checked in the tours.

## Result

The map breathes. To check: whether 40+ players with reduced-motion needs who leave motion on find it pleasant or tiring.
