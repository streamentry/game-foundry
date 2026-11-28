# v1 batch 29: item 50 finished (ticked, measured in portrait and landscape tours)

Date: 2026-11-28. Branch `feat/mosselora-v1-batch-29`, stacked on #160. Batch 26 left item 50 partial: far stretches were switched off but still built. A Haiku agent made the map build only nearby stretches. The coordinator presets the window before the first layout and verified the result in tours.

## What changed

- **The map view builds only the window.** `GardenMapView` (now `partial`; the new code is in `GardenMapView.Window.cs`) builds things, visitors, night creatures, lamps, snow caps, signs, mist, wild flowers, pair sprigs and spots only for the stretches on screen, plus one either side. When the view scrolls into a new range, `StretchWindow` calls `SetWindow` and the map lays out only that range again, keeping its scroll position and any fling.
- **Always built:**
  - The newest thing and the thing in hand, which the view may focus on.
  - The three neighbours and the sleeping cat.
  - The stretch paintings: one shared texture, switched on and off as before.
- **The first build is windowed too.** Attaching the window presets it to the top stretches before the first layout. If the view opens further down, the window corrects itself in the same frame, before anything is drawn.
- **Counters still see the whole river,** so the same mist wisps and river marks show: the water count, the 70-mark cap and the seeded dew glints.
- `GardenMapView.cs` did not grow (2,362 lines): every hook sits on an existing line.

## The test: memory flat after 30 stretches of scrolling

The tour lengthens the hub's river to 34 stretches and scrolls it 30 stretches as a swipe does, first with the window off and then on. Each step logs memory and the number of things built and live on the land (`MOSSELORA_WINDOW`).

| | Built (off) | Built (on) | Live (on) | Unity memory over 30 stretches (on) |
| --- | --- | --- | --- | --- |
| Portrait | 440–441 | 256 at the top, then 119–123 | 37–41 | 98,459 → 98,256 KB (flat) |
| Landscape | 436–437 | 231 at the top, then 107–108 | 29–30 | 98,051 → 97,671 KB (flat) |

- The count at the top is higher because the garden's things cluster in the first stretches.
- Further down, the built count stays the same however far the view scrolls. Adding stretches no longer adds built objects.

**Regression check:** every shot of both tours (183 each) was compared with batch 28's.
- The cottage card gained a stretch-name title from PR #150, merged from main in between.
- Everything else differing is known tour noise: the journal page lookup, and solve and walk timing.
- The touch scan still finds 0 targets under 48 pt in both orientations.

## Not shown by this test

- `layoutMs`, the whole hub rebuild, is about the same with and without the window (286 vs 302 ms portrait, 181 vs 161 ms landscape), because the rest of the hub dominates it. A clean timing of the map's own layout on a 100+ stretch river was not taken.
- The memory figures come from the simulator, not a real device.

## Checks

- Core tests: 5,213,841 assertions pass (`LazyStretchTests`; `StretchWindowTests` and `DayRhythmTests` updated to the new calls).
- Unity compile check is clean with and without `DEVELOPMENT_BUILD`; project check passes.
- Portrait and landscape tours: 183 and 184 shots, `MOSSELORA_QA_TOUR_DONE`.
