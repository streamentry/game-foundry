# I180 — Restored water greens the valley, stream by stream

Date: 2026-11-26. Branch: feat/mosselora-i127-plus.

## Player

After I178 and I179 the garden map was the same lush green from the first minute to the last. Restoring the creek, the whole point of every puzzle, changed nothing on the land. In an open world, the player's work should show *in the world*. The end-game plan calls this E5: "water shapes the land".

## PM decision

- **The valley starts dry.** Below the restored water, the land wears a soft straw wash, with a feathered edge where green meets dry.
- **Each restored stream greens the next stretch down the map**: the cottage's top rows from the start (14%), then a twelfth of the rest per stream, and all of it when the garden is finished.
- **You see it happen.** Right after a choice, the dry edge draws back down the map over about 1.4s (with gentle motion; instant otherwise), while your new thing grows in.
- It's a picture, not a meter: no numbers, no words.

## Dev

- `GardenMapView.Build(…, greened, greenedBefore)`: `BuildDryLand()` adds a straw-coloured `Dry land` image under all items, anchored from the bottom up to `1 − greened`, with a flipped `TopFade` as its soft edge. `Update()` eases the edge from `greenedBefore` to `greened` after a choice.
- `EvergroveApp.GreenedShare(back)`: `0.14 + 0.86 × restored ÷ 12` (1 when completed). The page map passes the before and after values when a choice has just been made; the hub shows the current value.

## Result (b190, iPhone 17 simulator; portrait, landscape and 150% tours, 23 each)

`I180-water-greens.jpg` (gitignored): after the first stream the top rows by the cottage and the new birch are green and the lower valley is straw; by stream 7 (winter board) more of the valley is green; the finished garden's home map is green throughout. Verify and the .NET tests pass.

**Not verified:** the edge's motion by hand (the tour skips motion), and whether players read dry versus green as "the water hasn't reached here yet" (a cold-read or M3 question).
