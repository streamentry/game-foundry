#!/usr/bin/env python3
"""Export the Mosselora companion line bank to a CSV for translators (smart companion CP29).

One row per line in the bank. Columns: id, group, kind, qualifier, voice, speaker, english, slots,
localizable_slots, notes, translation. The translation column is left empty for the translator.

id is the same value as the C# LineBank.LineId(group, voice, text): "cl_" plus 8 hex digits of a
32-bit FNV-1a hash over the UTF-16 code units of "group|voice|text". The voice in the hash is 0, 1,
2, or -1 for the "any" voice (the voice column then reads "any"). speaker is the neighbour who says
voice 0 (Mira), 1 (Orin) or 2 (Fen); "any" lines have no speaker.

The CSV is UTF-8 with a byte order mark so spreadsheet apps open it correctly, and every field is
quoted. The script only reads the bank and the rules; it never writes under games/evergrove/Assets.

Usage:
  python3 tools/evergrove_line_bank_export.py [--bank PATH] [--rules PATH] --out PATH.csv

--bank defaults to games/evergrove/Assets/Resources/Evergrove/Lines/en.json and --rules to
games/evergrove/docs/specs/companion-line-rules.json, both relative to the repository root.
Exit 0 on success, 1 on a bad bank, rules file or output path. Never calls a network service.
"""
from __future__ import annotations

import argparse
import csv
import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
GAME = ROOT / "games" / "evergrove"
DEFAULT_BANK = GAME / "Assets" / "Resources" / "Evergrove" / "Lines" / "en.json"
DEFAULT_RULES = GAME / "docs" / "specs" / "companion-line-rules.json"
ASSETS = GAME / "Assets"

COLUMNS = [
    "id", "group", "kind", "qualifier", "voice", "speaker", "english",
    "slots", "localizable_slots", "notes", "translation",
]
ANY_VOICE = "any"
SPEAKERS = {"0": "Mira", "1": "Orin", "2": "Fen"}
SLOT = re.compile(r"\{([^{}]*)\}")
FNV_OFFSET = 2166136261
FNV_PRIME = 16777619
MASK32 = 0xFFFFFFFF

# What each fact kind is about, in one plain sentence (from the FactKind comments in
# games/evergrove/Assets/Scripts/Core/CompanionFacts.cs). Keyed by kind, not by group.
KIND_NOTES = {
    "FruitGiven": "You gave this neighbour fruit from your garden, and they thank you for it.",
    "SolvedTogether": "A puzzle of this neighbour's kind was solved by taking turns with you.",
    "WeatherWhileAway": "The weather since your last visit, as this neighbour remembers it.",
    "LetterWaiting": "A letter is waiting in the letter box that you have not read on this device yet.",
    "GiftWaiting": "A cutting from family is waiting in the basket for you.",
    "HelpedBy": "A relative helped you with one of this neighbour's kind of puzzle.",
    "NewArrival": "A nursery plant of this neighbour's kind has arrived, and its puzzle is not solved yet.",
    "RiverClose": "You tended one more thing, and the mist lifts from the river.",
    "RiverFurther": "The river runs further than ever, and this is the season it brought.",
    "KindSolved": "How many puzzles of this neighbour's kind you have solved.",
    "WeatherToday": "Today's weather (rain, wind, heat or snow) as this neighbour sees it.",
    "PairingToFind": "A good-neighbour pairing of this neighbour's kind that you have not found yet.",
    "Hour": "The light at this moment: dawn, golden hour, dusk or night.",
    "NewestThing": "The newest thing of this neighbour's kind and where it stands in the garden.",
    "VisitorCame": "A visitor your good neighbours brought: an owl, a heron or a hedgehog.",
    "HourHabit": "The time of day you usually come to the garden, as this neighbour noticed it.",
    "WishProgress": "A hope this neighbour has, partly come true, with the true count so far.",
    "SeasonTurned": "The season the garden is in now, said once each time the season turns.",
    "YieldGiven": "What this season gave the garden: seedlings, seeds, fruit or jam.",
    "PairingFound": "A good-neighbour pairing you have already found.",
    "RainComing": "Rain upstream today that reaches the garden tomorrow.",
}


def line_id(group: str, voice: int, text: str) -> str:
    """CP21: the same ID as LineBank.LineId; FNV-1a over the UTF-16 code units of group|voice|text."""
    key = f"{group or ''}|{voice}|{text or ''}".encode("utf-16-le")
    h = FNV_OFFSET
    for i in range(0, len(key), 2):
        unit = key[i] | (key[i + 1] << 8)
        h ^= unit
        h = (h * FNV_PRIME) & MASK32
    return f"cl_{h:08x}"


def reject_duplicate_keys(pairs: list[tuple[str, object]]) -> dict:
    """Mirrors the C# bank reader: a key written twice is an error, not a silent overwrite."""
    result: dict = {}
    for key, value in pairs:
        if key in result:
            raise ValueError(f"duplicate key in bank: {key}")
        result[key] = value
    return result


def load_json(path: Path) -> dict:
    try:
        with path.open(encoding="utf-8") as handle:
            data = json.load(handle, object_pairs_hook=reject_duplicate_keys)
    except (OSError, json.JSONDecodeError, ValueError) as error:
        raise ValueError(f"cannot read {path}: {error}") from error
    if not isinstance(data, dict):
        raise ValueError(f"{path} must be a JSON object")
    return data


def split_group(group: str) -> tuple[str, str]:
    kind, _, qualifier = group.partition(".")
    return kind, qualifier


def slots_in(text: str) -> list[str]:
    seen: list[str] = []
    for name in SLOT.findall(text):
        if name not in seen:
            seen.append(name)
    return seen


def notes_for(kind: str, qualifier: str, slots: list[str], localizable: list[str]) -> str:
    parts = [KIND_NOTES[kind]]
    if qualifier:
        parts.append(f"This wording is for {qualifier}.")
    missing = [s for s in slots if s not in localizable]
    if missing:
        first = "{" + missing[0] + "}"
        etc = " etc." if len(missing) > 1 else ""
        parts.append(f"Keep {first}{etc} as is: this line will not be used in your language until those words are translated.")
    return " ".join(parts)


def build_rows(bank: dict, rules: dict) -> list[list[str]]:
    kinds = rules.get("kinds")
    if not isinstance(kinds, dict):
        raise ValueError("rules file has no kinds")
    localizable = set(rules.get("localizedSlots", []))
    rows: list[list[str]] = []
    for group, voices in bank.items():
        kind, qualifier = split_group(group)
        if kind not in KIND_NOTES or kind not in kinds:
            raise ValueError(f"unknown fact kind in bank: {group}")
        if not isinstance(voices, dict):
            raise ValueError(f"{group} must map voices to lists of lines")
        for voice_key, lines in voices.items():
            if voice_key not in ("0", "1", "2", ANY_VOICE):
                raise ValueError(f"unknown voice in bank: {group}/{voice_key}")
            if not isinstance(lines, list):
                raise ValueError(f"{group}/{voice_key} must be a list of lines")
            voice_number = -1 if voice_key == ANY_VOICE else int(voice_key)
            speaker = SPEAKERS.get(voice_key, "")
            for text in lines:
                if not isinstance(text, str):
                    raise ValueError(f"{group}/{voice_key} has a line that is not text")
                slots = slots_in(text)
                local = [s for s in slots if s in localizable]
                rows.append([
                    line_id(group, voice_number, text),
                    group,
                    kind,
                    qualifier,
                    voice_key,
                    speaker,
                    text,
                    ", ".join("{" + s + "}" for s in slots),
                    ", ".join("{" + s + "}" for s in local),
                    notes_for(kind, qualifier, slots, sorted(localizable)),
                    "",
                ])
    return rows


def write_csv(rows: list[list[str]], out: Path) -> None:
    out.parent.mkdir(parents=True, exist_ok=True)
    with out.open("w", encoding="utf-8-sig", newline="") as handle:
        writer = csv.writer(handle, quoting=csv.QUOTE_ALL)
        writer.writerow(COLUMNS)
        writer.writerows(rows)


def export(bank_path: Path, rules_path: Path, out: Path) -> int:
    """Writes the CSV and returns the number of line rows."""
    resolved = out.resolve()
    if ASSETS.resolve() in resolved.parents or resolved == ASSETS.resolve():
        raise ValueError("refusing to write under games/evergrove/Assets")
    rows = build_rows(load_json(bank_path), load_json(rules_path))
    write_csv(rows, out)
    return len(rows)


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description="Export the companion line bank to a translator CSV.")
    parser.add_argument("--bank", type=Path, default=DEFAULT_BANK)
    parser.add_argument("--rules", type=Path, default=DEFAULT_RULES)
    parser.add_argument("--out", type=Path, required=True)
    args = parser.parse_args(argv)
    try:
        count = export(args.bank, args.rules, args.out)
    except ValueError as error:
        print(f"error: {error}", file=sys.stderr)
        return 1
    print(f"wrote {count} rows to {args.out}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
