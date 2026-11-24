# I77 — Mira's coach note on parchment, not a dark panel

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

## Fresh Haiku review (current build: I73–I76 screens from live play)

| Review | Immersion | Navigation | Icon-first | Polish | Daily appeal | Fit 40–49 |
|---|---|---|---|---|---|---|
| I73 (b46) | 7 | 5 | 6 | 7 | 5 | 6 |
| I77 (b49) | 7 | 5 | 7 | 6 | 6 | 7 |

Daily appeal, icon-first and fit each rose by one point. Polish dropped by one point; the reviewer pointed to the mixed painted and flat styles, and singled out the coach panel.

How I handled the findings:

- **Accepted:** the first screen's coach was "a sans-serif dark green panel". It read as UI chrome sitting on the painted garden. Fixed here.
- **Rejected:** "move the leaf off the wordmark's 'a'". The leaf is part of the brand master in `games/evergrove/brand/`, not a layout bug.
- **Rejected:** "move the choice-card portraits and the arrow under the cards". The portrait is deliberately "who will remember" this choice. The arrow is undo of the last move, not back-navigation, so it belongs near the board, not with the menu.
- **Deferred:** the menu's language and text-size tiles (an EN code, a stepper instead of level dots). These need a design decision about the state indicators.

## Change (b50)

- Mira's coach bubble is now painted parchment (`PaperTile`, `CardFill`, `CardEdge` rim, soft lift), matching the choice cards. The tail is parchment too.
- The sentence is ink-coloured, 16 pt (up from white 15 pt).
- The painted spring and sprout sit in sage discs. The arrow and tapping hand are tinted the water-channel blue, so the picture still reads as "waves → sprout, tap".

## Evidence

- `I77-parchment-coach.png` shows b46 on the left and b50 on the right, both from a live launch on the iOS Simulator into stream 1.
- The verifier passes, and the b50 QA tour captured all 19 screens.

## Still unverified

- Contrast was judged by eye: dark ink on parchment.
- No players aged 40+ have tried this yet (m3-session-kit P1–P9).
