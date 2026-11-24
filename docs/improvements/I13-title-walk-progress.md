# I13 — Title screen shows how far the walk has come

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**Player problem.** The title screen was the same generic painting for a first-time player and for someone twelve streams in. Returning should feel like resuming a walk.

**Change.** The title screen shows the same twelve stepping stones as the stream screen (passed, current ring, ahead) under the tagline, just above the play button. The stones are drawn by the same `StreamStones` code, without the stream name. They are hidden after the ending.

**Vision fit.** Progress that never resets, with no dates and no streak (§2.6).

**Evidence.** b21 normal launch (`I13-I15-title-memory-menu.png`, left): the ring on stone 1 under the tagline, with the play button below.
