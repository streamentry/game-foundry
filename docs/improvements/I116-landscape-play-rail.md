# I116 — Sideways: menu and Mira on the painting, tools beside a bigger board

Date: 2026-11-25. Branch: feat/mosselora-painted-ui (PR #76).

## Problem

On the very first puzzle in landscape, Mira's teaching note sat above the board in the play column, and the column also started 82 pt down to clear the book button (I114). On a 402 pt tall phone this pushed **two thirds of the board, the water drops, Undo and Hint off the screen**, so a first-time player saw a note and one row of tiles. Without the note, tiles were still capped near the 44 pt minimum. In a Haiku cold-read of the first fix, the reviewer named small tiles for 40+ thumbs as the top remaining problem.

## Change (landscape only; portrait is pixel-identical to b105 across the 20-screen tour)

- **Navigation lives on the painting.** The menu and the book (with its new-memory sprout) sit together at the painting's top left, 12 pt down so the sprout is not clipped, with a 10 pt gap between them. The play column carries no chrome, so its top inset is back to 15 pt and the paper band behind the chrome is hidden.
- **Mira speaks from the painting.** On the first puzzle her note floats over the left painting, clear of the camera cutout, with its tail pointing right at the board. It still leaves after the first move.
- **Tool rail.** While the board is unsolved, Undo stands above Hint in a vertical rail right of the board, and the board steps left to make room. Tiles are sized by the height left after stones, drops and margins: **about 76 pt, up from 44–53**, on an iPhone 16 Pro. The Large board setting is capped to the same height.
- **The stone bed hugs the tiles** instead of stretching across the whole column.

## Verification (iOS simulator, iPhone, b106–b109)

- Fresh install, then rotate. The hub shows wordmark and Play. On the first puzzle, the whole board, the drops, the stream stones, the rail and Mira's note are all visible without scrolling (`I116-landscape-before.png` vs `I116-landscape-puzzle.png`, local only).
- Solved **in landscape** by tapping tiles in landscape coordinates. The choice cards appear (`-choice.png`); choosing wildflowers opens the memory screen, with menu and book on the painting and the card, stones, change-choice and continue in the column (`-memory.png`).
- A Haiku cold-read of b107 confirmed the whole puzzle shows on the first visit and the picture note teaches the rule. It also flagged the touching menu and book, the clipped sprout and the small tiles; all three are fixed in b108–b109.
- Portrait tours of b106, b107 and b109 ran to DONE (20 screens), with no screen differing from b105 by more than 500 pixels. C# tests: 41,113 assertions pass. The verifier reports OK.

## Not verified

- **iPad landscape:** simulator access was not granted this session.
- Larger text sizes in landscape: the `-mosseloraTextScale` launch argument did not take effect via `simctl launch`.
- A real device, and players aged 40+ (**age fit not validated**).
