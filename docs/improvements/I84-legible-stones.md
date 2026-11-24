# I84 — Progress stones pass 3:1 and grow; news marks larger

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

## Problem

The I83 Haiku review gave "fit for ages 40–49" a 6 and said the "stage dots… are small and low-contrast". I measured them. The upcoming stepping stones were translucent season tints at about **1.6:1** against the paper (`#F9F2E3`). That fails WCAG 1.4.11, which asks for 3:1 for meaningful graphics. They were also only 11 pt, with the current stone at 18 pt. The hub's news marks (I80) were 28 pt.

## Change (b60)

- **Season colors:** the stones are now opaque season tones, each measured against the paper:

  | Season | Contrast |
  |---|---|
  | Summer green | 3.4:1 |
  | Autumn amber | 3.3:1 |
  | Winter blue | 3.5:1 |

  The walked stones keep the deep water-channel teal.
- **Sizes:** upcoming stones go from 11 to 14 pt, and the current ring from 18 to 22 pt. Spacing goes from 6 to 7 pt; the row is about 253 pt wide and fits a 402 pt phone.
- **News marks:** the parchment disc goes from 28 to 32 pt and the chat glyph from 18 to 21 pt.

## Codex blocker (recorded, not worked around)

- The I83 review also flagged the "change your choice" glyph (two opposed arrows) as reading like shuffle. I wrote a Codex prompt, `art/prompts/ui-icons-6.txt`, asking for four candidate glyphs, and added a pipeline slicing step, `icons-ui-6` → `ui-choose-*`.
- `codex exec` failed with "Your workspace is out of credits."
- I tried composing a glyph from existing Codex icons (a sprout plus the turn arrow). The 128 px sources made it thin and left a stray fragment, so it was worse than the current icon and I did not ship it.
- The prompt and pipeline step are kept, ready for when credits return.

## Evidence

- `I84-legible-stones.png` shows the lower half of the hub: b59 on the left, b60 on the right. Both are live launches on the iOS Simulator.
- The verifier passes, and the b60 QA tour captured all 19 screens.

## Still unverified

- Contrast is computed from the colour values, not measured on a device screen.
- No players aged 40+ have tried this (m3-session-kit P1–P9).
