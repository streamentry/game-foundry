# v1 batch 22: items 45, 49 and 54 (verified in simulator tours)

Date: 2026-11-28. Branch `feat/mosselora-v1-batch-22`, stacked on #153. Haiku agents built the first pass. The coordinator ran the tours, which found three bugs; it fixed them, rebuilt and toured again in portrait and landscape.

**45 A river map.** The menu's "River map" opens a painted scroll of the river.
- Each open stretch is a line with its name. The stretch the view is on says "you are here" in words and wears a check.
- The mist below the last open stretch is named, and tapping a stretch travels there by boat.
- Family names (from letters, help and lanterns this device has) wait at the river's mouth.
- **Deviation from the item text:** family gardens are *listed*, not *pinned*. Nothing records where a relative's garden lies, and inventing a place would be dishonest.
- The back gesture and the close button both close it, and the sheet rises from the bottom like the others.

**49 Night creatures per stretch.** At dusk and night every open stretch has a creature: moths on even stretches, frogs on odd ones. Both are drawn in soft shapes like the other visitors, because no moth or frog art exists.
- Each sits mid-stretch, away from the screen's edges. A frog sits on a cell beside the water, a moth on a cell away from it (`NightCreatures.Spots`, pure).
- The first evening each is out, it is logged in "who visits" (I327) on the device, never in the garden file.

**54 A morning and an evening.** By local time of day:
- In the morning a dew-drop token by the pump offers a small board ("Dew on the grass this morning. A small board, if you fancy it.").
- At dusk a lantern string holds a neighbour's lantern letter, at most one a day. Who writes and the words are chosen by the date, with no randomness.
- Both tokens are 52 × 52 pt. Nothing here is saved to the garden.
- Rows were added to `return-reasons-v1.md`; neither token is a timer, streak or penalty.

**Tests:**
- **Night creatures and day rhythm:** `DayRhythmTests` checks:
  - creatures only in the evening, on every stretch for reaches 0–200;
  - spots on free land inside their own stretch, outside the outer two columns, with frogs beside water, on the real garden mask;
  - taken cells skipped, and no creature where no land is free;
  - the night cells kept on the map view across relayouts;
  - dew only in the morning, the lantern only at dusk, at most one letter a day, deterministic choice and copy that passes the wording rules.
- **River map:** `RiverMapSheetTests` checks the rows, the one-tap menu entry (`SetMenu(false); OpenRiverMap();`), the sheet's motion, back placement and close button, and the tour hook calling the same `OpenRiverMap`.
- The suite passes with 5,181,597 assertions, and the Unity compile check is clean with and without `DEVELOPMENT_BUILD`.

**Tours (iPhone simulator, portrait and landscape):**
- `17ze-river-map.png`: the map over the hub, six stretches, "Alder Reach, you are here", the mist line. In landscape the list scrolls inside the sheet.
- `20-morning-dew.png` (07:00) and `20-evening-lantern.png` (19:00): the dew drop and the lantern string by the pump.
- `20-night-reach-<n>.png` at 22:00, one per open stretch (five): a moth with its lamp halo on stretches 0, 2 and 4 and a frog by the water on 1 and 3, each mid-stretch in plain view. In landscape the frogs sit in view too. All five shots differ (the view travels to each stretch).

**Bugs the tours found (all fixed):**
1. The tour pressed "River map" when the menu wasn't open and landed on the story page. It now opens the map through a dev-only hook (`QaOpenRiverMap`) that calls the menu entry's own action.
2. The night tour travelled before the hub was built, so `TravelTo` returned early and every stretch's shot was the same view. The hub is now built first.
3. No moth or frog was visible on any stretch, for two reasons:
   - Spots were the first free cell from column 0, at the top-left edge under the menu button.
   - Worse, the cells were static and cleared on first use, while the map view lays its land out many times (resize, HUD clearances, travel's `KeepAt`). They are now kept on the view, as the day visitors' cells are.

**Open:**
- Moths and frogs are shape drawings, not painted art (art is checklist work 21–25).
- The lantern letter's words are English only.
- Real 40+ players have not seen any of this.
