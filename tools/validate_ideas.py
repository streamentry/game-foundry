#!/usr/bin/env python3
"""Validate the hypothesis backlog without confusing missing data with zero demand."""
from __future__ import annotations

import argparse
import csv
import re
import sys
from pathlib import Path
from urllib.parse import urlparse

FIELDS = [
    "id", "family", "primary_search_keyword", "mechanic",
    "differentiation_hypothesis", "engineering_effort_estimate",
    "research_queue", "listing_evidence_level", "example_competitor",
    "example_competitor_url", "observed_on", "apple_popularity_1to100",
    "popularity_country", "popularity_month", "popularity_source_url",
    "research_status",
]
FAMILIES = {"logic", "word", "spatial", "education", "audience"}
STATUSES = {"unvalidated", "measured", "interviewing", "prototype", "reject", "park", "ship"}
EVIDENCE = {"store_listing_observed", "hypothesis_only"}
QUEUES = {"screen_first", "explore"}


def is_https_url(text: str) -> bool:
    parsed = urlparse(text)
    return parsed.scheme == "https" and bool(parsed.netloc)


def validate_file(path: str | Path) -> list[str]:
    errors: list[str] = []
    with open(path, "r", encoding="utf-8", newline="") as handle:
        reader = csv.DictReader(handle)
        if reader.fieldnames != FIELDS:
            return [f"CSV columns differ from expected schema: {reader.fieldnames!r}"]
        rows = list(reader)
    if not rows:
        return ["Candidate list is empty."]
    seen_ids: set[str] = set()
    seen_queries: set[str] = set()
    for index, row in enumerate(rows, start=2):
        where = f"CSV line {index}"
        if None in row or any(value is None for value in row.values()):
            errors.append(f"{where}: malformed or extra columns")
            continue
        identifier = row["id"].strip()
        keyword = row["primary_search_keyword"].strip().casefold()
        if not re.fullmatch(r"GF-\d{3,}", identifier):
            errors.append(f"{where}: invalid id {identifier!r}")
        if identifier in seen_ids:
            errors.append(f"{where}: duplicate id {identifier!r}")
        if not keyword or keyword in seen_queries:
            errors.append(f"{where}: missing or duplicate search keyword {keyword!r}")
        seen_ids.add(identifier)
        seen_queries.add(keyword)
        for key in ("mechanic", "differentiation_hypothesis"):
            if not row[key].strip():
                errors.append(f"{where}: missing {key}")
        if row["family"] not in FAMILIES:
            errors.append(f"{where}: unknown family {row['family']!r}")
        if row["research_queue"] not in QUEUES:
            errors.append(f"{where}: unknown research queue")
        if row["research_status"] not in STATUSES:
            errors.append(f"{where}: unknown research status")
        if row["listing_evidence_level"] not in EVIDENCE:
            errors.append(f"{where}: unknown evidence level")
        listing_url = row["example_competitor_url"].strip()
        if listing_url:
            if not is_https_url(listing_url) or urlparse(listing_url).netloc != "apps.apple.com":
                errors.append(f"{where}: competitor URL must be a public App Store page")
            if not row["example_competitor"].strip():
                errors.append(f"{where}: listing URL without a competitor name")
            if not re.fullmatch(r"\d{4}-\d{2}-\d{2}", row["observed_on"].strip()):
                errors.append(f"{where}: listing lacks a YYYY-MM-DD observation date")
            if row["listing_evidence_level"] != "store_listing_observed":
                errors.append(f"{where}: listing evidence label inconsistent")
        elif row["listing_evidence_level"] == "store_listing_observed":
            errors.append(f"{where}: claims observed listing but has no URL")
        popularity = row["apple_popularity_1to100"].strip()
        provenances = [
            row["popularity_country"].strip(),
            row["popularity_month"].strip(),
            row["popularity_source_url"].strip(),
        ]
        if popularity:
            if not popularity.isdigit() or not 1 <= int(popularity) <= 100:
                errors.append(f"{where}: popularity must be integer 1..100")
            if not all(provenances):
                errors.append(f"{where}: popularity needs country, month and source URL")
            elif (
                not re.fullmatch(r"[A-Z]{2}", provenances[0])
                or not re.fullmatch(r"\d{4}-\d{2}", provenances[1])
                or not is_https_url(provenances[2])
            ):
                errors.append(f"{where}: invalid popularity provenance")
        elif any(provenances):
            errors.append(f"{where}: provenance exists but popularity is blank")
    return errors


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("path", nargs="?", default="docs/ideas/candidates.csv")
    args = parser.parse_args()
    try:
        errors = validate_file(args.path)
    except OSError as exc:
        print(f"ERROR: {exc}", file=sys.stderr)
        return 1
    if errors:
        for item in errors:
            print("ERROR:", item, file=sys.stderr)
        return 1
    with open(args.path, encoding="utf-8", newline="") as handle:
        rows = list(csv.DictReader(handle))
    observed = sum(bool(row["example_competitor_url"]) for row in rows)
    measured = sum(bool(row["apple_popularity_1to100"]) for row in rows)
    print(f"PASS: {len(rows)} candidate hypotheses; {observed} public listing anchors; {measured} measured popularity rows")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
