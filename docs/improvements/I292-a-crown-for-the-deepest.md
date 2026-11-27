# I292 — A crown for the deepest replay

Idea 7 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after shogi's promotion: a piece that travels far is crowned.

## Player

"I've replayed my quince a dozen times. The garden doesn't show it."

## PM decision

When a thing's replay has been solved at the deepest tier, it wears a second, wider ring of deeper gold around its solved ring. A first solve is never crowned, however deep the board, so the crown means coming back. A crown is never lost. Nothing is paid for it.

## Dev

- `ReplayBoards.Crowned(baseTier, replaysDone)` is true once the last solved replay was dealt at the deepest tier. Tests check that a first solve alone never counts, that a crown is never lost, that every thing can be crowned, and the exact replay counts (base 0 on its seventh replay, the deepest on its first). The tests were written alongside the method, not before it.
- `GardenMapView.Placed.Crowned` draws the outer ring. Story things and nursery plants both set it from the device's replay count (the same count that deepens boards, I259).
- Tour: `QaCrown` sets a flower's replays, shot 17j3.

## Result

Long-tended things are visible at a glance. Not known: whether 40+ players read the double ring as an honour, or as clutter.
