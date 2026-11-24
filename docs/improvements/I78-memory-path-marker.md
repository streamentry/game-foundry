# I78 — Memory screen shows where the arrow leads

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

## Problem

Navigation was the one Haiku axis stuck at 5 (I73 and I77 reviews). The I77 reviewer said "nothing marks the path from one screen to the next". On the memory screen, the arrow gave no hint where it led. The hub already showed the stepping stones and the next stream with your landmark (I73), but the memory screen, which comes just before it, did not.

## Change (b52)

- Above the memory screen's actions, the screen now shows the twelve stepping stones: stone 1 filled, stone 2 ringed as current. Under them is the next stream line from the hub: the painted landmark you just chose, then the stream's name ("🌼 Rainwater").
- So the arrow now visibly leads somewhere, and the screen shows that what you planted travels into that stream. The same marker appears on the hub, so the two screens agree.
- `NextStreamLine` gained an `abovePlay` flag. The hub moves the row above its Play button; the memory screen adds it in place.
- The first b51 build pushed the actions half off a 402×874 screen. The memory screen's village banner now draws at 0.9× instead of 1.15× height, so the memory card, path marker and both action buttons fit on the first screen. Every other screen keeps 1×.
- No new text keys. The stream name stays English-only, as on the hub; other languages show the stones only.

## Evidence

- `I78-memory-path-marker.png` shows b46 on the left (bare arrow) and b52 on the right (stones, landmark and next stream, then the arrow). Both come from real play: six rotations, choose wildflowers.
- The verifier passes, and the b52 QA tour captured all 19 screens.

## Still unverified

- Navigation has not been re-scored yet.
- No players aged 40+ have tried this (m3-session-kit P1–P9).
