# US App Store catalog scan — 2026-11-15

**Type:** observed *public iTunes Search API* product metadata, not a verified App Store organic ranking audit. **Not search volume, not downloads, not conversion, not revenue.**

## Method

- Queried Apple's [public Search API](https://performance-partners.apple.com/search-api) for `media=software`, `entity=software`, `country=US`.
- API matches are **not equivalent to organic App Store rankings** (ads and personalization are not represented).
- Eight queries use `limit=5`, two use `limit=10`. Counts are **truncated samples**, not the competitive universe.
- Observation date: 2026-11-15. Ratings and app update dates are dynamic.
- The results are replicable via the API query links. They are not evidence that an app's niche is attractive.

## Observed competitors (US storefront)

| Keyword | API query | Notable observed apps and US rating counts | Implication |
| --- | --- | --- | --- |
| Kakuro | [query](https://itunes.apple.com/search?term=kakuro&country=us&media=software&entity=software&limit=10) | [Conceptis Kakuro: Number Crossword](https://apps.apple.com/us/app/kakuro-number-crossword/id1197071375) 1,961; [Real Kakuro](https://apps.apple.com/us/app/real-kakuro/id459669325) 2,629; [Kakuro Endless](https://apps.apple.com/us/app/kakuro-endless/id1327617338) 449 | Multiple established incumbents; no easy opening shown. |
| Star Battle | [query](https://itunes.apple.com/search?term=star+battle+puzzle&country=us&media=software&entity=software&limit=10) | [Star Battle Puzzles](https://apps.apple.com/us/app/star-battle-puzzles/id6739070759) 134; [Star Battle Hoshi](https://apps.apple.com/us/app/star-battle-hoshi-puzzles/id1663477610) 273 | Multiple competing games and several recently updated. |
| Tents and Trees | [query](https://itunes.apple.com/search?term=tents+and+trees&country=us&media=software&entity=software&limit=5) | [Tents and Trees Puzzles](https://apps.apple.com/us/app/tents-and-trees-puzzles/id1279378379) 548; several newer 2026 listings with zero ratings | Existing trusted brand and recent entrants. |
| Hashi Bridges | [query](https://itunes.apple.com/search?term=hashi+bridges&country=us&media=software&entity=software&limit=5) | [Conceptis Hashi: Bridges](https://apps.apple.com/us/app/hashi-bridges/id567583852) 1,040; several new 2026 apps with few ratings | A well-established quality incumbent. |
| Futoshiki | [query](https://itunes.apple.com/search?term=futoshiki&country=us&media=software&entity=software&limit=5) | [The Futoshiki](https://apps.apple.com/us/app/the-futoshiki/id1398126397) 817; [Futoshiki Puzzle Game](https://apps.apple.com/us/app/futoshiki-puzzle-game/id846765771) 472 | Multiple specialist competitors. |
| Logic Grid Puzzles | [query](https://itunes.apple.com/search?term=logic+grid+puzzles&country=us&media=software&entity=software&limit=5) | [Logic Grid Puzzles](https://apps.apple.com/us/app/logic-grid-puzzles/id919571269) 39,226; [Logic Puzzles – Clue Game](https://apps.apple.com/us/app/logic-puzzles-clue-game/id1641732564) 58,839; [Cross Logic](https://apps.apple.com/us/app/cross-logic-puzzle-game/id1561830495) 49,396 | **Major correction**: generic term is *highly contested*. |
| Word Ladder | [query](https://itunes.apple.com/search?term=word+ladder&country=us&media=software&entity=software&limit=5) | [Word Ladder](https://apps.apple.com/us/app/word-ladder/id6450907105) 25; [Word Ladder – brain games](https://apps.apple.com/us/app/word-ladder-brain-games/id6471902631) 6; [Word Ladder Puzzle](https://apps.apple.com/us/app/word-ladder-puzzle/id6759207300) 2 | Could be underserved or have very little search demand. |
| Masyu Puzzle | [query](https://itunes.apple.com/search?term=masyu+puzzle&country=us&media=software&entity=software&limit=5) | [Masyu Puzzle](https://apps.apple.com/us/app/masyu-puzzle/id1071607348) 11; [Masyu Classic](https://apps.apple.com/us/app/masyu-classic/id6744591087) 8; [Masyu Infinite](https://apps.apple.com/us/app/masyu-infinite/id1192493351) 49 | Weak evidence of demand; fast kill test recommended. |
| Shikaku Puzzle | [query](https://itunes.apple.com/search?term=shikaku+puzzle&country=us&media=software&entity=software&limit=5) | [Fill Shikaku](https://apps.apple.com/us/app/fill-shikaku/id6443563905) 3,220; [Rectangles](https://apps.apple.com/us/app/rectangles-shikaku-puzzle/id6754390677) 85 | Large publisher and small fresh entrants. |
| Nurikabe | [query](https://itunes.apple.com/search?term=nurikabe&country=us&media=software&entity=software&limit=5) | [Conceptis Nurikabe](https://apps.apple.com/us/app/nurikabe-islands-walls/id581912445) 402; several new Nov 2026 listings with zero ratings | Active competition; cloning alone insufficient. |

## Updated interpretation

Our earlier shortlist over-weighted simple mechanics and under-weighted publisher strength. **Low engineering cost attracts more entrants**. One publisher, Conceptis, already has competitive offerings in Kakuro, Hashi and Nurikabe; broad Logic Grid has much larger incumbents than the initial sample suggested.

### Next research queue — not greenlit games

1. **Word Ladder:** measure non-branded Apple Ads search popularity; test whether the many low-rated listings actually have traffic.
2. **Masyu:** inexpensive technical spike, but potential near-zero keyword demand. Prioritize a demand kill test.
3. **Tents & Trees / Kakuro:** only worth building with observed repeat complaints and a defensible user-specific value proposition.
4. **Logic Grid:** deprioritize generic cloning; inspect subgenres such as original narrative mysteries if supported by demand data.

No authenticated Apple Ads popularity data or organic impressions have been collected. Review counts above **cannot** answer the high-volume part of the opportunity question.

## Reproduce

Dry run (no network): `python3 tools/scan_storefront.py --country US --max-keywords 5 --dry-run`

Explicit read-only live scan: `python3 tools/scan_storefront.py --country US --max-keywords 5 --live --output .apple-results/scan-us-2026-11-15.json`

The CLI rate-limits requests and saves snapshots locally. Do not commit raw catalog data without reviewing Apple's terms.
