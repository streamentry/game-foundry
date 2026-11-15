"""Structural checks for manual App Store evidence packs."""
from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tools"))

from validate_storefront_audit import validate_audit  # noqa: E402


class StorefrontAuditTests(unittest.TestCase):
    def make_audit(self, root: Path) -> dict:
        (root / "search.png").write_bytes(b"image placeholder for test")
        results = []
        for rank in range(1, 11):
            screenshot = f"result-{rank}.png"
            (root / screenshot).write_bytes(b"image placeholder for test")
            results.append({
                "organic_rank": rank,
                "is_sponsored": False,
                "name": f"Game {rank}",
                "publisher": f"Studio {rank}",
                "app_id": str(1000000000 + rank),
                "listing_url": f"https://apps.apple.com/us/app/game-{rank}/id{1000000000 + rank}",
                "rating_count": "1234",
                "average_rating": "4.5",
                "price": "Free",
                "currency": "USD",
                "last_updated": "2026-10-20",
                "aso_messaging": "Daily logic puzzles with offline play",
                "screenshot": screenshot,
            })
        reviews = []
        for index in range(20):
            positive = index % 2 == 0
            app_id = str(1000000001 + (index % 10))
            reviews.append({
                "app_id": app_id,
                "sentiment": "positive" if positive else "negative",
                "rating": 5 if positive else 1,
                "review_date": "2026-11-01",
                "source_url": f"https://apps.apple.com/us/app/game/id{app_id}",
                "theme": "clear controls" if positive else "intrusive ads",
                "paraphrase": (
                    f"Player {index} praised clear controls" if positive
                    else f"Player {index} objected to frequent interruptions"
                ),
            })
        return {
            "schema_version": 1,
            "candidate_id": "GF-001",
            "keyword": "daily logic puzzle",
            "query_classification": "non_branded",
            "country": "US",
            "captured_at": "2026-11-15T10:00:00+07:00",
            "device": {
                "kind": "iphone",
                "model": "iPhone test device",
                "os_version": "iOS 27.2",
                "storefront_country": "US",
            },
            "search_page_screenshot": "search.png",
            "results": results,
            "reviews": reviews,
            "review_sampling_limitation": "",
            "synthesis": {
                "judgment": "wedge",
                "differentiator": "Accessible no-timer logic puzzles with offline play",
                "acquisition_path": "Non-branded App Store search for daily logic puzzles",
                "no_wedge_reason": "",
                "falsifier": "Stop if measured non-branded results show no attainable rank path",
            },
        }

    def write_audit(self, root: Path, audit: dict) -> Path:
        path = root / "audit.json"
        path.write_text(json.dumps(audit), encoding="utf-8")
        return path

    def test_complete_pack_passes(self) -> None:
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            path = self.write_audit(root, self.make_audit(root))
            self.assertEqual([], validate_audit(path))

    def test_numeric_rating_metadata_is_accepted(self) -> None:
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            audit = self.make_audit(root)
            audit["results"][0]["rating_count"] = 1234
            audit["results"][0]["average_rating"] = 4.5
            errors = validate_audit(self.write_audit(root, audit))
        self.assertEqual([], errors)

    def test_rating_count_must_be_an_integer(self) -> None:
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            audit = self.make_audit(root)
            audit["results"][0]["rating_count"] = 1.5
            errors = validate_audit(self.write_audit(root, audit))
        self.assertTrue(any("rating_count must be an integer" in error for error in errors))

    def test_template_is_not_accepted_as_evidence(self) -> None:
        path = ROOT / "docs" / "research" / "storefront-audit-template.json"
        errors = validate_audit(path)
        self.assertTrue(errors)
        self.assertTrue(any("candidate_id" in error for error in errors))
        self.assertTrue(any("search_page_screenshot" in error for error in errors))

    def test_schema_version_must_be_integer_one(self) -> None:
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            audit = self.make_audit(root)
            audit["schema_version"] = True
            errors = validate_audit(self.write_audit(root, audit))
        self.assertTrue(any("schema_version must be 1" in error for error in errors))

    def test_rejects_missing_or_duplicate_organic_positions(self) -> None:
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            audit = self.make_audit(root)
            audit["results"].pop()
            audit["results"][0]["organic_rank"] = 2
            errors = validate_audit(self.write_audit(root, audit))
        self.assertTrue(any("exactly 10" in error for error in errors))
        self.assertTrue(any("duplicate organic rank" in error for error in errors))

    def test_rejects_sponsored_result_and_simulator(self) -> None:
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            audit = self.make_audit(root)
            audit["results"][0]["is_sponsored"] = True
            audit["device"]["kind"] = "simulator"
            errors = validate_audit(self.write_audit(root, audit))
        self.assertTrue(any("sponsored results" in error for error in errors))
        self.assertTrue(any("simulators are not accepted" in error for error in errors))

    def test_requires_reason_for_incomplete_review_sample(self) -> None:
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            audit = self.make_audit(root)
            audit["reviews"] = audit["reviews"][:4]
            errors = validate_audit(self.write_audit(root, audit))
        self.assertTrue(any("review_sampling_limitation" in error for error in errors))

    def test_rejects_duplicate_review_samples(self) -> None:
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            audit = self.make_audit(root)
            audit["reviews"][1] = dict(audit["reviews"][0])
            errors = validate_audit(self.write_audit(root, audit))
        self.assertTrue(any("duplicate review sample" in error for error in errors))

    def test_accepts_incomplete_reviews_only_with_documented_limit(self) -> None:
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            audit = self.make_audit(root)
            audit["reviews"] = audit["reviews"][:4]
            audit["review_sampling_limitation"] = "Only four recent reviews were available across the audited listings."
            errors = validate_audit(self.write_audit(root, audit))
        self.assertEqual([], errors)

    def test_rejects_screenshot_paths_outside_audit_folder(self) -> None:
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            audit = self.make_audit(root)
            audit["results"][0]["screenshot"] = "../outside.png"
            errors = validate_audit(self.write_audit(root, audit))
        self.assertTrue(any("screenshot must name an existing local file" in error for error in errors))

    def test_malformed_json_types_fail_with_validation_errors(self) -> None:
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            audit = self.make_audit(root)
            audit["device"]["kind"] = []
            audit["reviews"][0]["sentiment"] = []
            audit["reviews"][0]["source_url"] = None
            audit["synthesis"]["judgment"] = []
            errors = validate_audit(self.write_audit(root, audit))
        self.assertTrue(any("device.kind" in error for error in errors))
        self.assertTrue(any("sentiment" in error for error in errors))
        self.assertTrue(any("source_url" in error for error in errors))
        self.assertTrue(any("synthesis.judgment" in error for error in errors))

    def test_no_wedge_finding_must_name_reason_and_falsifier(self) -> None:
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            audit = self.make_audit(root)
            audit["synthesis"] = {
                "judgment": "no_wedge",
                "differentiator": "",
                "acquisition_path": "",
                "no_wedge_reason": "Incumbents satisfy the observed need without a recurring weakness.",
                "falsifier": "A measured query with weaker incumbents would reopen the decision.",
            }
            errors = validate_audit(self.write_audit(root, audit))
        self.assertEqual([], errors)


if __name__ == "__main__":
    unittest.main()
