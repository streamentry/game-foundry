# I35 — Seasons turn with progress: autumn and winter villages

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**Player problem.** Twelve streams all looked like the same June afternoon. Vision §4.2 asks for **seasons that arrive through the player's progress, never by the clock**, so a long-time player sees the year turn.

**Change.** Codex painted four seasonal **edits** of the creek strip (flowing and dry, autumn and winter), each keeping the exact composition so every landmark slot still lands on the bank (`art/generated/banner-autumn-*.png`, `banner-winter-*.png`, prompts in `art/prompts/`). `PaintedVillage.SeasonFor` maps the village's existing `LandscapePhase` to a season: phases 0–1 summer, 2 autumn (amber trees, drifting leaves, asters), 3 winter (snow on roofs and banks, thin ice at the water's edge, holly). A **finished** garden turns back to summer for the festival. The page strip and the hub both use it. Missing seasonal art falls back to summer. No date or clock is read anywhere.

**Evidence.** b27 QA tour, with two new tour shots (`15-autumn-village.png` at stream 8, `16-winter-village.png` at stream 11). `I31-I35-bed-seasons-menu.png` (middle) shows the planted trees, bridges and flowers standing in the autumn and the winter village.
