# I123 — The path starts at the spring, so the first stream already shows how far the water reaches

Date: 2026-11-25. Branch: feat/mosselora-painted-ui (PR #76).

## Problem

In the I120 cold-read, the painted stone path's weak spot was **the very first puzzle**. No stone has been walked yet, so there is no water to contrast with, and the mossy stones ahead "could be read as already completed".

## Change

- **The path starts at the spring**: the painted `glyph-waves` spring (already used in Mira's note) sits before the first stone, and the water stream runs from it to half way under your stone. From the first puzzle on, the stream reads "the water reaches here, and not beyond".
- The water code is extracted into one `PathWater(...)` helper, used by the spring, walked stones and your stone; it works in both RTL and LTR.

## Verification (b123)

- Blind Haiku cold-read, b122 vs b123 on the first puzzle: **version 2 (spring) is clearer** that nothing to the right is done: "the teal water line starts at the left and stops under the circled stone… in version 1, the mossy-green stones to the right look finished". Later in the season: done and here high, ahead medium.
- **iPhone SE (375 pt)** portrait tour: DONE (20 screens); the 13-item path (spring plus 12 stones) fits within the column margins (`I123-spring-se.png`, local only).
- iPhone 16 Pro portrait tour DONE (20). Unity build: 0 `error CS`. C# tests: 50,072 assertions pass. The verifier reports OK.

## Remaining

- The water line is a 6 pt stroke; the cold-read still calls it "easy to miss at phone size". A thicker stream is the next candidate, weighed against crowding the label below.
- Android, real devices, players aged 40+ (**age fit not validated**).
