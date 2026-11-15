#!/usr/bin/env python3
"""Triage real *observed* Apple Ads search terms without pretending to know market size.

Input: JSON produced by fetch_apple_popularity.py in --mode genre/contains/
starts-with. Output: CSV of observed keyword opportunities for HUMAN review.
No ranking by hypothetical competition is possible from this source.
"""
from __future__ import annotations

import argparse
import csv
import hashlib
import json
import re
import sys
from collections import defaultdict
from pathlib import Path

SOURCE = "https://api.ads.apple.com/v1/insights/apps/search-term-popularity/query"
FIELDS = [
    "country", "month", "observed_search_term", "search_popularity_global_1to100",
    "search_popularity_in_genre_1to100", "rank_in_genre",
    "candidate_family_hypothesis", "research_priority", "snapshot_sha256",
    "source_api", "status",
]
# Category patterns are intentionally broad; matching is a lead, not a verdict.
FAMILIES = {
    "logic": [
        r"\b(puzzle|logic|sudoku|kakuro|futoshiki|hashi|nurikabe|slitherlink|nonogram|shikaku|masyu|tents|star battle|number|cross sum|brain teaser)\b"
    ],
    "word": [r"\b(word|crossword|letter|cryptogram|acrostic|anagram|spelling|vocabulary|riddle)\b"],
    "spatial": [r"\b(jigsaw|maze|hidden object|spot the difference|tangram|tile|block|bridge|pipe)\b"],
    "education": [r"\b(math|geography|history|science|learning|educational)\b"],
}


def family_of(term: str) -> str:
    matches = [family for family, patterns in FAMILIES.items()
               if any(re.search(pattern, term, re.IGNORECASE) for pattern in patterns)]
    return ",".join(matches) if matches else "uncategorized"


def extract_candidates(snapshot: dict, source_hash: str, *, min_popularity: int = 1) -> tuple[list[dict], list[str]]:
    if snapshot.get("source") != SOURCE:
        raise ValueError("Snapshot source is not the expected Apple Ads endpoint")
    if not isinstance(snapshot.get("rows"), list):
        raise ValueError("Expected rows array")
    if not re.fullmatch(r"\d{4}-(0[1-9]|1[0-2])", str(snapshot.get("period", ""))):
        raise ValueError("Expected YYYY-MM period")
    if not 1 <= min_popularity <= 100:
        raise ValueError("Minimum popularity must be 1..100")
    # Never interpret any top-K sample as having found *all* available terms.
    truncated = any(q.get("truncated_to_top_k") for q in snapshot.get("queries", []))
    country_scope = set(snapshot.get("countries", []))
    found: dict[tuple[str, str, str], dict] = {}
    errors = []
    for index, row in enumerate(snapshot["rows"], start=1):
        if not isinstance(row, dict):
            errors.append(f"row {index} is not an object")
            continue
        month, country = str(row.get("month", "")), str(row.get("countryOrRegion", ""))
        term = row.get("searchTerm")
        pop = row.get("searchPopularity1to100")
        in_genre = row.get("searchPopularityInGenre")
        rank = row.get("rankInGenre")
        if month != snapshot["period"] or country not in country_scope or row.get("genre") != "GAMES":
            errors.append(f"row {index} scope mismatch")
            continue
        if not isinstance(term, str) or not term.strip() or type(pop) is not int or not 1 <= pop <= 100:
            errors.append(f"row {index} missing term or popularity")
            continue
        if type(in_genre) is not int or not 1 <= in_genre <= 100 or type(rank) is not int or rank < 1:
            errors.append(f"row {index} missing genre popularity or rank")
            continue
        if pop < min_popularity:
            continue
        key = (country, month, term.casefold().strip())
        if key in found:
            old = found[key]
            if old["search_popularity_global_1to100"] != pop or old["rank_in_genre"] != rank:
                errors.append(f"row {index} conflicting duplicate term: {term}")
            continue
        found[key] = {
            "country": country,
            "month": month,
            "observed_search_term": term.strip(),
            "search_popularity_global_1to100": pop,
            "search_popularity_in_genre_1to100": in_genre,
            "rank_in_genre": rank,
            "candidate_family_hypothesis": family_of(term),
            "research_priority": "human_review" if family_of(term) != "uncategorized" else "explore",
            "snapshot_sha256": source_hash,
            "source_api": SOURCE,
            "status": "search_signal_observed_competition_unverified",
        }
    results = sorted(found.values(), key=lambda r: (
        r["country"], -r["search_popularity_in_genre_1to100"],
        r["rank_in_genre"], r["observed_search_term"].casefold()
    ))
    if truncated:
        errors.append("coverage note: discovery returned a top-K subset, NOT the entire genre")
    return results, errors


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--snapshot", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--minimum-popularity", type=int, default=1)
    args = parser.parse_args()
    if args.snapshot.resolve() == args.output.resolve() or args.output.exists():
        parser.error("Output must be a new file separate from snapshot")
    try:
        raw = args.snapshot.read_bytes()
        snapshot = json.loads(raw)
        rows, notes = extract_candidates(snapshot, hashlib.sha256(raw).hexdigest(),
                                          min_popularity=args.minimum_popularity)
    except (OSError, ValueError, TypeError) as exc:
        print(f"ERROR: {exc}", file=sys.stderr)
        return 1
    critical = [note for note in notes if not note.startswith("coverage note:")]
    if critical:
        for note in critical[:10]:
            print("ERROR:", note, file=sys.stderr)
        return 1
    if not rows:
        print("No observed terms passed filters; no output written.", file=sys.stderr)
        return 1
    args.output.parent.mkdir(parents=True, exist_ok=True)
    try:
        with args.output.open("x", newline="", encoding="utf-8") as handle:
            writer = csv.DictWriter(handle, fieldnames=FIELDS)
            writer.writeheader()
            writer.writerows(rows)
    except FileExistsError:
        print("ERROR: output already exists", file=sys.stderr)
        return 1
    print(f"{len(rows)} measured search-term observations saved to {args.output}")
    for note in notes:
        print("NOTE:", note)
    print("Not a list of viable apps: brand intent, competitors and search ranking still unverified.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
