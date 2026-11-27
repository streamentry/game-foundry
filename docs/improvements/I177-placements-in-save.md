# I177 — Where things stand is saved and travels with the garden

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player

E4 lets a player put each thing they make exactly where they want it on the garden map. That only means something if **it stays where they put it**: across app restarts, a reset save slot, and the garden file they copy to another phone. And the garden's catalogue number (which twelve trees, flowers and builds it meets, I172) lived only in device settings, so a restored garden file on another phone would have shown different items.

## PM decision

Both now live in the **authoritative save and the garden file**, as optional additive fields, the same way the undo history (EG-111) was added:
- `garden`: which garden of the catalogue this is.
- `placements`: one map cell per choice; −1 = not placed yet (the map then uses its default spot).

Older saves and garden files keep working: they load as garden 0 with nothing placed, and older garden files still verify against their original checksum.

## Dev (Core only; no screen change in this commit)

- `WorldSnapshot`: `garden` and `placements` after `history` (JsonUtility field order).
- `WorldModel`: `Garden`, `Placements`, `New(mode, garden)` and `PlaceAt(index, cell)` (living mode, an existing choice, cell −1…9999). `Choose` adds an unplaced slot; `ReconsiderLatestChoice` trims it. `TryRestore` validates the garden (0…1,000,000), placements (no more than the choices, cells in range, none in puzzle modes), and treats a missing field as empty.
- `GardenTransfer`: the snapshot keys and canonical text add `garden` and `placements`. Import accepts the current checksum, the pre-map form (with history), and the pre-history form, the older two only when the map fields are at their defaults.
- Tests: `PlacementSaveTests` (14 checks: placing, ranges, choose and reconsider, save round trip, old-save migration, bad data refused, the garden file carries the map, a pre-map file imports, an edited placement fails its checksum). `UndoHistoryTests`' pre-history fixture now also strips the map keys, since real pre-history files predate the map.

## Result

.NET tests pass: **52,728 assertions**. Verify and the docs contract pass. The UI still reads the garden number from device settings. The next iteration switches it to `world.Garden` and adds tap-to-place.
