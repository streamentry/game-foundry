# I98 — Book opens on the memory screen; larger journal text

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

## Bug found while checking journal legibility (live, b77)

To check the journal at 150% text, I solved stream 1, chose wildflowers, and tapped the memories book on the **memory screen**. The sheet rose **completely blank**: no pages, and no close button.

The cause is that the journal pages were built only when `!showingMemory`. The book button (I12 onward), however, also appears on the memory screen, the moment a player is most likely to open it to see their new memory. This was a pre-existing bug, not caused by I95's mask or by text size. The QA tour never opened the book from that screen.

## Changes (b77, b78)

- **The fix:** the journal is built whenever the book can show (living mode, not completed, at least one choice), the memory screen included. The ending keeps its own call.
- **Journal legibility** (from the I96 review's "journal text is small" under fit-for-40–49):
  - remembered line 15 → **17 pt**, the memory card's body size;
  - stream label 12 → **13 pt**;
  - season stone 10 → 12 pt.

## Evidence (iOS Simulator)

`I98-journal-opens-everywhere.png`, left to right:

1. **b77, live:** the book opened from the memory screen is blank.
2. **b78, live:** the same action shows Fen's memory under "STREAM 1 / 12", with the close button.
3. **b77 QA tour `10-journal`:** four memories at the new sizes. Mira's line wraps to three lines and the cards stay aligned.

The verifier passes, and the b77 and b78 QA tours captured all 19 screens.

## Still unverified

- The journal at 150% after the fix was not re-checked; the reinstall reset the text size.
- A long, scrolling journal is still unchecked.
- Codex is still out of credits.
- No players aged 40+ have tried the game (m3-session-kit P1–P9).
