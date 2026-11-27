# SC03: The companion's line bank

Smart companion, iteration 3 of 7 ([spec](../../games/evergrove/docs/specs/proposals/smart-companion-2026-11-27.md)).

**Status: done.** The founder approved all 300 lines on 2026-11-27, and they ship in `en.json`.

## Player note

Neighbours now say the same true things in many different ways: 20 wordings per kind in each voice, never the same wording twice in 14 days, and the same wording for the rest of a visit. Kinds without reviewed lines yet still speak today's sentences.

## PM decision

These five fact kinds come first because they are heard most, or repeat most, today:

- `NewestThing` and `KindSolved` are the most frequent remarks. In the scripted week, the same "You freed the water…" came back on days 1, 4 and 8.
- `GiftWaiting` is a waiting fact, so it may be said on every visit until the cutting is planted. One wording would become a refrain.
- `FruitGiven` is the thank-you, and should feel like a response, not a recording.
- `SolvedTogether` is the family-play moment (vision §1a).

`NewArrival` and the rest follow in SC04.

The voices follow the spec:

- **Mira** (trees): warm, family sayings.
- **Orin** (built things): practical, dry, notices craft.
- **Fen** (flowers): bright, specific about bees, ladybirds, moths, wrens and the like.

Small-creature facts are general and true: butterflies taste with their feet, bees can see colours we can't, both robin parents feed their chicks, and there is the old custom of telling the bees. No line reports an event the save can't prove.

The lines were written by hand, with no API script.

## Dev

- `Core/LineBank.cs`:
  - Shape `{ "<FactKind>": { "0|1|2|any": [lines] } }`, parsed by a small strict reader. Unknown kinds or voices, duplicates and trailing text are refused, never half-read.
  - `Speak` picks a wording with `CompanionRanker.Variant` (hash of garden, day and key, skipping wordings heard in 14 days) and keeps this visit's wording.
  - `Fill` refuses any line whose slot is missing or empty, so a brace never reaches the screen.
- `Assets/Resources/Evergrove/Lines/en.json`: separate from `GameLocalization`'s catalogue (127 keys on main now). The docs contract's key count is unchanged by this iteration.
- `CompanionFacts.WorkOf(PuzzleKind)`: the `{work}` slot (for example "the seed sums"), on `KindSolved` and `SolvedTogether`.
- `EvergroveApp`:
  - `LineBankNow()` loads the bank for the current locale.
  - `CompanionWords` uses the bank or today's sentence.
  - The wording is recorded in memory and kept for the visit.
- Rules, written once in `games/evergrove/docs/specs/companion-line-rules.json` and enforced twice:
  - `tools/evergrove_line_bank_check.py [--review]` (shipped banks and review drafts);
  - `Tests/LineBankTests.cs`, the same rules in C#, plus a test that each rule catches a line breaking it.

  The rules:

  - every slot is one the kind fills;
  - at most 140 characters, filled with the game's longest real values, which the tests check are the longest;
  - no "!" and no digits;
  - the spec's banned phrases, plus "in a row", which the tests found missing;
  - words unsuited to a 7-year-old;
  - no time claims ("yesterday", "again", …);
  - a slot never starts a sentence or follows "a/an";
  - `{thing}` is never a verb's subject, because "cowslips" is plural;
  - no duplicates, and at least 20 lines per kind and voice.

## Result

- Drafts: `docs/improvements/review/companion-lines-{NewestThing,KindSolved,FruitGiven,GiftWaiting,SolvedTogether}.md`, 300 lines.
- The first checker run caught 7 lines with a plural-breaking subject and one "again". All were rewritten.
- `tools/evergrove_line_bank_check.py --review`: 300 lines, 0 problems.
- `dotnet run --project games/evergrove/Tests`: passed, 1,283,089 assertions. "Line bank SC03: 0 shipped lines and 300 draft lines pass every rule."

- After approval:
  - the lines moved into `en.json`, and each review file was marked approved and kept as the record;
  - a new test proves the shipped bank speaks, with every slot filled and within 140 characters, for every fact of each of the five kinds;
  - C# tests passed: 1,638,661 assertions. "Line bank SC03: 300 shipped lines and 300 draft lines pass every rule."
- Unity build `c5`: 0 `error CS`, and `Info.plist` present.
- Tours `c5p` (portrait), `c5l` (landscape) and `c5x` (150% text) each reached `MOSSELORA_QA_TOUR_DONE 128`.
- In the scripted week, a tree cutting now waits in Mira's basket, and she says it in a different wording on each of the 8 days. Orin's and Fen's solved and newest-thing remarks come from the bank too.
- At 150% text, Orin's longest line ("Plain sense and a bit of patience…") wraps to six lines. The card grows over the board and stays readable.
- Observed, not changed: a waiting fact (+50, exempt from the 3-day rule) takes that neighbour's remark on every visit until it's acted on. That is what the spec intends, but it should be watched in the player check (SC07).

## Not verified
- No physical device.
- No players aged 40+: age fit not validated.
