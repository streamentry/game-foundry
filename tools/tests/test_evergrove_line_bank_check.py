"""Offline tests for the Mosselora companion line bank checker (CP22 near-duplicates, CP24 length, CP25 punctuation).

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
    "maxWords": 28,
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


class OpeningWordTests(unittest.TestCase):
    """CP23: more than 5 lines in one group and voice starting with the same word warns."""

    def test_fires_at_six_lines_starting_the(self) -> None:
        lines = [f"The river carries message {i} down." for i in range(6)]
        bank = {"FruitGiven": {"0": lines}}
        warnings = checker.opening_words("en.json", bank)
        self.assertEqual(1, len(warnings))
        self.assertIn("en.json", warnings[0])
        self.assertIn("FruitGiven/0", warnings[0])
        self.assertIn("'the'", warnings[0])
        self.assertIn("6", warnings[0])

    def test_silent_at_five_lines_starting_the(self) -> None:
        lines = [f"The river carries message {i} down." for i in range(5)]
        bank = {"FruitGiven": {"0": lines}}
        self.assertEqual([], checker.opening_words("en.json", bank))

    def test_case_insensitive(self) -> None:
        lines = [
            "The river carries message one.",
            "the river carries message two.",
            "THE river carries message three.",
            "The river carries message four.",
            "tHe river carries message five.",
            "the river carries message six.",
        ]
        bank = {"FruitGiven": {"0": lines}}
        warnings = checker.opening_words("en.json", bank)
        self.assertEqual(1, len(warnings))
        self.assertIn("'the'", warnings[0])

    def test_punctuation_insensitive(self) -> None:
        lines = [
            '"The river carries message one.',
            "'The river carries message two.",
            "(The river carries message three.",
            "“The river carries message four.",
            "The river carries message five.",
            "...the river carries message six.",
        ]
        bank = {"FruitGiven": {"0": lines}}
        warnings = checker.opening_words("en.json", bank)
        self.assertEqual(1, len(warnings))
        self.assertIn("'the'", warnings[0])

    def test_slot_counts_as_its_own_token(self) -> None:
        lines = [f"{{thing}} sits by the river, number {i}." for i in range(6)]
        bank = {"FruitGiven": {"0": lines}}
        warnings = checker.opening_words("en.json", bank)
        self.assertEqual(1, len(warnings))
        self.assertIn("'{thing}'", warnings[0])

    def test_counts_only_within_one_group_and_voice(self) -> None:
        the_lines = [f"The river carries message {i}." for i in range(5)]
        bank = {
            "FruitGiven": {"0": the_lines, "1": the_lines[:1]},
            "WeatherToday.Rainy": {"0": the_lines[:1]},
        }
        self.assertEqual([], checker.opening_words("en.json", bank))


class ReadableLengthTests(unittest.TestCase):
    """CP24: a line over maxWords words, once filled with the longest values, is a problem."""

    def _rules(self, max_words: int) -> dict:
        return {**SYNTHETIC_RULES, "maxWords": max_words}

    def test_line_over_the_word_limit_is_a_problem(self) -> None:
        problems = checker.check_line("FruitGiven", "0", "One two three four five six.", self._rules(5))
        self.assertEqual(1, len(problems))
        self.assertIn("6 words", problems[0])
        self.assertIn("max 5", problems[0])

    def test_line_at_the_word_limit_passes(self) -> None:
        self.assertEqual([], checker.check_line("FruitGiven", "0", "One two three four five.", self._rules(5)))

    def test_slots_count_their_longest_words(self) -> None:
        # Raw: "I see {thing} now." is 4 words; filled with "a fern" it is 5, so a limit of 4 refuses it.
        problems = checker.check_line("FruitGiven", "0", "I see {thing} now.", self._rules(4))
        self.assertTrue(any("5 words" in p for p in problems), problems)

    def test_main_exits_nonzero_for_a_long_line(self) -> None:
        bank = {"FruitGiven": {"0": [DIFFERENT_A, "One two three four five six."]}}
        code, out = StrictFlagTests._run_bank(bank, {**SYNTHETIC_RULES, "maxWords": 5})
        self.assertEqual(1, code)
        self.assertIn("FAIL en.json: FruitGiven/0 #2", out)
        self.assertIn("6 words", out)


class PunctuationTests(unittest.TestCase):
    """CP25: double spaces, leading or trailing spaces, closing punctuation, mixed quote styles, space before punctuation."""

    def _problems(self, line: str) -> list[str]:
        return checker.punctuation_problems(line)

    def test_double_space_fires(self) -> None:
        self.assertTrue(any("double space" in p for p in self._problems("Rain  falls.")))

    def test_double_space_silent_on_single_spaces(self) -> None:
        self.assertEqual([], self._problems("Rain falls."))

    def test_leading_or_trailing_space_fires(self) -> None:
        self.assertTrue(any("leading or trailing" in p for p in self._problems(" Rain falls.")))
        self.assertTrue(any("leading or trailing" in p for p in self._problems("Rain falls. ")))
        self.assertTrue(any("leading or trailing" in p for p in self._problems("Rain falls.\n")))

    def test_leading_or_trailing_space_silent_when_trimmed(self) -> None:
        self.assertEqual([], self._problems("Rain falls."))

    def test_missing_closing_punctuation_fires(self) -> None:
        self.assertTrue(any("closing" in p for p in self._problems("Rain falls")))
        self.assertTrue(any("closing" in p for p in self._problems("Rain falls,")))
        self.assertTrue(any("closing" in p for p in self._problems("I see {thing}")))

    def test_closing_punctuation_silent_for_end_marks(self) -> None:
        for line in ["Rain falls.", "Is it raining?", "Rain falls\u2026", "Rain falls!"]:
            self.assertEqual([], self._problems(line), line)

    def test_closing_quote_or_bracket_after_end_mark_is_silent(self) -> None:
        for line in ['He said "rain falls."', "She said \u201crain falls.\u201d", "(Rain falls.)", "It fell [again.]"]:
            self.assertEqual([], self._problems(line), line)

    def test_closing_quote_without_end_mark_fires(self) -> None:
        self.assertTrue(any("closing" in p for p in self._problems('He said "rain falls"')))

    def test_mixed_straight_and_curly_quotes_fires(self) -> None:
        self.assertTrue(any("mixes" in p for p in self._problems('He said "hi." and \u2018bye.\u2019')))

    def test_straight_apostrophe_with_curly_double_quotes_fires(self) -> None:
        self.assertTrue(any("mixes" in p for p in self._problems("Don't go, \u201clove\u201d.")))

    def test_curly_apostrophe_with_straight_double_quotes_fires(self) -> None:
        self.assertTrue(any("mixes" in p for p in self._problems('Don\u2019t go, "love".')))

    def test_one_quote_style_is_silent(self) -> None:
        for line in ["Don't go, friend.", "He said \u201chi.\u201d", "He said \"hi.\"", "It\u2019s fine."]:
            self.assertEqual([], self._problems(line), line)

    def test_space_before_punctuation_fires(self) -> None:
        for line in ["Rain , falls.", "Rain falls .", "Rain falls ;", "Rain falls :", "Rain falls ?"]:
            self.assertTrue(any("before" in p for p in self._problems(line)), line)

    def test_space_before_punctuation_silent_when_attached(self) -> None:
        for line in ["Rain, falls.", "Rain falls\u2026", "Rain falls: wet.", "Wait; then rain falls."]:
            self.assertEqual([], self._problems(line), line)

    def test_check_line_reports_punctuation_problems(self) -> None:
        problems = checker.check_line("FruitGiven", "0", "Rain  falls", SYNTHETIC_RULES)
        self.assertTrue(any("double space" in p for p in problems), problems)
        self.assertTrue(any("closing" in p for p in problems), problems)

    def test_main_fails_on_punctuation_problem(self) -> None:
        bank = {"FruitGiven": {"0": [DIFFERENT_A, "Rain falls  today."]}}
        code, out = StrictFlagTests._run_bank(bank, SYNTHETIC_RULES)
        self.assertEqual(1, code)
        self.assertIn("FAIL en.json: FruitGiven/0 #2 double space", out)


class StrictFlagTests(unittest.TestCase):
    @staticmethod
    def _run_bank(bank: dict, rules: dict, *args: str) -> tuple[int, str]:
        with tempfile.TemporaryDirectory() as tmp:
            banks = Path(tmp) / "Lines"
            banks.mkdir()
            (banks / "en.json").write_text(json.dumps(bank), encoding="utf-8")
            rules_path = Path(tmp) / "rules.json"
            rules_path.write_text(json.dumps(rules), encoding="utf-8")
            out = io.StringIO()
            with patch.object(checker, "BANKS", banks), \
                    patch.object(checker, "RULES", rules_path), \
                    patch.object(checker, "REVIEW", Path(tmp) / "none"), \
                    patch.object(sys, "argv", ["evergrove_line_bank_check.py", *args]), \
                    contextlib.redirect_stdout(out):
                code = checker.main()
            return code, out.getvalue()

    def _run_main(self, bank: dict, *args: str) -> tuple[int, str]:
        return self._run_bank(bank, SYNTHETIC_RULES, *args)

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
