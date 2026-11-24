# I47 — Every remembered line appears with its keeper

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**Player problem.** Under the village strip, the line "Orin remembers the bridge you restored." was small grey text, while the same memory on the hub (I30) had Orin's portrait. One memory, two looks.

**Change.** A shared `KeeperLine`: the keeper's 34 pt portrait plus the remembered sentence at 14 pt in ink. It is used under the page's village strip and on the hub, so a memory always appears with the person who keeps it.

**Evidence.** b34 QA tour, 19 screens (the stream 2 page shows the keeper line under the strip).
