# I226 — Family help: the rules

## Player
- When I visit my granddaughter's garden (I216) I can only look. I'd like to do
  something for her while I'm there, the way you'd water a neighbour's plants.

## PM decision
- From the design spec (depth spine, Layer 5): solve one of their things'
  puzzles while visiting; it blooms in their garden when they open the parcel.
- A Help parcel names one grown thing in their garden. It costs nothing and
  pays nothing on either side, so there's nothing to farm. The owner keeps
  their own first-solve seeds and their own puzzle. The bloom is a mark of who
  helped, not a solve.
- One helper per thing: helping again replaces the name.

## Dev
- Core and tests were written by a Sonnet subagent and reviewed by me.
  - `ParcelGift.Help`; the thing's index rides in the existing `kind` key, with
    no format change.
  - New error: `ParcelError.NotInThisGarden`.
  - `WorldModel.Helped`, `HelpedFrom`, `IsHelped`, `HelpedBy`.
  - Snapshot fields `helped` and `helpedFrom`, validated fail-closed.
  - Garden-file keys with a `matchesPreHelp` checksum matcher, so older files
    still import.
- `NeighbourNotice`: the neighbour of that thing's kind names the helper until
  the owner solves it.
- `FamilyHelpTests` covers:
  - round trip and refusals;
  - seeds, solves and `PuzzlesSolved` unchanged;
  - replace on re-help;
  - snapshot and garden-file round trips, tamper refusals and old files;
  - the notice.

## Result
- Core tests: 71,748 assertions pass (117 new).
