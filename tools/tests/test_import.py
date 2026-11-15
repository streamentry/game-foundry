"""Synthetic, offline tests for Apple Ads popularity snapshot importer."""
from __future__ import annotations

import sys
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tools"))

from import_apple_popularity import parse_snapshot, API_URL  # noqa: E402
from fetch_apple_popularity import extract_rows  # noqa: E402


def sample():
    return {
        "source": API_URL,
        "retrieved_at_utc": "2026-11-15T01:00:00+00:00",
        "period": "2026-10",
        "countries": ["US", "GB"],
        "rows": [
            {"countryOrRegion": "US", "genre": "GAMES",
             "searchTerm": "Kakuro", "month": "2026-10",
             "rankInGenre": 120, "searchPopularity1to100": 32},
            {"countryOrRegion": "GB", "genre": "GAMES",
             "searchTerm": "Kakuro", "month": "2026-10",
             "rankInGenre": 290, "searchPopularity1to100": 24},
        ],
    }


class ImportTests(unittest.TestCase):
    def test_exact_keyword_matching_and_country_preservation(self):
        rows, errors = parse_snapshot(sample(), {"kakuro": "GF-001"}, "sample.json", "deadbeef")
        self.assertEqual([], errors)
        self.assertEqual(2, len(rows))
        self.assertEqual(["GB", "US"], [r["country"] for r in rows])
        self.assertEqual(["24", "32"], [r["apple_popularity_1to100"] for r in rows])
        self.assertTrue(all(not r["top10_competitor_audit_url"] for r in rows))
        self.assertTrue(all(not r["unmet_need_0to5"] for r in rows))
        self.assertTrue(all("deadbeef" in r["analyst_notes"] for r in rows))

    def test_non_candidates_are_not_added(self):
        payload = sample()
        payload["rows"][0]["searchTerm"] = "brand name not researched"
        rows, errors = parse_snapshot(payload, {"kakuro": "GF-001"}, "sample.json", "abc")
        self.assertEqual([], errors)
        self.assertEqual(1, len(rows))

    def test_invalid_popularity_rejected_not_zeroed(self):
        payload = sample()
        payload["rows"][0]["searchPopularity1to100"] = None
        rows, errors = parse_snapshot(payload, {"kakuro": "GF-001"}, "sample.json", "abc")
        self.assertEqual(1, len(rows))
        self.assertEqual(1, len(errors))
        self.assertIn("popularity", errors[0])

    def test_mismatched_country_fails(self):
        payload = sample()
        payload["rows"][0]["countryOrRegion"] = "JP"
        _, errors = parse_snapshot(payload, {"kakuro": "GF-001"}, "sample.json", "abc")
        self.assertTrue(errors)

    def test_duplicate_period_fails(self):
        payload = sample()
        payload["rows"].append(dict(payload["rows"][0]))
        _, errors = parse_snapshot(payload, {"kakuro": "GF-001"}, "sample.json", "abc")
        self.assertTrue(any("duplicate" in error for error in errors))

    def test_untrusted_source_rejected(self):
        payload = sample()
        payload["source"] = "https://example.org/synth"
        with self.assertRaises(ValueError):
            parse_snapshot(payload, {"kakuro": "GF-001"}, "sample.json", "abc")


class RequestIntegrityTests(unittest.TestCase):
    def test_empty_success_is_distinct_from_error(self):
        self.assertEqual([], extract_rows({"result": {"rows": []}}))
        with self.assertRaises(ValueError):
            extract_rows({"error": {"code": "FORBIDDEN"}})
        with self.assertRaises(ValueError):
            extract_rows({})
        with self.assertRaises(ValueError):
            extract_rows({"result": {"rows": [None]}})


if __name__ == "__main__":
    unittest.main()
