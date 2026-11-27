# I303 — Four round one board

Idea 39 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after the four sides of a mahjong table and a ludo board in India.

## Player

"There are four of us at the table. 'Take turns' only allows two."

## PM decision

The take-turns token cycles: alone, two, three, four, then alone again. Each player has a soft colour (sage, honey, sky, rose). Turns go round in order, one move each. Under the token, one pip per player shows whose turn it is by position, filled for the current player, so turns read without colour. With two players the faces still swap sides (I268). Nothing about who plays is stored; the garden only remembers that a puzzle was played together (I263).

## Dev

- `TakeTurns` (new, Core) has `NextPlayers`, `Next` and `OfferHint`. The test was run red first (12 errors without the class). It checks the cycle, that turns go round in order, that everyone gets a turn, and recovery from out-of-range counts.
- `TreePlay` swaps its bool for a player count. `PlayedTogether` is true with two or more players, so the save and the two-colour map mark are unchanged.
- Tour shot 17i3: four players, the second player's turn.

## Result

A whole table can play one board. Not known: whether four people around one phone works on a small screen, which is for the family co-play sessions.
