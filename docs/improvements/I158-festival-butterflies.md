# I158 — The festival ending has its butterflies too

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player

The ending is the reward for all twelve streams: the whole village at a lantern-lit table by the restored creek. Like the home screen before I157, it's a **still picture**, so the season's big moment feels like a closing slide.

## PM decision

The festival painting gets the same visitors as the home screen (I157): a painted butterfly drifting across now and then, with gentle motion only and no game effect. It's one shared behaviour, so the two "living" views of the village feel like the same place.

## Dev

`EvergroveApp`: `Painting(...)` now returns its node. The ending adds `HubButterflies` to the festival painting's masked node when gentle motion is on (not in the QA tour).

## Result (b167)

- Builds and tours pass (20 portrait and 5 landscape screens, the ending included, motion skipped). Verify passes.
- **Not visually verified at the ending:** the QA tour disables motion, and reaching the ending by hand means playing all 12 streams. The behaviour is the same component recorded working on the home screen in I157 (`I157-hub-butterfly.jpg`); only the host node differs (a masked painting of the same kind).
