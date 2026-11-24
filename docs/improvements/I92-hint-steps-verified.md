# I92 — Hint steps verified live; clearer row, turned tile swells

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

## Verification of I91's open item: hint step 3, live (b70, stream 1)

I pressed "Guide me" three times:

1. **Press 1:** honey rings on the middle row, and the wet tile stays blue.
2. **Press 2:** a ring on the garden tile.
3. **Press 3:** the hint turns the garden tile to its solution (facing east). The rings and level dots clear, and undo becomes available.

Step 3 works as designed.

## Haiku cold-read (first-time 55-year-old player, three hint screenshots)

- **Press 1:** "the board looks unchanged at this size". At screenshot scale the 75%-alpha row rings were too faint. **Accepted.**
- **Press 2:** "a gold frame around the sprout tile… the most helpful step". This confirms the I91 fix, where the goal glow no longer hides the hint.
- **Press 3:** "the board changes without explanation". The hint sets the final rotation directly, so the tile snaps. **Accepted.**
- **Declined:** the suggested text lines ("Turn this pipe toward the garden", "2 hints left"). They contradict the icon-first brief, and the spoken clues already carry those words. Hints are unlimited by design, so there is no count to show.

## Changes (b71)

- **Step-1 row rings:** full honey (100% alpha) and slightly thicker (multiplier 1.25, from 1.6). The step-2 focus ring (0.9) stays the boldest.
- **Step 3:** the tile the hint turned swells once, using `BloomPulse` (22%). It only animates when gentle motion is on, like every other motion in the game.

## Standing evidence for the brief, re-checked in the repo this iteration

- **40+ audience:** AGENTS.md lines 5–10 and the decision-log entry "2026-11-24 — Primary audience widened from 45+ to 40+".
- **Codex art:** 25 Codex-generated masters in `games/evergrove/art/generated`. The `icons-ui-6` change-choice glyph is still blocked: "Your workspace is out of credits" on four tries.
- **Haiku sub-agents:** 23 of the 91 earlier iteration logs record Haiku reviews, cold-reads or translations, and this iteration adds one more cold-read.
- **PR status:** PR #76 was merged on 2026-11-23 at 18:25 UTC. Later commits, including I73 onward, are on `feat/mosselora-painted-ui` but not yet in `main`.

## Evidence

- `I92-hint-steps-verified.png`, left to right:
  1. b70 press 1 (faint rings)
  2. b71 press 1 (clear rings)
  3. b70 press 2
  4. b70 press 3 (tile turned, marks cleared)
- The verifier passes, and the b71 QA tour captured all 19 screens.

## Still unverified

- The swell is an animation, so it is code-checked only.
- No players aged 40+ have used the hints (m3-session-kit P1–P9).
