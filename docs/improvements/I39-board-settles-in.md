# I39 — New boards settle in tile by tile

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**Player problem.** A new stream's board appeared all at once, with no sense of arriving somewhere new.

**Change.** On the first view of each stream, its tiles **fade onto the garden bed one by one**: a 0.3 s ease-out per tile, staggered 35 ms in reading order. A 3×3 board settles in about 0.6 s, a 4×4 in about 0.8 s. It plays once per stream (`settledRound`), never on later rebuilds of the same board. It is skipped with gentle motion off, and in the scripted QA tour so screenshots show settled boards.

**Evidence.** b30 tour, 19 screens. **Not verified:** frame by frame on a live stream change.
