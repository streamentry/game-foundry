# I75 — Large-text decision fallback verified live

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

## Check

I74 changed the post-solve glide to keep the whole board above the choices "when both fit", with the heading anchor as the fallback. I had not seen that fallback live. On b48 (iOS Simulator, 402×874 pt) I opened the menu, set the text size to 150%, closed the menu, and made the last solving move with a real tap.

## Result

- **Before the move:** at 150% the board, meter and the three action plaques all fit on one screen, and the tagline stays inside the vine corners.
- **After the glide:** the board plus the stacked cards do not fit, so the glide falls back to the heading anchor. The question reads in two lines. All three choices show as wide rows, with each painted landmark beside its name and the name unbroken. Undo stays reachable, and the village strip with its neighbors sits below.
- **Code:** no change was needed. The fallback behaves as intended.

## Noted, not changed

- The menu sheet opens with about 70 pt of empty parchment beside the close button. It is cosmetic; I left it for a later pass.

## Evidence

- `I75-large-text-choice-fallback.png` shows the 150% board on the left and the 150% decision after the glide on the right.

## Still unverified

- No players aged 40+ have tried this yet (m3-session-kit P1–P9).
