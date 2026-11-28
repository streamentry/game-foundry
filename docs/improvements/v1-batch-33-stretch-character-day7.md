# v1 batch 33: items 42 and 98 (ticked); 75 still partial

Date: 2026-11-28. Branch `feat/mosselora-v1-batch-33`, stacked on #165. The founder made three calls that day (`docs/research/decision-log.md`). A Haiku agent built item 42; the coordinator fixed what the first tour found and toured again on the 6.7″ simulator.

## 98 Set the Day-7 threshold before launch (ticked)

The bar is in the decision log, dated 2026-11-28, before any launch data exists. Day-7 retention of the first cohort, with at least 200 new players:
- **15% or more:** continue.
- **8% up to 15%:** iterate the first week once more.
- **Under 8%:** stop or pivot.

The decision log also records the definition, the minimum cohort, the per-store and per-age-band reading, and that no external benchmark is claimed. It is not yet measurable: the game has no install attribution or retention instrumentation.

## 42 Each stretch has a character (ticked)

- **Kinds follow the names.** Each of the 20 river names (`RiverNames`) has a kind that matches it: a mill, ford or falls is a **mill**; a pool or run of herons, kingfishers, swans or otters is a **heron pool**; banks, beds, bends and meadows are a **water meadow**. Later laps reuse the names, so the kinds follow them (`Core/StretchCharacter`).
- **Its own palette:** a soft tint over each stretch's painting (alpha 0.14–0.16). Green-gold for the meadow, wheat ochre for the mill, blue-grey for the heron pool.
- **Its own neighbour:** one of the existing three, per the founder's call. Mira keeps the meadow, Fen the mill she is mending, and Orin the heron pool where his boat is. When home shows a stretch beyond the first, that stretch's neighbour stands in it, on free land nearest the middle of the view. Golden-hour and evening spots still come first.
- **Its own puzzle flavour:** the puzzles that neighbour keeps (`NeighbourNotice.KeptBy`), shown as small puzzle marks under the stretch's name sign. The three flavours don't overlap.
- `GardenMapView.cs` did not grow (2,362 lines). The new code is in `GardenMapView.Stretch.cs`.

**The test, three stretches recognisable from a single screenshot:**
- `32-three-stretches.png`, the whole-river lookout, shows Willow Bend, Heron Pool, Mill Stream and Kingfisher Run, each with its name sign and its own puzzle marks.
- `32-stretch-2.png` shows Orin standing in Heron Pool; `32-stretch-3.png` shows Fen in Mill Stream.
- Both tours (portrait and landscape, 6.7″ simulator) log the kinds and neighbours: `MOSSELORA_STRETCH 2 HeronPool neighbour Orin`, `3 Mill neighbour Fen`.
- In the whole-river view the stretches read apart by their names and marks; the palette tint is subtle at that size.

**Fixed in review:**
1. The agent cycled kinds by index, so "Willow Bend" became the mill and "Mill Stream" the meadow. Kinds now follow the names.
2. The stretch's neighbour stood at the stretch's middle, which boat travel can leave just off-screen (Orin was missing from the heron-pool shot). They now stand nearest the middle of the view.
3. A fragile one-line `if … else` in `GardenMapView.cs` became a braced block.

**Also:** a landscape tour on the shared "QA iPhone" simulator was cut short when the app's data folder vanished mid-run, most likely another session reinstalling there. Both final tours ran on this session's own 6.7″ simulator.

## 75 Recipes (still partial)

The founder chose "gift or display only". But the game has one harvest (fruit), and the only food art is the apple, pear, jam and a basket. "Two harvests combine into a new food" needs:
- fruit split into kinds, which is a save change that also touches the market's fruit prices;
- art for each new dish.

That is more than this decision covers, so 75 stays unticked.

## Checks

- Core tests: 5,247,248 assertions pass (`StretchCharacterTests`).
- Unity compile check is clean with and without `DEVELOPMENT_BUILD`.
- Portrait and landscape tours: 195 and 196 shots. Reach audit 0 out; touch scan 0 under 48 pt.
