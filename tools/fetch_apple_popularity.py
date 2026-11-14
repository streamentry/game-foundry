#!/usr/bin/env python3
"""Read-only Apple Ads search popularity research. No live call without credentials.

Never commits API responses, guesses volumes, creates campaigns or buys ads.
Uses Python stdlib only; Apple Ads authentication must be configured separately.
"""
from __future__ import annotations

import argparse
import csv
import datetime as dt
import json
import os
import sys
import time
import urllib.error
import urllib.request
from pathlib import Path

API_URL = "https://api.ads.apple.com/v1/insights/apps/search-term-popularity/query"


def month_window(month: str) -> tuple[str, str]:
    first = dt.date.fromisoformat(month + "-01")
    if first.month == 12:
        following = first.replace(month=1, year=first.year + 1)
    else:
        following = first.replace(month=first.month + 1)
    return first.isoformat(), following.isoformat()


def read_keywords(path: Path) -> list[str]:
    with path.open(newline="", encoding="utf-8") as handle:
        reader = csv.DictReader(handle)
        if "primary_search_keyword" not in (reader.fieldnames or []):
            raise ValueError("Candidate file lacks primary_search_keyword")
        return sorted({row["primary_search_keyword"].strip() for row in reader if row["primary_search_keyword"].strip()})


def make_request(month: str, countries: list[str], keywords: list[str], offset: int, page_size: int) -> dict:
    start, end = month_window(month)
    return {
        "filters": [
            {"field": "countryOrRegion", "operator": "IN", "value": countries},
            {"field": "genre", "operator": "EQUALS", "value": "GAMES"},
            {"field": "searchTerm", "operator": "IN", "value": keywords},
        ],
        "timeRange": {"start": start, "end": end, "granularity": "MONTHLY"},
        "fields": ["rankInGenre", "searchPopularity1to100"],
        "sorting": [{"field": "rankInGenre", "order": "ASC"}],
        "pagination": {"offset": offset, "pageSize": page_size},
    }


def call_api(body: dict, token: str, account_id: str) -> dict:
    request = urllib.request.Request(
        API_URL,
        data=json.dumps(body).encode("utf-8"),
        headers={
            "Authorization": "Bearer " + token,
            "X-AP-Context": "adAccountId=" + account_id,
            "Content-Type": "application/json",
            "Accept": "application/json",
        },
        method="POST",
    )
    for attempt in range(4):
        try:
            with urllib.request.urlopen(request, timeout=45) as response:
                return json.load(response)
        except urllib.error.HTTPError as error:
            if error.code not in (429, 500, 502, 503) or attempt == 3:
                # Don't echo response bodies or headers that could contain account details.
                raise RuntimeError(f"Apple Ads API HTTP {error.code}") from None
            time.sleep(min(2 ** attempt, 8))
        except urllib.error.URLError as error:
            raise RuntimeError(f"Network error contacting Apple Ads API: {error.reason}") from None
    raise RuntimeError("Exhausted retry attempts")


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--month", required=True, help="YYYY-MM, e.g. 2026-10")
    parser.add_argument("--countries", default="US,GB,AU,DE,JP", help="Comma-separated storefront country codes")
    parser.add_argument("--candidates", type=Path, default=Path("docs/ideas/candidates.csv"))
    parser.add_argument("--batch-size", type=int, default=20, help="Maximum exact-match terms per API request (1..100)")
    parser.add_argument("--page-size", type=int, default=1000, help="API page size (1..10000)")
    parser.add_argument("--max-pages", type=int, default=10, help="Maximum pages per keyword batch")
    parser.add_argument("--output", type=Path, default=None, help="Local JSON output; do not commit raw reports")
    parser.add_argument("--dry-run", action="store_true", help="Print a request sample, no token/network required")
    args = parser.parse_args()

    try:
        month_window(args.month)
        keywords = read_keywords(args.candidates)
    except (ValueError, OSError) as error:
        parser.error(str(error))
    if not keywords:
        parser.error("No keyword hypotheses in candidate CSV")
    if not (1 <= args.batch_size <= 100 and 1 <= args.page_size <= 10000 and 1 <= args.max_pages <= 50):
        parser.error("Batch-size, page-size, or max-pages out of range")
    countries = [x.strip().upper() for x in args.countries.split(",") if x.strip()]
    if not countries or any(len(x) != 2 or not x.isalpha() for x in countries):
        parser.error("Countries must be 2-letter codes, comma separated")
    batches = [keywords[i : i + args.batch_size] for i in range(0, len(keywords), args.batch_size)]

    if args.dry_run:
        print(json.dumps({"request_sample": make_request(args.month, countries, batches[0], 0, args.page_size),
                          "batches_planned": len(batches), "live_api_called": False}, indent=2))
        return 0

    token = os.getenv("APPLE_ADS_ACCESS_TOKEN", "").strip()
    account_id = os.getenv("APPLE_ADS_ACCOUNT_ID", "").strip()
    if not token or not account_id:
        parser.error("Set APPLE_ADS_ACCESS_TOKEN and APPLE_ADS_ACCOUNT_ID or use --dry-run")
    if args.output is None:
        parser.error("Pass --output to intentionally choose where to save the raw report")

    rows: list[dict] = []
    for i, batch in enumerate(batches, start=1):
        for page in range(args.max_pages):
            request = make_request(args.month, countries, batch, page * args.page_size, args.page_size)
            try:
                response = call_api(request, token, account_id)
            except RuntimeError as exc:
                print(f"ERROR in batch {i}, page {page+1}: {exc}", file=sys.stderr)
                return 1
            result = response.get("result", {})
            batch_rows = result.get("rows", []) if isinstance(result, dict) else []
            if not isinstance(batch_rows, list):
                print(f"ERROR: unexpected Apple response structure in batch {i}", file=sys.stderr)
                return 1
            rows.extend(batch_rows)
            if len(batch_rows) < args.page_size:
                break

    report = {
        "source": API_URL,
        "retrieved_at_utc": dt.datetime.now(dt.timezone.utc).isoformat(),
        "period": args.month,
        "countries": countries,
        "keyword_hypotheses_queried": len(keywords),
        "note": "Relative popularity only; missing API results are not proof of zero demand.",
        "rows": rows,
    }
    args.output.parent.mkdir(parents=True, exist_ok=True)
    # Refuse inadvertent overwrite of a previous evidence snapshot.
    try:
        with args.output.open("x", encoding="utf-8") as handle:
            json.dump(report, handle, indent=2, ensure_ascii=False)
            handle.write("\n")
    except FileExistsError:
        print(f"ERROR: output exists: {args.output}. Choose a new snapshot filename.", file=sys.stderr)
        return 1
    print(f"Saved {len(rows)} observations for {len(keywords)} candidate keywords: {args.output}")
    print("No inference from missing results; audit rankings and reviews before scoring.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
