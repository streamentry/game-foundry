# I214 — Family parcels: the rules

## Player
- The founder wants relatives who live apart to share: seeds, a plant, a short
  letter. There's no server yet, and the game must stay complete offline.

## PM decision
- A **parcel** carries one gift (seeds 1–500, a cutting of a nursery plant, or
  letter only) plus an optional letter (280 characters, 6 lines) and the
  sender's name. It travels as text through any messaging app.
- Gifts move seeds, never create them: seeds sent leave the sender; a cutting
  costs the sender its nursery price and removes nothing they own.
- Opening: seeds are added; a cutting waits in the basket until planted (then
  it's a normal nursery plant with its own puzzle); the letter is kept (20
  newest) for the map.
- Safety: a parcel can't be opened twice in one garden, the sender can't open
  their own, a changed parcel is refused and changes nothing.
- The shared message is a friendly line plus one token with no quotes or
  spaces, because messaging apps "smarten" quotes and that would break the
  checksum for exactly the players we design for. The token is found anywhere
  in a paste, even in a forwarded message.
- **Known limit, accepted for offline:** a parcel isn't addressed, so one seed
  parcel forwarded to several relatives pays each of them. Only the future
  family-circle server can stop that. Recorded in the depth spine.

## Dev
- Built by a Sonnet subagent from a written brief, test-first (red on a
  missing type), reviewed and re-run here: `Core/FamilyParcel.cs`
  (`TryCreate/TryRead`, strict reader reusing GardenTransfer's JSON parser and
  SHA-256), `Core/WorldModel.Family.cs` (`SendParcel`, `Open`, `PlantGift`,
  `Gifts/Letters/LetterFrom`); new snapshot fields `gifts`, `parcelIds`,
  `letters`, `letterFrom`, validated fail-closed; garden file keys with older
  files still accepted.
- Added here, test-first: `FamilyParcel.Wrap/Unwrap` (URL-safe base64 token
  after `MOSSELORA-PARCEL:`).

## Result
- Core tests: 71,255 assertions pass (70,376 before). No player-visible change
  yet; the parcel screen is I215.
