"""CP28: offline tests for the checker's original rules (slots, kinds, banned and child-safe phrases,
time claims, minimum counts, voices, localized slots, pending kinds) and a check that the shipped en.json passes.

Each rule has a failing and a passing example. Synthetic banks live in temporary directories; the
real companion-line-rules.json and en.json are read only where a test says so (phrases, slots, counts).
"""
from __future__ import annotations

import contextlib
import importlib.util
import io
import json
import subprocess
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

REAL_RULES = json.loads(checker.RULES.read_text(encoding="utf-8"))

# Small rules for the rules that need a kind with a particular shape.
SYNTHETIC_RULES = {
    "maxChars": 200,
    "maxWords": 28,
    "longest": {"thing": "a fern", "name": "Ada"},
    "banned": ["streak"],
    "childSafe": ["blood"],
    "timeClaims": ["yesterday"],
    "subjectVerbs": ["sits"],
    "localizedSlots": ["name"],
    "kinds": {
        "Plain": {"slots": ["thing"], "voices": {"0": None, "1": None}, "min": 0},
        "Counted": {"slots": [], "voices": {"0": None}, "min": 2},
        "Absent": {"slots": [], "voices": {"0": None}, "min": 0},
        "Hidden": {"slots": [], "voices": {"0": None}, "min": 0, "pending": True},
    },
}

WORDS = [
    "oak", "birch", "willow", "maple", "cedar", "pine", "elm", "ash", "alder", "rowan",
    "hazel", "holly", "yew", "poplar", "larch", "beech", "linden", "spruce", "aspen", "chestnut",
]


def _lines(count: int) -> list[str]:
    """Distinct, digit-free lines (CP26 forbids shared text; a digit is not a slot)."""
    return [f"Rain falls softly on the {WORDS[i]} tree." for i in range(count)]


class _Runner:
    """Runs checker.main() against temporary banks and rules, the way the shipped tool runs."""

    @staticmethod
    def run(banks: dict[str, dict], rules: dict, *args: str) -> tuple[int, str]:
        with tempfile.TemporaryDirectory() as tmp:
            lines_dir = Path(tmp) / "Lines"
            lines_dir.mkdir()
            for name, bank in banks.items():
                (lines_dir / name).write_text(json.dumps(bank), encoding="utf-8")
            rules_path = Path(tmp) / "rules.json"
            rules_path.write_text(json.dumps(rules), encoding="utf-8")
            out = io.StringIO()
            with patch.object(checker, "BANKS", lines_dir), \
                    patch.object(checker, "RULES", rules_path), \
                    patch.object(checker, "REVIEW", Path(tmp) / "none"), \
                    patch.object(sys, "argv", ["evergrove_line_bank_check.py", *args]), \
                    contextlib.redirect_stdout(out):
                code = checker.main()
            return code, out.getvalue()


class UnknownSlotTests(unittest.TestCase):
    """A {slot} that no rule names is refused."""

    def test_unknown_slot_is_refused(self) -> None:
        problems = checker.check_line("NewestThing", "0", "Your {bogus} waits here.", REAL_RULES)
        self.assertTrue(any("slot {bogus} is not filled by NewestThing" in p for p in problems), problems)

    def test_known_slot_is_accepted(self) -> None:
        self.assertEqual([], checker.check_line("NewestThing", "0", "Your {thing} waits here.", REAL_RULES))

    def test_unknown_fact_kind_is_refused(self) -> None:
        problems = checker.check_line("NoSuchKind", "0", "Your fern waits here.", REAL_RULES)
        self.assertEqual(["unknown fact kind NoSuchKind"], problems)

    def test_unknown_kind_in_a_bank_is_refused(self) -> None:
        problems = checker.check_bank("en.json", {"NoSuchKind": {"0": ["Fern waits."]}}, REAL_RULES)
        self.assertTrue(any("unknown fact kind NoSuchKind" in p for p in problems), problems)


class KindSlotTests(unittest.TestCase):
    """A slot the kind does not fill is refused, even when the slot is known to other kinds."""

    def test_slot_a_kind_does_not_have_is_refused(self) -> None:
        # RiverClose fills no slots; {thing} exists for NewestThing only.
        problems = checker.check_line("RiverClose", "0", "Your {thing} waits here.", REAL_RULES)
        self.assertTrue(any("slot {thing} is not filled by RiverClose (allowed: none)" in p for p in problems), problems)

    def test_slot_the_kind_has_is_accepted(self) -> None:
        self.assertEqual([], checker.check_line("NewestThing", "0", "Your {thing} waits here.", REAL_RULES))


class BannedPhraseTests(unittest.TestCase):
    """Guilt phrases from the banned list are refused; ordinary wording passes."""

    def test_guilt_phrase_is_refused(self) -> None:
        problems = checker.check_line("FruitGiven", "0", "I missed you so much.", REAL_RULES)
        self.assertIn("banned phrase: 'missed you'", problems)

    def test_ordinary_wording_passes(self) -> None:
        self.assertEqual([], checker.check_line("FruitGiven", "0", "The orchard missed the rain.", REAL_RULES))

    def test_streak_phrase_is_refused(self) -> None:
        problems = checker.check_line("FruitGiven", "0", "Keep your streak going.", REAL_RULES)
        self.assertIn("banned phrase: 'streak'", problems)

    def test_word_containing_a_phrase_is_not_refused(self) -> None:
        # word_hit requires word boundaries: "missedly" is not "missed you"; "stress" is not "stressed".
        self.assertEqual([], checker.check_line("FruitGiven", "0", "The fruit is stressed-free.", REAL_RULES))


class ChildSafeTests(unittest.TestCase):
    """Child-unsafe words from the childSafe list are refused in any kind."""

    def test_child_unsafe_word_is_refused(self) -> None:
        problems = checker.check_line("FruitGiven", "0", "Blood orange is ripe.", REAL_RULES)
        self.assertIn("childSafe phrase: 'blood'", problems)

    def test_child_safe_line_passes(self) -> None:
        self.assertEqual([], checker.check_line("FruitGiven", "0", "Orange is ripe.", REAL_RULES))


class TimeClaimTests(unittest.TestCase):
    """Time claims are refused where a kind's allowTimeClaims is false, and allowed where it is true."""

    def test_time_claim_refused_where_not_allowed(self) -> None:
        # RiverClose has no allowTimeClaims, so the timeClaims group applies.
        problems = checker.check_line("RiverClose", "0", "The river came up yesterday.", REAL_RULES)
        self.assertIn("timeClaims phrase: 'yesterday'", problems)

    def test_no_time_claim_passes_where_not_allowed(self) -> None:
        self.assertEqual([], checker.check_line("RiverClose", "0", "The river is close.", REAL_RULES))

    def test_time_claim_allowed_where_true(self) -> None:
        # WeatherWhileAway has allowTimeClaims true; the same wording the check refuses above is accepted here.
        self.assertEqual([], checker.check_line("WeatherWhileAway", "0", "Yesterday {weather} came.", REAL_RULES))


class MinimumCountTests(unittest.TestCase):
    """At least the kind's min lines per voice (min), and minEach per qualifier value."""

    def test_too_few_lines_is_refused(self) -> None:
        problems = checker.check_bank("en.json", {"RainComing": {"1": _lines(19)}}, REAL_RULES)
        self.assertIn("en.json: RainComing/1 has 19 lines (needs 20)", problems)

    def test_enough_lines_passes(self) -> None:
        problems = checker.check_bank("en.json", {"RainComing": {"1": _lines(20)}}, REAL_RULES)
        self.assertEqual([], problems)

    def test_too_few_per_qualifier_value_is_refused(self) -> None:
        # RiverFurther needs 5 lines for each season; only Spring is given (20 lines in total, so min is met).
        problems = checker.check_bank("en.json", {"RiverFurther.Spring": {"0": _lines(20)}}, REAL_RULES)
        self.assertIn("en.json: RiverFurther.Summer/0 has 0 lines (needs 5)", problems)
        self.assertNotIn("en.json: RiverFurther/0 has 20 lines (needs 20)", problems)

    def test_enough_per_qualifier_value_passes(self) -> None:
        bank = {
            f"RiverFurther.{season}": {"0": [f"The river runs past the {w} bench." for w in WORDS[i * 5:(i + 1) * 5]]}
            for i, season in enumerate(["Spring", "Summer", "Autumn", "Winter"])
        }
        self.assertEqual([], checker.check_bank("en.json", bank, REAL_RULES))

    def test_too_few_lines_fails_the_run_with_complete(self) -> None:
        code, out = _Runner.run({"en.json": {"RainComing": {"1": _lines(3)}}}, REAL_RULES, "--complete")
        self.assertEqual(1, code)
        self.assertIn("FAIL en.json: RainComing/1 has 3 lines (needs 20)", out)


class VoiceTests(unittest.TestCase):
    """A voice the kind does not speak is refused."""

    def test_voice_not_allowed_for_kind_is_refused(self) -> None:
        # LetterWaiting is spoken by voice 1 only.
        problems = checker.check_bank("en.json", {"LetterWaiting": {"0": ["A letter from {name} waits."]}}, REAL_RULES)
        self.assertIn("en.json: LetterWaiting has voice '0', which doesn't speak LetterWaiting", problems)

    def test_voice_allowed_for_kind_is_accepted(self) -> None:
        problems = checker.check_bank("en.json", {"LetterWaiting": {"1": ["A letter from {name} waits."]}}, REAL_RULES)
        self.assertFalse(any("doesn't speak" in p for p in problems), problems)


class LocalizedSlotTests(unittest.TestCase):
    """A non-English bank may use only localizedSlots; {name} is allowed, {thing} (English item name) is not."""

    def test_non_localized_slot_in_other_language_is_refused(self) -> None:
        bank = {"de.json": {"NewArrival": {"0": ["Your {thing} arrived here."]}}}
        code, out = _Runner.run(bank, REAL_RULES)
        self.assertEqual(1, code)
        self.assertIn("de.json: NewArrival/0 uses {thing}, whose value is English", out)

    def test_localized_slot_in_other_language_is_allowed(self) -> None:
        bank = {"de.json": {"LetterWaiting": {"1": [f"Brief {w} von {{name}} wartet." for w in WORDS]}}}
        code, out = _Runner.run(bank, REAL_RULES)
        self.assertEqual(0, code, out)
        self.assertNotIn("whose value is English", out)


class PendingKindTests(unittest.TestCase):
    """A kind marked pending need not appear in the English bank under --complete; other kinds still must."""

    def test_pending_kind_missing_is_not_required_with_complete(self) -> None:
        problems = checker.check_bank("en.json", {}, SYNTHETIC_RULES, complete=True)
        self.assertIn("en.json: no lines for Absent", problems)
        self.assertFalse(any("no lines for Hidden" in p for p in problems), problems)

    def test_pending_kind_missing_is_ignored_without_complete(self) -> None:
        self.assertEqual([], checker.check_bank("en.json", {}, SYNTHETIC_RULES, complete=False))


class ShippedBankTests(unittest.TestCase):
    def test_real_en_json_passes_complete(self) -> None:
        result = subprocess.run(
            [sys.executable, str(SCRIPT), "--complete"],
            cwd=ROOT, capture_output=True, text=True, check=False,
        )
        self.assertEqual(0, result.returncode, result.stdout + result.stderr)
        self.assertIn("0 problems", result.stdout)


if __name__ == "__main__":
    unittest.main()
