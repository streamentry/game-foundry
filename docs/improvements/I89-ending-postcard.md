# I89 — Ending offers the postcard first

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

## Problem

The ending is the season's payoff: the festival painting, all twelve landmarks in order and a closing line. Its only action was the "new garden" seedling, in the primary teal. The spoiler-free garden postcard (vision 4.4, rung 1: no-server sharing) lived only in the menu. So at the one moment a player is most likely to want to show someone their garden, the game's strongest offer was to start over.

## Change (b66)

On the ending, the action row now has two icon-only plaques:

- **Copy garden postcard (primary teal, chat-leaf glyph):** copies the postcard to the clipboard. It is the same `GardenPostcard.Build`, with no stream line because the season is complete. A parchment toast on the plaque says the existing `postcard_copied` line ("Postcard copied. Paste it into a message to share your garden."). No rebuild, so the ending does not jump.
- **New garden (seedling, now quiet wood):** starting over erases the garden, so it is no longer the loudest control. The two-step confirm is unchanged.

The spoken names and press-and-hold tips reuse the existing `postcard` and `explore` keys. There is no new text. The menu's postcard is unchanged.

## Evidence

- `I89-ending-postcard.png` shows the lower half of the ending from the QA tour (`11-ending`): b65 on the left, b66 on the right.
- The verifier passes, and the b66 QA tour captured all 19 screens.

## Not verified

- The tap-and-toast was not exercised live, because reaching the ending by hand takes twelve streams. The copy call is the same one the menu postcard uses, and the toast is the same `RuleToast` used for rooted tiles.
- Codex art is still blocked by credits.
- No players aged 40+ have tried this (m3-session-kit P1–P9).
