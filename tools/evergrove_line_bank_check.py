#!/usr/bin/env python3
"""Check Mosselora companion lines against the line rules (smart companion SC03).

Checks the shipped bank (games/evergrove/Assets/Resources/Evergrove/Lines/*.json) and, with
--review, the draft review files (docs/improvements/review/companion-lines-*.md). The same rules
run in the C# tests (Tests/LineBankTests.cs); both read games/evergrove/docs/specs/companion-line-rules.json.

Rules: every {slot} is one its fact kind fills; at most maxChars characters once filled with the
longest real values; no "!"; no banned, child-unsafe or time-claim phrases; a {slot} is never a
sentence's first word, never follows "a"/"an", and {thing} is never a verb's subject; no digits;
no duplicates; at least minVariants per kind and voice.

Usage: tools/evergrove_line_bank_check.py [--review] [--min N]
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


def check_line(kind: str, voice: str, line: str, rules: dict) -> list[str]:
    problems = []
    allowed = rules["kinds"].get(kind)
    if allowed is None:
        return [f"unknown fact kind {kind}"]
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
    for group in ("banned", "childSafe", "timeClaims"):
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


def check_bank(name: str, bank: dict, rules: dict, minimum: int) -> list[str]:
    problems = []
    for kind, voices in bank.items():
        if kind not in rules["kinds"]:
            problems.append(f"{name}: unknown fact kind {kind}")
            continue
        for voice in rules["voices"]:
            lines = voices.get(voice, [])
            if len(lines) < minimum:
                problems.append(f"{name}: {kind}/{voice} has {len(lines)} lines (needs {minimum})")
        for voice, lines in voices.items():
            if voice not in rules["voices"] and voice != "any":
                problems.append(f"{name}: {kind} has unknown voice {voice!r}")
            seen = set()
            for i, line in enumerate(lines, 1):
                key = line.strip().lower()
                if key in seen:
                    problems.append(f"{name}: {kind}/{voice} #{i} duplicate: {line}")
                seen.add(key)
                for p in check_line(kind, voice, line, rules):
                    problems.append(f"{name}: {kind}/{voice} #{i} {p}: {line}")
    return problems


VOICE_HEADING = re.compile(r"^##\s+.*\(voice\s+(\d|any)\)\s*$")
ITEM = re.compile(r"^\d+\.\s+(.*\S)\s*$")


def parse_review(path: Path) -> tuple[str, dict]:
    kind = path.stem.replace("companion-lines-", "")
    voices: dict[str, list[str]] = {}
    voice = None
    for raw in path.read_text(encoding="utf-8").splitlines():
        heading = VOICE_HEADING.match(raw)
        if heading:
            voice = heading.group(1)
            voices.setdefault(voice, [])
            continue
        if raw.startswith("#"):
            voice = None
            continue
        item = ITEM.match(raw)
        if item and voice is not None:
            voices[voice].append(item.group(1))
    return kind, voices


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("--review", action="store_true", help="also check docs/improvements/review drafts")
    parser.add_argument("--min", type=int, default=None, help="override minVariants")
    args = parser.parse_args()
    rules = json.loads(RULES.read_text(encoding="utf-8"))
    minimum = rules["minVariants"] if args.min is None else args.min
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
        problems += check_bank(bank_path.name, bank, rules, minimum)
        counted += sum(len(v) for voices in bank.values() for v in voices.values())
    if args.review:
        for review in sorted(REVIEW.glob("companion-lines-*.md")):
            kind, voices = parse_review(review)
            problems += check_bank(review.name, {kind: voices}, rules, minimum)
            counted += sum(len(v) for v in voices.values())
    for p in problems:
        print("FAIL " + p)
    print(f"Companion line bank: {counted} lines checked, {len(problems)} problems.")
    return 1 if problems else 0


if __name__ == "__main__":
    sys.exit(main())
