# I97 — Share button shows the postcard it sends

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

## Problem

The I96 Haiku review scored icon-first buttons at 6. It named the ending's share button as a cause, a speech-bubble-and-leaf glyph that "does not clearly mean" what it does. The button copies a spoiler-free postcard of the finished garden to the clipboard (I89).

## Change (b76)

This uses the same approach as I87, where the button shows the thing it acts on. `ShowPostcard(Button)` replaces the glyph with:

- a small parchment postcard, 50×38 pt, with a soft lift;
- the season's own Codex festival painting (`ending-festival`), cropped inside it;
- the existing chat glyph moved to a small parchment badge at its lower-right corner.

There is no new art and no new text. The spoken name and press-and-hold tip stay "Copy garden postcard", and the action is unchanged.

## Cold-read (fresh Haiku agent, told only the screen context)

| Version | First read | Confidence | Could be mistaken for |
|---|---|---|---|
| **A** (b75, bubble and leaf) | "opens a chat or message area" | low | help, feedback, leave a note |
| **B** (b76, framed garden picture with chat badge) | "opens a view of the garden picture, possibly with a way to send it" | **medium** | photo gallery, view my garden |

The verdict was **B**: "the framed garden picture matches a postcard of my finished garden; A has no picture, so it gives no hint of sharing."

## Evidence

- `I97-postcard-share-button.png` shows the ending's action row from QA tour shot `11-ending`: b75 on top, b76 below.
- The verifier passes, and the b76 QA tour captured all 19 screens.

## Still open

- The remaining confusion is with "view my garden". A purpose-drawn Codex share glyph would help, but Codex is still out of credits.
- The cold-read is one AI agent, not players.
- No players aged 40+ have tried this (m3-session-kit P1–P9).
