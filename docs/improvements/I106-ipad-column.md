# I106 — iPad keeps a phone's height and a book-width column

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

## Why

The Unity project targets iPhone **and** iPad (`targetDevice: 2`), but no screen had ever been checked on an iPad. Many players aged 40+ prefer a tablet for exactly this kind of calm game.

## Check (iPad Air 11-inch simulator, iOS 26.5, b89)

The QA tour completed (`MOSSELORA_QA_TOUR_DONE 20`), but the layout was wrong for a tablet:

- **Cause:** the canvas scaler matched width and height equally (0.5), which suits phones (aspect 0.46–0.56). On the squarer iPad (aspect about 0.69) it left a logical page of about 507×730 pt: less height than a phone, while every painting scaled up with the width.
- **First puzzle:** the undo, hint and restart plaques were cut off at the bottom.
- **Memory screen:** the continue and change-choice buttons fell below the fold.

## Change (b90)

New `FitColumn()`, called from `FitSafeArea()` on every size change and once the page content exists:

- **On tablet-shaped screens** (safe-area width/height above 0.62), the scaler matches by height (`matchWidthOrHeight = 1`). The page then gets a phone's 896 pt of logical height.
- **The page becomes a centred, book-width column:** at most 480 pt, using extra side padding on the content's layout group. `PageWidth()` and the board's tile sizing subtract that inset, so paintings and the board size to the column, not to the screen.
- **Full-screen pieces stay full-screen:** the hub painting, the floating chrome, and the menu and journal sheets.
- **Phones are unchanged** (all are below 0.62).

## Evidence

- `I106-ipad-column.png`, left to right:
  1. iPad b89 first puzzle: tools cut off.
  2. iPad b90: board, drops and all three tools on screen.
  3. iPad b89 memory screen: buttons below the fold.
  4. iPad b90: both buttons on screen.
- The full b90 iPad tour, all 20 screens, fits: hub, choice cards, journals, ending, menu, and the 4×4 and 5×5 boards with their tools.
- **Phones unchanged:** pixel diffs of the 402×874 tour, b89 vs b90, show 0 changed pixels on 18 of 20 shots. The other two differ by 10 and 459 pixels, which is animation timing (tile swell, butterfly).
- The verifier passes.

## Still unverified

- iPad mini (aspect about 0.66 takes the same path) and the large iPad Pro were not run.
- iPad landscape: the app is portrait.
- Small Android phones.
- Codex is still out of credits.
- No players aged 40+ have tried the game (m3-session-kit P1–P9).
