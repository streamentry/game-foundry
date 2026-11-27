# I348 — Decide with a tick

Run 3, batch 3 ([player notes](../../games/evergrove/docs/playtest/run3-player-notes-2026-11-27.md), P11), on the choice preview from PR #82.

## Player

"The preview is honest, but it reads like a manual: 'Make this change' and 'Close' are big text buttons, and 'Row 1, column 1: corner pipe, dry' is engineering talk."

## PM decision

The decision is two round tokens, like every other decision in the garden: a large tick to make it so, and a smaller cross to step back. The next stream's protected tile still shows; its "row and column" description becomes screen-reader only, because the tile shows it. The heading, the choice's name and its true consequence stay as words: they're the decision.

## Dev

- `EvergroveApp.ChoicePreview`:
  - `IconButton` round tokens (`IconConfirm` 80, `IconCancel` 64), with the same object names ("Apply garden choice", "Close choice preview"), so the tour's stale-callback checks still hold;
  - the position label is transparent and ignores layout, but is still read aloud.

## Result

Choosing reads as a game decision. The preview's truthfulness (PR #82) is unchanged.
