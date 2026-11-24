# I01 — Painted butterfly lifts off the garden on solve

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**Player problem.** Finishing a creek was marked only by state changes (tiles turn green, droplets fill). For a calm game played for its small, daily pleasures, the moment of completion needs a small reward of its own, without fanfare, sound or score.

**Change.** Codex painted a two-frame brimstone butterfly (`games/evergrove/art/generated/butterfly.png`, prompt in `art/prompts/butterfly.txt`); `tools/evergrove_art_pipeline.py` slices it into `butterfly-open` and `butterfly-closed`. `ButterflyFlight` (MosseloraArt.cs) launches once on the solving rebuild. The butterfly lifts off the garden tile, beats its wings by alternating the two frames every 0.11 s, drifts up in a soft S-curve and fades out over 2.1 s, which falls within the 1.25 s solve hold and the glide that follows. It plays once and destroys itself; there is no idle motion.

**Vision fit.** §4.2 "a garden that answers": a living thing appears because the player restored the water. There is no score, streak or prompt.

**Evidence.** b15 iOS Simulator: a screenshot burst during the real solving tap shows the butterfly over the board in the first three frames, then the glide to "What will you help grow?" (`I01-butterfly-frames.png`). The QA tour produced all 17 screens.

**Not verified.** Whether adults 45+ notice it or find it pleasant; reduced-motion preference (EG-114) still to be wired.
