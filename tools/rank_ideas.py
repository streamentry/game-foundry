#!/usr/bin/env python3
"""Rank ONLY evidence-complete keyword/country/month measurements.

Missing values are unranked, never treated as zero. Scores are human-review
heuristics, not forecasts of installs, organic position or revenue.
"""
from __future__ import annotations

import argparse
import csv
import re
from pathlib import Path
from urllib.parse import urlparse

REQUIRED = [
    "candidate_id", "country", "month", "apple_popularity_1to100",
    "apple_source_url", "top10_competitor_audit_url",
    "competition_weakness_0to5", "review_evidence_url",
    "unmet_need_0to5", "repeat_use_potential_0to5",
    "development_efficiency_0to5", "analyst_notes",
]
NUMBERS = [
    "competition_weakness_0to5", "unmet_need_0to5",
    "repeat_use_potential_0to5", "development_efficiency_0to5",
]
SOURCES = ["apple_source_url", "top10_competitor_audit_url", "review_evidence_url"]


def _valid_source(value: str) -> bool:
    parsed = urlparse(value)
    return parsed.scheme == "https" and bool(parsed.netloc)


def score_measurement(row: dict[str, str]) -> float | None:
    """Return 0–100 score, or None if evidence is missing or invalid."""
    if any(not str(row.get(col, "")).strip() for col in REQUIRED):
        return None
    if not all(_valid_source(row[name]) for name in SOURCES):
        return None
    if not re.fullmatch(r"GF-\d{3,}", row["candidate_id"]):
        return None
    if not re.fullmatch(r"[A-Z]{2}", row["country"]):
        return None
    if not re.fullmatch(r"\d{4}-\d{2}", row["month"]):
        return None
    try:
        popularity = int(row["apple_popularity_1to100"])
        scores = [float(row[name]) for name in NUMBERS]
    except (ValueError, TypeError):
        return None
    if not 1 <= popularity <= 100 or any(not 0 <= v <= 5 for v in scores):
        return None
    # Explicit, revisable research heuristic:
    return round(
        popularity * 0.30
        + scores[0] * 5.0
        + scores[1] * 4.0
        + scores[2] * 3.0
        + scores[3] * 2.0,
        2,
    )


def rank_measurements(path: Path) -> tuple[list[tuple[float, dict[str, str]]], int]:
    with path.open(newline="", encoding="utf-8") as handle:
        reader = csv.DictReader(handle)
        if reader.fieldnames != REQUIRED:
            raise ValueError(f"Measurement schema mismatch: {reader.fieldnames}")
        observations = list(reader)
    scored = []
    for row in observations:
        score = score_measurement(row)
        if score is not None:
            scored.append((score, row))
    scored.sort(key=lambda item: item[0], reverse=True)
    return scored, len(observations) - len(scored)


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--input", type=Path, default=Path("docs/research/measurements.csv"))
    parser.add_argument("--minimum-popularity", type=int, default=20, help="Heuristic screen only")
    args = parser.parse_args()
    try:
        ranked, skipped = rank_measurements(args.input)
    except (OSError, ValueError) as exc:
        parser.error(str(exc))
    if not ranked:
        print("No evidence-complete measurements. Nothing ranked. Add documented Apple API and competitor/review observations first.")
        return 0
    print("Rank  Score  Gate     Candidate  Country  Month")
    for i, (score, row) in enumerate(ranked, start=1):
        popularity = int(row["apple_popularity_1to100"])
        gate = "pass" if popularity >= args.minimum_popularity else "review"
        print(f"{i:>4}  {score:>5.1f}  {gate:<7}  {row['candidate_id']:<9}  {row['country']:<7}  {row['month']}")
    print(f"Unscored (missing/invalid evidence): {skipped}")
    print("These ranks are screening scores, NOT proven search volume, conversion or revenue.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
