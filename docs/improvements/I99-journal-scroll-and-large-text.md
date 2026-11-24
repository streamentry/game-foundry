# I99 — Long journal and 150% journal verified; tour scrolls the sheet

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

## Open items from I95 and I98

- A long journal, with enough memories to scroll, had never been checked, including under I95's rounded sheet mask.
- The journal at 150% text, after I98's fix, had not been re-checked.

## QA tour change (implemented by a Haiku sub-agent, reviewed and run here)

A Haiku sub-agent got an exact spec and edited only `DevQaTour.cs`, with no build and no commit:

- **New helper `ScrollJournalToBottom()`:** it scrolls the journal sheet's own `ScrollRect` (the "Journal panel").
- **Bug fix in the tour:** `10b-journal-bottom` used to call `ScrollToBottom()`, which scrolls the page behind the sheet, so that shot never scrolled the journal. It now scrolls the journal.
- **New shot `11c-ending-journal-bottom`:** at the season's end, it opens the book with all twelve memories, scrolls to the last page, and closes it again.

My review covered three points:

- The diff matches the spec in one file.
- The ending shows the book (the chrome shows it whenever memories exist) and builds its journal (the ending branch calls `MemoryJournal`).
- Nothing in the tooling hard-codes the shot count. The tour now ends with `MOSSELORA_QA_TOUR_DONE 20`.

## Evidence

`I99-journal-scroll-and-large-text.png`, left to right:

1. **b79 `10b-journal-bottom`:** four memories, which fit without scrolling.
2. **b79 `11c-ending-journal-bottom`, twelve memories scrolled to the end:**
   - The pages clip cleanly inside the rounded painted sheet.
   - The season stones show autumn amber on streams 7–9 and winter blue on 10–12.
   - I90's varied memories read correctly: the third bridge (stream 7) is "Orin keeps your crossings in good repair", and the fourth (stream 10) cycles back to "Orin remembers the bridge you restored".
3. **b79, live at 150% text:** I solved stream 1, chose wildflowers, and tapped the book on the memory screen (the I98 path). Fen's memory shows at large size with its stream label and close button.

The verifier passes, and the b79 QA tour captured all 20 screens.

## Still blocked

- Codex art: out of credits.
- No players aged 40+ have tried the game (m3-session-kit P1–P9).
