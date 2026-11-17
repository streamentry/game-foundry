# Vietnam niche hypothesis — 2026-11-17

## Conclusion

No niche is proven to be both underserved and large enough to build yet. If Vietnam is an acceptable first storefront, **offline cờ tướng tactical training** is the strongest lead to audit first. It has measured search interest and a specific use case, but a strong direct competitor already offers offline puzzles and AI. This is an audit hypothesis, not a game selection.

## Measured search signal

Apple Ads' October 2026 Vietnam genre sample reports:

| Search term | Global popularity | Games popularity | Games rank |
| --- | ---: | ---: | ---: |
| `cờ tướng` | 79 | 83 | 19 |
| `cờ tướng offline` | 58 | 61 | 472 |

The 1–100 values are relative popularity, not query volume. The genre sample is top-K truncated. Patterns for `cờ thế`, `tàn cuộc`, `khai cuộc`, `học cờ tướng`, and `luyện cờ tướng` returned no rows; these intents remain **unknown**, not zero. [Apple Search Term Popularity API](https://developer.apple.com/documentation/apple-ads-platform-api/query-app-search-term-popularity-data).

The one-pair physical capture queue is [here](vn-cotuong-capture-queue-2026-11-17.csv); the local raw snapshot is `vn-subgenre-2026-10.json` with SHA-256 `b74303ab748e2a86d92abcc1565fca66909412ecfed86bb82f03c03285c714a7`.

## Existing supply and user signals

- [Cờ Tướng, Cờ Thế Offline: Haga](https://apps.apple.com/vn/app/c%E1%BB%9D-t%C6%B0%E1%BB%9Bng-c%E1%BB%9D-th%E1%BA%BF-offline-haga/id6443545303) shows 12K ratings at 4.8, offline play, thousands of board puzzles, AI, move suggestions, replay and undo. It offers a remove-ads purchase for 99,000 VND and was updated on 2026-10-05.
- [Co Tuong, Co Up Online - Ziga](https://apps.apple.com/vn/app/co-tuong-co-up-online-ziga/id1415712748) shows 130K ratings at 4.7 and provides online matchmaking, Elo ratings, friends and cross-platform accounts.
- The [Haga review page](https://apps.apple.com/vn/app/c%E1%BB%9D-t%C6%B0%E1%BB%9Bng-c%E1%BB%9D-th%E1%BA%BF-offline-haga/id6443545303?see-all=reviews&platform=iphone) includes several October–November 2026 complaints about ad volume and an older question about finding saved board puzzles. This is a small visible sample, not a representative frequency estimate; the overall rating is high.

**Strongest objection:** Haga already satisfies the basic “offline cờ tướng puzzles” job with substantial content and an AI. “Offline” or “remove ads” alone would be a weak differentiator. The search score for the more specific offline term is also far lower than the broad term, and Apple does not provide absolute volume.

## Other leads screened

- **Large-print word search:** generic `word search` scores 79 within US Games, but Apple Ads returned no rows for the large-print/senior modifiers, so their demand is unknown. The App Store already has new large-print products with adjustable text, high contrast, daily puzzles and offline play ([Word Search: Large Print Daily](https://apps.apple.com/us/app/word-search-large-print-daily/id6759038491); [Large Print Word Games](https://apps.apple.com/us/app/large-print-word-games/id6818025555)). Existing supply means the modifier alone is not a wedge.
- **Ad-free/offline Sudoku:** `sudoku free no ads` scores 71 within US Games, but current listings already offer ad-free offline play, larger text, daily puzzles and, in some cases, VoiceOver support ([Sudoku: No Ads & Offline](https://apps.apple.com/us/app/sudoku-no-ads-offline/id6502922368); [Sudoku: No Ads, Unlimited](https://apps.apple.com/us/app/sudoku-no-ads-unlimited/id6757938217)). This looks more crowded.
- **Vietnamese Rubik solver:** `giải rubik` scores 81 within VN Games, but that intent is primarily a solving utility, not clearly a game; the catalog includes established solver apps. Treat it as a product-category mismatch until user intent is audited.

## Cheapest falsification test

On a physical iPhone set to the Vietnam App Store, search exactly `cờ tướng offline`, save the first ten unsponsored results and listing details, then sample 20 recent positive and negative reviews across the top three relevant apps. Use the [storefront audit guide](storefront-audit.md) and preserve screenshots.

**Proposed screening rule, not market evidence:** continue to a one-week prototype only if at most two of the top ten offer a comparable offline tactics-training loop **and** at least five of the 20 sampled reviews describe a repeatable material problem that those apps do not solve. Otherwise park the idea. A prototype still would not validate organic acquisition.

Raw Apple Ads and catalog snapshots remain local in ignored `.apple-results/`.
