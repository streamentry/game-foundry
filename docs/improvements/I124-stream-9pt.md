# I124 — The water under the path is thick enough to see at a glance

Date: 2026-11-25. Branch: feat/mosselora-painted-ui (PR #76).

## Problem

The I123 cold-read's one remaining point: the 6 pt water stream under the stone path is "a thin teal stroke that is easy to miss at phone size". That stream is the only cue separating walked stones from stones ahead.

## Change

The stream is 9 pt (4 pt below the stone line, 5 pt up into it), up from 6 pt. It is still opaque and seamless, and still in the one `PathWater` helper.

## Verification (b124)

- Autumn-stream crop, b123 vs b124: the stream reads clearly as a band of water, and the stream name below keeps its gap (`I124-stream-9pt.png`, local only).
- iPhone 16 Pro portrait tour DONE (20). **iPhone SE portrait tour DONE (20)**: the path and stream fit, and nothing overlaps the stream name. Unity build: 0 `error CS`. The verifier reports OK.
- The small step in the stream next to your stone in the tour shot is the one-time "just arrived" swell on the last walked stone (`BloomPulse`), not layout.

## Not verified

- Android, real devices, players aged 40+ (**age fit not validated**).
