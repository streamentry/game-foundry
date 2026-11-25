# I171 — A catalogue of 200 trees, flowers and things to build

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player (founder's new goal, 2026-11-25)

"Only 2 kinds of trees are very boring. We should have like 100 kinds of trees and flowers, and building only bridges is boring. We should have 100 different kinds of things to build and fix. Be super creative."

Today every choice in every garden is one of the same three things: a shade tree, a bridge, a wildflower bed. Twelve streams, three pictures. By the third stream the world has nothing new to show, and a 40+ player returning daily has no reason to wonder "what will it be today?".

## PM decision

- **Keep the three mechanics**: grow (Mira), build and mend (Orin), bloom (Fen). They decide the next board (the protected landmark, `ReactiveWatercourses`) and they're what the save stores, so there's no save migration and no puzzle change.
- **Every choice becomes a specific thing** from a catalogue: **50 trees, 50 flowers and 100 things to build or mend**, 200 in all. "Plant a quince tree", "Mend the mill wheel", "Grow foxgloves", "Hang the rope bridge", "Relight the forge", "Build the bug hotel"…
- **Each garden draws its own twelve per family**, with no repeats inside a garden. The next garden continues where the last stopped, so all 200 appear within five (trees and flowers) or nine (builds) gardens. That's a reason to start another garden.
- Deterministic and offline: no clock, no random draw. English only (no new locale keys; other languages keep the family names).

## Dev

- New `Core/GardenCatalog.cs`: `GardenItem` (family, index, art key `tree-NN` / `flower-NN` / `build-NN`, card title, memory phrase, name). `Get(family, index)`, `For(garden, chapter, family)` (fixed seeded shuffle per family, garden g reads positions 12g…12g+11, wrapping) and `Order(family)`.
- `Tests/Program.cs` `GardenCatalogTests`: counts (50/50/100), unique names and art keys, tidy card and memory phrases, permutation, no repeat inside a garden over 40 gardens, consecutive gardens disjoint until wrap, full coverage within ⌈n/12⌉ gardens, determinism, a/an, verbs, range checks.
- Art prompts for 8 Codex sheets (25 icons each, 5×5, in the existing icon style): `art/prompts/catalog-*.txt` (generation running).

## Result

.NET tests pass: **52,649 assertions** (up from 50,072). Verify passes. Nothing visible yet: I172 wires the catalogue into the choice cards, memories, book and village; I173 adds the art.
