# I377: Keepers notice their visitors

Run 3, batch 8.

## PM decision

The keepers' hints (I370) lead naturally to this. Before you find a keeper's pairing, they hint at it. Afterwards, on one day in three, they mention the visitor it brought:

- **Mira** (trees): "There's an owl in your grove. I heard it last night, calling to the far trees."
- **Orin** (builds): "A heron's taken to fishing by what we built at the water. Patient bird. Good sign."
- **Fen** (flowers): "Something snuffles round your sheltered flowers at dusk. A hedgehog, I'd say. They know a safe spot."

This is the "AI companion in subtle ways" idea: they notice what changed in your garden because of you, and they don't say it every day.

## Dev

- `Visitors.KeeperVisitor(villager)` and `Visitors.KeeperRemark(world, villager, dayNumber)`. The remark appears when `(day + villager) % 3 == 0`, and is null until the visitor is welcomed.
- `NeighborSays` tries the remark after the pairing hint, so a keeper only ever gives one of the two.
- Tested: nothing before the visitor; exactly 10 days in 30.

Note: this batch also fixes the keeper names in the I370 doc. Villager index matches the family: Mira keeps trees, Orin builds, Fen flowers.
