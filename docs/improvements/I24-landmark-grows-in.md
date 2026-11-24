# I24 — The chosen landmark grows into the village

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**Player problem.** The core promise is "your choice changes the place". But on the memory screen the new tree, bridge or wildflowers were simply *there*, so the player never saw their choice arrive.

**Change.** On the memory screen right after a choice, the newest landmark of the chosen kind **grows up out of the ground**. It starts as a narrow sprout at 0.35 s, rises with an ease-out over 0.9 s, overshoots by 6% and settles. It is bottom-pivoted, so it grows from the bank. `PaintedVillage.GrowNewestKind` receives the kind from the world's last choice (0 tree, 1 crossing, 2 flowers), so the animation is always the truthful landmark. It plays once (`celebrateGrowth` is one-shot) and only with gentle motion on.

**Vision fit.** §3: the place visibly answers the choice.

**Evidence.** b25 live iOS Simulator: stream 1 was solved by real taps, then "Plant a shade tree" was chosen. A screenshot burst (`I24-tree-grows.png`) shows the page crossfade (frame 2, I19), the tree as a sapling (frame 3), then full size (frame 4 onward), with Mira's portrait on the memory card.
