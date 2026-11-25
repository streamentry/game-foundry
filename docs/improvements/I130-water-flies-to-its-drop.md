# I130 — Water flies from the tile to its drop

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player

The I129 cold read said it outright: "Nothing says what [the drops] count: moves left, water for the garden, or lives. I'd probably ask what they mean before I tapped anything." Playing it myself, I agree. Moves-left and lives are the two most common meanings of a row of icons under a puzzle, and both would make a calm game feel like a countdown.

## PM decision

- **No caption.** A label ("water reached 4 of 9") would be more web text, and the screen reader already speaks the exact sentence.
- **Show the link instead:** when a turn brings water to new tiles, a droplet lifts off each newly wet tile and arcs down into its slot in the meter, which then fills with a soft pulse. Cause and effect in half a second: a drop is a tile with water. It counts up, never down to zero, so it can't read as lives.
- Respect gentle motion: with motion off, the drops fill at once, as before.

## Dev

`EvergroveApp.cs`:
- `PaintDropsWithFlights(puzzle, insight)` replaces the direct `PaintDrops` call in `RefreshBoard`. It counts the tiles that were wet before the move, paints only those, and starts one `FlyDrop` per newly wet tile (in board order, 70ms apart). Drops not tied to a newly wet tile (the solve fills the rest) fill at once.
- `FlyDrop(tile, slot, order)`: a droplet image on the root canvas flies from the tile's centre to the slot's live position over 0.5s, on a small arc, shrinking from 1.15 to 0.8. On arrival it fills the slot if water still reaches that many tiles, with a `BloomPulse`. It stops if the board was rebuilt.
- Skipped when gentle motion is off and in the scripted QA tour, whose screenshots must be settled.

## Result (b133, iPhone 17 simulator, fresh install, screen recording)

- On the first board I turned the bottom-left elbow so it met the wet pipe above it. In the recording frames (every 0.1s), a droplet rises off the board, arcs down, and lands in the fifth slot. That slot fills and pulses, and the other drops stay as they were. Local frames: `docs/improvements/I130-drop-flight-frames.jpg` (gitignored).
- The scripted tour (motion skipped) still captures all 20 screens.
- **Found while recording:** on the first move, Mira's tip bubble disappears and the whole board jumps up about 150pt under the player's finger. I131 fixes that.

**Not verified:** whether players read the flight as "drops = tiles with water". It needs a cold read on video or a person. Also not checked: several drops flying at once on a big connection, and frame rate on an older phone.
