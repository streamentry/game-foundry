# I41 — Memory scrapbook rises as its own sheet

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**Player problem.** Opening the memory book appended the scrapbook to the bottom of the play page. The page grew, and the player had to scroll past the game to read their story: a web pattern.

**Change.** The scrapbook rises as **its own parchment sheet** over the dimmed garden, built with the same sheet code as the menu (`BuildSheet` now creates both). A close icon sits top-right, and tapping the garden behind it closes it too. The book button toggles it, and opening it closes the menu. The play page never changes length.

**Evidence.** b31 QA tour: `10-journal` / `10b-journal-bottom` show the "Our choices" scrapbook in the sheet over the dimmed page (`I41-journal-sheet.png`).
