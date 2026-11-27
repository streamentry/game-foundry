# I266 — A neighbour noticed the two of you

## Player
- A word from the village after we played together would make it a little
  occasion.

## PM decision
- The keeper of that puzzle's kind (I257) has news straight away (their speech
  bubble shows) and says it once, true to the kind. For example:
  - Fen: "The two of you matched the flower beds together. My bees approve."
  - Orin: "You cleared the garden tiles together, one pair each. That's how we
    played them."
- It never guesses who played: no "grandma", no "child", no names. Talking to
  anyone else lets the line pass.

## Dev
- `NeighbourNotice.TogetherLine(kind)`. Tested in `TendedKindTests`, written
  with the code:
  - every kind is named;
  - the line says "together" or "two of you";
  - none of the banned pressure or guessing words appear.
- `EvergroveApp`: `togetherKind` and `togetherKeeper` put the line first in
  `NeighborSays`; it clears in `OpenConversation` for anyone else.
- Tour: `17j3-fen-together`.

## Result (b289)
- Tours complete in portrait, landscape and 150% text (82 shots each).
  - `17i2`: the take-turns token in sage.
  - `17j2`: the forget-me-nots wear the two-colour mark.
  - `17j3`: Fen says "The two of you matched the flower beds together. My
    bees approve."
- Core tests: 344,155 assertions pass.
- Not yet tried with two real people on one device. That's the next family
  playtest.
