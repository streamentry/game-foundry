# I74 — Solved board stays whole above the choice

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

## Problem

After a solve, the glide to the choice landed with the decision heading about 110 pt below the top. On a 402×874 phone this cut the solved board in half under the floating menu button, so the payoff picture and the decision never appeared together. I saw this in real play on b46 (I73, `02-choice.png`). The I73 Haiku review gave navigation 5 and said the layout shifts and the board is cropped at the top edge.

## Change (b48)

- `ScrollToDecision` now measures the solved board, from its top including the painted bed, down to the bottom of the choice cards. When that span fits in the viewport below the floating menu button, the glide stops with the whole board just under the button.
- When it does not fit (text at 125% or 150%, which stacks the cards, or short phones), the glide keeps the old heading-anchored stop.
- New helpers: `DepthInContent` gives a node's edge depth on the page. `ChromeClearance` gives how far the menu button reaches into the viewport, capped at 160 pt.

## Evidence

- `I74-whole-board-above-choice.png`: b46 on the left, with the board cut. b48 on the right, with the whole board, the full meter, the question, all three cards and undo on one screen. Both come from real taps on the iOS Simulator: six rotations, then the automatic glide.
- The verifier passes, and the b48 QA tour captured all 19 screens. Core rules did not change; tests stay at 40,500 assertions.

## Still unverified

- The fallback for 125% and 150% text was not exercised live in this iteration.
- No players aged 40+ have seen this yet (m3-session-kit P1–P9).
