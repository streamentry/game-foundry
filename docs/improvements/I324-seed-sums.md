# I324 — Seed sums

Idea 10 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after the sum-cross puzzles of Japan, and the studio's own first-game research candidate.

## Player

"I like a little arithmetic with my logic, as long as it's fair."

## PM decision

A new kind: each row or column of beds adds up to the number on its marker (across in green at upper right, down in brown at lower left), with no number used twice in a row. Tap a bed, then a big number in the two-row tray; tap the same number to lift it. A number that can't fit glows rose: a hint, never a penalty.

Every board is solvable by reasoning alone, never by guessing:

| Tier | Size |
| --- | --- |
| 0 | 3×3 |
| 1 | 4×4 |
| 2 | 5×5 |

## Dev

- Core (a Sonnet subagent, test-first, merged after review): `SeedSumsGame`.
  - Boards are accepted only when a deduction solver finishes them, which also makes them unique; an independent brute-force solver confirms one solution for seeds 1 to 100 per tier.
  - Generation is a hill-climb over digits. On a laptop it averages 0.3, 3.4 and 11.9 ms per tier; rare tier-2 boards reach about 170 ms.
- Unity: `SumsBoard` draws the markers with their sums, the beds, and a two-row tray (1 to 5, 6 to 9), large enough for older hands.
- Orin keeps it, with his own words ("every row adds up, like a good ledger"), and it has its own melody, voice, badge and flourish.

## Result

The studio's research candidate is now playable inside Mosselora. Unverified:
- the rare slow tier-2 generation on a phone (if it hitches, precompute seeds);
- whether numerals sit well with the picture-led kinds.
