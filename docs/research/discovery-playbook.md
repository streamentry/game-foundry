# Search-first game opportunity discovery

The research goal is **high, discoverable App Store search demand** for a differentiated game, rather than filling a quota of game releases.

## Important source limitation

Apple Ads [Search Term Popularity Insights](https://developer.apple.com/documentation/apple-ads-platform-api/query-app-search-term-popularity-data) reports **relative scores and ranks** for popular terms. It does not provide absolute monthly searches or the complete long tail. Per the [official row documentation](https://developer.apple.com/documentation/apple-ads-platform-api/searchtermpopularityrow), results are limited to up to **500 search terms per country and genre combination**. Consequently, an absent term must be labeled **outside the observed coverage**, not 'zero searches'.

Apple's 1–100 overall scale measures popularity **across genres in the country**, whereas `searchPopularityInGenre` measures popularity within the given genre. Do not compare scores across geographic markets as if they were absolute counts.

## Stage A — Discover demand from Apple, not from our brainstorm

Use **existing authorization** for the Apple Ads Insights API. Obtain OAuth access legitimately; never post credentials or raw private responses to Git.

```bash
# Preview safe read-only query bodies; no API credentials or network calls.
python3 tools/fetch_apple_popularity.py --mode genre --month 2026-10 --countries US,GB,AU,DE,JP --dry-run
python3 tools/fetch_apple_popularity.py --mode contains --patterns puzzle,word,logic,offline --month 2026-10 --countries US --dry-run

# Authenticated read-only discovery: top 500 game terms per country/genre may be
# spread over multiple pages. This command caps coverage and reports truncation.
python3 tools/fetch_apple_popularity.py --mode genre --month 2026-10 --countries US,GB,AU,DE,JP --page-size 250 --max-pages 2 --output .apple-results/genre-2026-10.json

# Turn ONLY observed search terms into an auditable CSV; no AI-generated popularity.
python3 tools/discover_ideas.py --snapshot .apple-results/genre-2026-10.json --output .apple-results/opportunity-queue-2026-10.csv
```

The resulting queue is *not* a ranking of viable game opportunities. It records real, country-specific demand signals where Apple exposes data, plus an **unverified** mechanism classification.

## Stage B — Find actionable long-tail niches

For an observed broad term like `word puzzle`, research:
- Specific mechanic: word ladder, acrostic, cryptic crossword, etc.
- Accessibility: large print, offline, no timer.
- Language/country: localized queries and underserved dictionaries.
- Adjacent intent: printable, daily, educational.

Use `--mode contains` or `--mode starts-with` to try to find variations. These filters still operate over Apple's limited search term population; **absence remains unknown**.

Use public [iTunes Search API](https://performance-partners.apple.com/search-api) catalog discovery for competitor *existence*, update cadence, publisher identity and rating counts. **Its API ordering is not the organic App Store search-results page.** The previous [dated catalog snapshot](catalog-snapshot-2026-11-15.md) demonstrates why naïve "low competition" claims are misleading.

## Stage C — Supply and user gap

For the top *measured* terms:
1. On a real device configured to the target storefront, capture the first **10 organic App Store search results**, excluding ads. Record date, search exact string, country, and device metadata.
2. Sample 10–20 recent positive *and* negative reviews across the relevant competitors. Quantify complaints by theme. Do not infer review frequency from a handful of memorable comments.
3. State a single meaningful product wedge, not "cleaner UI": a measurable user improvement that incumbents don't deliver.
4. Separate consumer intent from existing game brand searches. Branded popularity is not generally accessible to another publisher.
5. Log what would make the opportunity fail: low non-branded demand, no organic rank path, impossible content economics, or no observable retention.

Only then apply [the scorecard](README.md); scores are prioritization heuristics, not predicted revenue.

## Stage D — Fastest falsification

A prospect qualifies for a **7-day prototype** only if:
- App Store popularity is measured or a clearly documented alternative demand signal is accepted.
- A genuine, repeatable competitor weakness is observed.
- A distinct product concept can be built and shown quickly.
- A testable organic acquisition path exists.

A prototype should not be equated with a product launch. A successful prototype proves the game works; a live launch measures discovery, conversion and retention.

## What automation can and cannot do

| Evidence source | Machine collection | Claim permitted |
| --- | --- | --- |
| Apple Ads genre popularity | Authorized API | Observed relative interest, country, month, limited coverage |
| Apple iTunes public API | Read-only client | Matched catalog apps and metadata, *not* organic ranking |
| App Store actual search UI | Human or permitted device automation | Dated organic position for specified device/locale |
| User reviews | Reviewed permitted source | Dated complaints with sampling biases |
| App Store Connect own analytics | First-party app account | Measured owned impressions, page conversion, installs |
| Other ASO datasets | Licensed provider | Estimates subject to vendor methodology |
| Brainstormed mechanics | None | Hypotheses, **never market metrics** |

## Decision checkpoint

**Do not start another game merely because the engine scaffolding exists.** The highest-value action is to complete a small, sourced demand/competition sample and let it change which game gets built next. Time-box research; if the API cannot be accessed, use a lower-confidence public-data and small launch experiment instead of pretending precision.
