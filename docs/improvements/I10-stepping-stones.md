# I10 — Stepping stones show the walk along the creek

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**Player problem.** Progress through the twelve streams was a line of text ("STREAM 1 / 12 · First Bend"). A garden walk should feel like a path you are walking.

**Change.** The stream pill is replaced by twelve **stepping stones**: passed streams are teal stones, the current stream is a larger leaf-green ring, and the streams ahead are faint. The stream's name ("First Bend") sits under the stones in English builds. "Stream n / 12" remains as spoken text for screen readers (transparent, outside layout). This is progress, not a streak: it never resets, has no dates, and waits however long the player is away.

**Vision fit.** §2.6 "Respect is permanent": progress without urgency.

**Evidence.** b20 live launch after the title screen (`I10-stepping-stones.png`): the ring sits on the first stone with 11 faint stones ahead.
