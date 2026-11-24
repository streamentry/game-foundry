# I105 — Smallest phone at largest text: journal stacks, empty drops pass 3:1

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

## Why

This is the hardest combination for players aged 40+: the smallest current iPhone (SE, 375×667 pt) with text at its largest (150%). It was untested and listed as open in I104.

## Check

- **Method:** QA tour on the iPhone SE simulator with the text-size preference preset to 150% (`defaults write … "evergrove:text-scale:v1" -int 150` after install).
- **Result:** b87 reached `MOSSELORA_QA_TOUR_DONE 20` and every screen fits:
  - first puzzle: note, board, drops and tools;
  - stacked choice cards;
  - memory screens with both buttons;
  - ending, menu, and the 4×4 and 5×5 boards.

## Haiku review of the SE 150% screens (checked before acting)

| Finding | Decision |
|---|---|
| "Developer controls in the player menu" | **Rejected.** "Start matched control (dev)" and the 2D/2.5D toggle compile only in Editor or Development builds (the verifier's dev-control guard). "Copy/Restore garden file" are real player features. |
| "Empty water drops are pale grey on cream, hard to read" | **Accepted after measuring:** the empty drop body measured **2.49:1** on paper, below WCAG 1.4.11's 3:1. |
| "Journal progress dots clipped behind the sheet" | **Rejected.** That is the page behind the dimmed sheet. |
| "Replay button small" | **Rejected.** It is 64 pt tall, above the 44 pt minimum. |

## Changes (b88, b89)

- **Journal at larger text:** above 100% text, journal pages stack one per row, the same rule the choice cards follow (I46). A memory then reads in two lines instead of four or five. The 100% two-column scrapbook is unchanged.
- **Empty water drops:** the tint goes from (0.40, 0.44, 0.40, 50% alpha) to a solid slate (0.36, 0.40, 0.37, 85%). Measured from the b89 QA tour: the empty drop body is now **5.91:1** on paper (was 2.49:1). The filled drops are unchanged.

## Evidence

`I105-small-phone-large-text.png`:

- **Top:** the SE journal at 150%. b87 squeezes "Orin / remembers the / bridge you / restored." into two narrow columns. b89 stacks pages one per row (two lines each). The b89 full journal scrolls to stream 12 with the pinned close button.
- **Bottom:** the drop meter, b87 (pale empty drops) above b89 (solid slate).

The verifier passes. The b88 and b89 QA tours each captured 20 screens on the 402×874 simulator, and the b89 SE 150% tour also captured 20.

## Still unverified

- iPad and small Android phones.
- Codex is still out of credits.
- No players aged 40+ have tried the game (m3-session-kit P1–P9).
