# Research method — topic selection like a PhD

**Research question:** Which niche game search needs have *measurable App Store demand* and a *genuinely underserved user experience*, where a small original product can reach players organically?

## Evidence labels (never conflate)

1. **Observed listing:** a publicly accessible App Store product exists. Not evidence of demand, keyword ranking, profitability or market weakness.
2. **Measured popularity:** Apple Ads `searchPopularity1to100` for a particular country, genre and week/month, with method and date recorded. Relative signal, **not downloads or absolute search counts**.
3. **Observed competition:** search the query in the **target App Store storefront**. Log the top 10 organic results, relevant app identity, ratings (with snapshot date), pricing, update cadence and review sample. One example listing does not qualify.
4. **Observed unmet need:** repeatable issue reported in dated reviews, accompanied by a proposed solution; sample negative and positive reviews to avoid cherry-picking.
5. **Validated product/distribution:** installs attributed to organic search and cohort retention, actual devices, and a differentiated product—not opinion or ratings.

## Data acquisition, 7-day batch

0. To cheaply collect reproducible *competitor catalog* evidence (without Apple Ads credentials):

   ```bash
   python3 tools/scan_storefront.py --country US --max-keywords 5 --dry-run
   python3 tools/scan_storefront.py --country US --max-keywords 5 --live --output .apple-results/us-5-queries.json
   ```

   This uses [Apple's public Search API](https://performance-partners.apple.com/search-api), throttles requests and writes only local snapshots. **The returned order is not official organic App Store ranking and the count is not keyword volume.** See [the dated competitor snapshot](catalog-snapshot-2026-11-15.md).

1. Start with the [candidate list](../ideas/candidates.csv). Deduplicate keywords per locale; cluster synonyms rather than build multiple clones.
2. Query the **official Apple Ads Platform API** `POST /v1/insights/apps/search-term-popularity/query` with `genre=GAMES`, `countryOrRegion` (US, GB, AU, DE, JP, VN), and recent `MONTHLY` or `WEEKLY_SUN_SAT` data. Requires account authentication and permissions. Follow the [official request format](https://developer.apple.com/documentation/apple-ads-platform-api/query-app-search-term-popularity-data); see [example query](apple-api-example.json).
3. Use the read-only helper (dry-run needs no account):

   ```bash
   python3 tools/fetch_apple_popularity.py --month 2026-10 --dry-run
   # With a valid Apple Ads OAuth token and App Store ad-account access:
   export APPLE_ADS_ACCESS_TOKEN="REPLACE_WITH_YOUR_TOKEN"
   export APPLE_ADS_ACCOUNT_ID="REPLACE_WITH_AD_ACCOUNT_ID"
   python3 tools/fetch_apple_popularity.py --month 2026-10 --countries US,GB,AU,DE,JP --output .apple-results/apple-2026-10.json
   ```

   The tool queries **read-only search popularity**. It does not create campaigns, spend money, modify CSV priorities or commit responses. OAuth onboarding and ad-account permissions are required; see [Apple API access overview](https://developer.apple.com/documentation/apple-ads-platform-api/access-overview). Review each response against the official API schema; retain evidence snapshots securely.
4. Convert the official authenticated JSON response into a structured **unscored** research worksheet without typing numbers by hand:

   ```bash
   python3 tools/import_apple_popularity.py \\
     --snapshot .apple-results/apple-2026-10.json \\
     --output .apple-results/measurements-draft-2026-10.csv
   ```

   The importer checks country, month, genre, numeric range, duplicate observations and exact candidate keyword matches. It records the source snapshot's SHA-256 hash for traceability. Competitor-audit and review columns remain **blank**, so the ranker refuses to score these rows until evidence is added. This prevents an Apple popularity observation from being mistaken for a viable game opportunity.
5. Export source-backed observations to a private research notes store or [measurements.csv](measurements.csv) with data provenance. This repository contains **no credential**, token or invented API response. Relative popularity values are not absolute volumes; missing results are not proofs of zero demand.
6. Inspect **10 organic results per keyword/country**, not only an app name or a browser result. Record dates and links in a new per-candidate evidence note.
7. Sample approximately 20 recent reviews across relevant competitors (when available). Classify issues such as intrusive ads, broken controls, tiny text, stale content, unreliable hints, paywall friction, retention or offline expectations. Don't cherry-pick one complaint.
8. Score only after recording demand, competitor and review sources. Run `python3 tools/rank_ideas.py --input docs/research/measurements.csv`.
9. Choose at most three research hypotheses to prototype. For each, define a 1-week proof-of-play and an organic launch experiment. Record an explicit continue/stop decision.

## Scoring (heuristic, not predictive)

When all evidence is present: **Demand 30 + competition weakness 25 + unmet need 20 + repeat-use potential 15 + development efficiency 10** (0–100 scale). Demand uses Apple's 1–100 index; other dimensions require analyst scores from 0 to 5 and supporting notes/sources. A threshold of **20/100 popularity** is an initial **screening heuristic**—not a guarantee of enough search volume. Test sensitivity to the threshold and country.

The ranker **refuses to score** unmeasured rows. Do not rank missing data as low demand. Score a *keyword–country–month* observation, not a game title globally. Guard against dominant brand-name searches that a new entrant cannot capture.

## Validation decisions

- **Continue research:** reliable non-branded keyword signal, plausible search position, repeated addressable unmet need.
- **Prototype:** new experience visibly different from top competitors, reproducible game mechanics, original content rights, no specialist backend.
- **Stop or park:** branded-only demand, negligible measured demand, compelling incumbents with no credible wedge, level-generation quality failures or no feasible organic distribution.
- **Commercial test:** one original app, small launch, measured organic impressions/install conversion and retention by cohort. Set numeric success thresholds **from observed baseline**, not fabricated industry averages.

## Primary references

- Apple Ads [Insights API](https://developer.apple.com/documentation/apple-ads-platform-api/insights-endpoints) and [Search Term Popularity Query](https://developer.apple.com/documentation/apple-ads-platform-api/query-app-search-term-popularity-data).
- [Apple App Review Guidelines](https://developer.apple.com/app-store/review/guidelines/) (especially 4.3 spam/copycat submissions).
- [Apple App Store product pages](market-scan-2026-11-15.md) used as dated **competitive existence** snapshots, not keyword volume.
