# I334 — A keepsake from the visit

Idea 40 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after omiyage, the Japanese custom of bringing something home from a visit.

## Player

"I loved the plant in my daughter's garden. I'd like one of my own to remember the visit by."

## PM decision

When visiting a relative's garden, a keepsake token at the top right shows their newest nursery plant. Tap it to take a cutting home. The cutting is copied, never taken from them: their garden is read-only and unchanged. It costs its nursery price in seeds, like buying, so the economy stays logical. It goes to your basket with a note from Fen. There's one keepsake per visit, and the token is faded until you have the seeds.

## Dev

- `WorldModel.TakeKeepsake(kind)` (Core). The test ran red first (6 errors). It checks:
  - it costs exactly the price and lands in the basket;
  - it's refused without the seeds, for unreal kinds, and outside a living garden.
- Unity: in `OpenVisit`'s chrome, `TakeKeepsake` saves, writes Fen's note, and reopens the visit without the token (`keepsakeTakenFrom` per visit).

## Result

Visits leave something behind in your own garden. Not known: whether the newest plant is the one people would choose. A picker would add a step; this keeps it to one tap.
