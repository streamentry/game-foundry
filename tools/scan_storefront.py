#!/usr/bin/env python3
"""Small, rate-limited App Store *catalog* scan using Apple's public Search API.

Useful for finding competitors and recent app metadata without paid ASO tools.
NOT official App Store organic ranking, keyword demand, download or revenue data.
Defaults to --dry-run. Live mode is read-only and requires explicit --live.
"""
from __future__ import annotations

import argparse
import csv
import datetime as dt
import json
import sys
import time
import urllib.error
import urllib.parse
import urllib.request
from pathlib import Path
from typing import Any

API_URL = "https://itunes.apple.com/search"
DOCS = "https://performance-partners.apple.com/search-api"


def search_url(keyword: str, country: str, limit: int = 20) -> str:
    if not keyword.strip():
        raise ValueError("Search keyword cannot be empty")
    if len(country) != 2 or not country.isalpha():
        raise ValueError("Country must be ISO alpha-2")
    if not 1 <= limit <= 50:
        raise ValueError("Limit must be 1..50 (conservative per-request cap)")
    query = urllib.parse.urlencode({
        "term": keyword.strip(),
        "country": country.lower(),
        "media": "software",
        "entity": "software",
        "limit": limit,
    })
    return API_URL + "?" + query


def candidate_keywords(path: Path, queue: str, max_keywords: int) -> list[tuple[str, str]]:
    with path.open(encoding="utf-8", newline="") as handle:
        reader = csv.DictReader(handle)
        names = set(reader.fieldnames or [])
        required = {"id", "primary_search_keyword", "research_queue"}
        if not required.issubset(names):
            raise ValueError(f"Missing candidate columns: {sorted(required - names)}")
        selected: list[tuple[str, str]] = []
        seen: set[str] = set()
        for row in reader:
            if queue != "all" and row["research_queue"] != queue:
                continue
            term = row["primary_search_keyword"].strip()
            if not term or term.casefold() in seen:
                continue
            seen.add(term.casefold())
            selected.append((row["id"].strip(), term))
            if len(selected) >= max_keywords:
                break
    return selected


def normalize_results(payload: dict[str, Any]) -> list[dict[str, Any]]:
    raw = payload.get("results")
    if not isinstance(raw, list) or not isinstance(payload.get("resultCount"), int):
        raise ValueError("Unexpected Apple public Search API JSON response")
    normalized = []
    for index, app in enumerate(raw, start=1):
        if not isinstance(app, dict):
            continue
        # Do not persist Apple icons, previews, description text or screenshots.
        normalized.append({
            "api_result_position": index,
            "app_id": app.get("trackId"),
            "name": app.get("trackName"),
            "developer": app.get("artistName"),
            "bundle_id": app.get("bundleId"),
            "app_store_url": app.get("trackViewUrl"),
            "genres": app.get("genres") or [],
            "primary_genre": app.get("primaryGenreName"),
            "rating": app.get("averageUserRating"),
            "rating_count": app.get("userRatingCount"),
            "price": app.get("price"),
            "currency": app.get("currency"),
            "release_date": app.get("releaseDate"),
            "updated_date": app.get("currentVersionReleaseDate"),
            "version": app.get("version"),
        })
    return normalized


def get_json(url: str, *, retries: int = 3) -> dict[str, Any]:
    request = urllib.request.Request(url, headers={
        "User-Agent": "GameFoundryResearch/0.1 (read-only, rate-limited)",
        "Accept": "application/json",
    })
    for attempt in range(retries):
        try:
            with urllib.request.urlopen(request, timeout=30) as response:
                payload = json.load(response)
            if not isinstance(payload, dict):
                raise ValueError("API response must be a JSON object")
            return payload
        except urllib.error.HTTPError as error:
            if error.code not in (429, 500, 502, 503) or attempt == retries - 1:
                raise RuntimeError(f"Apple public Search API HTTP {error.code}") from None
            time.sleep(min(5 * (2 ** attempt), 30))
        except urllib.error.URLError as error:
            raise RuntimeError(f"Apple public Search API connection failed: {error.reason}") from None
    raise RuntimeError("Retry budget exceeded")


def run_scan(
    entries: list[tuple[str, str]],
    country: str,
    per_query: int,
    delay: float,
) -> dict[str, Any]:
    observations: list[dict[str, Any]] = []
    for i, (candidate_id, keyword) in enumerate(entries):
        if i:
            time.sleep(delay)
        url = search_url(keyword, country, per_query)
        payload = get_json(url)
        observations.append({
            "candidate_id": candidate_id,
            "keyword": keyword,
            "country": country.upper(),
            "requested_url": url,
            "queried_at_utc": dt.datetime.now(dt.timezone.utc).isoformat(),
            "api_result_count": payload["resultCount"],
            "returned_apps": normalize_results(payload),
        })
        print(f"{candidate_id} {country}: {keyword!r} -> {len(observations[-1]['returned_apps'])} catalog results", file=sys.stderr)
    return {
        "source": DOCS,
        "method": "public iTunes Search API (catalog discovery, not App Store search ranking)",
        "is_official_organic_app_store_rank": False,
        "is_search_volume": False,
        "retrieved_at_utc": dt.datetime.now(dt.timezone.utc).isoformat(),
        "observations": observations,
    }


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--candidates", type=Path, default=Path("docs/ideas/candidates.csv"))
    parser.add_argument("--queue", choices=["screen_first", "explore", "all"], default="screen_first")
    parser.add_argument("--country", default="US")
    parser.add_argument("--max-keywords", type=int, default=8)
    parser.add_argument("--per-query", type=int, default=20)
    parser.add_argument("--delay", type=float, default=3.5, help="Between-request delay; min 3.5 s to respect Apple limits")
    parser.add_argument("--output", type=Path, default=None, help="Local snapshot, preferably .apple-results/*.json")
    mode = parser.add_mutually_exclusive_group()
    mode.add_argument("--dry-run", action="store_true", help="Show requests only (default)")
    mode.add_argument("--live", action="store_true", help="Explicitly make read-only public API requests")
    args = parser.parse_args()

    if not 1 <= args.max_keywords <= 25:
        parser.error("--max-keywords must be in 1..25")
    if not 1 <= args.per_query <= 50:
        parser.error("--per-query must be in 1..50")
    if args.delay < 3.5:
        parser.error("--delay must be at least 3.5 seconds")
    try:
        entries = candidate_keywords(args.candidates, args.queue, args.max_keywords)
        for _, term in entries:
            search_url(term, args.country, args.per_query)
    except (OSError, ValueError) as exc:
        parser.error(str(exc))
    if not entries:
        parser.error("No keyword candidates found")

    if not args.live:
        print(json.dumps({
            "mode": "dry_run",
            "requests_planned": len(entries),
            "country": args.country.upper(),
            "urls": [{"candidate_id": cid, "url": search_url(term, args.country, args.per_query)}
                     for cid, term in entries],
            "caveat": "Catalog search result order is NOT verified App Store organic ranking.",
        }, indent=2))
        return 0

    if args.output is None:
        parser.error("--live requires --output (results are not committed automatically)")
    if args.output.exists():
        parser.error("Refusing to overwrite an existing evidence snapshot")

    try:
        report = run_scan(entries, args.country, args.per_query, args.delay)
    except (RuntimeError, ValueError, KeyError) as exc:
        print(f"ERROR: {exc}. No partial snapshot was written.", file=sys.stderr)
        return 1

    args.output.parent.mkdir(parents=True, exist_ok=True)
    try:
        with args.output.open("x", encoding="utf-8") as handle:
            json.dump(report, handle, indent=2, ensure_ascii=False)
            handle.write("\n")
    except FileExistsError:
        print("ERROR: output filename already exists", file=sys.stderr)
        return 1
    print(f"Saved {len(report['observations'])} keyword searches to {args.output}")
    print("NEVER interpret public search API position as organic App Store rank or search volume.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
