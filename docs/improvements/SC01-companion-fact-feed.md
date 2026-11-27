# SC01: The companion's fact feed

Smart companion, iteration 1 of 7 ([spec](../../games/evergrove/docs/specs/proposals/smart-companion-2026-11-27.md)).

## Player note

Nothing changes for players. Each neighbour says exactly what they said before, word for word.

## PM decision

The spec's first step moves every source of what a neighbour says into one list of true facts. The hand-ordered chain then becomes a choice over that list, with no change in what anyone sees. Later iterations change the choice (SC02) and the words (SC03) one at a time, so each change can be judged on its own.

Main has one source the spec didn't list: the good-neighbour hint (I370) sits between today's weather and the hour. It moved in as `PairingToFind`, in the same place.

## Dev

- `Core/CompanionFacts.cs`:
  - `FactKind`: 14 kinds, one per source in today's chain. `NeighbourNotice.Line`'s branches are split into their own kinds (gift waiting, helped by family, new arrival, river close, river further, fruit given, kind solved).
  - `Fact` (immutable): kind, keeper (0 Mira, 1 Orin, 2 Fen, -1 anyone), stable key, slots, base weight, `Waiting`, `Fresh`, and the old chain's order.
  - `CompanionContext` (immutable): the local day, the last return, letters read on this device, today's weather and light, fruit just given, a puzzle just solved together, the painted map.
  - `CompanionFacts.For(world, context)`: pure and deterministic. A puzzle-only garden has no facts.
- `Core/CompanionSpeech.cs`:
  - `Legacy(fact, speaker)`: today's sentence for each kind, built from the fact's slots.
  - `LegacyPick(facts, speaker)`: the old chain's order over the feed.
  - `PairingHint` moved here from `EvergroveApp`.
- Small refactors, with no change in behaviour:
  - `DayWeather.WhileAway` and `SinceSentence` split out of `SinceLastVisit`.
  - `NeighbourNotice.KeptBy`, plus `internal` access to its sentence helpers.
- `EvergroveApp.NeighborSays` is now feed → legacy pick → legacy sentence, falling back to `LocalizedVillage` as before. `sinceLine` became `sinceSeen` (the date, not the sentence), and the unused `HourRemark` was removed.

## Result

`Tests/CompanionFactsTests.cs` holds a verbatim port of `NeighborSays` as it stood on main at `0dcabc9`. It is compared with feed → old sentence for every constructed save, context and neighbour: 4,800 comparisons, all identical. In 4,757 a noticing line speaks; in 43 the story line does. The saves cover:

- empty, growing, solved and finished gardens, and gardens with every good neighbour found;
- river close and river further;
- fruit, cuttings, family help, nursery plants, letters, pairings and play together.

The contexts cover every weather, every light phase, short and long returns, letters read, and fruit just given.

The test also checks that every fact kind appears, that facts are true only under their condition (a clock set back claims nothing, and yesterday is not "a while"), that keys are deterministic and unique, and that every fact has words in its keeper's voice.

- `dotnet run --project games/evergrove/Tests`: passed, 1,195,801 assertions.
- `tools/check_evergrove_docs.py` and `tools/verify_evergrove_project.py`: OK.
- The Unity build and tours are recorded with SC02. Both iterations ship in one PR, and SC01 alone has no visible change to shoot.

## Not verified

- No physical device.
- No players aged 40+: age fit not validated.
