# I115 — Sideways, the next arrow is on the first screen

Date: 2026-11-25. Branch: feat/mosselora-painted-ui (PR #76).

## Problem

In landscape the memory screen (shown after a solve and a choice) put the **continue arrow below the screen**. On a 402 pt tall phone the card still carried the neighbour's 72 pt portrait, the sprig padding and the next-stream line, so the main action needed a scroll. In a cold-read of the I114 screen, a Haiku reviewer found "no visible button to keep playing" and would have to guess between the card, the garden and the book.

## Change (landscape only)

- The neighbour's portrait is left out of the card. The full-height painting on the left already shows that neighbour doing what you chose.
- The next-stream line ("Rainwater") is left out; the stream stones still show where the arrow leads.
- The card's top padding goes from 34 to 26 pt; the sprigs still fit.

Portrait is untouched: every change is gated on `landscape`.

## Verification

- b105 on the iOS simulator: fresh install, solve First Bend, choose wildflowers, rotate. The card sits below the book, and the change-choice tile and continue arrow are fully on screen beside the painting (`I115-landscape-memory-before/after.png`, local only).
- Haiku cold-read of the before/after shots. Before: no visible way to keep playing. After: the arrow is directly under the memory text and visible without scrolling. It also flagged a "black pill", which is the simulator's Dynamic Island cutout, not app UI.
- The b105 portrait QA tour ran to DONE (20 screens). C# tests: 41,113 assertions pass. The verifier reports OK.

## Not verified

- A real device, iPad landscape, players aged 40+, and larger text sizes in landscape.
