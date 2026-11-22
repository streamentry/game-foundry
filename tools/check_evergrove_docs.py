#!/usr/bin/env python3
"""Keep Evergrove's canonical handbook navigable and source-aligned.

This is an inexpensive source-contract guard. It is NOT a Unity compile,
native device QA, original market research or a translation quality audit.
"""
from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
GAME = ROOT / "games" / "evergrove"
DOCS = GAME / "docs"

REQUIRED = [
    "README.md", "PRODUCT.md", "GAMEPLAY_AND_WORLD_RULES.md",
    "ARCHITECTURE.md", "SAVE_AND_MIGRATION.md", "PLAYER_EXPERIENCE.md",
    "VALIDATION.md", "BUILD_AND_QA.md", "DECISIONS_AND_RISKS.md",
    "TOM_TAT.vi.md",
]

def fail(message: str) -> None:
    raise AssertionError(message)

def source_text(path: Path) -> str:
    if not path.is_file():
        fail(f"Missing required documentation: {path.relative_to(ROOT)}")
    return path.read_text(encoding="utf-8")

def main() -> int:
    index = source_text(DOCS / "README.md")
    for name in REQUIRED:
        if name != "README.md" and f"]({name})" not in index:
            fail(f"Missing handbook navigation link: {name}")

    target_docs = [DOCS / name for name in REQUIRED] + [
        GAME / "README.md", GAME / "AGENTS.md",
    ] + sorted((DOCS / "specs").glob("*.md"))
    checked_links = 0
    for path in target_docs:
        body = source_text(path)
        if "§" in body:
            fail(f"Markdown placeholder was not replaced in {path}")
        if body.count("```") % 2:
            fail(f"Unmatched fenced code block in {path}")
        if not body.startswith("# "):
            fail(f"Missing title in {path}")
        if len(body.split()) < 150:
            fail(f"Suspiciously incomplete handbook file: {path}")

        # Ignore full URLs, fragments, email addresses and Markdown image refs.
        for target in re.findall(r"(?<!!)\[[^\]]+\]\(([^)]+)\)", body):
            base = target.split("#", 1)[0]
            if not base or "://" in base or base.startswith(("mailto:", "/")):
                continue
            resolved = (path.parent / base).resolve()
            if not resolved.is_relative_to(ROOT):
                fail(f"Local link escapes repo: {path}: {target}")
            if not resolved.is_file():
                fail(f"Broken relative documentation link: {path}: {target}")
            checked_links += 1

    pipe = source_text(GAME / "Assets/Scripts/Core/PipeRules.cs")
    world = source_text(GAME / "Assets/Scripts/Core/WorldModel.cs")
    branch = source_text(GAME / "Assets/Scripts/Core/ReactiveWatercourses.cs")
    locale = source_text(GAME / "Assets/Scripts/Core/GameLocalization.cs")
    if pipe.count("new Puzzle(") != 12:
        fail("PuzzleCatalog no longer has exactly twelve authored fixtures; review handbook")
    if "public const int Version = 4" not in world:
        fail("World snapshot version changed; update save handbook and migration QA")
    if "int turns = variant;" not in branch or "AnchoredTile" not in pipe:
        fail("Branch implementation changed; audit gameplay handbook")
    if locale.count("new GameLanguage(") != 30:
        fail("Language registry changed; update handbook language count")
    keys = re.search(
        r"private static readonly string\[\] Ids = \{([\s\S]*?)\};", locale
    )
    if not keys or len(re.findall(r'"[^"]+"', keys.group(1))) != 81:
        fail("Localization key count changed; update documentation and catalog validator")

    print(
        f"Evergrove documentation contract: {len(target_docs)} documents, "
        f"{checked_links} verified local links, 12 puzzles, "
        "v4 saves, 30 language choices, 81 keys."
    )
    return 0

if __name__ == "__main__":
    try:
        sys.exit(main())
    except (AssertionError, OSError, UnicodeError) as exc:
        print(f"Evergrove documentation contract FAILED: {exc}", file=sys.stderr)
        sys.exit(1)
