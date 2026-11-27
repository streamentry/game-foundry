# v1 batch 26: item 15 (ticked, verified by a logged tour session)

Date: 2026-11-28. Branch `feat/mosselora-v1-batch-26`, stacked on #157. The coordinator built this item directly, as it is a small change.

## 15 The menu remembers

The corner button's fan now opens with the token used last first, right under the corner button the thumb just tapped. Every other token keeps its usual order, so only one token ever moves.
- **The rule:** `Core/FanOrder.Arrange`, pure. A blank or unknown last-used name leaves the usual order.
- **Remembering:** each fan token records itself when used, on the device only (PlayerPrefs `evergrove:fan-last`, never the save or the garden file).
- **Arranging:** the column is arranged before the tokens fade in, so the fade-in order follows too.

**The test, second-tap distance in a logged session:**
- The tour opens the fan, uses "Look" (the quiet garden) once, then opens the fan again.
- Each time it logs the distance from the corner button's centre to every token (`MOSSELORA_FAN`, canvas points).

| Tour | Before: "Look" | After: "Look" |
| --- | --- | --- |
| Portrait | 4th, 309 pt | 1st, 81 pt |
| Landscape | 4th, 309 pt | 1st, 81 pt |

- Before, "Settings" was first, because an earlier tour step used it last. After, it is back in its usual last place, and Memories and Almanac stayed where they were.
- Shots: `24-fan-before.png` and `24-fan-remembers.png`.

**Tests:** `FanOrderTests` checks:
- the used token comes first, and the others keep their order;
- an unknown or blank name changes nothing;
- the input is never changed;
- every token is first after use;
- the app wiring: remembered on use, arranged before the fade-in;
- the tour session logs and shots.

## Checks

- Core tests: 5,213,050 assertions pass.
- Unity compile check is clean with and without `DEVELOPMENT_BUILD`; project check passes.
- Portrait and landscape tours: 168 shots each, `MOSSELORA_QA_TOUR_DONE`.

## Open

- Moving the last-used token trades a shorter reach for a less fixed layout. Whether 40+ players prefer that, or find it disorienting, is untested.
- The tour's "logged session" is scripted, not a real player's session.

## 50 Performance budget for an endless world (partial, not ticked)

**Built:** a stretch window. On any river of four or more stretches, only the stretches on screen plus one either side stay live:
- Everything else on the land is set inactive and comes back as the view nears it: each stretch's painting, things, visitors, signs, neighbours and shadows.
- Layers that span the whole river stay live.
- The rule is pure (`Core/StretchWindowRule`). The component does work only when the range or the land's children change, never every frame.
- `GardenMapView.cs` did not grow: one call sits on an existing line.

**Measured:**
- The tour lengthens the hub's river to 34 stretches (a dev-only override, 0 in play).
- It scrolls 30 stretches one at a time, the way a swipe moves the view (not by re-laying out), first with the window off and then on.
- Each step logs the managed heap, Unity's allocated memory and the live count (`MOSSELORA_WINDOW`).

| Window | Live things on the land | Unity memory over 30 stretches | Managed heap |
| --- | --- | --- | --- |
| Off | 290 throughout | 100,594 → 101,033 KB (+0.4%) | +100 KB |
| On | 290 at the top, then 18–26 | 101,164 → 100,711 KB (flat, slightly down) | flat apart from GC swings |

**Regression check:** every shot of a portrait and a landscape tour (168 shared shots each) was compared with the tour before the window. The differences are known tour noise: the journal page picked by an unordered lookup, timing-dependent walk frames, and one neighbour caught at another point of his walk. No thing or creature went missing.

**Why it isn't ticked:**
- The item asks that far stretches not be built at all, with pooling. This window still builds every stretch's objects at layout and only switches far ones off, so the build cost still grows with the river.
- The test can't tell the difference: the stretch paintings share one texture, so memory while scrolling barely grows even without the window.
- Done means building only the nearby stretches in `GardenMapView.Layout` and reusing their objects, which needs room made in that size-capped file first. A test on a long river (100+ stretches) should also measure layout time, not just memory while scrolling.
