# Storefront audit pack

Use one audit pack for one measured keyword, country, and capture date. It records the actual App Store search page on a physical iPhone or iPad. Public Search API results and simulator screenshots do not qualify as organic rank evidence.

## Capture

1. Copy `storefront-audit-template.json` into the ignored local folder `docs/research/raw/storefront-audits/GF-001-US-2026-11-15/audit.json`.
2. Record the exact query, storefront country, local capture time with timezone, physical device model, iOS version, and whether the term is non-branded.
3. Capture the search page and each of the 10 organic listings. Save image files beside `audit.json`; use relative paths in `search_page_screenshot` and each result's `screenshot` field.
4. Record listing metadata as seen. Use the literal `not_displayed` for a rating count, average rating, or update date that is not shown. Never turn missing metadata into zero.
5. Sample about 20 recent positive and negative reviews across those apps when available. Record rating, date, source link, theme, and a short paraphrase. If fewer than 20 reviews or both sentiments are unavailable, state why in `review_sampling_limitation`.
6. Choose `wedge` only with a visible player benefit and a plausible acquisition path. Otherwise record `no_wedge` and the evidence that falsified the opportunity. State what finding would reverse the decision.

Each result row needs its rank, `is_sponsored: false`, app name and publisher, numeric App Store ID, listing URL, rating count, average rating, price, currency, last update date, ASO messaging, and screenshot path. Rating count may be a non-negative integer or `not_displayed`; average rating may be 0–5 or `not_displayed`; last update may be `YYYY-MM-DD` or `not_displayed`.

Each review row needs `app_id`, `sentiment` (`positive` or `negative`), `rating` (1–5), `review_date`, `source_url`, `theme`, and a short `paraphrase`. The app ID must match one of the ten captured results.

Run the local structural check:

```sh
python3 tools/validate_storefront_audit.py docs/research/raw/storefront-audits/GF-001-US-2026-11-15/audit.json
```

Only after it passes, add authorized HTTPS links to the audit and review evidence in the matching row of `measurements.csv`, then run the ranker. The JSON and screenshots stay in the ignored local evidence folder unless you deliberately move them to an approved evidence store. The validator checks required fields, ranks, sources, review sampling notes, local screenshots, and wedge/no-wedge reasoning. It cannot verify the device, screenshots, organic order, review content, or conclusions; a human must compare the pack with the captured storefront.
