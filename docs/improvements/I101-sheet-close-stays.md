# I101 — Sheet close buttons stay in reach while pages scroll

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

## Problem (from the I100 final review, confirmed in the QA tour)

The close button of the menu and journal sheets lived inside their scrolling content. In a long journal (I99's `11c-ending-journal-bottom`, twelve memories) or a scrolled menu, it scrolled away with the pages. The only way out was then tapping the dimmed garden at the sheet's edge, which is invisible to a player who doesn't know it.

## Change (b80, b81)

- **Fixed header band:** each sheet's viewport now stops 78 pt below the panel top (`SheetHeader`). The pages scroll under that band, which stays still.
- **`SheetClose(sheetContent, name, close)`:** puts the close plaque in the panel itself, top-right, outside the scroll. It is rebuilt with the page, so its spoken name follows the current language, and it replaces the previous plaque. It sits right after the paper grain in the hierarchy, so screen readers meet it first.
- **Removed:** the in-content "Journal top" and "Menu top" rows. The settings tiles and journal title now start at the top of the pages.
- **b81:** the band went from 70 to 78 pt, because the painted plaque stands a few points taller than its 52 pt layout box and touched the first scrolled row.
- No new art, text or keys.

## Evidence

`I101-sheet-close-stays.png`, left to right:

1. **b79 `11c-ending-journal-bottom`:** at the end of twelve memories, the close button is gone.
2. **b81, same shot:** the close plaque is in its band, and the pages clip below it.
3. **b81 menu, live, scrolled to the end** (postcard, garden file, privacy line): the close plaque is still in reach.

The verifier passes, and the b80 and b81 QA tours captured all 20 screens.

## Still unverified

- The new screen-reader position of the close button was not logged.
- The close plaque sits like a tab on the band edge. A painted band edge would be nicer, but Codex is out of credits.
- No players aged 40+ have tried the game (m3-session-kit P1–P9).
