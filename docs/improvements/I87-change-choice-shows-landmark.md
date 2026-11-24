# I87 — Change-choice button shows the landmark it changes

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

## Problem

Three Haiku design reviews (I77, I79 and I83) said the memory screen's "change your choice" button, two opposed arrows, reads as "shuffle". The planned fix was a new Codex glyph, `art/prompts/ui-icons-6.txt`, but Codex failed three times with "Your workspace is out of credits". You chose to keep polishing without new Codex art.

## Change (b64)

The button now shows what it would change rather than an abstract verb:

- **Picture:** the painted landmark you just chose, at 46 pt and full colour. These are the Codex choice icons from earlier iterations: shade tree, crossing or wildflowers.
- **Turn-back badge:** a small parchment badge (24 pt) at the landmark's lower-right corner holds the existing undo glyph, tinted water-blue.
- **Unchanged:** the spoken name ("Change your choice"), the press-and-hold tip and the action.

The helper `ShowChoiceToChange(Button, VillageChoice)` swaps the button's tinted glyph for the painted sprite and adds the badge.

## Cold-read (fresh Haiku agent, told only the screen context, not the button's purpose)

| Version | First read | Confidence | Could be mistaken for |
|---|---|---|---|
| **A** (b63, two opposed arrows) | "swaps or shuffles something" | low | switch view, shuffle, trade places |
| **B** (b64, landmark with turn-back) | "undoes the flower pick, or takes me back to choose again" | **medium** | general back or exit, reset |

The verdict: **B** "more clearly means change the choice I just made… the wildflower image ties the button to the flowers I grew."

## Evidence

- `I87-change-choice-shows-landmark.png` shows the memory screen's action row: b63 on top, b64 below. Both are from the QA tour shot `06-consequence-flowers`.
- The verifier passes, and the b64 QA tour captured all 19 screens.

## Still open

- The cold-read is one AI agent, not players.
- The remaining confusion is with a generic "back" button. A purpose-drawn Codex glyph is still queued for when credits return.
- No players aged 40+ have tried this (m3-session-kit P1–P9).
