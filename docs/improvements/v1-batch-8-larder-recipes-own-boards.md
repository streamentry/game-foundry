# v1 batch 8: item 76 (ticked); 75 and 37 (partial)

Date: 2026-11-27. Branch `feat/mosselora-v1-batch-8`, stacked on #134. Two Haiku agents did the work, and the coordinator reviewed it.

## 76 Storage has a limit you can grow (ticked)

The larder gets up to 3 shelves, bought at the stall with seeds (new ledger reason Larder = 12):

| Shelves | Fruit | Jam | This shelf costs |
| --- | --- | --- | --- |
| 0 | 99 | 10 | n/a |
| 1 | 119 | 15 | 150 |
| 2 | 139 | 20 | 300 |
| 3 | 159 | 25 | 600 |

- **Every cap check now uses `FruitCapacity` or `JamCapacity`:** picking fruit, parcels (LarderFull), MakeJam, restore checks, pollination fruit, seasonal yields, stall and trade room, and the cottage jam button. The last few were found and routed by the coordinator.
- **Autumn yield fixed:** it used to clamp silently, dropping fruit when the larder was nearly full. It now waits until all 5 fit, so a full larder never loses anything silently.

## 75 Recipes (partial, not ticked)

`Core/Recipes` makes cooking discoverable: a recipe is found the first time it's cooked, saved as a bitmask, and listed in the almanac. The only recipe is Jam (3 fruit → 1 jar). The item asks for two harvests combining into a new food, but the game has one harvest (fruit). A second harvest or a new food would need its own gift, stall price within the no-profit proof, and almanac copy. **Design decision for the founder.**

## 37 Boards drawn from your own garden (partial, not ticked)

`Core/OwnGardenBoards` picks nonogram pictures from what this garden grew and bought, using a 28-entry mapping from catalogue items to pictures. An exact solution counter proves every board is unique: all 31 library pictures have exactly one solution, over 1,683 boards checked, cross-checked against brute force. It isn't wired into the app yet:

- **Sharing:** a board shared with family is sent as (seed, tier) only, so the recipient would see a picture from their own garden. The parcel needs to carry the picture index first.
- **Mosaic:** its target is a Unity sprite, with no shape data in Core.

## Verification

- `.NET` suite, run as CI does: 1,765,923 assertions pass. The no-profit proof still holds.
- Unity 6.3 runtime compile: 0 errors with both the release and dev define sets.
- `verify_evergrove_project.py` and `check_evergrove_docs.py`: OK.
- **Not run:** editor tour and device.
