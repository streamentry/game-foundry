# I313 — A wish for each neighbour

Idea 31 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after caring games where each companion has a wish you can grant before they move on.

## Player

"The neighbours talk about my garden, but they never want anything. They feel like signposts."

## PM decision

Each neighbour hopes for something the garden can give:
- Mira hopes to see five trees tended.
- Orin hopes to see the river run two stretches past the story.
- Fen hopes to see six kinds of flower growing.

The cottage card shows the hope nearest to coming true, with its true count ("4 of 6"). It's one line, not three, so the card stays calm. When a hope comes true, that neighbour writes a thank-you letter, once. There's no timer, no reward and no reminder: it's a hope, not a quest.

## Dev

- `NeighbourWish.For(world, villager)` (Core, new) counts only from the save:
  - trees solved, story and nursery;
  - river reaches past `GardenMap.MaxReaches`;
  - distinct flower items, story and nursery.
- The test was written before the class. It checks that every wish is open in a new garden, that the letters pass the letterbox rules, and the counts against a 12-chapter garden.
- `NoteGrantedWishes` runs when home is built; a per-garden device flag means each thank-you is sent once.

## Result

The companions want something, and the player can give it. Not known: whether the targets (5, 2 and 6) feel near or far to a real player. Tune them in the playtest.
