# Apple Ads market scan — 2026-11-17

**Decision:** No first game selected. The measured signals identify search intents to audit, but do not establish an underserved market or a path to organic rank.

## Evidence collected

- Apple Ads Platform API, monthly period `2026-10`, storefronts US, GB, AU, DE and JP.
- Genre discovery returned 2,500 observations (500 per storefront). All five genre queries reached the collector's top-K limit; terms outside the returned sample remain **unknown**.
- Exact searches for the repository's 100 brainstormed keywords returned 0 rows across 25 queries. This is **unknown coverage**, not zero demand.
- Contains searches for `puzzle`, `logic`, `word`, `sudoku`, `block`, `number`, `mahjong` and `nonogram` returned 113 observations across 40 query/storefront pairs. Japanese patterns (`数独`, `ナンプレ`, `クロスワード`, `パズル`, `ブロック`, `脳トレ`) returned 12 observations across six JP queries.
- Apple reports relative 1–100 popularity and genre rank, not monthly searches, downloads, revenue or organic result position. [Search Term Popularity API](https://developer.apple.com/documentation/apple-ads-platform-api/query-app-search-term-popularity-data).

## Ten query/storefront pairs queued for a physical audit

These are measured search signals, not recommendations to build each game. `Global` is Apple popularity across genres; `Games` is popularity within games. Neither score is a search-volume estimate. `Genre rank` is not organic App Store rank.

| Storefront | Search term | Global | Games | Genre rank |
| --- | --- | ---: | ---: | ---: |
| US | mahjong | 74 | 87 | 17 |
| US | sudoku | 72 | 84 | 29 |
| US | color by number | 72 | 83 | 33 |
| US | puzzle games | 69 | 81 | 49 |
| US | word search | 68 | 79 | 75 |
| GB | block puzzle | 68 | 78 | 56 |
| GB | crossword | 67 | 77 | 62 |
| GB | jigsaw puzzle | 63 | 73 | 124 |
| DE | nonogram | 57 | 66 | 327 |
| JP | ナンプレ | 73 | 81 | 95 |

The corresponding capture queue is [storefront-capture-queue-2026-11-17.csv](storefront-capture-queue-2026-11-17.csv). Each pair still needs ten unsponsored results, listing metadata, screenshots and a balanced recent-review sample from the named storefront on a controlled physical device.

## Competition check

Apple's public catalog Search API returned 183 catalog matches for the ten US query samples. Its result order is **not** organic rank. The sample includes established listings such as Sudoku.com (1,874,551 ratings), Vita Mahjong (1,828,922), Happy Color (3,449,168), Block Blast (2,858,164), Wordscapes (1,145,069) and Nonogram.com (196,830), as observed on 2026-11-17. Rating counts are cumulative listing snapshots; they do not show searches, downloads, revenue or market share. [Apple Search API](https://performance-partners.apple.com/search-api).

This supports a competition-risk finding for broad puzzle terms. It does not prove that every niche is closed. We have not audited real organic positions or sampled reviews, so no product wedge or `no-wedge` conclusion is justified yet.

## Local evidence snapshots

Raw snapshots remain in ignored `.apple-results/` and are not committed. SHA-256 hashes:

| Snapshot | SHA-256 |
| --- | --- |
| `genre-2026-10.json` | `e463d2391cedee9d101aef29cf7b8d2933e0479ca65e61a7d541ba8470321929` |
| `candidate-exact-2026-10.json` | `95da428c2978454408972dbeb9d5366ca4e3987350a40861a9d81db6f0044a04` |
| `pattern-discovery-2026-10.json` | `11d04b6436be733d1631403c9f4d5c5f037df408c6bdea6d482a81a4217d0a8f` |
| `jp-pattern-discovery-2026-10.json` | `f867062c54f47d5a506c604dece788a3cb43c990c5b7c69056f4b369205b6593` |
| `us-demand-catalog-2026-10.json` | `18d31798ccfcc4a389b59d1996fac9fcf8e40127288ee3b01153ac8c12c3d7b5` |

## Next gate

Capture the queued searches on physical devices configured to US, GB, DE and JP storefronts; sample positive and negative reviews; then write a supported wedge or no-wedge finding. Until then, keep `Takuzu Lab` as an engineering reference and do not choose or build a release game.
