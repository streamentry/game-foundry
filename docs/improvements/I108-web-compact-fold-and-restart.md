# I108 — Web slice: first tile back above the fold; menu Restart works

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

## Problem

`main`'s CI was red on `evergrove-browser-viewport` (the web first-minute research slice, also copied to the site's `/preview/game`). At 320×568, the smallest phone class, the first puzzle tile sat at y=516, below the fold the test requires. Fixing that uncovered a second, hidden failure: "Reset attempt lost in notes". Neither came from PR #77: both fail identically on `main` at `cd66d3b`.

## Causes (both from the web design iterations on `main`)

1. **I97 (safe-area insets)** forced `padding: max(18px, …) !important` on `.app`, overriding the compact 15 px gutter. At 320 px the "4 of 9 stream reached" pill then squeezed the chapter line and title into four lines each (164 pt tall instead of 107).
2. **I09 (restart safeguard)** made Restart need two taps. The test still tapped once, and the menu's Restart item called `click()` once, so it **only armed the safeguard and never restarted**. The armed state was visible (warm colour, wobble) but not spoken.

## Changes

- **`style.css` (research slice and site copy, kept byte-identical):**
  - At ≤410 px the gutter is `max(15px, env(safe-area-inset-*))`. I97's safe-area protection stays.
  - At ≤360 px the progress pill wraps under the chapter title, so the chapter line and title fit on one line each.
- **`game.js` (both copies):**
  - The menu's Restart confirms at once, because choosing it in the menu is already deliberate.
  - While armed, the button's `aria-label` becomes "Tap again to restart stream", and resets afterwards.
- **`tools/verify_evergrove_browser_viewports.py`:** follows the two-tap design. It asserts that the first tap visibly arms (class) and audibly arms (label), then confirms.

## Evidence (local Chromium, the CI-pinned Playwright 1.56.0)

- 320×568, first tile: y=516 → **452** (limit 512). The chapter and title now fit on one line each. The screenshot is local only: `docs/improvements/I108-web-320-fold.png` (gitignored).
- `verify_evergrove_browser_viewports.py`: **PASS** at 320×568, 375×667 and 430×932 (first tile, touch, Undo, full creek, choice/focus, no script errors).
- Menu Restart after one move: before the fix the board stayed at "5 of 9" (no restart); after it, "4 of 9".

## Also

- Codex was retried after the PR #77 merge (10th try) and still fails with "out of credits".
- No players aged 40+ have tried the game (m3-session-kit P1–P9).
