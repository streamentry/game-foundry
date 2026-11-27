# A smart companion without a runtime model

**Status:** proposal, 2026-11-27. Not built. Founder question: "what would be the best way to make everything smart without having to try too hard?"

## The idea in one paragraph

Players experience "smart" as three things: the game **notices** what they did, **remembers** it, and **never repeats itself**. All three can be done with plain rules over the save. So the intelligence lives in one small "what matters now" picker:

- a **fact feed** reads true facts from the save;
- a **ranker** chooses the one worth saying now;
- a **line bank** supplies the words, written once at build time (with a large model's help), reviewed by a person and shipped as data.

No model runs on the phone. It works offline on every device, can be translated into all 30 languages, and every line is true by construction.

## Where we stand (main, 2026-11-27)

- `EvergroveApp.NeighborSays` is a hand-ordered if-chain over about ten sources, in this order:
  1. thanks for a nursery plant of their kind (I210);
  2. a puzzle solved together (`NeighbourNotice.TogetherLine`);
  3. the weather since your last visit (I359, Mira);
  4. a letter waiting (I236, Orin);
  5. `NeighbourNotice.Line` (puzzles solved of their kind, river closeness);
  6. today's weather (`DayWeather.Remark`, I357/I358);
  7. the hour (I281);
  8. the newest thing of their kind and where it stands (I184);
  9. the story's own lines (`LocalizedVillage.Speak`).
- Other true sources exist but each has its own path: `NeighbourWish`, `NeighbourGifts`, `CottageSummary`, `SeasonTales`, the festival note, `Visitors`, fruit given.
- In flight on run-3 branches, not yet on main: good-neighbour pairing hints, visitor remarks (`Visitors.KeeperRemark`), pollination, seasonal yields.
- **Outside English, neighbours only speak the story's lines.** The noticing lines are built from English string concatenation and can't be translated.
- **Repetition is managed case by case.** For example, one day in three for the visitor remarks, and the since-last-visit line once per return. There is no shared memory of what was said.

The parts are good and honest. What's missing is one shared shape for them.

## Design

### 1. Fact feed (Core, pure)

`CompanionFacts.For(WorldModel world, CompanionContext context)` returns a list of `Fact` values:

| Field | Meaning |
| --- | --- |
| `Kind` | an enum such as `PlantOfTheirKind`, `SolvedTogether`, `WeatherWhileAway`, `LetterWaiting`, `KindSolved`, `RiverClose`, `WeatherToday`, `Hour`, `NewestThing`, `WishProgress`, `GiftWaiting`, `FruitGiven`, `SeasonTurned`, `VisitorCame` (later: `PairingFound`, `YieldReady`) |
| `Keeper` | who it belongs to: 0 Mira (trees), 1 Orin (built things), 2 Fen (flowers), or -1 for anyone |
| `Key` | a stable identity for memory, e.g. `"visitor:owl"` or `"letter:Nan:3"` |
| `Slots` | values for the words: `{thing}`, `{where}`, `{days}`, `{name}`, `{count}`, `{weather}` |
| `Weight` | how much it matters (below) |

The context carries what the save doesn't hold: the local date and hour, today's weather, unread letters on this device, and the date of the last return.

Rules:

- **Only true facts.** Each fact type is built from the save or context and nothing else, as `NeighbourNotice` already is. If a fact can't be shown true, it isn't produced.
- **No guilt, no pressure.** There are no facts about absence as a loss, no streaks and no counts of missed days. "It rained twice while you were away" is fine. "You haven't been here in 3 days" is not.
- Existing sources move into the feed one by one, unchanged in meaning.

### 2. Ranker (Core, pure, deterministic)

`CompanionRanker.Pick(facts, memory, keeper, dayNumber)` returns one fact or none.

Weight is added up from:

| Signal | Weight | Example |
| --- | --- | --- |
| Waiting for you | +50 | a letter unread, a cutting in the basket |
| New: never said before | +30 | the first time the owl came |
| Rare: once a season or less | +20 | the season turned, a yield is ready |
| Your own doing, recently | +15 | you gave Orin fruit, solved a tree puzzle together |
| Their own line of work | +10 | Fen and flowers |
| Timely ambient | +5 | today's weather, the hour |

Then:

- Skip a fact whose `Key` was said in the last 3 days, unless it's still waiting (an unread letter).
- Each neighbour says one remark per visit to the garden. A fact goes to its keeper; anyone-facts go to whoever has nothing better.
- Ties break by a hash of (garden, day, key). No random numbers, so tours and tests are repeatable.
- With nothing worth saying, return none and let the story line speak, as today.

**Memory** is device-local: what was said and when, as a short list of (key, variant, day) in PlayerPrefs, like the per-device replay counts (I259). It's conversation state, not garden state, so the save and the garden file don't change. A garden restored on another device may hear a remark again, which is acceptable.

### 3. Line bank (data, localizable)

- Path: `Assets/Resources/Evergrove/Lines/en.json`, one file per locale.
- Keep it **separate from `GameLocalization`'s 125-key catalogue**, so the docs contract and existing catalogues don't change.
- Shape: `{ "<FactKind>": { "<keeper 0|1|2|any>": ["line with {slots}", ...] } }`.
- Target is 20–50 variants per fact kind per voice. The variant is chosen by a hash of (garden, day, key), skipping any variant said in the last 14 days.
- **Voices**, from the existing lines:
  - Mira: warm, a little wise, family sayings ("my grandmother called it").
  - Orin: practical and dry, notices craft and the lane.
  - Fen: bright, specific about flowers and small creatures.
- **Rules every line must pass** (checked by tests):
  - every `{slot}` is filled by its fact;
  - at most 140 characters, so it stays readable at 150% text;
  - no exclamation marks;
  - no banned phrases: "missed you", "where have you been", "don't forget", "streak", "come back", "every day", or any health or wellbeing claim;
  - it claims nothing beyond its slots.
- **Locales:** English first. Translations follow the existing catalogue workflow and stay drafts until a native speaker reviews them. Until a locale has a bank, that locale keeps `LocalizedVillage` as today. This finally lets the noticing lines speak other languages.

### 4. Writing the bank (offline, once per fact kind)

- `tools/evergrove_line_bank.py` drafts variants with a large model from:
  - the voice notes above;
  - the fact kind's slots;
  - five hand-written examples per voice;
  - the banned-phrase list.
- The API key is read from the environment, never committed. The script never runs in CI or in the app.
- **A person reads every line before it's committed.** Drafts land in a review file, accepted lines move into `en.json`, and rejected ones are deleted. Without review, it's AI slop.
- The same checks the tests run also run in the script, so bad drafts never reach review.

### 5. One ranker, many uses

The same facts and weights decide:

- what each neighbour says (above);
- **where the home arrow points** (I351 today points at the waiting thing; the ranker's top "waiting" fact replaces the special case);
- **the order of the cottage card's lines** (I299);
- **a soft mark on one menu icon** when something there is waiting for you, such as a letter or a cutting. This shows content that is waiting, never a count and never a nag;
- **the weekly letter from a neighbour** (later): the top three facts of the week as a short letter in one voice.

## Build order (iterations)

1. `Fact`, `CompanionFacts`: move the existing sources in, with no change in what players see. Tests compare against today's `NeighborSays` for a set of saves.
2. `CompanionRanker` and device memory. `NeighborSays` becomes "feed → rank → speak", still using today's sentences.
3. The line bank format, loader and checks. Ship 5 fact kinds with 20 English variants per voice, written with the script and reviewed.
4. The rest of the fact kinds, including the run-3 ones (pairings, visitors, yields) once merged.
5. Home arrow, cottage card and menu mark use the ranker.
6. Translated banks for locales as they're reviewed.
7. A player check (below).

Each iteration is small, testable and shippable alone. Steps 1–2 change nothing visible, which makes them safe.

## Tests

- **Fact feed:** for constructed saves, each fact appears only when its condition holds, and its slots match the save.
- **Ranker:** deterministic for the same inputs; never repeats a key within 3 days (unless waiting) or a variant within 14 days; one remark per neighbour per visit; returns none when nothing qualifies.
- **Line bank:** every kind and voice has at least the target number of variants; every line passes the rules above; every slot used by a line exists on its fact kind.
- **Tours:** the existing `20-fen-at-night` and talk shots, plus one shot per neighbour after a scripted week of play.

## Preflight (AGENTS.md)

| Question | Answer |
| --- | --- |
| **Truth** | Observed: today's lines are true, English-only, and repetition is handled ad hoc. Assumption: players feel "noticed and remembered" more than they want free conversation. That is unknown and is what step 7 tests. |
| **Owner** | Founder goal: "a real AI companion in subtle ways", "smart without trying too hard". |
| **Delete** | No runtime model, no backend, no new save fields, no free text input. |
| **User value** | Neighbours that notice what you did, in your language, without repeating themselves. |
| **Audience fit** | Not validated with adults 40+. Children 7+ co-play, so every line is pre-written and reviewed; nothing is generated at runtime. |
| **Falsification** | A week with the bank for 5–10 adults aged 40+. Ask what the neighbours said. |
| **Stop** | Stop if testers can't recall any remark, call them repetitive within the week, or prefer the old lines. |
| **Evidence** | Players' own words, quoting a remark back, recorded in `docs/research/decision-log.md`. |

## What this is not

- Not a chatbot: no free text input, no runtime generation, no account, no network.
- Not a wellbeing or loneliness feature. The game must never claim either.
- **A runtime model is a later, optional layer.** If testers ask for more, Apple's on-device model (supported iPhones only) could reword a ranked line or write a longer letter from the same facts, always falling back to the bank. The ranker and facts stay the foundation either way.
