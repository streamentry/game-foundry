#!/usr/bin/env python3
"""Check Mosselora companion lines against the line rules (smart companion SC03).

Checks the shipped bank (games/evergrove/Assets/Resources/Evergrove/Lines/*.json) and, with
--review, the draft review files (docs/improvements/review/companion-lines-*.md). The same rules
run in the C# tests (Tests/LineBankTests.cs); both read games/evergrove/docs/specs/companion-line-rules.json.

Rules: every {slot} is one its fact kind fills; at most maxChars characters once filled with the
longest real values; no "!"; no banned, child-unsafe or time-claim phrases; a {slot} is never a
sentence's first word, never follows "a"/"an", and {thing} is never a verb's subject; no digits;
no duplicates; at least the kind's minimum per voice (and per qualifier value, for kinds keyed
"Kind.Value" such as "WeatherToday.Rainy"); only the voices that speak a kind.

Usage: tools/evergrove_line_bank_check.py [--review] [--complete]
Exit 0 when every line passes, 1 otherwise. Never calls a network service.
"""
from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
GAME = ROOT / "games" / "evergrove"
RULES = GAME / "docs" / "specs" / "companion-line-rules.json"
BANKS = GAME / "Assets" / "Resources" / "Evergrove" / "Lines"
REVIEW = ROOT / "docs" / "improvements" / "review"
SLOT = re.compile(r"\{([^{}]*)\}")


def word_hit(text: str, phrase: str) -> bool:
    return re.search(r"(?<![a-z])" + re.escape(phrase) + r"(?![a-z])", text) is not None


def split_key(key: str) -> tuple[str, str | None]:
    kind, _, qualifier = key.partition(".")
    return kind, (qualifier or None)


def check_line(kind: str, voice: str, line: str, rules: dict) -> list[str]:
    problems = []
    spec = rules["kinds"].get(kind)
    if spec is None:
        return [f"unknown fact kind {kind}"]
    allowed = spec["slots"]
    if line != line.strip() or not line:
        problems.append("empty or untrimmed")
    if "\n" in line:
        problems.append("line break")
    used = SLOT.findall(line)
    if line.count("{") != len(used) or line.count("}") != len(used):
        problems.append("unbalanced braces")
    for name in used:
        if name not in allowed:
            problems.append(f"slot {{{name}}} is not filled by {kind} (allowed: {allowed or 'none'})")
    filled = SLOT.sub(lambda m: rules["longest"].get(m.group(1), m.group(0)), line)
    if len(filled) > rules["maxChars"]:
        problems.append(f"{len(filled)} chars filled with the longest values (max {rules['maxChars']})")
    if "!" in line:
        problems.append("exclamation mark")
    if re.search(r"\d", line):
        problems.append("a number not from a slot")
    lower = filled.lower()
    groups = ["banned", "childSafe"] + ([] if spec.get("allowTimeClaims") else ["timeClaims"])
    for group in groups:
        for phrase in rules[group]:
            if word_hit(lower, phrase.lower()):
                problems.append(f"{group} phrase: {phrase!r}")
    if re.search(r"(^|[.;?]\s+)\{", line):
        problems.append("a slot starts a sentence (it would be lowercase)")
    if re.search(r"(?i)\ban? \{", line):
        problems.append("'a'/'an' before a slot (article clash)")
    verbs = "|".join(rules["subjectVerbs"])
    if re.search(r"\{thing\}(?: \{where\})?,? (?:" + verbs + r")\b", line):
        problems.append("{thing} is a verb's subject (breaks for plural names)")
    return problems


def check_bank(name: str, bank: dict, rules: dict, complete: bool = False) -> list[str]:
    """bank: {"Kind" or "Kind.Value": {voice: [lines]}}. Counts are checked for every kind present
    (all of its voices and qualifier values); with complete=True, every kind in the rules must be present."""
    problems = []
    present: dict[str, dict[str, dict[str | None, int]]] = {}
    for key, voices in bank.items():
        kind, qualifier = split_key(key)
        spec = rules["kinds"].get(kind)
        if spec is None:
            problems.append(f"{name}: unknown fact kind {key}")
            continue
        if (qualifier is None) != (spec.get("qualifier") is None):
            problems.append(f"{name}: {key} " + ("needs a qualifier" if qualifier is None else "takes no qualifier"))
            continue
        for voice, lines in voices.items():
            if voice not in spec["voices"]:
                problems.append(f"{name}: {key} has voice {voice!r}, which doesn't speak {kind}")
                continue
            values = spec["voices"][voice]
            if qualifier is not None and qualifier not in values:
                problems.append(f"{name}: {key} voice {voice} needs no {qualifier!r} lines (allowed: {values})")
            present.setdefault(kind, {}).setdefault(voice, {})[qualifier] = len(lines)
            seen = set()
            for i, line in enumerate(lines, 1):
                low = line.strip().lower()
                if low in seen:
                    problems.append(f"{name}: {key}/{voice} #{i} duplicate: {line}")
                seen.add(low)
                for p in check_line(kind, voice, line, rules):
                    problems.append(f"{name}: {key}/{voice} #{i} {p}: {line}")
    kinds = {k: v for k, v in rules["kinds"].items() if not v.get("pending")} if complete else {k: rules["kinds"][k] for k in present}
    for kind, spec in kinds.items():
        got = present.get(kind, {})
        if not got and complete:
            problems.append(f"{name}: no lines for {kind}")
            continue
        for voice, values in spec["voices"].items():
            counts = got.get(voice, {})
            total = sum(counts.values())
            if total < spec["min"]:
                problems.append(f"{name}: {kind}/{voice} has {total} lines (needs {spec['min']})")
            for value in values or []:
                if counts.get(value, 0) < spec["minEach"]:
                    problems.append(f"{name}: {kind}.{value}/{voice} has {counts.get(value, 0)} lines (needs {spec['minEach']})")
    return problems


VOICE_HEADING = re.compile(r"^##\s+.*\(voice\s+(\d|any)\)(?:\s*·\s*(\w+))?\s*$")
ITEM = re.compile(r"^\d+\.\s+(.*\S)\s*$")


def parse_review(path: Path) -> tuple[str, dict]:
    """Returns (kind, {bank key: {voice: [lines]}}); a heading "## Mira (voice 0) · Rainy" opens a qualified list."""
    kind = path.stem.replace("companion-lines-", "")
    bank: dict[str, dict[str, list[str]]] = {}
    target = None
    for raw in path.read_text(encoding="utf-8").splitlines():
        heading = VOICE_HEADING.match(raw)
        if heading:
            key = kind + ("." + heading.group(2) if heading.group(2) else "")
            target = bank.setdefault(key, {}).setdefault(heading.group(1), [])
            continue
        if raw.startswith("#"):
            target = None
            continue
        item = ITEM.match(raw)
        if item and target is not None:
            target.append(item.group(1))
    return kind, bank


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("--review", action="store_true", help="also check docs/improvements/review drafts")
    parser.add_argument("--complete", action="store_true", help="the English bank must cover every kind in the rules")
    args = parser.parse_args()
    rules = json.loads(RULES.read_text(encoding="utf-8"))
    problems: list[str] = []
    counted = 0
    for bank_path in sorted(BANKS.glob("*.json")):
        try:
            bank = json.loads(bank_path.read_text(encoding="utf-8"))
        except json.JSONDecodeError as error:
            problems.append(f"{bank_path.name}: not valid JSON ({error})")
            continue
        if not isinstance(bank, dict):
            problems.append(f"{bank_path.name}: not an object")
            continue
        problems += check_bank(bank_path.name, bank, rules, args.complete and bank_path.name == "en.json")
        counted += sum(len(v) for voices in bank.values() for v in voices.values())
    if args.review:
        for review in sorted(REVIEW.glob("companion-lines-*.md")):
            _, bank = parse_review(review)
            problems += check_bank(review.name, bank, rules)
            counted += sum(len(v) for voices in bank.values() for v in voices.values())
    for p in problems:
        print("FAIL " + p)
    print(f"Companion line bank: {counted} lines checked, {len(problems)} problems.")
    return 1 if problems else 0


if __name__ == "__main__":
    sys.exit(main())
