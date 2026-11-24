# I19 — Screens crossfade like turning a storybook page

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**Player problem.** Moving between screens (solved → memory → next stream, opening the journal, a neighbor's card) replaced the whole page in one frame, like a web page reloading.

**Change.** When the *screen* changes (screen kind, stream, solved state, journal, conversation or language picker; not a tile turn), the page crossfades in over 0.26 s with a smoothstep curve (`FadeInPage`, a CanvasGroup on the page). It reads like turning a page in a storybook. With gentle motion off it is instant. The scripted QA tour skips it so screenshots capture settled screens.

**Evidence.** b23 QA tour, 17 screens (transitions skipped by design). On the live simulator, opening the hub and continuing to stream 1 crossfades.

**Not verified.** The crossfade frame by frame (a recording is planned with the next motion review).
