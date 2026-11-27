# I294 — A quiet garden

Idea 50 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after ma, the Japanese sense of meaningful empty space.

## Player

"Sometimes I just want to look at it, without buttons everywhere."

## PM decision

A "Look" token in the corner fan (living gardens only) hides home's controls, words and purse. Only the garden stays, with its light, seasons and neighbours. A tap anywhere brings everything back. A small sprig in the corner shows where to tap, then fades (2.5 s, or 4 s with reduced motion). It has no timer and no reward; it's simply a place to look.

## Dev

- `EvergroveApp.EnterQuiet` hides every active child of the title screen except the village window, and the chrome. An invisible full-screen button runs `ExitQuiet`, which restores exactly what was hidden. If home is rebuilt meanwhile, the layer goes with it and nothing is left hidden.
- The fan token uses the painted sprig, not a word.
- Tour: `QaQuiet`, shot 17j4.

## Result

A watch-only view exists. Unverified: whether players find the token, and whether a tap anywhere to return is clear without the cue.
