# Google Keyword Planner niche scan — Vietnam — 2026-11-21

## Finding

**Nối Từ** is the strongest niche hypothesis to audit next. It is not a production selection. Google Keyword Planner shows a substantial web-search range, while direct App Store listings reveal a repeatable-looking word-validation complaint. The evidence still does not prove App Store demand, low organic competition, or a licensed dictionary source.

## Keyword Planner evidence

The planner was set to Vietnam, Vietnamese, Google Search network, and the historical range November 2025–October 2026. The focused result page showed 121 ideas; this note transcribes only the relevant visible rows. The CSV export did not complete, so there is no raw Google Planner file in the repository.

The Google Ads page did not create or launch a campaign, and no money was spent.

| Seed | Average monthly searches | Google Ads competition | 3-month / YoY change |
| --- | ---: | --- | --- |
| `nối từ` | 10K–100K | Low | 0% / 0% |
| `nối từ online` | 1K–10K | Low | +900% / +900% |
| `game nối từ` | 1K–10K | Low | 0% / 0% |
| `nối chữ tiếng Việt` | 10–100 | Low | 0% / 0% |
| `cờ tướng offline` | 1K–10K | Low | 0% / +900% |

These are ranges, not exact query counts. Google says average monthly searches include close variants and depend on the selected time range, location, and network; the `Competition` column counts advertisers, not competing apps ([Keyword Planner historical metrics](https://support.google.com/google-ads/answer/3022575?hl=en), [Keyword Planner workflow](https://support.google.com/google-ads/answer/7337243?hl=en)). The +900% changes may reflect a low baseline. The planner output is Google web search, not App Store search or installs.

The Apple Ads API did not return `nối từ` in the limited popular-term universe. Per the [API documentation](https://developer.apple.com/documentation/apple-ads-platform-api/query-app-search-term-popularity-data), this absence is unknown, not zero.

## App Store supply and review signal

The public Apple catalog search for `nối từ` returned 18 catalog matches; this order is not verified organic App Store ranking. Direct listings include:

- [Nối từ - Chơi nối từ đi](https://apps.apple.com/vn/app/n%E1%BB%91i-t%E1%BB%AB-ch%C6%A1i-n%E1%BB%91i-t%E1%BB%AB-%C4%91i/id6448744698): 2.9K ratings, 3.3 stars; AI opponent, 15-second turns, daily challenge, leaderboard, and rewarded ads. Visible reviews complain that valid words can be rejected and invalid words accepted.
- [Nối chữ tiếng Việt siêu khó](https://apps.apple.com/vn/app/n%E1%BB%91i-ch%E1%BB%AF-ti%E1%BA%BFng-vi%E1%BB%87t-si%C3%AAu-kh%C3%B3/id6482039027): 175 ratings, 4.0 stars; a visible review questions the app's word meanings and dictionary accuracy.
- [Chơi nối từ đi](https://apps.apple.com/vn/app/ch%C6%A1i-n%E1%BB%91i-t%E1%BB%AB-%C4%91i/id1520500710): 278 ratings, 1.3 stars; visible reviews also report word-validation errors. Its latest listed update is from 2023.

These are small, non-random public review samples, with some older reviews. They are a signal to investigate, not a complaint-frequency estimate. [Words to Win](https://apps.apple.com/vn/app/words-to-win-guess-words/id1605702371) is adjacent word gameplay, but not the same word-chain rule.

The physical audit lead is captured in [this one-query queue](vn-noitu-capture-queue-2026-11-21.csv). It marks Apple Ads popularity as unknown because the term was not returned in the limited API sample.

## Product hypothesis

A Vietnam-first word-chain game whose core promise is **reliable, transparent word validation**: show why a phrase is accepted or rejected, allow offline solo or pass-and-play, and avoid forced ad interruptions. The differentiator is lexicon accuracy and fair rules, not a cosmetic refresh or a generic AI opponent.

**Strongest counterargument:** there are already direct word-chain apps, and Google search volume may include people looking for rules or web play instead of an app. The most successful direct listing already has nearly 3,000 ratings. No physical App Store rank or current review-frequency sample exists.

## Dictionary rights and quality gate

A verified Vietnamese phrase list is the load-bearing assumption:

- [duyet/vietnamese-wordlist](https://github.com/duyet/vietnamese-wordlist) contains about 74K words and phrases under GPL-2.0.
- [vntk/dictionary](https://github.com/vntk/dictionary) is MIT-licensed, but its repository gives little detail about the source lineage of the data.
- [NhanAZ-Drops' source dossier](https://github.com/NhanAZ-Drops/tu-dien-tieng-viet/blob/main/SOURCES.md) describes a source-attributed beta dataset built from mixed licenses, including GPL, CC BY-SA/GFDL, and some entries whose licensing is unclear.

Do not bundle any of these datasets until the exact data license and distribution obligations are verified. A dictionary that is licensed but inaccurate would still fail the proposed product promise.

## Cheapest falsification test

1. On a physical iPhone set to the Vietnam App Store, capture the first ten unsponsored results for `nối từ` and record titles, ratings, prices, updates, and screenshots.
2. Sample 20 recent positive and negative reviews across the top three direct apps. Count dictionary errors, repeated prompts, ads, and other concrete complaints separately.
3. Identify a commercial-compatible word/phrase source and adjudicate 200 common chains with two Vietnamese reviewers.

**Proposed screening rule, not an industry benchmark:** proceed to a one-week prototype only if at least five of 20 recent reviews across two or more direct apps show a repeated, unresolved validation problem; the top ten do not contain more than three high-rated direct word-chain games; and the 200-chain test reaches at least 98% reviewer agreement. Otherwise, park it.
