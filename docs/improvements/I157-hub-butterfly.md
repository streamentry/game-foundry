# I157 — A butterfly visits the home painting

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player

The home screen is the most beautiful view in the game, a full painted village, and it's **completely still**. It's what a returning player sees every morning, and a still picture says "menu screen". Every top cosy game's title view breathes a little: leaves, birds, light. The brief asks for "super immersive", and this is the first thing a 40+ player sees each day.

## PM decision

- **Now and then a painted butterfly drifts across the village** (the same watercolour butterfly that celebrates a solve): first after about 2.5s, then every 8–13s. It rises diagonally with a gentle sway over 4.5s, flapping, fading in and out at 95% opacity, clipped to the painting.
- Only with gentle motion on. It's decoration only: no game state, no clock and no reward, and nothing to tap (it never takes input). It never runs on the board, where attention belongs to the puzzle.

## Dev

- New `HubButterflies` component (in `MosseloraArt.cs`): it owns one flier at a time and alternates the open and closed wing sprites every 0.13s.
- `EvergroveApp` home screen: added to the masked "Village window" when gentle motion is on (not in the QA tour).

## Result (b166, iPhone 17 simulator, fresh install, home screen recorded every 0.5s)

The frame differences show a moving region rising from about y 363 to y 159 across frames 1–5 (about 2.5s), then nothing. The zoomed frame (`docs/improvements/I157-hub-butterfly.jpg`, gitignored) shows the pale butterfly crossing in front of the cottage window, between Mira and Orin. Tours capture all 20 portrait and 5 landscape screens. Verify passes.

**Not verified:** whether players notice it, or find it distracting (P9 in the M3 kit covers motion). Battery: one small sprite animating while the home screen is open is negligible, but it's not measured.
