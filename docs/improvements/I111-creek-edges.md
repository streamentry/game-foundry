# I111 — Creek spans the iPad and melts into the paper

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

## Why

I110 left three things unchecked: the painted creek behind the board at large text on a small phone, on iPad, and its CI run.

## Checks (QA tours, b95)

- **iPhone SE, 150% text:** `MOSSELORA_QA_TOUR_DONE 20`. The creek frames the board, the large text stays clear, and both tools are on screen.
- **iPad Air:** `MOSSELORA_QA_TOUR_DONE 20`, but **defect:** the creek was a hard-edged rectangle the width of the book column (I106), floating on plain paper. On phones the band also ended in a ruled line under the drops.

## Changes (b96, b97)

- The creek window extends by the tablet `columnInset`, so on iPad it runs from screen edge to screen edge, like on phones.
- The bottom of the band melts into the paper using the existing `TopFade` sprite, flipped and paper-tinted, centre-pivoted so the flip stays inside the mask.
- **Regression caught and fixed in b96 → b97:**
  - The creek is a child of the board, so it draws after the text above it. The b96 paper-coloured top fade covered "Rainwater · A little water can change a place." on iPad and the last line of Mira's tip on the phone.
  - The band now stops at the bed's top, where the stones already make a natural edge, and only the bottom melts.

## Evidence (local only, gitignored)

- `I111-creek-edges.png`, left to right:
  1. iPad b95: hard column-width rectangle.
  2. iPad b97: full width, soft bottom, text intact.
  3. Phone b95: ruled line under the drops.
  4. Phone b97: soft melt, Mira's tip intact.
- `I111-se150-creek.png`: iPhone SE at 150%, b95.
- The verifier passes, and the b96 and b97 QA tours captured all 20 screens on the 402×874 simulator and the iPad.

## Not re-run

- SE at 150% on b97. The b97 change only removes height above the board, so it cannot push anything off screen.
- No players aged 40+ have tried the game.
