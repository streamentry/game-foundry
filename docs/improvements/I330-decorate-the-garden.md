# I330 — Decorate the garden

Ideas 14 and 16 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after Swedish block-building sandboxes (free creation at no cost) and American games where you visit what others built.

## Player

"I'd love a bench by the river. Just because."

## PM decision

From the cottage card, choose a decoration, then tap where it goes:
- stepping stones;
- a creekside bench;
- a picket fence;
- lantern posts.

All use the catalogue's own paintings. They're free, with no seeds and no limit but a generous 60. Tap one to pick it up and move it; tap it again to put it away. Decorations never stand on a thing, and things never land on them. When family visit your garden, they see your decorations where you set them.

## Dev

- Core (subagent):
  - `Decoration`, `PlaceDecor`, `MoveDecor`, `RemoveDecor`, `Decor` and `Occupied`.
  - Plant, Buy, MovePlanting, MoveBought, PlantGift and PlaceAt now refuse decorated cells (test-first).
  - The save gains `decor`, which travels in the garden file, with a visit round-trip test.
- Unity:
  - Home's `Occupied()` is `world.Occupied`; decorations are map things (index 4000 plus n), and a held one is ringed.
  - The visit map shows a relative's decorations.
  - Tour 17j7 sets down a bench and stepping stones.

## Result

The garden is yours to arrange. A new decoration has no on-map preview while you choose its spot; watch whether players understand to tap the land.
