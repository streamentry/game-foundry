# I144 — Water runs tile by tile from the spring

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player

The whole genre is about one moment: you turn the last piece of a channel and **watch the water run** through everything you set up. In Mosselora, a turn that connected three or four tiles at once made **all of them turn wet in the same frame**. It read as a state change ("these are now connected"), not as water arriving. That's the most "web" thing left on the board itself: a re-render, not a flow.

## PM decision

When a turn brings water to several tiles, they fill **in the order the water reaches them**: by steps along connected pipes from the spring, 0.07s a step, starting with the nearest newly wet tile. Each tile's swell and its drop flight (I130) start as that tile fills, so the meter fills in the same order.

- With gentle motion off it's instant, as before, and the QA tour stays settled.
- If the player turns another tile mid-cascade, each delayed tile fills from the board's state **at that moment**, so the cascade can never show water where none flows.
- The solve keeps its own existing bloom outward from the spring (it rebuilds the board).

## Dev

`EvergroveApp.cs`:
- `WaterArrival(puzzle, angles, insight)`: a breadth-first walk from the spring over connected, flowing pipes, giving each newly wet tile `(steps − nearest new step) × 0.07s`. It returns null when nothing should stagger.
- `RefreshBoard` paints every tile through one helper, `PaintRefreshedTile(…, held)`, so the refresh and the running water share a single visual rule. `verify_evergrove_project.py` requires exactly one in-place `TileFill` rule and water painted from `insight.Flowing`; my first version broke both and verify caught it before the commit. A newly wet tile with a delay > 0 is painted `held` (dry for now), its `BloomPulse` is delayed by the same amount, and `WetLater(tile, delay)` repaints it from the live `world` (abandoned on any rebuild). Open-port markers update at once, since they're geometry, not water.
- `PaintDropsWithFlights(…, arrive)` sorts newly wet tiles by arrival and starts each `FlyDrop` at that tile's arrival time (`FlyDrop` now takes a start delay in seconds).

## Result (b150, then b152 after the refactor, iPhone 17 simulator, hands-on, recorded every 0.05s)

Setup: on the first board I turned the middle-right and bottom-right elbows first, then turned the bottom-left elbow to meet the wet pipe above it. In `I144-water-runs.jpg` (gitignored), after the bottom-left tile swings, the bottom-left tile fills, then the bottom-middle tile about 0.07s later, then the bottom-right, then the middle-right. Droplets lift off each one in turn and the meter fills in the same order. The sprout tile (still facing away) stays dry.

- The scripted tour (settled, no cascade) captures all 20 screens. Verify passes.

**Not verified:** long cascades on 4×4 boards (up to ~1s of fill; watch for impatience), and frame rate on an older phone.
