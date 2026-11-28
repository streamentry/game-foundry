"""Offline tests for the Mosselora companion translation import check (CP30).

Synthetic banks, rules and translator CSVs live in temporary directories. The real bank is only read.
Nothing is written under games/evergrove/Assets; the one refusal test for that path writes nothing at all.
"""
from __future__ import annotations

import contextlib
import csv
import importlib.util
import io
import json
import sys
import tempfile
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
TOOLS = ROOT / "tools"
SCRIPT = TOOLS / "evergrove_line_bank_import.py"
EXPORT_SCRIPT = TOOLS / "evergrove_line_bank_export.py"
ASSETS = ROOT / "games" / "evergrove" / "Assets"
REAL_BANK = ROOT / "games" / "evergrove" / "Assets" / "Resources" / "Evergrove" / "Lines" / "en.json"
REAL_RULES = ROOT / "games" / "evergrove" / "docs" / "specs" / "companion-line-rules.json"
DRAFT_HEADER = "DRAFT — not reviewed by a native speaker; do not copy into Resources until reviewed."

if str(TOOLS) not in sys.path:
    sys.path.insert(0, str(TOOLS))

_spec = importlib.util.spec_from_file_location("evergrove_line_bank_import", SCRIPT)
importer = importlib.util.module_from_spec(_spec)
sys.modules[_spec.name] = importer
_spec.loader.exec_module(importer)

exporter = importer.exporter

THANKS_TEXT = "Thank you, {name}."
PLAIN_TEXT = "Thanks!"
WEATHER_TEXT = "It is {weather} today."
ID_THANKS = exporter.line_id("FruitGiven", 0, THANKS_TEXT)
ID_PLAIN = exporter.line_id("FruitGiven", -1, PLAIN_TEXT)
ID_WEATHER = exporter.line_id("WeatherToday", -1, WEATHER_TEXT)

SAMPLE_BANK = {
    "FruitGiven": {"0": [THANKS_TEXT], "any": [PLAIN_TEXT]},
    "WeatherToday": {"any": [WEATHER_TEXT]},
}
SAMPLE_RULES = {
    "localizedSlots": ["name"],
    "kinds": {"FruitGiven": {}, "WeatherToday": {}},
}


def run(argv: list[str]) -> tuple[int, str]:
    out = io.StringIO()
    with contextlib.redirect_stdout(out), contextlib.redirect_stderr(io.StringIO()):
        code = importer.main(argv)
    return code, out.getvalue()


class ImportTests(unittest.TestCase):
    def setUp(self) -> None:
        temp = tempfile.TemporaryDirectory()
        self.addCleanup(temp.cleanup)
        self.tmp = Path(temp.name)
        self.bank_path = self.tmp / "en.json"
        self.rules_path = self.tmp / "rules.json"
        self.bank_path.write_text(json.dumps(SAMPLE_BANK), encoding="utf-8")
        self.rules_path.write_text(json.dumps(SAMPLE_RULES), encoding="utf-8")
        self.out = self.tmp / "review" / "fr"

    def make_csv(self, fills: dict[str, str]) -> Path:
        """Exports the sample bank, fills the translation column, and appends unknown ids as extra rows."""
        base = self.tmp / "base.csv"
        exporter.export(self.bank_path, self.rules_path, base)
        with base.open(encoding="utf-8-sig", newline="") as handle:
            rows = list(csv.DictReader(handle))
        known = {row["id"] for row in rows}
        for row in rows:
            if row["id"] in fills:
                row["translation"] = fills[row["id"]]
        for key, value in fills.items():
            if key not in known:
                rows.append({column: "" for column in exporter.COLUMNS} | {
                    "id": key, "english": "Unknown line", "translation": value,
                })
        path = self.tmp / "translated.csv"
        with path.open("w", encoding="utf-8-sig", newline="") as handle:
            writer = csv.DictWriter(handle, fieldnames=exporter.COLUMNS, quoting=csv.QUOTE_ALL)
            writer.writeheader()
            writer.writerows(rows)
        return path

    def run_import(self, fills: dict[str, str]) -> tuple[int, str]:
        csv_path = self.make_csv(fills)
        return run([
            "--locale", "fr", "--csv", str(csv_path),
            "--bank", str(self.bank_path), "--rules", str(self.rules_path),
            "--out-dir", str(self.out),
        ])

    def draft(self) -> str:
        return (self.out / importer.DRAFT_NAME).read_text(encoding="utf-8")

    def test_valid_row_written_as_draft(self) -> None:
        code, report = self.run_import({ID_THANKS: "Merci, {name}."})
        self.assertEqual(code, 0, report)
        draft = self.draft()
        self.assertIn(DRAFT_HEADER, draft)
        self.assertIn(f"- {ID_THANKS}: Merci, {{name}}. (English: {THANKS_TEXT})", draft)
        self.assertNotIn(ID_PLAIN, draft)
        self.assertIn("accepted 1, refused 0, skipped 0", report)

    def test_bad_slot_refused(self) -> None:
        code, report = self.run_import({ID_THANKS: "Merci, {thing}."})
        self.assertEqual(code, 1)
        self.assertIn("{thing}", report)
        self.assertNotIn(ID_THANKS, self.draft())

    def test_dropped_name_refused(self) -> None:
        code, report = self.run_import({ID_THANKS: "Merci !"})
        self.assertEqual(code, 1)
        self.assertIn("drops {name}", report)
        self.assertNotIn(ID_THANKS, self.draft())

    def test_unknown_id_refused(self) -> None:
        code, report = self.run_import({"cl_00000000": "Bonjour."})
        self.assertEqual(code, 1)
        self.assertIn("cl_00000000", report)
        self.assertIn("not a line in the English bank", report)

    def test_non_localizable_row_skipped(self) -> None:
        code, report = self.run_import({ID_WEATHER: "Il fait {weather}."})
        self.assertEqual(code, 0, report)
        self.assertIn(f"skipped {ID_WEATHER}", report)
        self.assertNotIn(ID_WEATHER, self.draft())

    def test_invalid_braces_refused(self) -> None:
        code, report = self.run_import({ID_THANKS: "Merci, {name"})
        self.assertEqual(code, 1)
        self.assertIn("braces", report)

    def test_added_slot_refused(self) -> None:
        code, report = self.run_import({ID_PLAIN: "Merci {name} !"})
        self.assertEqual(code, 1)
        self.assertIn("adds {name}", report)
        self.assertNotIn(ID_PLAIN, self.draft())

    def test_assets_out_dir_refused_before_any_write(self) -> None:
        csv_path = self.make_csv({ID_THANKS: "Merci, {name}."})
        target = ASSETS / "CP30TestShouldNotExist"
        code, _ = run([
            "--locale", "fr", "--csv", str(csv_path),
            "--bank", str(self.bank_path), "--rules", str(self.rules_path),
            "--out-dir", str(target / "fr"),
        ])
        self.assertEqual(code, 2)
        self.assertFalse(target.exists())

    def test_assets_itself_refused(self) -> None:
        csv_path = self.make_csv({})
        code, _ = run([
            "--locale", "fr", "--csv", str(csv_path),
            "--bank", str(self.bank_path), "--rules", str(self.rules_path),
            "--out-dir", str(ASSETS),
        ])
        self.assertEqual(code, 2)

    def test_bad_locale_refused(self) -> None:
        csv_path = self.make_csv({})
        code, _ = run([
            "--locale", "../evil", "--csv", str(csv_path),
            "--bank", str(self.bank_path), "--rules", str(self.rules_path),
            "--out-dir", str(self.out),
        ])
        self.assertEqual(code, 2)
        self.assertFalse(self.out.exists())

    def test_writes_only_the_draft_file(self) -> None:
        code, _ = self.run_import({ID_THANKS: "Merci, {name}."})
        self.assertEqual(code, 0)
        self.assertEqual([path.name for path in self.out.iterdir()], [importer.DRAFT_NAME])

    def test_real_bank_with_no_translations_is_clean(self) -> None:
        csv_path = self.tmp / "real.csv"
        exporter.export(REAL_BANK, REAL_RULES, csv_path)
        code, report = run([
            "--locale", "fr", "--csv", str(csv_path),
            "--bank", str(REAL_BANK), "--rules", str(REAL_RULES),
            "--out-dir", str(self.out),
        ])
        self.assertEqual(code, 0, report)
        self.assertIn("accepted 0, refused 0, skipped 0", report)


if __name__ == "__main__":
    unittest.main()
