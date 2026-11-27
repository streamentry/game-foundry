# I237 — The letter flies into the box

## Player
- When I open a parcel with a letter, it's read in the family card. Nothing
  connects it to my letterbox.

## PM decision
- After opening a parcel that carries words, back home a sealed envelope arcs
  into the letterbox, and the box swells once. Then it sits there, sealed, until
  you read it.

## Dev
- `FlyFruit` is now `FlyInto(target, art, size)`, used for the pear (I229) and
  the letter. The family `Open` hook sets `letterArrived`.

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
