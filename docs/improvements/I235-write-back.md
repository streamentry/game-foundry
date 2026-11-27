# I235 — Write back

## Player
- After reading Nan's letter I want to answer. Today I'd close it, open the menu,
  find Family, and start from nothing.

## PM decision
- A painted pen token sits on the letter. One tap opens Family's send view with
  "Dear Nan," already begun and "just a letter" picked. You can still add a
  gift.

## Dev
- `EvergroveApp.WriteBack(to)`; `FamilySheet.Prefill(words)`.

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
