# I139 — Big boards keep their water drops full size, in two rows

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player

Mid-game (autumn and winter, 4×4 boards): since I129 the sixteen water drops sat between the two round tokens, shrunk to about 70% of their size. On the phone they were roughly 11pt-tall specks, too small to count at a glance and too small to see a single drop fill when water arrives (I130's flight lands in a dot). For people over 40 who hold the phone at arm's length, that's the wrong place to save space.

## PM decision

Stack big boards' drops in **two rows at full size** (22pt) between the tokens: 8 + 8 for a 4×4 board. Two rows (48pt) fit inside the 76pt token height, so the line under the board stays one line. The shrink stays only as a last resort if even two rows can't fit.

## Dev

`EvergroveApp.FlankDrops`: when the meter is wider than the room between the tokens and holds more than 9 drops, it swaps the meter's `HorizontalLayoutGroup` for a `GridLayoutGroup` (cell 22pt × the droplet's aspect, 4pt spacing, `ceil(n/2)` columns) and sets the meter's preferred size to the grid's size. The old shrink path runs only if the horizontal group is still there. `waterDrops`, `PaintDrops` and the I130 flights are unchanged: they target the same images.

## Result (b144, iPhone 17 simulator, scripted tour)

- `16-winter-village`, b143 compared with b144: the 16 drops are now two rows of 8 at full size (two filled, top-left), centred between Undo and Hint. Local crop: `docs/improvements/I139-drops-two-rows.jpg` (gitignored).
- 3×3 boards are unchanged (9 drops in one row). The scripted tour captures all 20 screens. Verify passes.

**Not verified:** the drop flight landing in the second row (code path shared, not recorded), and 150% text, where the tokens grow and the room shrinks.
