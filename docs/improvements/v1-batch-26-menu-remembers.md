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
