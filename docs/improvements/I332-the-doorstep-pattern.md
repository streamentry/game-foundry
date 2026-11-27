# I332 — The doorstep pattern

Idea 44 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after kolam and rangoli, the symmetric patterns drawn at the threshold each morning in India.

## Player

"Something gentle and beautiful to finish, not a test."

## PM decision

A new kind in the flowers' last nursery edition (Fen keeps it): a doorstep pattern that mirrors left to right and top to bottom, and also across the diagonals at the deepest tier. A few marks are given, edged in gold. Every mark tells you its mirror images, and faint lines on the board show the mirrors. Tap a square to draw a rice-flour petal, again to leave it bare. When it's solved, the petals bloom outward from the centre.

| Tier | Size |
| --- | --- |
| 0 | 5×5 |
| 1 | 7×7 |
| 2 | 8×8 |

## Dev

- Core (a Sonnet subagent, test-first, merged): `DoorstepGame`.
  - Patterns are drawn from rings, diamonds, discs, diagonals and petal clusters, then mirrored; 35 to 65% of squares are drawn.
  - There's one given per mirror family, spread over all four quarters.
  - Uniqueness is proven by brute force over orbits.
- Unity: `DoorstepBoard` draws earth and chalk, gold-edged givens, faint mirror lines and a two-tap hint.

## Result

A calm, symmetric kind. The patterns are original geometry, not traditional designs, and need a look from someone who knows kolam.
