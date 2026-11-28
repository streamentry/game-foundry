#!/usr/bin/env python3
"""Check Mosselora companion lines against the line rules (smart companion SC03).

Checks the shipped bank (games/evergrove/Assets/Resources/Evergrove/Lines/*.json) and, with
--review, the draft review files (docs/improvements/review/companion-lines-*.md). The same rules
run in the C# tests (Tests/LineBankTests.cs); both read games/evergrove/docs/specs/companion-line-rules.json.

Rules: every {slot} is one its fact kind fills; at most maxChars characters once filled with the
longest real values; no "!"; no banned, child-unsafe or time-claim phrases; a {slot} is never a
sentence's first word, never follows "a"/"an", and {thing} is never a verb's subject; no digits;
no duplicates; CP26 no line text shared by two voices or two groups; CP25 punctuation (no double or edge spaces, a closing . ! ? or ellipsis, no mixing straight
and curly quotes, no space before , . ! ? ; :); at least the kind's minimum per voice (and per qualifier value, for kinds keyed
"Kind.Value" such as "WeatherToday.Rainy"); only the voices that speak a kind.

Warnings (printed as "warning: ...") do not fail the run. Near-duplicates: two lines in one group
and voice whose word sets have Jaccard similarity >= 0.75 (CP22). More than 5 lines in one group and
voice starting with the same word also warns (CP23).

CP27: --report prints a GitHub-flavoured markdown coverage table after the checks (one row per group,
sorted; line counts per voice, the "any" voice, average and longest raw line in words, then a Total row).
It never changes the exit code.

Usage: tools/evergrove_line_bank_check.py [--review] [--complete] [--strict] [--report]
Exit 0 when every line passes, 1 otherwise. --strict turns warnings into problems.
Never calls a network service.
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
# CP22: a slot such as {thing} stays one token; other punctuation is dropped; stop words are kept.
TOKEN = re.compile(r"\{[^{}]*\}|[^\W_]+(?:'[^\W_]+)*")
NEAR_DUPLICATE = 0.75
# CP25: a line may end with one of these, optionally followed by closing quotes or brackets.
END_MARKS = (".", "!", "?", "\u2026")
CLOSERS = "\"'\u201d\u2019)]\u00bb"
STRAIGHT_QUOTES = "'\""
CURLY_QUOTES = "\u2018\u2019\u201c\u201d"
SPACE_BEFORE_PUNCTUATION = re.compile(r"\s[,.!?;:]")


def word_hit(text: str, phrase: str) -> bool:
    return re.search(r"(?<![a-z])" + re.escape(phrase) + r"(?![a-z])", text) is not None


def split_key(key: str) -> tuple[str, str | None]:
    kind, _, qualifier = key.partition(".")
    return kind, (qualifier or None)


def punctuation_problems(line: str) -> list[str]:
    """CP25: punctuation hygiene. A curly apostrophe (\u2019) and a straight one (') count as mixing quote
    styles when the same line also has straight double quotes, so one line uses one quote style."""
    problems = []
    if "  " in line:
        problems.append("double space")
    if line != line.strip():
        problems.append("leading or trailing space")
    if line.strip() and not line.rstrip(CLOSERS).endswith(END_MARKS):
        problems.append("no closing . ! ? or \u2026 (a closing quote or bracket may follow it)")
    if any(c in line for c in STRAIGHT_QUOTES) and any(c in line for c in CURLY_QUOTES):
        problems.append("mixes straight (' \") and curly (\u2018 \u2019 \u201c \u201d) quotes")
    if SPACE_BEFORE_PUNCTUATION.search(line):
        problems.append("space before , . ! ? ; or :")
    return problems


def check_line(kind: str, voice: str, line: str, rules: dict) -> list[str]:
    problems = []
    spec = rules["kinds"].get(kind)
    if spec is None:
        return [f"unknown fact kind {kind}"]
    allowed = spec["slots"]
    if not line.strip():
        problems.append("empty")
    problems += punctuation_problems(line)
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
    # CP24: readable at 150% text; counted after filling, the same as the character limit.
    words = len(filled.split())
    if words > rules["maxWords"]:
        problems.append(f"{words} words filled with the longest values (max {rules['maxWords']})")
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


def shared_lines(name: str, bank: dict) -> list[str]:
    """CP26: one problem per line text (trimmed, lowercased) that appears under two different (group, voice)
    pairs, naming every place. The "any" voice counts as its own voice, so it can't repeat a numbered voice
    of the same group. Repeats within one (group, voice) are left to the exact-duplicate rule."""
    places: dict[str, list[tuple[str, str, int, str]]] = {}
    for key, voices in bank.items():
        for voice, lines in voices.items():
            for i, line in enumerate(lines, 1):
                places.setdefault(line.strip().lower(), []).append((key, voice, i, line))
    problems = []
    for entries in places.values():
        if len({(key, voice) for key, voice, _, _ in entries}) < 2:
            continue
        where = ", ".join(f"{key}/{voice} #{i}" for key, voice, i, _ in entries)
        problems.append(f"{name}: shared line in {where}: {entries[0][3]}")
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
    # CP26: the same line may not sit in two voices or two groups (checked for the whole bank, once).
    problems += shared_lines(name, bank)
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


def tokens(line: str) -> set[str]:
    return set(TOKEN.findall(line.lower()))


def near_duplicates(name: str, bank: dict) -> list[str]:
    """CP22: warnings for line pairs in one group and voice with Jaccard similarity >= NEAR_DUPLICATE.
    Exact duplicates (same text ignoring case and edge spaces) are already problems, so they are skipped."""
    warnings = []
    for key, voices in bank.items():
        for voice, lines in voices.items():
            sets = [tokens(line) for line in lines]
            for i, first in enumerate(sets):
                for j in range(i + 1, len(sets)):
                    second = sets[j]
                    if not first or not second:
                        continue
                    if lines[i].strip().lower() == lines[j].strip().lower():
                        continue
                    similarity = len(first & second) / len(first | second)
                    if similarity >= NEAR_DUPLICATE:
                        warnings.append(f"{name}: {key}/{voice} #{i + 1} and #{j + 1} are near-duplicates "
                                        f"(Jaccard {similarity:.2f}): {lines[i]} / {lines[j]}")
    return warnings


OPENING_WORD_MAX = 5


def opening_words(name: str, bank: dict) -> list[str]:
    """CP23: warn when more than OPENING_WORD_MAX lines in one group and voice start with the same word.
    The first token is lowercased, so quotes and punctuation before it are ignored; a {slot} is its own token."""
    warnings = []
    for key, voices in bank.items():
        for voice, lines in voices.items():
            counts: dict[str, int] = {}
            for line in lines:
                first = TOKEN.search(line.strip())
                if first is None:
                    continue
                word = first.group(0).lower()
                counts[word] = counts.get(word, 0) + 1
            for word, count in counts.items():
                if count > OPENING_WORD_MAX:
                    warnings.append(f"{name}: {key}/{voice} has {count} lines starting with {word!r} "
                                    f"(max {OPENING_WORD_MAX})")
    return warnings


# CP27: columns of the coverage table; a voice cell is blank when the group has no lines for that voice.
REPORT_VOICES = ("0", "1", "2", "any")
REPORT_HEADER = "| Group | Voice 0 | Voice 1 | Voice 2 | Any | Avg words | Longest (words) |"


def _report_row(label: str, counts: dict[str, int], words: list[int]) -> str:
    cells = [str(counts[v]) if counts.get(v) else "" for v in REPORT_VOICES]
    average = f"{sum(words) / len(words):.1f}" if words else ""
    longest = str(max(words)) if words else ""
    return "| " + " | ".join([label, *cells, average, longest]) + " |"


def coverage_table(banks: list[dict]) -> str:
    """CP27: markdown table over the raw lines (a {slot} counts as one word), one row per bank key sorted,
    then a Total row. Average words is one decimal; blank when a group has no lines."""
    groups: dict[str, tuple[dict[str, int], list[int]]] = {}
    total_counts: dict[str, int] = {}
    total_words: list[int] = []
    for bank in banks:
        for key, voices in bank.items():
            counts, words = groups.setdefault(key, ({}, []))
            for voice, lines in voices.items():
                counts[voice] = counts.get(voice, 0) + len(lines)
                total_counts[voice] = total_counts.get(voice, 0) + len(lines)
                row_words = [len(line.split()) for line in lines]
                words.extend(row_words)
                total_words.extend(row_words)
    rows = [REPORT_HEADER, "| " + " | ".join(["---"] * 7) + " |"]
    rows += [_report_row(key, *groups[key]) for key in sorted(groups)]
    rows.append(_report_row("Total", total_counts, total_words))
    return "\n".join(rows)


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
    parser.add_argument("--strict", action="store_true", help="treat warnings (near-duplicates) as problems")
    parser.add_argument("--report", action="store_true", help="print a markdown coverage table after the checks (CP27)")
    args = parser.parse_args()
    rules = json.loads(RULES.read_text(encoding="utf-8"))
    problems: list[str] = []
    warnings: list[str] = []
    counted = 0
    shipped: list[dict] = []
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
        shipped.append(bank)
        warnings += near_duplicates(bank_path.name, bank)
        warnings += opening_words(bank_path.name, bank)
        if bank_path.name != "en.json":
            # SC06: another language may only use slots that need no translation.
            for key, voices in bank.items():
                for voice, lines in voices.items():
                    for line in lines:
                        for slot in SLOT.findall(line):
                            if slot not in rules.get("localizedSlots", []):
                                problems.append(f"{bank_path.name}: {key}/{voice} uses {{{slot}}}, whose value is English: {line}")
        counted += sum(len(v) for voices in bank.values() for v in voices.values())
    if args.review:
        for review in sorted(REVIEW.glob("companion-lines-*.md")):
            _, bank = parse_review(review)
            problems += check_bank(review.name, bank, rules)
            warnings += near_duplicates(review.name, bank)
            warnings += opening_words(review.name, bank)
            counted += sum(len(v) for voices in bank.values() for v in voices.values())
    if args.strict:
        problems += warnings
        warnings = []
    for p in problems:
        print("FAIL " + p)
    for w in warnings:
        print("warning: " + w)
    print(f"Companion line bank: {counted} lines checked, {len(problems)} problems, {len(warnings)} warnings.")
    if args.report:
        print()
        print(coverage_table(shipped))
    return 1 if problems else 0


if __name__ == "__main__":
    sys.exit(main())
