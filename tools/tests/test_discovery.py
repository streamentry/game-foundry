"""Offline contract tests for genre-first Apple Ads research (synthetic fixtures only)."""
from __future__ import annotations
import sys
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tools"))

from fetch_apple_popularity import (month_window, query_specs, make_request,
                                   validate_rows, collect)
from discover_ideas import extract_candidates, SOURCE


def row(term="word ladder", pop=32, country="US", rank=11):
    return {
        "month": "2026-10", "countryOrRegion": country, "genre": "GAMES",
        "searchTerm": term, "rankInGenre": rank,
        "searchPopularity1to100": pop,
        "searchPopularityInGenre": 70,
        "searchPopularity1to5": 3,
    }


class DiscoveryTests(unittest.TestCase):
    def test_date_windows_and_invalid_month(self):
        self.assertEqual(("2026-10-01", "2026-11-01"), month_window("2026-10"))
        self.assertEqual(("2026-12-01", "2027-01-01"), month_window("2026-12"))
        with self.assertRaises(ValueError):
            month_window("2026-13")

    def test_explicit_country_partitioning_and_query_shape(self):
        specs = query_specs("contains", ["US", "GB"], ["puzzle", "word"], 20)
        self.assertEqual(4, len(specs))
        first = make_request("2026-10", specs[0], 0, 20)
        self.assertEqual("US", first["filters"][0]["value"])
        self.assertEqual("CONTAINS", first["filters"][2]["operator"])
        self.assertEqual("puzzle", first["filters"][2]["value"])
        self.assertNotIn("ignoreCase", first["filters"][2])
        self.assertEqual("2026-11-01", first["timeRange"]["end"])

    def test_genre_has_no_keyword_filter(self):
        spec = query_specs("genre", ["US"], [], 20)[0]
        req = make_request("2026-10", spec, 0, 200)
        self.assertEqual(2, len(req["filters"]))

    def test_validate_scope_and_exact(self):
        spec = query_specs("exact", ["US"], ["word ladder"], 20)[0]
        validate_rows([row()], spec, "2026-10")
        with self.assertRaises(ValueError):
            validate_rows([row(country="GB")], spec, "2026-10")
        with self.assertRaises(ValueError):
            validate_rows([row(term="brand-new game")], spec, "2026-10")

    def test_collect_complete_genre(self):
        calls = []
        spec = query_specs("genre", ["US"], [], 20)
        def fetch(req):
            calls.append(req["pagination"]["offset"])
            if len(calls) == 1:
                return {"result": {"rows": [row(term="word ladder", rank=1), row(term="kakuro", rank=2)]},
                        "pagination": {"offset": 0, "pageSize": 2}}
            return {"result": {"rows": [row(term="masyu", rank=3)]},
                    "pagination": {"offset": 2, "pageSize": 2}}
        result = collect("2026-10", spec, 2, 5, fetch)
        self.assertEqual([0, 2], calls)
        self.assertEqual(3, len(result["rows"]))
        self.assertFalse(result["queries"][0]["truncated_to_top_k"])

    def test_genre_truncation_not_silent(self):
        spec = query_specs("genre", ["US"], [], 20)
        def fetch(_):
            return {"result": {"rows": [row()], "pagination": {"offset": 0, "pageSize": 1}}}
        result = collect("2026-10", spec, 1, 1, fetch)
        self.assertTrue(result["queries"][0]["truncated_to_top_k"])

    def test_exact_truncation_fails_closed(self):
        spec = query_specs("exact", ["US"], ["word ladder"], 20)
        with self.assertRaises(ValueError):
            collect("2026-10", spec, 1, 1,
                    lambda _: {"result": {"rows": [row()],
                               "pagination": {"offset": 0, "pageSize": 1}}})

    def test_discovery_only_observed_terms(self):
        fixture = {
            "source": SOURCE, "period": "2026-10", "countries": ["US"],
            "queries": [{"truncated_to_top_k": True}],
            "rows": [row("word ladder", 23), row("generic big game", 49)],
        }
        found, notes = extract_candidates(fixture, "abc")
        self.assertEqual(2, len(found))
        self.assertEqual("word", next(r for r in found if r["observed_search_term"] == "word ladder")["candidate_family_hypothesis"])
        self.assertTrue(any("top-K" in n for n in notes))
        self.assertTrue(all(r["status"] == "search_signal_observed_competition_unverified" for r in found))

    def test_duplicate_conflict_is_error(self):
        fixture = {
            "source": SOURCE, "period": "2026-10", "countries": ["US"],
            "queries": [], "rows": [row("word ladder", 20), row("word ladder", 30)],
        }
        _, notes = extract_candidates(fixture, "abc")
        self.assertTrue(any("conflicting duplicate" in n for n in notes))

    def test_incorrect_source_fails(self):
        with self.assertRaises(ValueError):
            extract_candidates({"source": "https://example.com", "rows": []}, "hash")


if __name__ == "__main__":
    unittest.main()
