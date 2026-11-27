# I315 — Neighbours give back

Idea 32 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after village life games, where neighbours give back as you come to know them.

## Player

"I tend everyone's things, and they never give me anything back."

## PM decision

At four true milestones, 8, 20, 40 and 80 things solved, a neighbour leaves something of their own kind in your basket, with a note:
- Mira, a tree cutting;
- Orin, something he made;
- Fen, a flower cutting.

The plant comes from a nursery row you already have open, so it's never ahead of your garden. There are four in all, never endless, and earned by play like first-solve seeds, so the economy stays logical. If the basket is full, it waits for room.

## Dev

- `NeighbourGifts` (Core, new) holds `Milestones`, `Due(solved, given)`, `From(milestone)` and `CuttingFor(world, milestone)`. `WorldModel.ReceiveNeighbourCutting(kind)` puts it in the basket only in a living garden, only a real kind, and only with room.
- The test ran red first (20 errors). It checks:
  - nothing before the first milestone, and never twice;
  - only four in all;
  - the giver's own family, from an open row;
  - givers take turns;
  - nothing but the basket changes;
  - only real kinds, only in a living garden.
- `NoteNeighbourGifts` runs when home is built; a per-garden device count stops duplicates. Orin's note says he made it, not that it's a cutting.

## Result

Tending is returned in kind. The milestones are a guess to tune with the playtest's economy notes.
