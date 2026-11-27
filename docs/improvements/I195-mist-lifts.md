# I195 — When a stream opens new land, the mist lifts while you watch

Date: 2026-11-26. Branch: feat/mosselora-world-first.

## Player (b222)

Every third stream a new reach of river opens (I192), but nothing tells me so. The mist below the garden is simply gone the next time I look. Gaining new land should be the best moment of the week.

## PM decision

- On the memory card of the stream that opens a reach (the 3rd, 6th and 9th), the map **frames the place where the old river meets the new reach**, and the new reach **starts under the same dry morning mist that waited below**, which **lifts over about two seconds** (gentle motion on), with the gentle chime if sound is on.
- No words, no banner, no reward pop-up: the land itself is the reward.

## Dev

`GardenMapView.LiftMist(reach)`: a mist overlay (straw wash plus fade) over that reach's rows, a `Lift` behaviour that fades and gently rises (0.6 s wait, 2.2 s lift, then gone), and `liftFocus`, so `Layout` frames the reach boundary instead of the newest thing. `EvergroveApp.PageGardenMap` calls it when the celebrated choice count is a multiple of `StreamsPerReach` and the garden isn't finished, then `PlayChime()`. `DevQaTour`: shot `05b-new-reach.png` 1.4 s into the third memory card.

## Result (b223 → b224; portrait, 150% and landscape tours, 27 each, all complete)

`I195-mist-lifts.jpg`: left, the third memory card framing the meeting of the first and second reaches, the second still partly misted mid-lift; right, the first memory card (no new reach), unchanged. b223 showed the lift only at the very bottom edge of the window, so b224 frames the boundary. Project verify passes.
