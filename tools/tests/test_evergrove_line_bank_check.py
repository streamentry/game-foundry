"""Offline tests for the Mosselora companion line bank checker (CP22 near-duplicates).

Uses synthetic banks and rules in temporary directories; never reads the shipped bank.
"""
from __future__ import annotations

import contextlib
import importlib.util
import io
import json
import sys
import tempfile
import unittest
from pathlib import Path
from unittest.mock import patch

ROOT = Path(__file__).resolve().parents[2]
SCRIPT = ROOT / "tools" / "evergrove_line_bank_check.py"

_spec = importlib.util.spec_from_file_location("evergrove_line_bank_check", SCRIPT)
checker = importlib.util.module_from_spec(_spec)
sys.modules[_spec.name] = checker
_spec.loader.exec_module(checker)

SYNTHETIC_RULES = {
    "maxChars": 200,
    "longest": {"thing": "a fern"},
    "banned": [],
    "childSafe": [],
    "timeClaims": [],
    "subjectVerbs": ["sits", "rests"],
    "localizedSlots": [],
    "kinds": {
        "FruitGiven": {"slots": ["thing"], "voices": {"0": None}, "min": 0, "minEach": 0},
    },
}

NEAR_A = "The hedgehog came at dusk to visit your flower."
NEAR_B = "The hedgehog came at dusk to see your flower."
DIFFERENT_A = "Rain is falling on the river today."
DIFFERENT_B = "Bright {thing} waits by the old bridge."


class NearDuplicateTests(unittest.TestCase):
    def test_slots_stay_single_tokens(self) -> None:
        self.assertIn("{thing}", checker.tokens("A {thing} came, quickly!"))
        self.assertEqual({"it's", "fine"}, checker.tokens("It's fine."))

    def test_fires_on_near_identical_lines(self) -> None:
        bank = {"FruitGiven": {"0": [NEAR_A, NEAR_B]}}
        warnings = checker.near_duplicates("en.json", bank)
        self.assertEqual(1, len(warnings))
        self.assertIn("en.json", warnings[0])
        self.assertIn("FruitGiven/0", warnings[0])
        self.assertIn("#1 and #2", warnings[0])

    def test_silent_on_different_lines(self) -> None:
        bank = {"FruitGiven": {"0": [DIFFERENT_A, DIFFERENT_B]}}
        self.assertEqual([], checker.near_duplicates("en.json", bank))

    def test_only_compares_within_one_group_and_voice(self) -> None:
        bank = {
            "FruitGiven": {"0": [NEAR_A], "1": [NEAR_B]},
            "WeatherToday.Rainy": {"0": [NEAR_B]},
        }
        self.assertEqual([], checker.near_duplicates("en.json", bank))


class StrictFlagTests(unittest.TestCase):
    def _run_main(self, bank: dict, *args: str) -> tuple[int, str]:
        with tempfile.TemporaryDirectory() as tmp:
            banks = Path(tmp) / "Lines"
            banks.mkdir()
            (banks / "en.json").write_text(json.dumps(bank), encoding="utf-8")
            rules_path = Path(tmp) / "rules.json"
            rules_path.write_text(json.dumps(SYNTHETIC_RULES), encoding="utf-8")
            out = io.StringIO()
            with patch.object(checker, "BANKS", banks), \
                    patch.object(checker, "RULES", rules_path), \
                    patch.object(checker, "REVIEW", Path(tmp) / "none"), \
                    patch.object(sys, "argv", ["evergrove_line_bank_check.py", *args]), \
                    contextlib.redirect_stdout(out):
                code = checker.main()
            return code, out.getvalue()

    def test_warning_does_not_fail_by_default(self) -> None:
        bank = {"FruitGiven": {"0": [NEAR_A, NEAR_B]}}
        code, out = self._run_main(bank)
        self.assertEqual(0, code)
        self.assertIn("warning: en.json: FruitGiven/0 #1 and #2", out)
        self.assertIn("1 warnings", out)

    def test_strict_turns_warning_into_failure(self) -> None:
        bank = {"FruitGiven": {"0": [NEAR_A, NEAR_B]}}
        code, out = self._run_main(bank, "--strict")
        self.assertNotEqual(0, code)
        self.assertIn("FAIL en.json: FruitGiven/0 #1 and #2", out)

    def test_strict_passes_clean_bank(self) -> None:
        bank = {"FruitGiven": {"0": [DIFFERENT_A, DIFFERENT_B]}}
        code, _ = self._run_main(bank, "--strict")
        self.assertEqual(0, code)


if __name__ == "__main__":
    unittest.main()
