# I128 — The book is as tall as its pages

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player

I opened the book after four streams. A pale sheet rose to 86% of the screen. "Our choices" and four pages sat in a box drawn inside the sheet, with half the sheet empty below it. It read as a web pop-up with a card in it, not as a book I was opening, and it hid the garden for no reason.

## PM decision

- **Delete:** the box inside the sheet. The pages lie on the sheet's own painted paper.
- **Simplify:** a sheet is as tall as what it holds, so the garden stays visible above a short journal. A long journal still stops at 86% and scrolls.
- This applies to the menu sheet too: it is the same component, so release builds with a short menu also stop showing an empty half-sheet.

## Dev

`EvergroveApp.cs`:
- `FitSheetToPages(sheet, pages)` sets the panel's top anchor to (pages height + header 78 + 14) ÷ sheet height, clamped to 34–86%. `Rebuild` calls it after layout for whichever sheet is open, and now also rebuilds the journal layout first.
- `MemoryJournal` builds the "A living journal" card with a transparent fill (no edge, no shadow).

## Result (b129, iPhone 17 simulator, scripted tour)

- With four memories (`10-journal`), the sheet now rises to about half the screen. The stone path, the chapter name and the top of the board stay visible above it, and the pages sit directly on the paper.
- With twelve memories (the ending journal), it stops at 86% and scrolls as before.
- The development menu is long enough to reach the 86% cap, so it looks unchanged.
- Local comparison: `docs/improvements/I128-journal-fits.jpg` (gitignored). The scripted tour captures all 20 screens.

**Not verified:** a release-build menu (shorter, so it should now sit lower), and landscape and iPad sheets.
