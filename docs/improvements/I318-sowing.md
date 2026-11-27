# I318 — Sowing

Idea 8 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after the mancala family of Africa and Southeast Asia (Ô ăn quan in Vietnam), played here as a solitaire.

## Player

"I'd love a puzzle that feels like counting seeds into my hand."

## PM decision

A new kind of puzzle: a row of beds with a basket at the near end.
- A bed is ripe when it holds as many seeds as its distance from the basket (the number under it).
- Tap a ripe bed and its seeds are sown, one per bed toward the basket, the last falling in.
- Clear every seed into the basket.

Ripe beds show a soft green, so the choice is which ripe bed, not arithmetic. If the order goes astray and no bed is ripe, the beds shiver and the seeds return to the start: no penalty, no count of tries. The rule is old and generic; the board, words and art are Mosselora's own.

## Dev

- Core (a Sonnet subagent, test-first, merged after review): `SowingGame`.
  - Each seed total has exactly one clearable start, generated backwards from an empty board; the tests prove uniqueness by brute force for 1 to 10 seeds.
  - The hint (the ripe bed nearest the basket) always wins; seeds 1 to 200 per tier are cleared by hints.

  | Tier | Seeds | Beds |
  | --- | --- | --- |
  | 0 | 6 to 9 | 4 |
  | 1 | 12 to 16 | 6 |
  | 2 | 20 to 26 | 7 to 8 |

- Unity: `SowBoard` draws the beds, painted seeds (up to 8 per bed), each bed's distance, and the basket with its count. It has a first-time picture rule and a two-tap hint.
- Joining the garden (with I324):
  - `PuzzleKind.Sowing` is in the story cycle, so every garden's later chapters meet it.
  - Fen keeps it, with her own words.
  - It has its own melody phrase, touch voice, map badge and flourish.

## Result

A new kind that's about order, not sums. Not checked with players: whether the "distance" rule reads from the picture rule alone.
