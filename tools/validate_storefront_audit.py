#!/usr/bin/env python3
"""Check structure and local evidence files for a manual App Store audit.

This validates completeness, not the truth of a claimed organic ranking or review.
"""
from __future__ import annotations

import argparse
import json
import re
import sys
from datetime import datetime
from pathlib import Path
from urllib.parse import urlparse

COUNTRY = re.compile(r"[A-Z]{2}")
CANDIDATE = re.compile(r"GF-\d{3,}")
DATE = re.compile(r"\d{4}-\d{2}-\d{2}")
RATING_COUNT = re.compile(r"\d+")
NOT_DISPLAYED = "not_displayed"
RESULT_FIELDS = (
    "name", "publisher", "app_id", "listing_url", "rating_count",
    "average_rating", "price", "currency", "last_updated",
    "aso_messaging", "screenshot",
)


def _text(value: object) -> bool:
    return isinstance(value, str) and bool(value.strip())


def _https_url(value: object) -> bool:
    if not _text(value):
        return False
    parsed = urlparse(value.strip())
    return parsed.scheme == "https" and bool(parsed.netloc)


def _valid_date(value: object) -> bool:
    if not isinstance(value, str) or not DATE.fullmatch(value):
        return False
    try:
        datetime.strptime(value, "%Y-%m-%d")
    except ValueError:
        return False
    return True


def _valid_capture_time(value: object) -> bool:
    if not _text(value):
        return False
    try:
        captured = datetime.fromisoformat(value.strip().replace("Z", "+00:00"))
    except ValueError:
        return False
    return captured.tzinfo is not None and captured.utcoffset() is not None


def _evidence_file(value: object, audit_dir: Path) -> bool:
    if not _text(value):
        return False
    path = Path(value.strip())
    if path.is_absolute():
        return False
    try:
        resolved = (audit_dir / path).resolve(strict=True)
        resolved.relative_to(audit_dir.resolve())
    except (OSError, ValueError):
        return False
    return resolved.is_file()


def _check_result(row: object, index: int, audit_dir: Path, errors: list[str]) -> None:
    where = f"results[{index}]"
    if not isinstance(row, dict):
        errors.append(f"{where}: expected an object")
        return
    rank = row.get("organic_rank")
    if not isinstance(rank, int) or isinstance(rank, bool) or not 1 <= rank <= 10:
        errors.append(f"{where}: organic_rank must be an integer from 1 to 10")
    if row.get("is_sponsored") is not False:
        errors.append(f"{where}: sponsored results are not accepted as organic results")
    for field in RESULT_FIELDS:
        value = row.get(field)
        numeric_metadata = (
            (field == "rating_count" and isinstance(value, int) and not isinstance(value, bool))
            or (field == "average_rating" and isinstance(value, (int, float)) and not isinstance(value, bool))
        )
        if not _text(value) and not numeric_metadata:
            errors.append(f"{where}: missing {field}")
    app_id = row.get("app_id")
    if _text(app_id) and not app_id.strip().isdigit():
        errors.append(f"{where}: app_id must contain digits only")
    if _text(row.get("listing_url")):
        parsed = urlparse(row["listing_url"].strip())
        if not _https_url(row["listing_url"]) or parsed.netloc.lower() != "apps.apple.com":
            errors.append(f"{where}: listing_url must be an HTTPS App Store listing")
    rating_count = row.get("rating_count")
    if isinstance(rating_count, int) and not isinstance(rating_count, bool):
        if rating_count < 0:
            errors.append(f"{where}: rating_count cannot be negative")
    elif isinstance(rating_count, float):
        errors.append(f"{where}: rating_count must be an integer")
    elif _text(rating_count) and rating_count.strip() != NOT_DISPLAYED:
        if not RATING_COUNT.fullmatch(rating_count.strip()):
            errors.append(f"{where}: rating_count must be digits or '{NOT_DISPLAYED}'")
    average = row.get("average_rating")
    if isinstance(average, (int, float)) and not isinstance(average, bool):
        if not 0 <= average <= 5:
            errors.append(f"{where}: average_rating must be 0..5")
    elif _text(average) and average.strip() != NOT_DISPLAYED:
        try:
            average_value = float(average)
        except (TypeError, ValueError):
            errors.append(f"{where}: average_rating must be 0..5 or '{NOT_DISPLAYED}'")
        else:
            if not 0 <= average_value <= 5:
                errors.append(f"{where}: average_rating must be 0..5")
    updated = row.get("last_updated")
    if _text(updated) and updated.strip() != NOT_DISPLAYED and not _valid_date(updated):
        errors.append(f"{where}: last_updated must be YYYY-MM-DD or '{NOT_DISPLAYED}'")
    if _text(row.get("screenshot")) and not _evidence_file(row["screenshot"], audit_dir):
        errors.append(f"{where}: screenshot must name an existing local file inside the audit folder")


def validate_audit(path: Path) -> list[str]:
    errors: list[str] = []
    try:
        audit = json.loads(path.read_text(encoding="utf-8"))
    except OSError as exc:
        return [f"cannot read audit file: {exc}"]
    except json.JSONDecodeError as exc:
        return [f"invalid JSON: {exc}"]
    if not isinstance(audit, dict):
        return ["audit root must be a JSON object"]
    audit_dir = path.resolve().parent

    schema_version = audit.get("schema_version")
    if not isinstance(schema_version, int) or isinstance(schema_version, bool) or schema_version != 1:
        errors.append("schema_version must be 1")
    candidate_id = audit.get("candidate_id")
    if not _text(candidate_id) or not CANDIDATE.fullmatch(candidate_id.strip()):
        errors.append("candidate_id must match GF-###")
    if not _text(audit.get("keyword")):
        errors.append("keyword is required")
    if audit.get("query_classification") != "non_branded":
        errors.append("query_classification must be 'non_branded'")
    country = audit.get("country")
    if not _text(country) or not COUNTRY.fullmatch(country.strip()):
        errors.append("country must be an uppercase two-letter code")
        country = ""
    if not _valid_capture_time(audit.get("captured_at")):
        errors.append("captured_at must be an ISO-8601 timestamp with a timezone")

    device = audit.get("device")
    if not isinstance(device, dict):
        errors.append("device must be an object")
    else:
        if device.get("kind") not in ("iphone", "ipad"):
            errors.append("device.kind must identify a physical iPhone or iPad; simulators are not accepted")
        for field in ("model", "os_version"):
            if not _text(device.get(field)):
                errors.append(f"device.{field} is required")
        if device.get("storefront_country") != country:
            errors.append("device storefront_country must match country")

    if not _evidence_file(audit.get("search_page_screenshot"), audit_dir):
        errors.append("search_page_screenshot must name an existing local screenshot inside the audit folder")

    results = audit.get("results")
    if not isinstance(results, list):
        errors.append("results must be an array of 10 organic listings")
        results = []
    if len(results) != 10:
        errors.append(f"results must contain exactly 10 listings; found {len(results)}")
    seen_ranks: set[int] = set()
    seen_app_ids: set[str] = set()
    for index, row in enumerate(results):
        _check_result(row, index, audit_dir, errors)
        if isinstance(row, dict):
            rank = row.get("organic_rank")
            if isinstance(rank, int) and not isinstance(rank, bool):
                if rank in seen_ranks:
                    errors.append(f"results: duplicate organic rank {rank}")
                seen_ranks.add(rank)
            app_id = row.get("app_id")
            if _text(app_id):
                app_id = app_id.strip()
                if app_id in seen_app_ids:
                    errors.append(f"results: duplicate app_id {app_id}")
                seen_app_ids.add(app_id)
    if seen_ranks and seen_ranks != set(range(1, 11)):
        errors.append("results ranks must cover every organic position from 1 through 10")

    reviews = audit.get("reviews")
    if not isinstance(reviews, list):
        errors.append("reviews must be an array")
        reviews = []
    limitation = audit.get("review_sampling_limitation")
    sentiments: set[str] = set()
    seen_reviews: set[tuple[str, str, int, str]] = set()
    for index, review in enumerate(reviews):
        where = f"reviews[{index}]"
        if not isinstance(review, dict):
            errors.append(f"{where}: expected an object")
            continue
        app_id = review.get("app_id")
        if not _text(app_id) or app_id.strip() not in seen_app_ids:
            errors.append(f"{where}: app_id must match one of the 10 audited listings")
        sentiment = review.get("sentiment")
        if sentiment not in ("positive", "negative"):
            errors.append(f"{where}: sentiment must be positive or negative")
        else:
            sentiments.add(sentiment)
        rating = review.get("rating")
        if not isinstance(rating, int) or isinstance(rating, bool) or not 1 <= rating <= 5:
            errors.append(f"{where}: rating must be an integer from 1 to 5")
        elif sentiment == "positive" and rating < 4:
            errors.append(f"{where}: positive sentiment conflicts with rating {rating}")
        elif sentiment == "negative" and rating > 2:
            errors.append(f"{where}: negative sentiment conflicts with rating {rating}")
        if not _valid_date(review.get("review_date")):
            errors.append(f"{where}: review_date must be YYYY-MM-DD")
        source_url = review.get("source_url")
        if not _https_url(source_url) or urlparse(source_url.strip()).netloc.lower() != "apps.apple.com":
            errors.append(f"{where}: source_url must be an HTTPS App Store page")
        if not _text(review.get("theme")):
            errors.append(f"{where}: theme is required")
        if not _text(review.get("paraphrase")):
            errors.append(f"{where}: paraphrase the concrete complaint or praise")
        elif _text(app_id) and _valid_date(review.get("review_date")) and isinstance(rating, int) and not isinstance(rating, bool):
            key = (app_id.strip(), review["review_date"], rating, review["paraphrase"].strip().casefold())
            if key in seen_reviews:
                errors.append(f"{where}: duplicate review sample")
            seen_reviews.add(key)

    if len(reviews) < 20 or sentiments != {"positive", "negative"}:
        if not _text(limitation):
            errors.append("review_sampling_limitation is required when fewer than 20 mixed-sentiment reviews are available")

    synthesis = audit.get("synthesis")
    if not isinstance(synthesis, dict):
        errors.append("synthesis must be an object")
    else:
        judgment = synthesis.get("judgment")
        if judgment not in ("wedge", "no_wedge"):
            errors.append("synthesis.judgment must be 'wedge' or 'no_wedge'")
        if not _text(synthesis.get("falsifier")):
            errors.append("synthesis.falsifier must state what evidence would reverse the judgment")
        if judgment == "wedge":
            for field in ("differentiator", "acquisition_path"):
                if not _text(synthesis.get(field)):
                    errors.append(f"synthesis.{field} is required for a wedge judgment")
        if judgment == "no_wedge" and not _text(synthesis.get("no_wedge_reason")):
            errors.append("synthesis.no_wedge_reason is required for a no-wedge judgment")

    return errors


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("audit", type=Path, help="Completed audit JSON file")
    args = parser.parse_args()
    errors = validate_audit(args.audit)
    if errors:
        max_errors = 30
        for error in errors[:max_errors]:
            print(f"ERROR: {error}", file=sys.stderr)
        if len(errors) > max_errors:
            print(f"ERROR: {len(errors) - max_errors} additional errors omitted", file=sys.stderr)
        return 1
    print("PASS: audit structure, 10 organic listings, local screenshots, review sample or limitation, and synthesis are complete")
    print("This validates structure only; it cannot verify the storefront, device, screenshots, or analyst claims.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
