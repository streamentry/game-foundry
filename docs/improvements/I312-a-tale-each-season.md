# I312 — A tale each season

Idea 38 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after A Thousand and One Nights (Persia and Arabia): one tale at a time, by turns, never all at once.

## Player

"I like Mira, Orin and Fen, but I don't know anything about them."

## PM decision

Each season note (I311) is a short tale from the neighbour's own life: a grandmother's ribbon on the first tree to leaf, the first bridge Orin built washing away, seed twists that grew forty years on. The tellers take turns, Mira, then Orin, then Fen, so there are twelve tales, one per neighbour per season. Each is under 280 characters. They're memories, never claims about the player and never pressure.

## Dev

- `SeasonTales` (Core, new) holds the twelve tales and `For(season, turn)`. The tests ran red first (5 errors). They check that:
  - every note passes the letterbox rules (`ValidName`, `ValidLetter`);
  - none uses a pressure word or "!";
  - each names its season;
  - all three neighbours tell;
  - the tales are twelve distinct ones;
  - tellers rotate in threes.
- The turn count is per garden (`evergrove:season-turns:<garden>`), on the device.

## Result

The companions have pasts. The tales are original writing, in English only, and need native review before other languages ship. Unverified: whether players read them.
