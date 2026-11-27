# SC01: The companion's fact feed

Smart companion, iteration 1 of 7 ([spec](../../games/evergrove/docs/specs/proposals/smart-companion-2026-11-27.md)).

## Player note

Nothing changes for players. Each neighbour says exactly what they said before, word for word.

## PM decision

The spec's first step moves every source of what a neighbour says into one list of true facts. The hand-ordered chain then becomes a choice over that list, with no change in what anyone sees. Later iterations change the choice (SC02) and the words (SC03) one at a time, so each change can be judged on its own.

Main has sources the spec didn't list. The good-neighbour hints (I370, plus I383's deeper three once the first four are found) sit between today's weather and the hour, and became `PairingToFind`. Run 3's visitor remark (I377, `Visitors.KeeperRemark`) follows them and became `VisitorCame`. Then come the hour you usually come (I419, `Companion.HabitLine`), which became `HourHabit`, and the welcome after a long time away (I420, `Companion.WelcomeBack`), which joined `WeatherWhileAway` as its `welcome` slot. All keep their place in the chain.

## Dev

- `Core/CompanionFacts.cs`:
  - `FactKind`: 16 kinds, one per source in today's chain. `NeighbourNotice.Line`'s branches are split into their own kinds (gift waiting, helped by family, new arrival, river close, river further, fruit given, kind solved).
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

`Tests/CompanionFactsTests.cs` holds a verbatim port of `NeighborSays` as it stood on main at `0cf0825` (after run 3's I373–I422 merged in). It is compared with feed → old sentence for every constructed save, context and neighbour: 16,683 comparisons, all identical. In 16,473 a noticing line speaks; in 210 the story line does. The saves cover:

- empty, growing, solved and finished gardens, and gardens with every good neighbour found;
- river close and river further;
- fruit, cuttings, family help, nursery plants, letters, pairings and play together.

The contexts cover every weather, every light phase, short and long returns, letters read, fruit just given, three different days, so each keeper's "one day in three" visitor remark falls on one of them, a clear hour habit for every phase over seven days (I419's "one day in seven"), and returns after a week and after a month (I420).

The test also checks that every fact kind appears, that facts are true only under their condition (a clock set back claims nothing, and yesterday is not "a while"), that keys are deterministic and unique, and that every fact has words in its keeper's voice.

- `dotnet run --project games/evergrove/Tests`: passed, 1,633,291 assertions (after merging main at `0cf0825`).
- `tools/check_evergrove_docs.py` and `tools/verify_evergrove_project.py`: OK.
- The Unity build and tours are recorded with SC02. Both iterations ship in one PR, and SC01 alone has no visible change to shoot.

## Not verified

- No physical device.
- No players aged 40+: age fit not validated.
