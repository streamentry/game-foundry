# I95 — Menu and journal sheets are painted paper too

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

## Problem

I94 put the Codex watercolour paper under every screen, but the menu and journal sheets that rise over the garden kept a flat parchment fill. I named this as open in I94, and the latest hook feedback repeated it ("Menu and journal panels are still flat").

## Change (b74)

In `BuildSheet`, used by both the menu and the journal:

- A full-size child `"<name> paper"` carries `MosseloraArt.PagePaper`, the same tiled Codex grain and page tint as the background. It sits first, behind the sheet's content, and ignores layout.
- A `Mask` on the sheet panel (graphic still shown) clips the grain to the panel's own rounded 26 pt shape, so the corners stay round. The rim (`Edge`) and the soft lift shadow are unchanged.

There is no new art. This reuses the existing Codex `paper-grain` master through I94's `page-grain` sprite.

## Evidence (iOS Simulator)

`I95-painted-sheets.png`, left to right:

1. **b65 journal (QA tour):** flat parchment sheet.
2. **b74 journal (QA tour `10-journal`):** watercolour grain clipped to rounded corners, with every card, stream label and the close button in place.
3. **b74 menu, live:** grain in the sheet. Tapping "Gentle chime" inside the masked sheet switched it on, so input passes through the mask.

The verifier passes, and the b74 QA tour captured all 19 screens.

## Still unverified

- The journal was not scrolled with many memories under the new mask; the ScrollRect's own RectMask2D still clips its content.
- Codex is still out of credits for new art.
- No players aged 40+ have tried this (m3-session-kit P1–P9).
