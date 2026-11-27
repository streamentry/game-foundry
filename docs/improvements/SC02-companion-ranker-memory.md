# SC02: The companion's ranker and memory

Smart companion, iteration 2 of 7 ([spec](../../games/evergrove/docs/specs/proposals/smart-companion-2026-11-27.md)).

## Player note

Neighbours now pick the one thing worth saying, remember what they've said on this device, and don't repeat a remark within three days unless it is still waiting for you, such as an unread letter. Each neighbour makes one remark per visit. Weather and the light are shared out, so one neighbour mentions the rain instead of all three. The words are still today's sentences. New wordings come with the reviewed line bank (SC03).

## PM decision

"Feed → rank → speak", as specified:

| Signal | Weight |
| --- | --- |
| Waiting for you (letter, cutting, family help) | +50 |
| Never said before | +30 |
| Rare (river close or further, weather while away) | +20 |
| Your own doing (fruit, solves, a new plant) | +15 |
| Their own line of work | +10 |
| Ambient (today's weather, the hour, a pairing hint) | +5 |

Three choices the spec left open:

- **A visit** starts when the app comes back to the foreground, on a new day, or in another garden. Once a neighbour has spoken, their remark stays the same for the rest of the visit, so reopening the card or rebuilding the page never changes it. One exception: something you just did for them, such as handing over fruit, is answered at once.
- **"Fresh" facts** (fruit handed over, a puzzle solved together just now) count as waiting (+50), so the thank-you comes first, as it did before.
- **Mira's "since you were last here"** used to be cleared when her card closed. Now the 3-day rule keeps it from repeating, and it lasts only for the day of the return. If she has something weightier, such as a cutting in the basket, she says that instead, and the weather line can still come on a later visit that day.

## Dev

- `Core/CompanionMemory.cs` (immutable):
  - Each remark is kept as (day, speaker, kind, variant, key).
  - Serialized to one PlayerPrefs string per garden (`evergrove:companion-heard:<garden>`), never the save or the garden file.
  - Kept for 60 days and at most 300 remarks. Unreadable lines are skipped. Separators in keys are made safe.
- `Core/CompanionRanker.cs` (pure):
  - `Pick` (one speaker), `Allot` (all three, sharing anyone-facts), `Score`, `Eligible`, `Variant` (the 14-day wording rule, ready for SC03) and `Hash` (FNV-1a over garden|day|key, because `string.GetHashCode` changes between runs).
  - A `canSay` predicate keeps a voice from being given a fact it has no words for.
- `EvergroveApp`:
  - `CompanionPick` feeds `NeighborSays`.
  - `OpenConversation` records the remark (`CompanionHeard`) after taking the line, so `HasNews` and rebuilds stay stable.
  - The visit resets on resume, on a new day, and on a garden change.
- QA tour: a scripted week (`QaSetDay`). Nine things grown, two tended, a letter from Anh, and a different weather each day. Each neighbour is spoken to once a day, with the lines logged (`MOSSELORA_COMPANION`), then `21-week-{mira,orin,fen}.png` on day eight.

## Result

- `Tests/CompanionRankerTests.cs`:
  - weights;
  - deterministic ties that take turns across days;
  - the 3-day rule (waiting facts exempt, a clock set back hides nothing);
  - one remark per visit, with fresh facts answered mid-visit;
  - anyone-facts going to exactly one neighbour, over 40 gardens;
  - `canSay`;
  - the 14-day wording rule (no repeat across 14 days of 20 wordings, least-recently-heard when all are used);
  - memory round-trip, pruning and the cap;
  - a scripted week over a real garden: 21 remarks, none repeated within 3 days, and no two neighbours saying the same fact on one visit.
- `dotnet run --project games/evergrove/Tests`: passed, 1,196,208 assertions. The SC01 parity test still passes: `LegacyPick` is unchanged and kept as the reference.
- Unity iOS simulator build `c1`: 0 `error CS`, and `Info.plist` present (`com.streamentry.evergrove.playtest`).
- Tours on the "Mosselora QA iPhone" simulator, all reaching `MOSSELORA_QA_TOUR_DONE 121`: portrait `c1p`, landscape `c1l`, and 150% text `c1x`. The week shots read cleanly in all three. At 150% Mira's line wraps to four lines inside the card.
- The week as logged: day 1 Mira on the water she freed, Orin on the old painting, Fen with a shade hint. By days 4 and 8 the same facts come back in the same old wording. That is the repetition SC03's variants are for.

## Not verified

- No physical device.
- No players aged 40+: age fit not validated.
- The letter wasn't mentioned in the tour's week, because earlier tour steps read it on this device (`letters-read`). The waiting-letter path is covered by the Core tests, not by a shot.
