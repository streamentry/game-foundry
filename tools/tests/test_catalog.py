"""Offline checks for the public Apple catalog sampler.

Tests use synthetic payloads; no network request, popularity or ranking is
inferred from them.
"""
from __future__ import annotations

import csv
import sys
import tempfile
import unittest
from pathlib import Path
from unittest.mock import patch
from urllib.parse import parse_qs, urlparse

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tools"))

from scan_storefront import (  # noqa: E402
    search_url, candidate_keywords, normalize_results, run_scan,
)


class CatalogTests(unittest.TestCase):
    def test_url_is_encoded_and_scoped(self) -> None:
        url = search_url("word ladder & clues", "US", 15)
        parsed = urlparse(url)
        self.assertEqual("itunes.apple.com", parsed.netloc)
        query = parse_qs(parsed.query)
        self.assertEqual(["word ladder & clues"], query["term"])
        self.assertEqual(["us"], query["country"])
        self.assertEqual(["software"], query["media"])
        self.assertEqual(["software"], query["entity"])
        self.assertEqual(["15"], query["limit"])

    def test_rejects_invalid_limits(self) -> None:
        for n in (0, 51):
            with self.subTest(limit=n):
                with self.assertRaises(ValueError):
                    search_url("logic", "US", n)

    def test_candidate_queue_filter(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            path = Path(temp) / "candidates.csv"
            with path.open("w", newline="", encoding="utf-8") as handle:
                writer = csv.DictWriter(handle, fieldnames=["id", "primary_search_keyword", "research_queue"])
                writer.writeheader()
                writer.writerow({"id": "GF-001", "primary_search_keyword": "hashi", "research_queue": "screen_first"})
                writer.writerow({"id": "GF-002", "primary_search_keyword": "word ladder", "research_queue": "explore"})
                writer.writerow({"id": "GF-003", "primary_search_keyword": "HASHI", "research_queue": "screen_first"})
            self.assertEqual([("GF-001", "hashi")], candidate_keywords(path, "screen_first", 10))
            self.assertEqual([("GF-002", "word ladder")], candidate_keywords(path, "explore", 10))
            self.assertEqual(2, len(candidate_keywords(path, "all", 10)))

    def test_metadata_only_and_not_organic_rank(self) -> None:
        payload = {
            "resultCount": 1,
            "results": [{
                "trackId": 123, "trackName": "Sample Logic",
                "artistName": "Publisher", "userRatingCount": 12,
                "description": "Copyrighted description we must not persist",
                "screenshotUrls": ["https://example.org/asset.png"],
            }],
        }
        rows = normalize_results(payload)
        self.assertEqual(1, len(rows))
        self.assertEqual("Sample Logic", rows[0]["name"])
        self.assertEqual(1, rows[0]["api_result_position"])
        self.assertNotIn("description", rows[0])
        self.assertNotIn("screenshotUrls", rows[0])

        with patch("scan_storefront.get_json", return_value=payload):
            report = run_scan([("GF-004", "logic grid")], "US", 5, 3.5)
        self.assertIs(report["is_official_organic_app_store_rank"], False)
        self.assertIs(report["is_search_volume"], False)
        self.assertEqual(1, len(report["observations"]))
        self.assertEqual("GF-004", report["observations"][0]["candidate_id"])

    def test_missing_schema_fails_closed(self) -> None:
        with self.assertRaises(ValueError):
            normalize_results({"results": []})
        with self.assertRaises(ValueError):
            normalize_results({"resultCount": 0, "results": {}})


if __name__ == "__main__":
    unittest.main()
