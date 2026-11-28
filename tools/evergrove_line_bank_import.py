#!/usr/bin/env python3
"""Check a translator's filled-in companion line CSV and write a DRAFT review file (smart companion CP30).

Reads a CSV in the CP29 export format (tools/evergrove_line_bank_export.py) where some rows have a
translation. Each filled row is checked against the English bank and the rules:

- refused (exit 1): the id is not a line in the English bank, the English text no longer matches the
  bank (re-export), the id appears twice, the translation has braces that are not a valid {slot}, it
  uses a slot outside localizedSlots, it adds a slot the English line does not have, or it drops a
  localizable slot the English line had;
- skipped with a note: the English line uses a slot that is not localizable yet, so that line cannot be
  said in this language (SC06). Skipped rows are not written and are not refusals;
- accepted otherwise, and written to <out-dir>/companion-lines-draft.md as a DRAFT.

The draft is never a JSON bank and is never ready for Resources until a native speaker reviews it.
The tool refuses any out-dir inside games/evergrove/Assets (exit 2) and writes nothing else.

Usage:
  python3 tools/evergrove_line_bank_import.py --locale fr --csv PATH.csv
      [--bank PATH] [--rules PATH] [--out-dir docs/improvements/review/fr]

--bank defaults to games/evergrove/Assets/Resources/Evergrove/Lines/en.json and --rules to
games/evergrove/docs/specs/companion-line-rules.json. The default out-dir is
docs/improvements/review/<locale> under the repository root.

Exit codes: 0 when every filled row is accepted or skipped; 1 when any row is refused (the draft still
holds the accepted rows) or when an input file cannot be read; 2 for a bad --locale or an out-dir under
games/evergrove/Assets.
"""
from __future__ import annotations

import argparse
import csv
import re
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
import evergrove_line_bank_export as exporter  # noqa: E402  (same directory; reuses the CP21 id and CP29 reader)

ROOT = exporter.ROOT
ASSETS = exporter.ASSETS
DEFAULT_BANK = exporter.DEFAULT_BANK
DEFAULT_RULES = exporter.DEFAULT_RULES
DRAFT_NAME = "companion-lines-draft.md"
DRAFT_HEADER = "DRAFT — not reviewed by a native speaker; do not copy into Resources until reviewed."
LOCALE = re.compile(r"^[a-z]{2,3}(-[A-Za-z0-9]{2,8})?$")
REQUIRED_COLUMNS = ("id", "english", "translation")
VALID_SLOT = re.compile(r"\{([A-Za-z_][A-Za-z0-9_]*)\}")


def is_under_assets(path: Path) -> bool:
    resolved = path.resolve()
    assets = ASSETS.resolve()
    return resolved == assets or assets in resolved.parents


def read_translations(path: Path) -> list[dict]:
    try:
        with path.open(encoding="utf-8-sig", newline="") as handle:
            reader = csv.DictReader(handle)
            missing = [column for column in REQUIRED_COLUMNS if column not in (reader.fieldnames or [])]
            if missing:
                raise ValueError(f"CSV is missing columns: {', '.join(missing)}")
            return list(reader)
    except (OSError, csv.Error, UnicodeDecodeError) as error:
        raise ValueError(f"cannot read {path}: {error}") from error


def check_translation(translation: str, english: str, localizable: set[str]) -> str | None:
    """Returns the refusal reason, or None when the translation keeps exactly the English localizable slots."""
    if "{" in VALID_SLOT.sub("", translation) or "}" in VALID_SLOT.sub("", translation):
        return "braces that are not a valid {slot}"
    used = sorted(set(VALID_SLOT.findall(translation)))
    english_slots = exporter.slots_in(english)
    for slot in used:
        if slot not in localizable:
            return f"uses {{{slot}}}, which is not a localizable slot"
        if slot not in english_slots:
            return f"adds {{{slot}}}, which the English line does not have"
    for slot in english_slots:
        if slot in localizable and slot not in used:
            return f"drops {{{slot}}}"
    return None


def review(
    csv_rows: list[dict], bank_rows: list[dict], localizable: set[str]
) -> tuple[dict[str, str], list[str], list[str]]:
    """Returns (accepted id -> translation, refusal messages, skip messages)."""
    by_id = {row["id"]: row for row in bank_rows}
    accepted: dict[str, str] = {}
    refused: list[str] = []
    skipped: list[str] = []
    seen: set[str] = set()
    for line in csv_rows:
        translation = (line.get("translation") or "").strip()
        if not translation:
            continue
        line_id = (line.get("id") or "").strip()
        if line_id in seen:
            refused.append(f"{line_id}: id appears more than once in the CSV")
            continue
        seen.add(line_id)
        bank_row = by_id.get(line_id)
        if bank_row is None:
            refused.append(f"{line_id}: not a line in the English bank (re-export the CSV)")
            continue
        english = bank_row["english"]
        if (line.get("english") or "") != english:
            refused.append(f"{line_id}: English text differs from the bank (re-export the CSV)")
            continue
        non_localizable = [s for s in exporter.slots_in(english) if s not in localizable]
        if non_localizable:
            names = ", ".join("{" + s + "}" for s in non_localizable)
            skipped.append(f"{line_id}: English uses {names}, not localizable yet")
            continue
        reason = check_translation(translation, english, localizable)
        if reason is not None:
            refused.append(f"{line_id}: {reason}")
        else:
            accepted[line_id] = " ".join(translation.split())
    return accepted, refused, skipped


def voice_label(row: dict) -> str:
    if row["voice"] == exporter.ANY_VOICE:
        return "Any neighbour"
    speaker = exporter.SPEAKERS.get(row["voice"], "")
    return f"{speaker} (voice {row['voice']})" if speaker else f"voice {row['voice']}"


def draft_text(bank_rows: list[dict], accepted: dict[str, str], counts: tuple[int, int, int], source: str, locale: str) -> str:
    accepted_count, refused_count, skipped_count = counts
    lines = [
        f"# {DRAFT_HEADER}",
        "",
        f"Locale: {locale}. Source CSV: {source}. Generated by tools/evergrove_line_bank_import.py.",
        f"Accepted {accepted_count}, refused {refused_count} (not included here), "
        f"skipped {skipped_count} (English uses a slot that is not localizable yet).",
    ]
    current_group = None
    current_voice = None
    for row in bank_rows:
        if row["id"] not in accepted:
            continue
        if row["group"] != current_group:
            lines += ["", f"## {row['group']}"]
            current_group = row["group"]
            current_voice = None
        label = voice_label(row)
        if label != current_voice:
            lines += ["", f"### {label}"]
            current_voice = label
        lines.append(f"- {row['id']}: {accepted[row['id']]} (English: {row['english']})")
    return "\n".join(lines) + "\n"


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description="Check a translated companion line CSV and write a DRAFT review file.")
    parser.add_argument("--locale", required=True, help="e.g. fr, de, pt-BR")
    parser.add_argument("--csv", type=Path, required=True, help="CSV from the CP29 export, with translations filled in")
    parser.add_argument("--bank", type=Path, default=DEFAULT_BANK)
    parser.add_argument("--rules", type=Path, default=DEFAULT_RULES)
    parser.add_argument("--out-dir", type=Path, default=None)
    args = parser.parse_args(argv)

    if not LOCALE.match(args.locale):
        print("error: --locale must look like fr, de or pt-BR", file=sys.stderr)
        return 2
    out_dir = args.out_dir or ROOT / "docs" / "improvements" / "review" / args.locale
    if is_under_assets(out_dir):
        print("error: refusing to write under games/evergrove/Assets; use docs/improvements/review/<locale>", file=sys.stderr)
        return 2

    try:
        bank = exporter.load_json(args.bank)
        rules = exporter.load_json(args.rules)
        localizable = set(rules.get("localizedSlots", []))
        bank_rows = [dict(zip(exporter.COLUMNS, row)) for row in exporter.build_rows(bank, rules)]
        csv_rows = read_translations(args.csv)
    except ValueError as error:
        print(f"error: {error}", file=sys.stderr)
        return 1

    accepted, refused, skipped = review(csv_rows, bank_rows, localizable)
    text = draft_text(bank_rows, accepted, (len(accepted), len(refused), len(skipped)), args.csv.name, args.locale)
    out_dir.mkdir(parents=True, exist_ok=True)
    draft = out_dir / DRAFT_NAME
    draft.write_text(text, encoding="utf-8")

    print(f"accepted {len(accepted)}, refused {len(refused)}, skipped {len(skipped)}")
    for message in refused:
        print(f"refused {message}")
    for message in skipped:
        print(f"skipped {message}")
    print(f"wrote {draft}")
    return 1 if refused else 0


if __name__ == "__main__":
    sys.exit(main())
