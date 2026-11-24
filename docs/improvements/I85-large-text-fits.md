# I85 — At 150% text, hub, first board and memory fit

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

## Problem

The game offers text at 100%, 125% and 150%, and adults 40+ are the players most likely to choose 150%. Played live at 150% on b60 (402×874 pt), three screens failed:

1. **Hub:** the lower section of words and Play had a fixed 30% of the screen. The wordmark touched the bottom of the village strip. A returning player's hub adds a memory line and the next stream, and would overflow it.
2. **First puzzle:** Mira's coach note grew to four large lines plus its picture row, which pushed the undo, hint and restart plaques partly off the bottom.
3. **Memory screen:** the continue arrow and the change-choice button fell below the fold.

## Changes (b61, b62)

- **Hub (`FitHubStack`):** one frame after building, when the canvas has its real size, the lower section measures its preferred height plus 28 pt of air. If that is more than the default 30%, the painting window and the village strip move up by exactly the difference, capped at 52%. At 100% nothing moves; the b61 screenshot matches b60.
- **Coach note:** above 100% text the picture row (waves → sprout, tap hand) steps aside, and the sentence stays at the player's chosen size. Since I64 the sentence carries the rule on its own.
- **Memory screen:** above 100% text the decorative village banner drops from 0.9× to 0.5× height, and the memory vignette to 0.78×. The words keep their full size.
- **Principle:** larger text never shrinks text. Decoration yields room.

## Evidence (live, iOS Simulator, text at 150%)

In `I85-large-text-fits.png`, the top row is before and the bottom row is after:

| Screen | Before (b60/b61) | After (b61/b62) |
|---|---|---|
| Hub | Wordmark tight against the strip | Painting moved up, air above the wordmark |
| First puzzle | Action plaques cut off | Words-only coach; board, drops and all three plaques on screen |
| Memory screen | Actions below the fold | Swap and continue both visible |

- The returning hub at 150% was also checked live: memory line, landmark plus "Rainwater", and Play all fit.
- The verifier passes, and the b61 and b62 QA tours captured all 19 screens.
- Codex is still out of credits ("Your workspace is out of credits"), so the queued change-choice icon (I84) is still not generated. I retried once this iteration.

## Still unverified

- 125% was not checked live; the same thresholds apply.
- Shorter phones (SE class) were not checked.
- No players aged 40+ have tried this (m3-session-kit P1–P9).
