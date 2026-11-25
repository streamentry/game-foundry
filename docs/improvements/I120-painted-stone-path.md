# I120 — The stream path is painted stones: water under the walked ones, a ring on yours

Date: 2026-11-25. Branch: feat/mosselora-painted-ui (PR #76).

## Problem

The stream-progress row (12 coloured dots plus a ring) appears on the hub, the puzzle and the memory screen. It read like web pagination, the last obviously "web" element on every screen.

## Change: a painted path across the creek

- **Codex art (I119 master, viewed this time):** five painted river stones in the board-bed style. Stones ahead wear their season (summer moss, an autumn leaf, winter frost); walked stones are wet slate; your stone grows a sprout. They are sliced by the pipeline into `stone-*` sprites (96 px).
- **Three cues, each added because a blind Haiku cold-read asked for it:**
  1. *b114, stones only:* "OLD reads faster"; walked and ahead stones could not be told apart.
  2. *b116–b118, plus a stream:* a thin, opaque teal stream (`WaterChannel`) runs under every walked stone and half way under yours ("the water got this far"). Done and here moved to **high** confidence, but "the ring was one high-contrast shape the eye finds at once".
  3. *b120, plus a ring:* the old leaf-green ring returns as a **thin 3 pt pill** around your stone (new 9-sliced `MosseloraArt.MarkerRing`, so the stroke does not scale; a bold ring at that size swallowed the stone in b119). **Final read: "the NEW stones read faster for finding your position" and "feel more like part of the game world".** Done and here: high confidence; ahead: medium.
- **Contrast:** pale stones are deepened by ×0.84, so every stone keeps at least 3:1 against the paper, as the old dots did (WCAG 1.4.11, I84): summer 3.9, autumn 4.1, winter 3.4, here 3.2. Walked stones are 4.4 untinted.
- **Size:** 22 pt stones, 34 pt for yours, 4 pt spacing: 320 pt across, which fits portrait and the landscape column.
- Falls back to the old dots if the stone sprites are missing. RTL reverses the stream direction.

## Verification

- b114–b120 Unity iOS simulator builds: 0 `error CS`; the portrait tour ran to DONE (20 screens) on each.
- Before/after crops of the first puzzle, the memory screen and the autumn stream (`I120-path-first.png`, `I120-path-autumn.png`, local only).
- Landscape b120: the ringed path sits above the board in the play column, and everything still fits (`I120-landscape.png`).
- C# tests: 49,481 assertions pass. The verifier reports OK.

## Remaining (from the last cold-read)

- Stones ahead are "medium" confidence. On the first puzzle, mossy stones ahead could be misread as done, because there is no stream yet to contrast with. A visible "8 of 12" was suggested but not added (text on a control-free row; the position is already spoken). Watch for this in M3.
- iPad, Android, larger text, real devices, players aged 40+: **age fit not validated**.
