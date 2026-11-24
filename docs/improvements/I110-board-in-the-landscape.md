# I110 — Board sits in this season's painted creek

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

## Problem

The I109 Haiku review scored **Immersion 4**: the play screens "look like a web page", and the painted world shows only "as a thin strip at the bottom". It suggested a full-bleed painting behind the board. I declined that in I109 because a busy painting behind the tiles would cost legibility.

## Change (b94, b95): a careful version

- **New `CreekBehindBoard()`:** behind the board's stone bed it draws the **current season's** Codex creek painting (`banner-stream-`, `banner-autumn-` or `banner-winter-`), **dry while the stream is unsolved and flowing once solved**, at 55% opacity. It spans page edge to page edge plus a 46 pt band above and below the bed, clipped by a `RectMask2D`.
- **Legibility:** the tiles stay opaque parchment and sit above the stone bed, so the puzzle itself is untouched.
- **Living world only:** the Puzzle-only control keeps the plain page, which keeps the M4 comparison clean (verified on tour shot `12-matched-chapter2`).
- No new art (Codex is still out of credits) and no new text.
- **b94 → b95:** the band grew from 22 to 46 pt, because at 22 pt the stone bed hid most of the painting.

## Blind Haiku comparison (old and new in random order, nothing said about what changed)

| Pair | New version picked? | Legibility |
|---|---|---|
| First puzzle (b93 vs b95) | **Yes, low confidence:** the new one has "painted ground beneath the board"; the old one "drops to a flat cream strip that reads more like a web page" | Equal |
| Winter 5×5 (b93 vs b95) | Could not tell them apart | Equal |

The evidence is weak but in the right direction, with **no legibility cost**. The I103 journal tilt, by contrast, leaned against the change and was removed. Kept.

## Evidence

- `I110-board-in-the-landscape.png` (local only, gitignored), left to right:
  1. b93 first puzzle
  2. b95 first puzzle
  3. b93 winter
  4. b95 winter
- The verifier passes, and the b94 and b95 QA tours captured all 20 screens.

## Still unverified

- Whether players aged 40+ feel more immersed is unknown (M3 sessions).
- The painting at 125% and 150% text, iPad and SE was not re-checked.
