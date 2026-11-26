# I215 — Send and open a family parcel

## Player
- I214 made parcels possible in code; nothing on screen let me send one.

## PM decision
- One **Family** token in the menu fan (the neighbours icon) replaces the
  separate postcard token; the postcard moves inside the Family sheet. The fan
  stays at five tokens.
- The sheet, over your garden:
  - top row: close, **Send**, **Open**, postcard, and your seeds;
  - Send: your name (remembered), pick one gift (10 / 50 / 100 seeds, just a
    letter, or a cutting of a nursery plant you own, at its price), a few words,
    then the round send token opens the phone's share sheet (or copies, if
    there is none). The shared message starts with a friendly line ("Grandma
    Lan sent you 50 seeds in Mosselora…") and carries the parcel as one token.
  - Open: "Copy the message they sent you, then tap here." One round token
    reads the clipboard, finds the token anywhere in the message, and shows who
    it's from, their letter, and the gift.
  - Every refusal is in plain words ("You've already opened this parcel.", "This
    parcel was changed on the way…", "This parcel came from your own garden.").
- A cutting waits in the basket with a small red ribbon; pick it, tap the grass,
  and it becomes your own nursery plant with its own puzzle.

## Dev
- `Unity/FamilySheet.cs` (new, self-contained; hooks into the app for world
  operations, share, clipboard and name). `EvergroveApp.OpenFamily`,
  `GiftTokens`, `giftPick` in `OnMapLand`; `TreePlay.Token/Pill` made internal
  for reuse. English only for now.
- Tour: opens Family, sends 50 seeds with a letter as "Grandma Lan", then opens
  a parcel from "Nan" (a clover cutting and a letter) pasted inside other text.

## Result
- `I215-family.jpg` (local only): send, sent (purse 50 → 0), Nan's parcel
  opened, 150%, sideways.
- Portrait, landscape and 150% tours: 44 shots each, complete (b260). Sideways, the card first ran off the screen (b254); it now shrinks to fit.
- Not yet by hand on the device with a real share sheet; not tested with
  families.
