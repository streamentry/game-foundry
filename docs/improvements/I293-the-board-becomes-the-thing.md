# I293 — The solved board becomes the thing

Idea 46 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after puzzle games where the solved world transforms.

## Player

"I solve it, it sparkles, and then it's gone. What did I do it for?"

## PM decision

After the board blooms, it fades into the thing it belongs to: the painting grows in the board's place under the top row, and then the card closes as before. With reduced motion, the thing simply appears.

## Dev

- `TreePlay.SettleIntoThing()` runs in `Finish()` for every kind: a cream veil over the card below the top row, with the thing's painting. A small `SettleIn` component fades it in from 1.0 s and grows the painting from 70% to 100% over 0.7 s. With reduced motion it shows at once.
- The card closes at 2.9 s instead of 2.2 s, so the change can be seen. Tour waits that ended a solve at 2.6 s now wait 3.2 s.
- Tour: shot 17g2 one second after the water solve.

## Result

Every solve ends on the thing, not on a grid. The extra 0.7 s before closing is a guess to check with players.
