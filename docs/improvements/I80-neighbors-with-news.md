# I80 — Hub marks neighbors with something new to say

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

## Problem

Daily appeal is the weakest AI-review score (6 in the I79 review). The reviewer's reason: "nothing on the hub says what is new today". The game rules out the usual answers: no streaks, no timers, no clock-driven rewards, no notifications. The vision says seasons arrive through progress, never the clock.

## Design

The honest "new" thing in Mosselora is a neighbor who has something to say that you haven't heard. Their lines change only when your village changes: a new landmark (I73) or a turning season (I72). So the hub now marks those neighbors.

- On the hub, a neighbor whose current line differs from the last line you heard from them wears a small parchment speech mark at the top-right of their portrait. The mark has a water-blue chat glyph and a soft lift, and swells once when gentle motion is on.
- Tapping the neighbor, which steps from the hub into their memory (as before), stores that line as heard (`evergrove:heard-<n>` in PlayerPrefs, local only). The mark goes away until the village changes again.
- The mark is in Living mode only, and on the hub only, never during play.
- A brand-new garden shows the mark on all three neighbors, which invites the player to meet them.

## Evidence (b55, live on the iOS Simulator)

`I80-neighbors-with-news.png`, left to right:

1. **Fresh hub:** all three neighbors carry the mark.
2. **Tap Mira:** "I'm Mira. Could we plant a tree beside the water?"
3. **App terminated and relaunched:** Mira's mark is gone, and Orin's and Fen's remain. Heard state survives a relaunch.

The verifier passes, and the b55 QA tour captured all 19 screens.

## Not done / unverified

- The mark has no spoken text. Screen readers still hear just the neighbor's name, because a localized "has something new to say" would need a new key in 29 catalogs. This is a follow-up.
- No players aged 40+ have tried this (m3-session-kit P1–P9). Whether it actually brings anyone back tomorrow is exactly what those sessions measure.
