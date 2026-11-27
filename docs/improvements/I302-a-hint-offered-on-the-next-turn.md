# I302 — A hint offered on the next turn

Idea 48 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after gentle platformers that offer help to small hands without making it the rule.

## Player

"When I pass the phone to my grandson, he gets stuck and I end up doing it for him."

## PM decision

When taking turns, every turn after the first player's makes the hint token glow softly in honey, a quiet offer. The first player is usually the grown-up who started the game. Nothing is pressed for anyone. On the first player's turn, and when playing alone, the token is plain.

## Dev

- `TakeTurns.OfferHint(turn, players)` is true on turns 1 and up when more than one person plays. It's tested with I303.
- `TreePlay.PaintTurn` tints the hint token honey and, with gentle motion, adds a slow `Breathe`; it clears both on the first player's turn.
- Tour shot 17i3 is taken on the second player's turn.

## Result

Help is visible when it is most likely needed. Not checked: whether it helps a child, or reads as "you're expected to need help". The family co-play protocol should watch for that.
