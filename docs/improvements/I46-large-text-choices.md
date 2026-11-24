# I46 — 150% text pass: choices stack as wide cards

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**What.** A full pass of the QA tour at **150% text** (the largest setting, chosen by many 60+ players) over the design as of I44, then a fix for what broke.

**Found (b32 at 150%, `I46-150pct-before.png`).**
- Choice cards: the narrow side-by-side cards broke words mid-word ("Wildflow / ers", "Restore / a / crossing"). **Fixed.**
- Everything else held: coach bubble, board, icon tools with captions, memory card, scrapbook sheet, labelled menu tiles and neighbor name plates all wrap and grow without clipping.

**Change.** With text above 100%, the three choices stack as **wide cards**: the painting (92 pt, with the keeper's portrait) on the left and the name and consequence left-aligned on the right. At 100% they stay three tall cards side by side (I09).

**Evidence.** b33 at 150% vs 100% (`I46-150pct-after.png`). Both tours complete with 19 screens.
