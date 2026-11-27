# I231 — Bring a neighbour some fruit

## Player
- Fen talks to me about my flowers every day. I'd like to give something back.

## PM decision
- When you have fruit, a neighbour's card shows a basket token beside the
  check. Tap it: one fruit goes to them, the chime plays, and they thank you at
  once:
  - Fen: "I made jam with your fruit. There's a jar with your name on it."
  - Mira: "...I'll dry some for the winter."
  - Orin: "...the best part of the day."
- The thanks comes first right after giving, whatever else is news. Later it's
  remembered truthfully at a lower priority (I228 rules: after family gifts,
  help, arrivals and Mira's river lines).
- No meter, no hearts, no "friendship level". The vision bans relationship
  meters; the count only lets them remember truthfully.

## Dev
- `NeighbourNotice.ThanksFor(villager, world)`, test-first in `FruitTests`.
- `EvergroveApp.GiveFruit` uses `OpenConversation(villager, afterFruit: true)`;
  `fruitThanks` puts the thank-you first. The painted basket keeps its colours
  (not tinted like a glyph).
- Tour: `17za-give-fruit`, `17zb-fen-thanks`.

## Result
- b272:
  - `17za`: Fen speaks of the clover waiting in your basket, with the basket
    token beside the check.
  - `17zb`: after giving, she thanks you with the jam line, also at 150% text.
- Core tests: 71,970 assertions pass.
