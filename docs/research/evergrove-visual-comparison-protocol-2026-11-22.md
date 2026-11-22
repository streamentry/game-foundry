# Evergrove: visual fidelity versus lived consequences — comparison plan

**Status:** test protocol, **not a completed player study**. Written 2026-11-22 after adding a switchable 2.5D prototype. No real-device screenshots or measured outcomes are implied.

## Decision question

Does the small fixed-camera **2.5D miniature** communicate the same lasting changes more clearly and delightfully than the existing **2D illustrated garden**, *at acceptable mobile performance and authoring cost*?

This is a **different decision** from the main Living World vs Puzzle Only product experiment. Do not confuse “people like 3D” with “the consequence system makes them return.”

## Stimulus

- **2D control:** existing `VillageIllustration.cs`. No network, same colors/narrative.
- **2.5D treatment:** generated `VillageDiorama.cs`, one 768×384 orthographic RenderTexture, default **motion off**. Camera stays fixed; Unity primitive assets are original. It uses the same saved world events via `VillageScenePlan`.
- **Equivalent world histories:** (A) empty world, (B) one tree, (C) tree + crossing, (D) tree + flowers, (E) all three, (F) endgame after twelve choices.
- **Keep fixed:** device class, narrative text, puzzle content, timing of scenes, sound off, visibility and explanation of causal changes. Do not show one style with better lighting/animation and then attribute preference to 3D.
- **Assign order:** for participants 1,3,5,... show 2D then 2.5D; for 2,4,6,... show the reverse. The built-in toggle is for a facilitator; do not have players self-select if you intend a meaningful comparison.

## Cheap test protocol (10–15 representative players)

1. Ask player to look for ten seconds at each style (balanced order) without the story panel.
2. Ask **“What changed? Why?”** after showing tree, bridge and flower combinations. Record exact unprompted observations.
3. Let them play one actual water puzzle, make one choice, and then inspect the resulting world in each art style. Ask which feels more understandable, distinctive and inviting to revisit.
4. Use **a polished reference game** alongside both homegrown versions to avoid false praise from comparing only two rough prototypes.
5. Capture obstacles in touch controls, text, scroll, eye comfort, motion sensitivity and meaningful attachment, not merely “which screenshot is prettier?”
6. On each target device, record Unity Editor/import errors; Android/iOS cold launch, median/p95 input latency and frame time, memory peaks, texture GPU footprint, battery/heat, and whether the preview fails safely back to 2D. **No measurements have been collected yet.**

## Testable success and stop rules

- **Continue exploring 2.5D** only if a majority of sampled players **accurately identify more causal changes** or clearly prefer the miniature *after* understanding both treatments, **and** performance/authoring cost remains manageable on lower-end devices. This threshold is directional with small samples.
- **Keep 2D** if consequence comprehension is similar or better, if the miniature looks generic or distracts from the puzzle, or if battery/memory/tooling effort is disproportionate.
- **Stop the art branch early** if phones cannot render it reliably; never block basic offline puzzle play on the diorama.

Record raw participant counts and quotes in this directory, separating observed outcomes from guesses. Do not compute spurious precision from fewer than 20 people.

## Manual device checklist

- [ ] Open Unity 6.3 project, compile both renderers and run the editor playtest scene.
- [ ] Flip 2D ↔ 2.5D before and after the same world choices; confirm the same tree, crossing, meadow, visitors and butterflies appear.
- [ ] Force-close/reopen after choice; confirm the same diorama composition from the restored JSON state.
- [ ] Enable/disable optional motion, verify no flashing/jank, toggle 2D to confirm the preview camera stops rendering.
- [ ] Test on compact ~320px viewport, safe-area/notch phone and tablet; puzzle board must stay in bounds.
- [ ] Check CPU/GPU/memory and screen readers; 2D baseline should always remain playable if shaders or RenderTexture are unavailable.
- [ ] Talk to Mira, Orin and Fen before and after combinations; only observed choices may be referred to as “remembered.”
- [ ] Run C# domain tests, then Unity Editor smoke and actual touch builds; passing C# tests alone is insufficient.

**Important:** The player experience is still bounded by 12 authored puzzles and hand-written interactions. A pretty 2.5D scene does not make the world infinite, social or AI-driven.
