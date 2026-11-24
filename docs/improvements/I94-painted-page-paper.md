# I94 — The whole page becomes painted paper

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

## Problem

Every painted element in Mosselora sits on one flat cream fill (`#F9F2E3`): the board bed, the plaques, the villagers and the vignettes. The tiles and cards already carry the Codex-painted watercolour paper grain (`tile-paper`, since the early painted-UI iterations), but the page around them was a plain colour. That flat page is the last thing that reads "web page" rather than "painted sheet". The hook's latest feedback names immersion as unproven.

## Change (b73)

- **Art pipeline:** new `page_grain()` in `tools/evergrove_art_pipeline.py`. It takes the existing Codex master `art/generated/paper-grain.png` (no new generation; Codex is still out of credits). It mirrors it 2×2 so the tile is seamless, flattens it to near-white (gain 2.6) and saves `page-grain.png` at 512 px with a full-rect mesh, which Unity needs for tiling.
- **`MosseloraArt.PagePaper(Image, Color)`:** tiles the grain (about 180 pt repeat) tinted with the page colour, and falls back to the flat colour if the sprite is missing.
- **Where it applies:** the game's root backdrop (under every scrolling screen) and the hub. The page now reads as watercolour paper everywhere, and the existing tile and card grain sits on top of it.
- **Colour:** the page tint is unchanged, so text and icon contrast is unchanged. The grain moves luminance by only a few percent.

## Evidence

`I94-painted-page-paper.png` has two parts:

- **Left:** the empty page area under the village strip on stream 2, from QA tour shot `09-chapter2-after-flowers`. b72 on top is flat cream; b73 below shows watercolour fibre.
- **Right:** the live b73 hub's lower half. The wordmark, tagline, season stones and Play sit on painted paper with no visible tiling seams.

The verifier passes, and the b73 QA tour captured all 19 screens.

## Still unverified

- Seams and grain were judged on the simulator at 3× only. I have not checked them on a physical display or iPad.
- The menu and journal sheets keep their flat parchment rounded panels for now.
- Codex is still out of credits for new art.
- No players aged 40+ have tried this (m3-session-kit P1–P9).
