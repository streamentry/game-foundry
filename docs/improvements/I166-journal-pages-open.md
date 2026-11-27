# I166 — Every page in the book opens to its memory

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player (Haiku walk-through of b152, as a 52-year-old)

"Screen 5, 'Our choices': the numbered cards look like a recap, but **I can't tell whether they're tappable.**"

They weren't. The book showed each memory as a small page (stone, number, tiny painting, one line), with nowhere to go from there. A scrapbook you can't open is a list.

## PM decision

**Every page opens.** Tapping a page lifts that memory to full size inside the book: the stream's stone and number, **the large painted memory** (the same vignette as the memory card) with the keeper's portrait in its corner, the remembered line in the book serif, and **what it changed**. For a landmark that is the constraint it put on the next board ("Bridge foundations anchor this pipe. It cannot rotate."), which is the game's core claim written in the player's own history. A round back token, or a tap anywhere on the page, returns to the book. Closing the book also closes the page.

The full record line, with its "Chapter 1 · Orin · Restore a crossing" heading, is spoken only. On screen it read like a database row (first build).

## Dev

- `EvergroveApp`: `journalOpenPage` state. Each memory page in `MemoryJournal` becomes a `Button` (press tint and squash) that opens `JournalPageOpen(index, record)`, which builds the open page in the journal sheet. Closing the sheet resets the state. `FitSheetToPages` sizes the sheet to the open page.
- `DevQaTour`: after the journal shots it presses a page, saves `10c-journal-page.png` and presses "Back to the book", so every tour now covers the open page (21 portrait shots, 6 landscape).

## Result (b177 → b178, iPhone 17 simulator, tours)

- b177 (`I166-open-page-first.jpg`): the page opened, but the consequence showed the raw "Chapter 1 · Orin · …" record line.
- b178 (`I166-open-page.jpg`, both gitignored): portrait (Orin's bridge, "Orin crosses your bridges every morning." plus the bridge-foundation rule), 150% text (Fen's flowers, large serif, the back token still on screen) and landscape (scrolls inside the sheet). All three tours pass (21/6/21). Verify passes.

**Note:** the tour presses whichever page Unity finds first, so the open page varies between runs. **Not verified:** VoiceOver reading the open page, and whether players discover that pages open (no affordance beyond the press tint).
