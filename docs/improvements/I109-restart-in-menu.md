# I109 — Board keeps two tools; Restart stream moves to the menu

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

## Fresh Haiku review of the merged build (b92): navigation and immersion

Scores: **Navigation 6, Immersion 4.** I checked both findings against the screens and the brief.

| Finding | Decision |
|---|---|
| "Undo, hint and reset always visible at equal weight; undo and reset both use arrow glyphs; reset is destructive. Move reset into the menu." | **Accepted.** It matches the brief ("does not need to display all buttons every time; maybe we just need a menu button"). `WorldModel.Restart()` clears the undo history, so it cannot be undone. |
| "…and show hint only after a stall" | **Rejected.** AGENTS.md says to keep help and accessibility options visible for players 40+, and a stall timer is the clock-driven behaviour the vision rules out. |
| "Play screens look like a flat cream web page; put a full-bleed painting behind the board" | **Not taken now.** The page is the I94 painted paper (too subtle at 1000 px), and a busy painting behind the tiles would cost board legibility. Revisit with players. |

## Change (b93)

- **Board:** two tools, Undo and Guide me, each now half the row and easier to hit. Restart is gone from the play row.
- **Menu:** a "Restart stream" tile at the top, shown **only** when a stream is in progress with moves to reset (`!Completed && !Solved && !showingMemory && CanUndo`). Tapping it restarts through the same `Act(… "restart")` path (playtest notes still count it) and closes the menu.
- No new text or art. It reuses the `restart` key and the existing Codex restart glyph.

## Evidence (live, iOS Simulator, b93)

`I109-restart-in-menu.png` is local only (gitignored). Left to right:

1. The first puzzle with two tools.
2. After one move, the menu shows "Restart stream" above the settings.
3. After tapping it, the board is back at the start: 4 of 9, Undo disabled, Mira's tip back, menu closed.

The verifier passes, and the b93 QA tour captured all 20 screens.

## Still unverified

- Whether players aged 40+ find Restart in the menu when they want it is an M3 session question.
- Codex is still out of credits.
