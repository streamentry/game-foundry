# v1 batches 30–32: items 41, 94, 13 and 5 (verified in simulator tours, including a 6.7″ phone)

Date: 2026-11-28. Branch `feat/mosselora-v1-batch-32`, which holds batches 30 and 31 too, stacked on #162. Every item follows the founder's design calls of 2026-11-28 (`docs/research/decision-log.md`, two entries).
- Haiku agents built each part, two at a time in parallel.
- The coordinator reviewed each part, fixed what the tours found, and ran 15 tours: portrait, landscape, 150% text in both, and a new 6.7″ simulator (iPhone 16 Plus) in landscape.

## 41 Scroll beyond the mist (batch 30)

- **The rule:** once there is nothing new left to solve (every story thing and every bought plant already solved), each replay also counts toward the river, at one new stretch per 8 replays (`GardenMap.ReplaysPerReach`). The river had stopped at about 58 stretches.
- **The save:** one new field, `riverReplays`. It never decreases, is clamped on restore, reads 0 in old saves and puzzle mode, and adds no seeds.
- **Mist drops:** they count replays once nothing new is left.
- **No stretch count is stored:** the river is derived from solves and replays, bounded only by the 100,000 safety bound, which a reflection test checks.
- `RiverReplayTests` covers the rule and the save. A simulated garden goes from 58 stretches to 63 after 40 replays, with an identical seed balance against a control garden.

## 94 One-hand landscape and portrait (batches 30 and 32)

- **The menu moved to a bottom corner,** and its fan opens upward. The fan wraps into another column rather than climbing above the one-thumb band (`ThumbReach.TopOfReach`).
- **Swipe down to close:** any sheet or card closes with a downward swipe anywhere on it, at least 90 pt and mostly vertical (`SwipeClose`). The top-right close button stays the one visual back position. A test fails any future close site without the swipe.
- **Upright, the card's controls move to its foot:** hint, take turns, clashes and send now sit at the card's foot. The thing's picture and the seed pill stay at the top.
- **The test, the reach audit:** the tours audit every non-garden control on home, in the fan and on a puzzle card against the `ThumbReach` model, a simplification of published thumb-zone studies (a heuristic, not a measurement).

| Tour | Out of one-thumb reach |
| --- | --- |
| Batch 30, portrait / 150% | 12 (card top row, the fan's Settings) |
| Batch 30, landscape / 150% | 3 |
| **Batch 32, portrait, landscape and both at 150%** | **0** (the card's close counts as reachable by swipe) |

The touch scan stayed at 0 targets under 48 pt throughout.

## 13 Context buttons where the thumb is (batch 31)

- **Play and Move beside the thing:** picking up a thing shows two 52 pt tokens beside it, and the bottom play token is gone.
  - Play opens its puzzle.
  - Move says "Tap where it should go." and keeps the thing in hand.
- **The tokens are not neighbours:** they are placed after the neighbours (drawn on top) and are not returned as neighbours. The map treats what it is given as people, with snow footprints and stepping off signs. Each layout replaces the previous row.
- **The test, taps per action** (`ThingTaps`):
  - Before: play 2 and move 2.
  - After: play 2 and move 2 (3 if the optional Move token is used).
  - The change is about reach, not tap count: the Play token now sits 74 pt from the held thing in portrait and 104 pt in landscape (`MOSSELORA_THING play-distance`). The old token sat at the bottom of the screen.

## 5 Remember the hand (batch 31)

- **Asked once,** on the first long-press, but never during a puzzle, the opening or an open sheet: "Left hand", "Right hand" or "Not now". The answer is kept on the device only and can be changed in settings.
- **What mirrors:** a right hand moves the menu to the bottom-right, the fan hangs on that side, the bottom reserves swap, and sideways the card's control panel follows the thumb.
- **The test, a thumb-reach heatmap in landscape on a 6.7″ phone:** a new iPhone 16 Plus simulator, with `tools/evergrove_hand_heatmap.py` drawing it from the tour log.
  - Batch 31: left hand 9/9 reachable, right hand 8/9.
  - Batch 32, after fixes: 9/9 for both hands.

## Bugs the tours found (all fixed)

1. **Leftover puzzle cards.** The batch 28 zoom tour step left three unsolved puzzle cards open behind home, which polluted later audits. Each card is now closed.
2. **The right-handed fan hung from the wrong button.** It anchored to the page's hidden left-corner button and ran off the screen (Settings at x = −39). It now hangs from home's own corner button.
3. **The right-corner menu covered Play** in landscape. The play row now keeps clear of it. The first padding moved Play only half the reserve, because the row centres its contents; it now moves a whole reserve, with Play's centre at x 683 pt and the menu's at 790 pt.
4. **Thing tokens under neighbours.** Fen's portrait drew over the Move token and would have taken its taps. The tokens are now placed after the neighbours.
5. Item 41's agent left one stretch-progress call counting solves only (the season plot in the mist). It now counts replays too.

## Checks

- Core tests: 5,241,126 assertions pass (`RiverReplayTests`, `ThumbReachTests`, `ThingActionsTests`, `HandednessTests`, `SwipeCloseTests`, `CardFootTests`).
- Unity compile check is clean with and without `DEVELOPMENT_BUILD`; project and docs checks pass.
- Home at rest still shows 2 controls; the fan fits on screen in every tour.

## Open

- The thumb-reach model is a heuristic. Whether 40+ players reach these controls comfortably with one hand is untested.
- How the swipe-to-close feels (follow distance, threshold) has not been tried on a device.
- A thing token can cover a neighbour's name label while the thing is held.
