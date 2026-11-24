# I11 — Painted title screen as a calm daily threshold

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**Player problem.** The app opened straight onto a scrolling page, like a website. A game played as a small daily ritual needs a calm threshold: a breath before play.

**Change.** On every launch, a **title screen** shows the Codex village painting full-bleed (an aspect-ratio-fitted cover), fading into parchment. The wordmark, the tagline and one big teal **play** button (84 pt, arrow icon) sit below it. Tapping play reveals the garden exactly where the player left it. The QA tour skips the title so its board-first screenshots stay comparable; release and normal launches always show it.

**Vision fit.** §2.6 "Respect is permanent". There is no streak, no "you were away 3 days" and no reward for returning; the garden simply waited (§4.2 "a garden left alone waits unchanged").

**Evidence.** b20 normal launch on the iOS Simulator (`I09-I11-title-cards.png`, left): the painting, wordmark, tagline and play button. Tapping play opens stream 1 (`I10-stepping-stones.png`).

**Not verified.** Whether players find the extra tap welcoming or slow. Measure time to first input in M3 (spec target: median first input under 15 s).
