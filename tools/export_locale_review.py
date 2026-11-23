#!/usr/bin/env python3
"""Export a Markdown review packet for one Evergrove (Mosselora) locale catalog.

Source of truth:
- English keys and text: the parallel `Ids` and `English` string arrays in
  games/evergrove/Assets/Scripts/Core/GameLocalization.cs.
- Locale text: games/evergrove/Assets/Resources/Evergrove/Locales/<code>.json
  with the shape {"code": ..., "entries": [{"key": ..., "value": ...}]}.

The packet is for a human native reviewer. The placeholder column is only a
mechanical flag (for example {0} present in English but missing in the
translation). It is not a translation quality check and does not certify
anything.

Usage:
  python3 tools/export_locale_review.py ja [--out PATH]

Exit codes: 0 success, 2 unknown locale, missing/extra/duplicate key or an
unparseable source.
"""
from __future__ import annotations

import argparse
import datetime as dt
import json
import re
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
GAME = ROOT / "games" / "evergrove"
LOCALIZATION = GAME / "Assets" / "Scripts" / "Core" / "GameLocalization.cs"
CATALOG_DIR = GAME / "Assets" / "Resources" / "Evergrove" / "Locales"
SOURCE_LOCALE = "en"

PLACEHOLDER = re.compile(r"\{(\d+)\}")
CS_STRING = re.compile(r'"((?:[^"\\]|\\.)*)"')
CS_LANGUAGE = re.compile(r'new GameLanguage\("([^"]+)",\s*"((?:[^"\\]|\\.)*)"')
CS_SIMPLE_ESCAPES = {"n": "\n", "t": "\t", "r": "\r", "\\": "\\", '"': '"', "'": "'", "0": "\0"}

# Locale-specific copy that is NOT in the JSON catalogs. Listed in the packet
# so a reviewer knows where to look; existence is checked at export time.
OTHER_LOCALE_SOURCES = [
    ("Language names and right-to-left flags", "games/evergrove/Assets/Scripts/Core/GameLocalization.cs"),
    ("Locale-to-font selection (uGUI fallback)", "games/evergrove/Assets/Scripts/Unity/LocaleRuntime.cs"),
    ("Shipping allowlist (English only until certified)", "games/evergrove/Assets/Scripts/Core/LocaleShippingPolicy.cs"),
    ("Village copy and NPC names (via catalog keys)", "games/evergrove/Assets/Scripts/Core/LocalizedVillage.cs"),
]


class ExportError(Exception):
    """A condition that makes the packet untrustworthy; reported and exit 2."""


def unescape_cs(raw: str) -> str:
    out: list[str] = []
    i = 0
    while i < len(raw):
        ch = raw[i]
        if ch != "\\":
            out.append(ch)
            i += 1
            continue
        if i + 1 >= len(raw):
            raise ExportError("dangling backslash in a C# string literal")
        nxt = raw[i + 1]
        if nxt == "u":
            out.append(chr(int(raw[i + 2 : i + 6], 16)))
            i += 6
        elif nxt in CS_SIMPLE_ESCAPES:
            out.append(CS_SIMPLE_ESCAPES[nxt])
            i += 2
        else:
            raise ExportError(f"unsupported C# escape \\{nxt} in a source string")
    return "".join(out)


def cs_string_array(text: str, name: str) -> list[str]:
    match = re.search(
        r"private static readonly string\[\] " + re.escape(name) + r" = \{(.*?)\};",
        text,
        re.S,
    )
    if not match:
        raise ExportError(f"could not find string array '{name}' in {LOCALIZATION.relative_to(ROOT)}")
    return [unescape_cs(s) for s in CS_STRING.findall(match.group(1))]


def load_source() -> tuple[list[str], list[str], dict[str, str]]:
    if not LOCALIZATION.is_file():
        raise ExportError(f"missing source file {LOCALIZATION.relative_to(ROOT)}")
    text = LOCALIZATION.read_text(encoding="utf-8")
    keys = cs_string_array(text, "Ids")
    english = cs_string_array(text, "English")
    if len(keys) != len(english):
        raise ExportError(f"source key table mismatch: {len(keys)} ids vs {len(english)} English strings")
    if len(set(keys)) != len(keys):
        raise ExportError("duplicate key in the source Ids table")
    languages = {code: unescape_cs(native) for code, native in CS_LANGUAGE.findall(text)}
    if not languages:
        raise ExportError("could not find the GameLanguage table in the source file")
    return keys, english, languages


def load_catalog(code: str, keys: list[str]) -> dict[str, str]:
    path = CATALOG_DIR / f"{code}.json"
    rel = path.relative_to(ROOT)
    if not path.is_file():
        raise ExportError(f"no catalog for '{code}' at {rel}")
    data = json.loads(path.read_text(encoding="utf-8"))
    if data.get("code") != code:
        raise ExportError(f"{rel}: code field is {data.get('code')!r}, expected {code!r}")
    values: dict[str, str] = {}
    for item in data.get("entries", []):
        key = item.get("key")
        if key in values:
            raise ExportError(f"{rel}: duplicate key {key!r}")
        value = item.get("value", "")
        if not isinstance(value, str) or not value.strip():
            raise ExportError(f"{rel}: empty value for key {key!r}")
        values[key] = value
    missing = [k for k in keys if k not in values]
    if missing:
        raise ExportError(f"{rel}: missing {len(missing)} key(s), first: {', '.join(missing[:8])}")
    extra = [k for k in values if k not in set(keys)]
    if extra:
        raise ExportError(f"{rel}: {len(extra)} key(s) not in the source table, first: {', '.join(extra[:8])}")
    return values


def placeholder_flag(english: str, local: str, code: str) -> str:
    en_slots = sorted(PLACEHOLDER.findall(english), key=int)
    lo_slots = sorted(PLACEHOLDER.findall(local), key=int)
    notes: list[str] = []
    if en_slots != lo_slots:
        notes.append(f"MISMATCH en {fmt_slots(en_slots)} / {code} {fmt_slots(lo_slots)}")
    if re.search(r"[{}]", PLACEHOLDER.sub("", local)):
        notes.append("STRAY BRACE (string.Format risk)")
    return "; ".join(notes) if notes else "OK"


def fmt_slots(slots: list[str]) -> str:
    return "none" if not slots else ",".join("{" + s + "}" for s in slots)


def cell(text: str) -> str:
    return text.replace("|", "\\|").replace("\r\n", " ").replace("\n", " ")


def git(*args: str) -> subprocess.CompletedProcess:
    return subprocess.run(["git", "-C", str(ROOT), *args], capture_output=True, text=True, check=False)


def source_state(paths: list[str]) -> tuple[str, str]:
    head = git("rev-parse", "HEAD")
    if head.returncode != 0:
        raise ExportError("git rev-parse HEAD failed; run from a git checkout")
    dirty = git("status", "--porcelain", "--", *paths)
    changed = [line[3:] for line in dirty.stdout.splitlines() if line.strip()]
    note = "clean for these files" if not changed else "UNCOMMITTED changes: " + ", ".join(changed)
    return head.stdout.strip(), note


def render(code: str, native: str, keys: list[str], english: list[str], values: dict[str, str]) -> tuple[str, int]:
    catalog_rel = (CATALOG_DIR / f"{code}.json").relative_to(ROOT).as_posix()
    source_rel = LOCALIZATION.relative_to(ROOT).as_posix()
    sha, dirty_note = source_state([source_rel, catalog_rel])
    today = dt.date.today().isoformat()

    rows: list[str] = []
    flags = 0
    for key, en in zip(keys, english):
        local = values[key]
        flag = placeholder_flag(en, local, code)
        if flag != "OK":
            flags += 1
        rows.append(f"| `{cell(key)}` | {cell(en)} | {cell(local)} | {cell(flag)} |  |  |")

    lines = [
        f"# Native review packet: Mosselora ({code})",
        "",
        "**Status: Draft — not native-reviewed. Simulator screenshots only; physical-device and store-copy review are separate steps.**",
        "",
        f"- Public name: Mosselora (internal codename Evergrove; do not put reviewer identities in this repo)",
        f"- Locale: `{code}` ({cell(native)})",
        f"- Source commit: `{sha}` (`git rev-parse HEAD`)",
        f"- Localization source and catalog at that commit vs working tree: {dirty_note}",
        f"- Generated: {today} by `tools/export_locale_review.py`",
        f"- Keys: {len(keys)} (source table in `{source_rel}`; catalog `{catalog_rel}` has {len(values)} entries)",
        f"- Placeholder flags: {flags}",
        "",
        "Other locale-specific copy, outside the catalog:",
        "",
    ]
    for label, rel in OTHER_LOCALE_SOURCES:
        present = "" if (ROOT / rel).is_file() else " (MISSING FILE)"
        lines.append(f"- {label}: `{rel}`{present}")
    lines += [
        "- Dev-only buttons (for example `Start matched control (dev)`) are English and not player-facing; they are not in the catalog.",
        "",
        "Verdict: OK / Change / Question. Leave the verdict and note blank in this file; record reviews privately as described in the sign-off section.",
        "",
        "| Key | English source | " + code + " text | Placeholders | Verdict | Reviewer note |",
        "| --- | --- | --- | --- | --- | --- |",
        *rows,
        "",
        *review_footer(code),
    ]
    return "\n".join(lines), len(keys)


SCREENSHOT_DIR = ROOT / "games/evergrove/docs/qa/ios-simulator-2026-11-23/locales"
SIGN_OFF_FIELDS = (
    "Internal reviewer ID (private record kept outside the repo)",
    "Native reviewer qualification (summary, no contact details)",
    "Review date",
    "Source commit (from header)",
    "Build hash tested on device",
    "Screenshot set reviewed",
    "Placeholders reviewed (Y/N, notes)",
    "Glyph coverage (device, font)",
    "Bidi / Indic shaping (N/A where not applicable)",
    "Line breaks (notes)",
    "Compact layout (iPhone, Android, large text)",
    "Store copy reviewed (Y/N)",
    "Decision (Pass / Pass with changes / Fail / Deferred)",
    "Outstanding changes (verdict Change / Question rows)",
)


def review_footer(code: str) -> list[str]:
    """Guidance link, simulator screenshots for the locale and a blank sign-off block.

    Links are relative to games/evergrove/docs/qa/native-review/, where packets live.
    """
    lines = [
        "## How to review",
        "",
        "Follow the shared guidance in [README.md](README.md): how to review, what to judge and what not to do.",
        "",
        "Simulator screenshots for this locale (context only, not device evidence):",
        "",
    ]
    shots = sorted((SCREENSHOT_DIR / code).glob("*.png"))
    if shots:
        for shot in shots:
            lines.append(f"- [{shot.stem}](../ios-simulator-2026-11-23/locales/{code}/{shot.name})")
    else:
        lines.append("- None captured for this locale yet.")
    lines += [
        "",
        "## Sign-off block (leave blank in this repository)",
        "",
        "| Field | Value |",
        "| --- | --- |",
        f"| Locale | {code} |",
        *[f"| {field} | |" for field in SIGN_OFF_FIELDS],
        "",
    ]
    return lines


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description="Export a native-review Markdown packet for one locale.")
    parser.add_argument("locale", help="locale code, for example ja or vi")
    parser.add_argument("--out", type=Path, help="write to this file instead of stdout")
    args = parser.parse_args(argv)

    try:
        keys, english, languages = load_source()
        code = args.locale
        if code == SOURCE_LOCALE:
            raise ExportError("'en' is the source locale; there is nothing to translate-review")
        if code not in languages:
            raise ExportError(f"unknown locale '{code}'. Supported: {', '.join(sorted(languages))}")
        values = load_catalog(code, keys)
        markdown, count = render(code, languages[code], keys, english, values)
    except ExportError as err:
        print(f"export_locale_review: error: {err}", file=sys.stderr)
        return 2

    if args.out:
        out = args.out if args.out.is_absolute() else ROOT / args.out
        out.parent.mkdir(parents=True, exist_ok=True)
        out.write_text(markdown, encoding="utf-8")
        print(f"wrote {out} ({count} keys)", file=sys.stderr)
    else:
        sys.stdout.write(markdown)
    return 0


if __name__ == "__main__":
    sys.exit(main())
