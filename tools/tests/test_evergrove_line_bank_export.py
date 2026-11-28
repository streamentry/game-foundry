"""Offline tests for the Mosselora companion line bank translator export (CP29).

Synthetic banks live in temporary directories. The real bank is only read, and its export goes to a
temporary directory, never games/evergrove/Assets.
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
SCRIPT = ROOT / "tools" / "evergrove_line_bank_export.py"
REAL_BANK = ROOT / "games" / "evergrove" / "Assets" / "Resources" / "Evergrove" / "Lines" / "en.json"
REAL_RULES = ROOT / "games" / "evergrove" / "docs" / "specs" / "companion-line-rules.json"

_spec = importlib.util.spec_from_file_location("evergrove_line_bank_export", SCRIPT)
exporter = importlib.util.module_from_spec(_spec)
sys.modules[_spec.name] = exporter
_spec.loader.exec_module(exporter)

# CP21: pinned in games/evergrove/Tests/LineBankTests.cs (LineIds): LineId("FruitGiven", 0, "Thank you, Crème.").
PINNED_LINE = "Thank you, Crème."
PINNED_ID = "cl_f8f7c620"


def read_rows(path: Path) -> list[dict]:
    with path.open(encoding="utf-8-sig", newline="") as handle:
        return list(csv.DictReader(handle))


def count_lines(bank_path: Path) -> int:
    data = json.loads(bank_path.read_text(encoding="utf-8"))
    return sum(len(lines) for voices in data.values() for lines in voices.values())


class LineIdTests(unittest.TestCase):
    def test_matches_pinned_csharp_value(self) -> None:
        self.assertEqual(exporter.line_id("FruitGiven", 0, PINNED_LINE), PINNED_ID)

    def test_any_voice_hashes_as_minus_one(self) -> None:
        self.assertEqual(exporter.line_id("FruitGiven", -1, "Thank you."),
                         exporter.line_id("FruitGiven", -1, "Thank you."))
        self.assertNotEqual(exporter.line_id("FruitGiven", -1, "Thank you."),
                            exporter.line_id("FruitGiven", 0, "Thank you."))


class ExportTests(unittest.TestCase):
    def setUp(self) -> None:
        self.tmp = tempfile.TemporaryDirectory()
        self.dir = Path(self.tmp.name)

    def tearDown(self) -> None:
        self.tmp.cleanup()

    def write_bank(self, data: dict) -> Path:
        path = self.dir / "bank.json"
        path.write_text(json.dumps(data, ensure_ascii=False), encoding="utf-8")
        return path

    def test_real_bank_row_count_matches_lines_and_ids_are_unique(self) -> None:
        out = self.dir / "real.csv"
        count = exporter.export(REAL_BANK, REAL_RULES, out)
        self.assertEqual(count, count_lines(REAL_BANK))
        rows = read_rows(out)
        self.assertEqual(len(rows), count)
        self.assertEqual(len({row["id"] for row in rows}), len(rows))

    def test_pinned_line_row_has_pinned_id_and_speaker(self) -> None:
        bank = self.write_bank({"FruitGiven": {"0": [PINNED_LINE]}})
        out = self.dir / "pinned.csv"
        exporter.export(bank, REAL_RULES, out)
        rows = read_rows(out)
        self.assertEqual(len(rows), 1)
        self.assertEqual(rows[0]["id"], PINNED_ID)
        self.assertEqual(rows[0]["speaker"], "Mira")
        self.assertEqual(rows[0]["english"], PINNED_LINE)
        self.assertEqual(rows[0]["translation"], "")

    def test_slots_speaker_and_notes_for_unlocalized_slots(self) -> None:
        bank = self.write_bank({
            "NewestThing": {"2": ["Your {thing} is down by the {where}."]},
            "HourHabit.Dawn": {"0": ["You come at dawn, {thing}."]},
        })
        out = self.dir / "slots.csv"
        exporter.export(bank, REAL_RULES, out)
        rows = {row["group"] + "/" + row["voice"]: row for row in read_rows(out)}
        newest = rows["NewestThing/2"]
        self.assertEqual(newest["kind"], "NewestThing")
        self.assertEqual(newest["qualifier"], "")
        self.assertEqual(newest["speaker"], "Fen")
        self.assertEqual(newest["slots"], "{thing}, {where}")
        self.assertEqual(newest["localizable_slots"], "")
        self.assertIn("Keep {thing} etc. as is", newest["notes"])
        habit = rows["HourHabit.Dawn/0"]
        self.assertEqual(habit["kind"], "HourHabit")
        self.assertEqual(habit["qualifier"], "Dawn")
        self.assertEqual(habit["speaker"], "Mira")

    def test_any_voice_has_no_speaker(self) -> None:
        bank = self.write_bank({"FruitGiven": {"any": ["Fruit, thanks."]}})
        out = self.dir / "any.csv"
        exporter.export(bank, REAL_RULES, out)
        row = read_rows(out)[0]
        self.assertEqual(row["voice"], "any")
        self.assertEqual(row["speaker"], "")

    def test_output_has_bom_and_all_columns(self) -> None:
        out = self.dir / "bom.csv"
        exporter.export(REAL_BANK, REAL_RULES, out)
        raw = out.read_bytes()
        self.assertTrue(raw.startswith(b"\xef\xbb\xbf"))
        with out.open(encoding="utf-8-sig", newline="") as handle:
            header = next(csv.reader(handle))
        self.assertEqual(header, exporter.COLUMNS)

    def test_refuses_to_write_under_assets(self) -> None:
        target = ROOT / "games" / "evergrove" / "Assets" / "should_not_exist.csv"
        with self.assertRaises(ValueError):
            exporter.export(REAL_BANK, REAL_RULES, target)
        self.assertFalse(target.exists())

    def test_unknown_kind_is_an_error(self) -> None:
        bank = self.write_bank({"NotAKind": {"0": ["Hello."]}})
        with self.assertRaises(ValueError):
            exporter.export(bank, REAL_RULES, self.dir / "bad.csv")

    def test_main_reports_row_count_on_stdout(self) -> None:
        bank = self.write_bank({"FruitGiven": {"0": [PINNED_LINE]}})
        out = self.dir / "main.csv"
        buffer = io.StringIO()
        with contextlib.redirect_stdout(buffer):
            code = exporter.main(["--bank", str(bank), "--rules", str(REAL_RULES), "--out", str(out)])
        self.assertEqual(code, 0)
        self.assertIn("wrote 1 rows", buffer.getvalue())


if __name__ == "__main__":
    unittest.main()
