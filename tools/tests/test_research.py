"""Offline research and fixture tests: no network or third-party dependencies."""
from __future__ import annotations

import csv
import sys
import tempfile
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tools"))

from rank_ideas import REQUIRED, score_measurement, rank_measurements  # noqa: E402
from validate_ideas import validate_file  # noqa: E402
from verify_takuzu_fixture import solutions, matching  # noqa: E402


class ResearchTests(unittest.TestCase):
    def test_backlog_schema_and_uniqueness(self) -> None:
        path = ROOT / "docs" / "ideas" / "candidates.csv"
        self.assertEqual([], validate_file(path))
        with path.open(newline="", encoding="utf-8") as handle:
            data = list(csv.DictReader(handle))
        self.assertEqual(100, len(data))
        self.assertTrue(all(not item["apple_popularity_1to100"] for item in data))

    def test_no_measurements_means_no_rankings(self) -> None:
        path = ROOT / "docs" / "research" / "measurements.csv"
        ranked, skipped = rank_measurements(path)
        self.assertEqual([], ranked)
        self.assertEqual(0, skipped)

    def example_measurement(self) -> dict[str, str]:
        return {
            "candidate_id": "GF-001",
            "country": "US",
            "month": "2026-10",
            "apple_popularity_1to100": "50",
            "apple_source_url": "https://example.org/apple-evidence",
            "top10_competitor_audit_url": "https://example.org/audit",
            "competition_weakness_0to5": "3",
            "review_evidence_url": "https://example.org/reviews",
            "unmet_need_0to5": "4",
            "repeat_use_potential_0to5": "4",
            "development_efficiency_0to5": "5",
            "analyst_notes": "Synthetic test fixture only; not real market data",
        }

    def test_score_requires_complete_evidence(self) -> None:
        row = self.example_measurement()
        self.assertAlmostEqual(68.0, score_measurement(row))
        row["apple_source_url"] = ""
        self.assertIsNone(score_measurement(row))

    def test_score_rejects_bogus_range(self) -> None:
        row = self.example_measurement()
        row["apple_popularity_1to100"] = "101"
        self.assertIsNone(score_measurement(row))

    def test_synthetic_measurement_sorts(self) -> None:
        row = self.example_measurement()
        second = dict(row)
        second["candidate_id"] = "GF-002"
        second["apple_popularity_1to100"] = "30"
        with tempfile.TemporaryDirectory() as tmp:
            path = Path(tmp) / "observations.csv"
            with path.open("w", newline="", encoding="utf-8") as handle:
                writer = csv.DictWriter(handle, fieldnames=REQUIRED)
                writer.writeheader()
                writer.writerows([second, row])
            ranked, skipped = rank_measurements(path)
        self.assertEqual(0, skipped)
        self.assertEqual("GF-001", ranked[0][1]["candidate_id"])

    def test_reference_puzzle_uniqueness(self) -> None:
        all_solutions = solutions()
        self.assertEqual(72, len(all_solutions))
        self.assertEqual(1, len(matching(all_solutions)))


if __name__ == "__main__":
    unittest.main()
