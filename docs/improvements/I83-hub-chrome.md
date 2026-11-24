# I83 — Hub wears the menu and memories book

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

## Fresh Haiku review (screens from I79–I82, live play)

| Review | Immersion | Navigation | Icon-first | Polish | Daily appeal | Fit 40–49 |
|---|---|---|---|---|---|---|
| I73 (b46) | 7 | 5 | 6 | 7 | 5 | 6 |
| I77 (b49) | 7 | 5 | 7 | 6 | 6 | 7 |
| I79 (b52) | 7 | 6 | 7 | 7 | 6 | 6 |
| I83 (b56) | 7 | 5 | 7 | 7 | **7** | 6 |

Daily appeal reached 7 for the first time, after I80's neighbor news marks. Navigation fell back to 5. The reviewer's first defect was right: "no menu or journal button is visible on the opening screen, which is where a returning player starts."

The other findings:

- **Deferred to the next iteration:** the "change your choice" glyph (two opposed arrows) reads as shuffle. It needs new Codex art.
- **Rejected:**
  - "Keep the wordmark on the choice screen." I74 scrolls so the whole solved board and all three choices fit on one screen, and the wordmark is what gives way.
  - "Put undo in a shared control row." I79 already gave undo the same bottom-left slot.

## Change (b59)

- The hub now wears the same chrome as the game, in the same corners:
  - **Menu (top-left):** closes the hub and opens the menu sheet.
  - **Memories book with its count (top-right):** shown once there is at least one memory. It closes the hub and opens the journal at the top.
- The buttons sit in a bar fitted to `Screen.safeArea` at the hub's top edge, so they clear the notch exactly like the in-game chrome. They render above the hub painting.
- No new keys: they reuse the `menu` and `journal_show` labels.

## Evidence (live, iOS Simulator)

`I83-hub-chrome.png`, left to right:

1. b55 hub with no chrome.
2. b59 fresh hub: the menu button is top-left.
3. Tapping it opens the menu sheet.
4. After solving stream 1, choosing wildflowers and relaunching: the hub shows the menu and the book with "1". All three neighbors carry news again, because the new landmark changed what each one says (I80).
5. Tapping the book opens the journal on Fen's memory.

The verifier passes, and the b59 QA tour captured all 19 screens.

## Still unverified

- The screen-reader order puts the hub chrome after Play, because hierarchy order decides it. Not changed.
- No players aged 40+ have tried this (m3-session-kit P1–P9).
