# I193 — The basket: plant more of anything you've grown, anywhere you like

Date: 2026-11-26. Branch: feat/mosselora-world-first.

## Player (b217, fresh garden after stream 1)

The river is long now, but after a stream I own exactly one tulip in a valley that runs on and on. I want to *make* something: a row of tulips along the bank, a little orchard by the cottage. In a sandbox game the thing you unlock is a kind of block you can use as often as you like. Here every unlock is a single object.

## PM decision: the gentle Minecraft loop

- **Solve a stream → unlock a kind → plant it as often as you like.** Puzzles stay the reason to come back; the river is where you build.
- On home, bottom left, a round **basket** token shows the newest thing you grew (with a soft pulse until you first use it).
- Tap it and a **tray** slides in along the bottom (play, stones and sign step aside): one round token per thing you've grown, newest first. The newest is ready in your hand. **Tap the grass, and one more grows there; tap again, another.** The view never jumps while you plant.
- **Tap any planting to pick it up** (soft ring and a tapping finger): tap the grass to move it, or tap the basket, now a green *put back* arrow, to return it.
- Water, the cottage, the path and taken cells refuse gently (nothing happens, the thing stays in hand). No inventory to manage, no costs, no limits a person will meet (200 per garden).
- Your stream choices stay special: the memory card still asks you to place the one you just made; plantings are copies.

## Dev

- `Core/WorldModel`: additive `plantings` (pairs of choice index and cell, absent in older saves), `Plant`, `MovePlanting`, `RemovePlanting`, `Plantings`, `PlantingCount`, `MaxPlantings` 200; validated on restore (even length, only grown things, real cells, living mode only); reconsidering a choice returns its plantings.
- `Core/GardenTransfer`: the garden file carries `plantings`; files from before I193 still import (a pre-plantings checksum form).
- `Core/GardenMap`: `Layout(choices, placed, plantings)` keeps planted cells for the plantings, so defaults step around them; `IsFree`.
- Tests (`PlantingTests`): planting, moving, putting back, bounds, save round trip, refusing bad lists, the garden file round trip and checksum, reconsidering, the limit, and defaults avoiding plantings. `RemoveMapKeys` also strips the new key for the pre-map form.
- `GardenMapView`: `Position` and `KeepAt` hold the view across rebuilds.
- `EvergroveApp`: plantings join `MapItems` (index 1000+k); `OnMapItem` and `OnMapLand` handle holding, moving and planting; `BasketControls` builds the token and the scrolling tray; `PutBack`; `RefreshMaps` keeps the hub's view. QA hook `QaPlantFromBasket` and shot `17c-basket.png`.

### Bug found by playing: taps on the map landed half a map too low (since I190)

I190 moved the painting into a top-pivoted child, but `TapLand` still assumed a centred pivot, so every tap on the home map (and the memory card's) asked for a cell half a map below the finger. Usually that cell was outside the map and nothing happened. `TapLand` now measures from the painting's own rect. The tour can't catch this (it calls `OnMapLand` directly), and I didn't hand-test placing in I190–I192. Lesson: any change to map geometry needs a real tap on the simulator.

## Result (b218 → b220; portrait, landscape and 150% tours, 25 each, all complete)

- `I193-hands-on.jpg` (b220, real taps): three tulips planted where tapped beside the first; one picked up (ring, finger, green put-back arrow); then moved across the river, and another put back.
- `I193-basket-tour.jpg`: the year's garden with its basket token, and the open tray with wisteria in hand and three more wisterias planted beside Fen.
- Mist below the river is softer (82%) so home's foot is less like a page footer.

.NET tests pass (52,783 assertions). Project verify passes. **Not yet:** the basket in landscape; the board-stage and memory maps show plantings but have no basket.
