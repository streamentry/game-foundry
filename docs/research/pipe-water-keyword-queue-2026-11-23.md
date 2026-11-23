# Pipe and water puzzle keyword capture queue (2026-11-23)

## Purpose

GF-064 (`pipe connect puzzle`, rotate/tap mechanics) is a hypothesis. Its search demand has never been measured. This queue lists 26 seed queries (22 US English, 4 non-English follow-up rows) for capture. It records no measurements. Every measurement field is `unknown` until captured. Status is `queued_hypothesis`.

Files: [pipe-water-keyword-queue-2026-11-23.csv](pipe-water-keyword-queue-2026-11-23.csv). Columns match [the 30-keyword screen](45plus-30-keyword-screen-2026-11-21.csv).

## How to capture each row

1. **Popularity (where available):** query Apple Ads search-term popularity for the keyword, US storefront, genre GAMES, recent MONTHLY or WEEKLY data. Use `tools/fetch_apple_popularity.py` with a valid account token. Without an account, leave popularity `unknown`. Do not substitute Google volumes or planner bands.
2. **Competition (top 10):** on a physical iPhone configured to the target storefront, record the 10 unsponsored organic results per keyword. Follow [storefront-audit.md](storefront-audit.md) and [the JSON template](storefront-audit-template.json). Store the pack under `docs/research/raw/storefront-audits/`, then run `python3 tools/validate_storefront_audit.py <audit.json>`. Simulator screenshots and the public Search API do not qualify.
3. **Reviews:** sample about 20 recent reviews across those apps, mixing positive and negative. Classify themes such as ads, controls, text size, hints, timers and offline play. Record any sampling limit. Do not cherry-pick.
4. **Non-English rows (F01–F04):** capture only after a native speaker reviews the phrase. Record the market and language separately. Do not pool them with US English results.
5. **Age fit:** leave `unknown` unless the source directly supports adults 45+. Broad keyword volumes and generic audience phrases (T05) are not age evidence.

## Screening heuristic (heuristic only, not a decision rule)

A keyword-country-month observation with Apple popularity of at least 20/100 is a screening heuristic from [the research README](README.md). It is not a guarantee of enough search volume. Test sensitivity to the threshold and to the country. Apply it only to measured rows, never to `unknown`. Guard against brand-dominated results that a new entrant cannot capture.

## Stop rule

- If no US non-branded query reaches the screen with a plausible top-10 position, search is not a viable primary channel for this concept. Then test referral and community distribution before production.
- If Apple popularity cannot be measured (no account access), the stop rule cannot be applied. Record that as `unknown` and do not conclude low demand. Capture the top-10 and review evidence first.
- Stop the mechanic if the top-10 is dominated by incumbents with no credible wedge (see the README "Stop or park" criteria).

## Recording results

After capture, add one row per keyword and country to [measurements.csv](measurements.csv). Use the queue id (P01, T03, F02 and so on) in `analyst_notes`, along with the audit JSON path and review sample. Link the audit only after the validator passes. Scores are produced only after demand, competitor and review evidence all exist. Run `python3 tools/rank_ideas.py --input docs/research/measurements.csv`. Do not edit the queue CSV to add measurements; keep measurements in measurements.csv.

## Scope

This queue prepares capture only. It does not measure demand, approve production, or change the Mosselora or Evergrove decision log. Age-band validation with adults 45+ remains a separate step.
