#!/usr/bin/env python3
"""Read-only Apple Ads search popularity: exact research and source-first discovery.

Authenticated Apple Ads insights are relative search signals, NOT monthly volumes.
Use --dry-run to preview a request without credentials or network access.
Never creates a campaign, buys ads, or publishes a game.
"""
from __future__ import annotations

import argparse
import csv
import datetime as dt
import json
import os
import re
import sys
import time
import urllib.error
import urllib.request
from pathlib import Path
from typing import Callable

API_URL = "https://api.ads.apple.com/v1/insights/apps/search-term-popularity/query"
DOCUMENTATION = "https://developer.apple.com/documentation/apple-ads-platform-api/query-app-search-term-popularity-data"
FIELDS = ["rankInGenre", "searchPopularityInGenre", "searchPopularity1to100", "searchPopularity1to5"]
MODES = ("exact", "contains", "starts-with", "genre")


def month_window(month: str) -> tuple[str, str]:
    """The monthly API window uses the first day of the following month as end."""
    if not re.fullmatch(r"\d{4}-(0[1-9]|1[0-2])", month):
        raise ValueError("Month must have format YYYY-MM")
    first = dt.date.fromisoformat(month + "-01")
    following = first.replace(month=1, year=first.year + 1) if first.month == 12 else first.replace(month=first.month + 1)
    return first.isoformat(), following.isoformat()


def read_keywords(path: Path) -> list[str]:
    with path.open("r", encoding="utf-8", newline="") as handle:
        reader = csv.DictReader(handle)
        if "primary_search_keyword" not in (reader.fieldnames or []):
            raise ValueError("CSV lacks primary_search_keyword column")
        return sorted({row["primary_search_keyword"].strip() for row in reader if row["primary_search_keyword"].strip()}, key=str.casefold)


def query_specs(mode: str, countries: list[str], terms: list[str], batch_size: int) -> list[dict]:
    """Always scope each query to ONE country to prevent one market masking another."""
    if mode not in MODES:
        raise ValueError("Unsupported mode")
    if not countries or any(not re.fullmatch(r"[A-Z]{2}", country) for country in countries):
        raise ValueError("Countries must be 2-letter uppercase ISO codes")
    if not 1 <= batch_size <= 100:
        raise ValueError("Batch size must be 1..100")
    if mode != "genre" and not terms:
        raise ValueError(f"Mode {mode!r} requires at least one search term/pattern")
    if mode == "exact":
        batches = [terms[i:i+batch_size] for i in range(0, len(terms), batch_size)]
    elif mode == "genre":
        batches = [[]]
    else:
        batches = [[term] for term in terms]
    return [{"country": country, "mode": mode, "terms": batch} for country in countries for batch in batches]


def make_request(month: str, spec: dict, offset: int, page_size: int) -> dict:
    start, end = month_window(month)
    country = spec["country"]
    mode = spec["mode"]
    filters: list[dict] = [
        {"field": "countryOrRegion", "operator": "EQUALS", "value": country},
        {"field": "genre", "operator": "EQUALS", "value": "GAMES"},
    ]
    if mode == "exact":
        filters.append({"field": "searchTerm", "operator": "IN", "value": spec["terms"]})
    elif mode in ("contains", "starts-with"):
        filters.append({
            "field": "searchTerm",
            "operator": "CONTAINS" if mode == "contains" else "STARTS_WITH",
            "value": spec["terms"][0],
        })
    return {
        "filters": filters,
        "timeRange": {"start": start, "end": end, "granularity": "MONTHLY"},
        "fields": FIELDS,
        "sorting": [{"field": "rankInGenre", "order": "ASC"}],
        "pagination": {"offset": offset, "pageSize": page_size},
    }


def extract_rows(response: dict) -> list[dict]:
    """Fail closed if an API error envelope masquerades as an empty result set."""
    if not isinstance(response, dict) or response.get("error") is not None:
        raise ValueError("Apple Ads response is not a success object")
    result = response.get("result")
    if not isinstance(result, dict) or not isinstance(result.get("rows"), list):
        raise ValueError("Expected Apple Ads result.rows array")
    if not all(isinstance(row, dict) for row in result["rows"]):
        raise ValueError("Apple Ads result.rows contains a non-object")
    return result["rows"]


def validate_rows(rows: list[dict], spec: dict, month: str) -> None:
    """Never silently store wrong-country, wrong-period, or incomplete metrics."""
    for i, row in enumerate(rows, start=1):
        if row.get("month") != month or row.get("countryOrRegion") != spec["country"] or row.get("genre") != "GAMES":
            raise ValueError(f"Row {i}: wrong month, country or genre")
        popularity = row.get("searchPopularity1to100")
        rank = row.get("rankInGenre")
        if not isinstance(row.get("searchTerm"), str) or not row["searchTerm"].strip():
            raise ValueError(f"Row {i}: missing searchTerm")
        if type(popularity) is not int or not 1 <= popularity <= 100:
            raise ValueError(f"Row {i}: missing/invalid 1..100 popularity")
        if type(rank) is not int or rank < 1:
            raise ValueError(f"Row {i}: invalid rankInGenre")
        if spec["mode"] == "exact" and row["searchTerm"].casefold() not in {x.casefold() for x in spec["terms"]}:
            raise ValueError(f"Row {i}: term not in exact requested batch")


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
                # Never echo raw HTTP payloads or headers (may contain account data).
                raise RuntimeError(f"Apple Ads API HTTP {error.code}") from None
            retry_after = error.headers.get("Retry-After", "")
            try:
                pause = max(float(retry_after), 0)
            except (ValueError, TypeError):
                pause = 2 ** attempt
            time.sleep(min(max(pause, 1.0), 30.0))
        except urllib.error.URLError:
            raise RuntimeError("Network failure contacting Apple Ads API") from None
    raise RuntimeError("Apple Ads API retry budget exceeded")


def collect(
    month: str,
    specs: list[dict],
    page_size: int,
    max_pages: int,
    fetch: Callable[[dict], dict],
    pause: float = 0.0,
) -> dict:
    """Collect all exact matches or a *clearly marked* top-K discovery sample."""
    observations: list[dict] = []
    coverage: list[dict] = []
    for index, spec in enumerate(specs):
        if index and pause:
            time.sleep(pause)
        batch_rows: list[dict] = []
        truncated = False
        for page in range(max_pages):
            if page and pause:
                time.sleep(pause)
            offset = page * page_size
            request = make_request(month, spec, offset, page_size)
            response = fetch(request)
            rows = extract_rows(response)
            validate_rows(rows, spec, month)
            batch_rows.extend(rows)
            pagination = response.get("pagination")
            total = pagination.get("totalCount") if isinstance(pagination, dict) else None
            if total is not None and (type(total) is not int or total < 0):
                raise ValueError("Invalid Apple Ads pagination.totalCount")
            complete = (total is not None and offset + len(rows) >= total) or len(rows) < page_size
            if complete:
                break
            if page == max_pages - 1:
                if spec["mode"] == "exact":
                    raise ValueError("Exact keyword query exceeded --max-pages; refusing incomplete snapshot")
                truncated = True
        observations.extend(batch_rows)
        coverage.append({
            "country": spec["country"],
            "mode": spec["mode"],
            "patterns_or_terms": spec["terms"],
            "rows_returned": len(batch_rows),
            "truncated_to_top_k": truncated,
        })
    return {
        "source": API_URL,
        "documentation": DOCUMENTATION,
        "retrieved_at_utc": dt.datetime.now(dt.timezone.utc).isoformat(),
        "period": month,
        "countries": sorted({spec["country"] for spec in specs}),
        "modes": sorted({spec["mode"] for spec in specs}),
        "queries": coverage,
        "rows": observations,
        "meaning": "Apple relative popularity 1..100 (not monthly searches/downloads); per-genre rank; truncated_to_top_k is explicitly partial coverage",
    }


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--month", required=True, help="Complete UTC month, e.g. 2026-10")
    parser.add_argument("--countries", default="US,GB,AU,DE,JP")
    parser.add_argument("--mode", choices=MODES, default="exact",
                        help="exact: measure ideas; contains/starts-with: discover long-tail; genre: top terms")
    parser.add_argument("--patterns", default="", help="Comma-separated discovery terms, e.g. puzzle,logic,word")
    parser.add_argument("--candidates", type=Path, default=Path("docs/ideas/candidates.csv"))
    parser.add_argument("--batch-size", type=int, default=20)
    parser.add_argument("--page-size", type=int, default=200)
    parser.add_argument("--max-pages", type=int, default=3)
    parser.add_argument("--delay", type=float, default=1.0, help="Seconds between read-only API requests")
    parser.add_argument("--output", type=Path, help="New local JSON snapshot (never overwritten)")
    parser.add_argument("--dry-run", action="store_true", help="Preview requests without credentials/network")
    args = parser.parse_args()
    try:
        month_window(args.month)
        if args.page_size < 1 or args.page_size > 10000 or not 1 <= args.max_pages <= 100:
            raise ValueError("page-size must be 1..10000 and max-pages must be 1..100")
        if args.delay < 0:
            raise ValueError("delay must be nonnegative")
        countries = [x.strip().upper() for x in args.countries.split(",") if x.strip()]
        if len(set(countries)) != len(countries):
            raise ValueError("Countries must not repeat")
        if args.mode == "exact":
            if args.patterns:
                raise ValueError("Do not use --patterns in exact mode; edit candidates CSV")
            terms = read_keywords(args.candidates)
        elif args.mode == "genre":
            if args.patterns:
                raise ValueError("Genre-wide discovery does not accept --patterns")
            terms = []
        else:
            terms = [x.strip() for x in args.patterns.split(",") if x.strip()]
            if not terms:
                raise ValueError("--patterns is required for contains/starts-with discovery")
        if len(set(x.casefold() for x in terms)) != len(terms):
            raise ValueError("Duplicate terms/patterns")
        specs = query_specs(args.mode, countries, terms, args.batch_size)
    except (OSError, ValueError) as error:
        parser.error(str(error))

    if args.dry_run:
        print(json.dumps({
            "live_api_called": False,
            "requests_planned": len(specs),
            "sample_requests": [make_request(args.month, spec, 0, args.page_size) for spec in specs[:2]],
            "warning": "Relative popularity only. Discovery may be deliberately top-K truncated.",
        }, indent=2))
        return 0

    token = os.getenv("APPLE_ADS_ACCESS_TOKEN", "").strip()
    account_id = os.getenv("APPLE_ADS_ACCOUNT_ID", "").strip()
    if not token or not account_id:
        parser.error("Provide APPLE_ADS_ACCESS_TOKEN and APPLE_ADS_ACCOUNT_ID as environment variables")
    if args.output is None:
        parser.error("--output is required for an authenticated call")
    if args.output.exists():
        parser.error("Refusing to overwrite an existing snapshot")

    try:
        report = collect(args.month, specs, args.page_size, args.max_pages,
                         lambda body: call_api(body, token, account_id), args.delay)
    except (RuntimeError, ValueError) as error:
        print(f"ERROR: {error}. No snapshot written.", file=sys.stderr)
        return 1

    args.output.parent.mkdir(parents=True, exist_ok=True)
    try:
        with args.output.open("x", encoding="utf-8") as handle:
            json.dump(report, handle, ensure_ascii=False, indent=2)
            handle.write("\n")
    except FileExistsError:
        print("ERROR: output file already exists", file=sys.stderr)
        return 1

    truncated = sum(q["truncated_to_top_k"] for q in report["queries"])
    print(f"Saved {len(report['rows'])} rows for {len(specs)} queries to {args.output}")
    if truncated:
        print(f"WARNING: {truncated} discovery queries were truncated to top-K; unreturned terms are UNKNOWN")
    print("No campaigns created or money spent. Keep raw evidence snapshots private.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
