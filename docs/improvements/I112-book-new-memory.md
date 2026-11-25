# I112 — Book shows a new-memory mark instead of a bare count

Date: 2026-11-25. Branch: feat/mosselora-painted-ui (PR #76).

## Fresh Haiku review (b97)

| Immersion | Navigation | Icon-first | Polish | Daily appeal | Fit 40–49 |
|---|---|---|---|---|---|
| 7 | 5 | 6 | 8 | 6 | 6 |

Immersion went from 4 (I109 review) to 7 after the I110 and I111 creek. The top finding was that **the memories book's bare count has no visible meaning** on the button.

- **Rejected:** its example, "12 on the ending, then 10 on winter". That is a QA tour artifact, because the winter shot comes from a separate test garden with 10 memories.
- **Accepted:** the core point. A bare number on an icon button does not say what it counts.

## Change (b98, b99)

- **The number is removed from the book**, on the hub and in the game.
- **A new-memory mark appears instead:** the same small parchment disc as the neighbours' news (I80), carrying the painted Codex sprout. It shows only while the garden holds a memory the player has not opened, and swells once when gentle motion is on.
- **Opening the journal reads every memory so far** (`evergrove:journal-seen` = the memory count), so the mark clears.
- **Bug found while verifying (b98 → b99):** the seen count was global. After a larger garden was read, here the QA tour's 12-memory journal, a restored or new smaller garden never showed the mark. Now a stored count above the garden's memory count resets to that count. It belongs to another garden, so its memories count as already seen.
- No new text or keys.

## Evidence (live, iOS Simulator, b99)

`I112-book-new-memory.png` (local only, gitignored):

1. **Left:** the hub with 3 memories and 2 read, so the book wears the sprout mark.
2. **Right:** after opening the journal and relaunching, the book is plain.

The prefs values moved as expected: 12 (left by the tour) → 1 (reset for the smaller garden) → 3 (after reading).

The verifier passes, and the b98 and b99 QA tours captured all 20 screens.

## Still unverified

- Whether players aged 40+ notice the mark and want to open the book is an M3 session question.
