# I104 — iPhone SE fits every screen; stale toasts cleared

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

## Why

Every check so far ran on a tall 402×874 pt phone. Many players aged 40+ use smaller phones, such as the iPhone SE class at 375×667 pt, the smallest current iPhone screen. This has been listed as unverified since I85.

## Check (iPhone SE 3rd generation simulator, iOS 18.5, b86 then b87)

The scripted QA tour plays every screen by itself; tapping on the new simulator needed a permission nobody was there to grant. On the SE, the tour reached `MOSSELORA_QA_TOUR_DONE 20` and all 20 screens fit:

- **Hub:** the painting, the three neighbors with news, the brand logo, tagline, stones and Play, with no clipping.
- **First puzzle:** Mira's note, the whole 3×3 board, water drops and all three tool plaques.
- **Solved:** the whole board, the question, three choice cards and undo (I74's glide).
- **Memory screens:** the painted moment, caption, stones, the next stream and both action buttons.
- **Journal:** 4 and 12 memories with the pinned close button (I101). The ending has the postcard and new-garden buttons, and the menu scrolls to the end.
- **4×4 and 5×5 boards** (streams 7 and 10, autumn and winter): the whole board plus its tools.

## Defect found and fixed (b87)

Shot `14-garden-file` (SE, b86): a rooted-tile toast ("Bridge foundations anchor this pipe. It cannot rotate.") was still floating over the menu sheet, pointing at a tile that was no longer on screen. Toasts live 3 s on the root canvas, and opening the menu rebuilds the page beneath them. It is easiest to hit on a small screen, where the toast and the sheet overlap.

- **Fix:** new `RuleToast.Dismiss()`, called at the start of every `Rebuild`. Both toast sources, the rooted-tile tap and the ending postcard, show without rebuilding, so they still appear. They just no longer outlive their screen.
- **Verified:** SE b87 shot 14 shows a clean menu, and shot 13 still shows the toast above the tapped rooted tile.

## Evidence

- `I104-iphone-se-all-screens.png`: all 20 QA tour screens on the iPhone SE (b87).
- `I104-stale-toast-fixed.png`: SE shot 14, b86 (stale toast over the menu) beside b87 (clean).
- The verifier passes, and the b87 tour captured all 20 screens on both the 402×874 and SE simulators.

## Still unverified

- SE at 125% and 150% text.
- iPad and Android small screens.
- Codex is still out of credits.
- No players aged 40+ have tried the game (m3-session-kit P1–P9).
