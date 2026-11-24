# I03 — Menu sheet and floating chrome replace on-page settings

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**Player problem.** The page read like a long web form. Under the puzzle sat the settings, the journal toggle, garden-file controls, new garden, research tools and the promise line, all shown all the time. A 45+ player opening the game to relax had to scroll past controls they rarely need.

**Change.** Smart navigation:
- **Floating chrome.** A leaf **menu** button (new Codex glyph, `art/generated/icons-ui-3.png`) is top-left, and the **memory book** with its count is top-right. Both float over the page and never scroll away. The book only appears once there are memories.
- **Menu sheet.** A parchment sheet rises over the dimmed garden; tapping the garden closes it. Its top is an icon-only settings row (language, text size, board size, sound) with a close button. Below are the garden file pair, new garden, the "Offline · No timer · No ads · No account" promise, and the dev-only research tools.
- **The page** keeps only play (board, droplets, tools), the village strip, the story and the neighbors.
- **Screen readers** now walk the whole canvas, so chrome, page and the open sheet are all spoken. The QA tour opens the menu through the real button when it needs a control inside.

**Vision fit.** §2.6–2.7: calm, readable, no clutter. "Learned in seconds" needs fewer things on screen.

**Evidence.** b16 iOS Simulator QA tour: 17 screens, no exceptions (`I03-menu-sheet.png`: stream 1, stream 2, journal with the book badge, menu sheet open).

**Known issue → I04.** The floating buttons overlap the wordmark at the top and the stream pill when the page scrolls.
