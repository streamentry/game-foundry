# I236 — Orin saw the flag go up

## Player
- I might not notice the envelope on the letterbox. A neighbour mentioning it
  would be natural, the way a neighbour does.

## PM decision
- While a letter waits unread, Orin says: "There's a letter in your box from
  Nan. I saw the flag go up this morning." The line goes once you've read it.
- It is true and passing. Unread state is the app's own record (as for the
  sealed envelope), not part of the save, so this line lives beside it in the
  app. A fruit thank-you (I231) still comes first.

## Dev
- `EvergroveApp.NeighborSays` checks `LettersReadKey` against `world.Letters`.

## Result
- b276 tours complete in portrait, landscape and 150% text (57 shots each).
  - `17t`: the envelope flies into the letterbox, which stands under the purse;
    neighbours keep clear of it.
  - `17t2`: Orin's line.
  - `17u0`: the sealed envelope.
  - `17u`: the letter. "Love, Nan" isn't signed twice.
  - `17u2`: "Dear Nan," begun.
- Found in b275 and fixed in b276:
  - the letterbox covered Mira's portrait (the home's top clear band is now 220
    points when there are letters);
  - the signature repeated a sign-off;
  - the tour showed home instead of Orin's card.
- Not yet tried by players aged 40+, or on a real device.
