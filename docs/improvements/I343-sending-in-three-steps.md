# I343 — Sending in three steps

Run 3, batch 2 ([player notes](../../games/evergrove/docs/playtest/run3-player-notes-2026-11-27.md), P1).

## Player

"Too much at once. My name, eight gift tokens, a wish, a stamp, a letter and a send arrow on one page feels like a form, not a present."

## PM decision

Sending becomes three calm steps:
1. **The gift.**
2. **How it's wrapped.**
3. **A few words, and who it's from.**

Three dots show where you are; a back token is always there; nothing is sent until the last step's send token. Each step fits a phone screen without scrolling, even at 150% text.

## Dev

- `FamilySheet`:
  - `sendStep` and `StepDots`;
  - `StepTokens` (back and onward, as round icon tokens);
  - `BuildGiftStep`, `BuildWrapStep` and `BuildWordsStep`.
- After sending, it starts again at step one.
- The parcel rules and economy are untouched: the same `DoSend` and the same parcels.
- Tour: `QaStep` shows the wrapping and words steps (17p1, 17p2).

## Result

Sending feels like giving, one decision at a time. The extra taps are a trade-off to watch: if players want it faster, step two can be skipped for letter-only parcels.
