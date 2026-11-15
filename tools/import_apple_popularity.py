#!/usr/bin/env python3
"""Convert an authenticated Apple Ads popularity snapshot to an unscored worksheet.

Input: JSON written by tools/fetch_apple_popularity.py.
Output: docs/research/measurements.csv-compatible rows. Competition/review scores
are LEFT BLANK. This cannot make a candidate 'validated' by itself.

The input was sourced from a credentialed API call; preserve the original file.
This tool validates structure, not API authenticity.
"""
from __future__ import annotations

import argparse
import csv
import hashlib
import json
import re
import sys
from pathlib import Path

from rank_ideas import REQUIRED

API_URL = "https://api.ads.apple.com/v1/insights/apps/search-term-popularity/query"


def load_ids(path: Path) -> dict[str, str]:
    with path.open(newline="", encoding="utf-8") as handle:
        candidates = list(csv.DictReader(handle))
    result: dict[str, str] = {}
    for row in candidates:
        keyword = row["primary_search_keyword"].strip().casefold()
        if not keyword or keyword in result:
            raise ValueError("Candidate list has duplicate or blank keywords")
        result[keyword] = row["id"]
    return result


def parse_snapshot(snapshot: dict, candidates: dict[str, str], source_name: str, source_hash: str) -> tuple[list[dict[str, str]], list[str]]:
    if snapshot.get("source") != API_URL:
        raise ValueError("Input is not declared as the Apple Ads search popularity endpoint")
    if not re.fullmatch(r"\d{4}-\d{2}", str(snapshot.get("period", ""))):
        raise ValueError("Snapshot period must be YYYY-MM")
    if not isinstance(snapshot.get("rows"), list):
        raise ValueError("Snapshot has no data rows array")
    if not isinstance(snapshot.get("countries"), list) or not snapshot["countries"]:
        raise ValueError("Snapshot has no countries")
    if not snapshot.get("retrieved_at_utc"):
        raise ValueError("Snapshot has no retrieved_at_utc timestamp")

    countries = set(snapshot["countries"])
    period = snapshot["period"]
    matched: list[dict[str, str]] = []
    errors: list[str] = []
    seen: set[tuple[str, str, str]] = set()

    for index, row in enumerate(snapshot["rows"], start=1):
        if not isinstance(row, dict):
            errors.append(f"row {index}: not an object")
            continue
        keyword = str(row.get("searchTerm", "")).strip()
        cid = candidates.get(keyword.casefold())
        if cid is None:
            continue  # The official API can return adjacent matches; retain only exact hypotheses.
        country = str(row.get("countryOrRegion", ""))
        month = str(row.get("month", ""))
        popularity = row.get("searchPopularity1to100")
        if row.get("genre") != "GAMES" or country not in countries or month != period:
            errors.append(f"row {index}: mismatched genre/country/month")
            continue
        if type(popularity) is not int or not 1 <= popularity <= 100:
            errors.append(f"row {index}: missing or invalid Apple popularity 1..100")
            continue
        key = (cid, country, month)
        if key in seen:
            errors.append(f"row {index}: duplicate candidate/country/month")
            continue
        seen.add(key)
        entry = {field: "" for field in REQUIRED}
        entry.update({
            "candidate_id": cid,
            "country": country,
            "month": month,
            "apple_popularity_1to100": str(popularity),
            "apple_source_url": API_URL,
            "analyst_notes": (
                f"Snapshot {source_name}; SHA256={source_hash}; "
                f"retrieved={snapshot['retrieved_at_utc']}; "
                f"rankInGenre={row.get('rankInGenre', 'unknown')}; "
                "competition and reviews NOT audited"
            ),
        })
        matched.append(entry)

    matched.sort(key=lambda x: (x["candidate_id"], x["country"], x["month"]))
    return matched, errors


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--snapshot", type=Path, required=True, help="Read-only Apple Ads JSON snapshot")
    parser.add_argument("--candidates", type=Path, default=Path("docs/ideas/candidates.csv"))
    parser.add_argument("--output", type=Path, required=True, help="New draft CSV file; cannot overwrite")
    args = parser.parse_args()
    if args.snapshot.resolve() == args.output.resolve():
        parser.error("Output cannot overwrite input")
    if args.output.exists():
        parser.error("Refusing to overwrite existing worksheet")
    try:
        raw = args.snapshot.read_bytes()
        snapshot = json.loads(raw)
        ids = load_ids(args.candidates)
        entries, errors = parse_snapshot(
            snapshot, ids, args.snapshot.name, hashlib.sha256(raw).hexdigest()
        )
    except (OSError, ValueError, KeyError, TypeError) as exc:
        print(f"ERROR: {exc}", file=sys.stderr)
        return 1
    if errors:
        for error in errors[:20]:
            print("ERROR:", error, file=sys.stderr)
        return 1
    if not entries:
        print("ERROR: no exact candidate keywords had valid popularity rows; leaving output absent", file=sys.stderr)
        return 1
    args.output.parent.mkdir(parents=True, exist_ok=True)
    try:
        with args.output.open("x", newline="", encoding="utf-8") as handle:
            writer = csv.DictWriter(handle, fieldnames=REQUIRED)
            writer.writeheader()
            writer.writerows(entries)
    except FileExistsError:
        print("ERROR: output already exists; choose another name", file=sys.stderr)
        return 1
    print(f"Wrote {len(entries)} evidence-backed Apple popularity observations to {args.output}")
    print("NOT ranked: competitor and review evidence is intentionally still blank.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
