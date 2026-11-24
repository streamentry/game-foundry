# I79 — Wordmark clears the chrome; undo keeps one place

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

## Fresh Haiku review (screens from I74–I78, live play)

| Review | Immersion | Navigation | Icon-first | Polish | Daily appeal | Fit 40–49 |
|---|---|---|---|---|---|---|
| I73 (b46) | 7 | 5 | 6 | 7 | 5 | 6 |
| I77 (b49) | 7 | 5 | 7 | 6 | 6 | 7 |
| I79 (b52) | 7 | **6** | 7 | **7** | 6 | 6 |

Navigation rose for the first time, after I74 (the whole board above the choices) and I78 (the memory screen shows where its arrow leads). Polish recovered after I77's parchment coach note.

How I handled the findings:

- **Accepted: wordmark crowding.** On the memory screen the book button sat against the end of the wordmark, and the menu button nearly touched the "M".
- **Accepted: undo position.** Undo sits bottom-left under the board but was centred under the choice cards, so it moved between screens.
- **Accepted: coach note padding.** The coach note's last word ("water.") sat on the parchment rim.
- **Rejected: move the portrait medallions.** They deliberately show "who will remember" each choice; this is the third review to raise it, and the decision stands.

## Changes (b53, b54)

- **Wordmark:** it now sits in a child rect inset 84 pt from both sides, past the floating menu button and the book button with its count badge. On a 402 pt phone it keeps about 17 pt of clear space on both sides, scaling down slightly instead of touching either button. b53 used 74 pt, which left only about 10 pt beside the book's badge, so b54 widened it.
- **Undo:** the choice screen's undo row is left-aligned, so the arrow keeps the same bottom-left slot it has under the board.
- **Coach note:** padding is now 12 pt top and 18 pt bottom (was 10 and 10), so the last line clears the rim.
- **QA tooling (not in the repo):** the tour collector now waits for the tour's own `MOSSELORA_QA_TOUR_DONE` line instead of "14 files + 3 s". b53 had copied 18 of 19 shots before the last one was written. The app had logged all 19, and they were recovered from its container.

## Evidence

- `I79-clear-wordmark-steady-undo.png`: the top row is before (b50 coach, b48 choice, b52 memory), the bottom row is after (b53 coach, b53 choice, b54 memory). All are from real taps on the iOS Simulator.
- The verifier passes, and the b53 and b54 QA tours captured all 19 screens.

## Still unverified

- No players aged 40+ have tried this (m3-session-kit P1–P9).
