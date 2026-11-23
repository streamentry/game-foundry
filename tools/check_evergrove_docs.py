#!/usr/bin/env python3
"""Keep Evergrove's canonical handbook navigable and source-aligned.

This is an inexpensive source-contract guard. It is NOT a Unity compile,
native device QA, original market research or a translation quality audit.
"""
from pathlib import Path
import json
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
GAME = ROOT / "games" / "evergrove"
DOCS = GAME / "docs"
SPECS = DOCS / "specs"

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
    ]
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
    if not keys or len(re.findall(r'"[^"]+"', keys.group(1))) != 97:
        fail("Localization key count changed; update documentation and catalog validator")

    # Compare the legacy build handbook against the actual dependency manifest.
    # This would have caught stale uGUI 2.6.0 / 76-key instructions on main.
    manifest = json.loads(source_text(GAME / "Packages/manifest.json"))
    build_handbook = source_text(DOCS / "BUILD_AND_QA.md")
    ugui = manifest["dependencies"]["com.unity.ugui"]
    if f"com.unity.ugui {ugui}" not in build_handbook:
        fail("Build handbook uGUI version differs from the actual manifest")
    canonical_key_count = len(re.findall(r'"[^"]+"', keys.group(1)))
    if f"{canonical_key_count} keys each" not in build_handbook:
        fail("Build handbook key count differs from the localization registry")

    check_specification_contract()

    print(
        f"Evergrove documentation contract: {len(target_docs)} documents, "
        f"{checked_links} verified local links, 12 puzzles, "
        "v4 saves, 30 language choices, 97 keys."
    )
    return 0


SPEC_DOCUMENTS = [
    "README.md",
    "01-PRODUCT-AND-EXPERIENCE.md",
    "02-FEATURE-BACKLOG.md",
    "03-PUZZLES-AND-CONTENT.md",
    "04-WORLD-AND-NARRATIVE.md",
    "05-ACCESSIBILITY-AND-LOCALIZATION.md",
    "06-ENGINEERING-AND-RELEASE.md",
    "07-VALIDATION-AND-GTM.md",
    "08-EVIDENCE-AND-DECISIONS.md",
    "09-EXECUTION-CONTROL.md",
    "10-EXPERIMENT-PROTOCOLS.md",
    "11-EVIDENCE-AND-RELEASE-PACKETS.md",
    "12-M1-M2-EXECUTION-CHECKLIST.md",
]


def check_specification_contract() -> None:
    """Reject stale fake-verified feature claims and source/spec drift.

    This is still static source verification, NOT Unity/native/device QA.
    """
    index = source_text(SPECS / "README.md")
    backlog = source_text(SPECS / "02-FEATURE-BACKLOG.md")
    for name in SPEC_DOCUMENTS:
        body = source_text(SPECS / name)
        if name != "README.md" and f"]({name})" not in index:
            fail(f"Unlinked Evergrove feature spec: {name}")
        if not body.startswith("# ") or body.count("\x60\x60\x60") % 2:
            fail(f"Malformed or unclosed feature spec Markdown: {name}")
        for destination in re.findall(r"(?<!!)\[[^\]]+\]\(([^)]+)\)", body):
            bare = destination.split("#", 1)[0]
            if not bare or "://" in bare or bare.startswith(("mailto:", "/")):
                continue
            path = (SPECS / bare).resolve()
            if not path.is_relative_to(ROOT) or not path.is_file():
                fail(f"Broken relative Evergrove spec link: {name}: {destination}")

    matrix = json.loads(source_text(SPECS / "verification-matrix.json"))
    if matrix.get("schema_version") != 1:
        fail("Unsupported specification verification matrix schema")
    if not re.fullmatch(r"[a-f0-9]{40}", matrix.get("audit_baseline_commit", "")):
        fail("Evergrove spec baseline must use an immutable 40-character commit SHA")
    states = {
        "Proposed", "Ready", "In Progress", "Implemented-Unverified",
        "Verified", "Deferred", "Rejected"
    }
    gates = {"M0", "M1", "M2", "M3", "M4", "M5", "M6"}
    features = matrix.get("features", [])
    tests = matrix.get("tests", [])
    expected_ids = {f"EG-{index:03d}" for index in range(1, 14)}
    expected_ids.update(f"EG-{index:03d}" for index in range(101, 117))
    expected_ids.update(f"EG-{index:03d}" for index in range(201, 209))
    feature_by_id = {feature.get("id"): feature for feature in features}
    if len(features) != 37 or set(feature_by_id) != expected_ids:
        fail("Every one of 37 feature IDs must be present exactly once")
    test_by_id = {case.get("id"): case for case in tests}
    if len(tests) != len(test_by_id) or len(tests) < 40:
        fail("Verification test IDs must be unique and cover all feature areas")
    for test_id, case in test_by_id.items():
        if not re.fullmatch(r"(?:BLD|SAVE|PUZ|LOC|A11Y|PRIV|VAL|WLD|UX|TOOL|PERF|STORE)-\d{3}", test_id):
            fail(f"Invalid stable verification test ID: {test_id}")
        if case.get("gate") not in gates:
            fail(f"Unknown test gate: {test_id}")
        if not case.get("method") or not case.get("passing_criteria") or not case.get("evidence_required"):
            fail(f"Verification case is not actionable: {test_id}")
        if type(case.get("verified")) is not bool:
            fail(f"Verified flag must be boolean: {test_id}")
        if case.get("verified"):
            source_sha = case.get("source_commit_sha", "")
            artifact_sha = case.get("build_artifact_sha256")
            if not re.fullmatch(r"[a-f0-9]{40}", source_sha):
                fail(f"Verified test must pin the exact source commit: {test_id}")
            if case.get("method") != "Automated" and not (
                isinstance(artifact_sha, str) and
                re.fullmatch(r"[a-f0-9]{64}", artifact_sha)
            ):
                fail(f"Non-automated verified test must pin an actual artifact hash: {test_id}")
            if artifact_sha is not None and not re.fullmatch(r"[a-f0-9]{64}", artifact_sha):
                fail(f"Malformed verified artifact digest: {test_id}")
            if not case.get("reviewer_role") or not re.match(
                r"^\d{4}-\d{2}-\d{2}T", case.get("reviewed_at_utc", "")
            ):
                fail(f"Missing verified test reviewer or UTC time: {test_id}")
            path = case.get("evidence_path", "")
            if not path.startswith(f"games/evergrove/docs/evidence/{source_sha}/"):
                fail(f"Verified evidence is not scoped to its source SHA: {test_id}")
            evidence = (ROOT / path).resolve()
            if not evidence.is_relative_to(ROOT) or not evidence.is_file():
                fail(f"Missing verified test evidence: {test_id}")
            proof = source_text(evidence)
            for required in (
                f"test_id: {test_id}",
                f"source_commit_sha: {source_sha}",
                "result: PASS",
            ):
                if required not in proof:
                    fail(f"Evidence packet missing {required}: {test_id}")
            if artifact_sha and f"build_artifact_sha256: {artifact_sha}" not in proof:
                fail(f"Evidence packet hash mismatch: {test_id}")
    for id, feature in feature_by_id.items():
        if feature.get("status") not in states or feature.get("decision_gate") not in gates:
            fail(f"Feature lifecycle/gate invalid: {id}")
        expected_evidence = f"games/evergrove/docs/evidence/<SOURCE_SHA>/{id}/verification.md"
        if feature.get("expected_artifact") != expected_evidence:
            fail(f"Noncanonical source-SHA-scoped evidence path: {id}")
        if type(feature.get("blocked")) is not bool:
            fail(f"Blocked must be an independent boolean for {id}")
        for required in (
            "title", "owner_role", "player_visible_outcome",
            "expected_artifact", "stop_or_rollback", "dependency_notes"
        ):
            if not feature.get(required):
                fail(f"Missing {required} for feature {id}")
        refs = feature.get("verification_ids", [])
        if not refs or any(ref not in test_by_id for ref in refs):
            fail(f"Feature {id} must have valid named verification tests")
        deps = feature.get("dependency_ids", [])
        if id in deps or any(dep not in feature_by_id for dep in deps):
            fail(f"Feature {id} has unknown/cyclic direct dependency")
        if len(refs) != len(set(refs)) or len(deps) != len(set(deps)):
            fail(f"Feature {id} duplicates a test/dependency")
        heading = f"### {id} — {feature['title']}"
        if heading not in backlog:
            fail(f"Feature title/section disagrees with the matrix: {id}")
        start = backlog.index(heading)
        end = backlog.find("\n### ", start + len(heading))
        block = backlog[start:end if end >= 0 else len(backlog)]
        if "**Execution:**" not in block or feature["owner_role"] not in block:
            fail(f"Missing human-visible execution contract for {id}")
        if expected_evidence not in block:
            fail(f"Backlog evidence path disagrees with registry: {id}")
        if feature["decision_gate"] not in block or any(ref not in block for ref in refs):
            fail(f"Stale gate/test ID in human feature spec: {id}")
        if feature.get("status") == "Verified":
            if not feature.get("evidence_artifacts") or not all(
                test_by_id[t]["verified"] for t in refs
            ):
                fail(f"Feature {id} cannot be Verified without every mandatory test")
            for relative in feature["evidence_artifacts"]:
                proof = (ROOT / relative).resolve()
                if not proof.is_relative_to(ROOT) or not proof.is_file():
                    fail(f"Missing feature proof: {id} / {relative}")
        elif feature.get("evidence_artifacts"):
            fail(f"Feature {id} reports evidence without verified lifecycle: review first")

    visited = set()
    active = set()

    def visit(feature_id: str) -> None:
        if feature_id in active:
            fail(f"Feature dependency cycle: {feature_id}")
        if feature_id in visited:
            return
        active.add(feature_id)
        for dep in feature_by_id[feature_id]["dependency_ids"]:
            visit(dep)
        active.remove(feature_id)
        visited.add(feature_id)

    for feature_id in feature_by_id:
        visit(feature_id)

    # Every named test must have a feature owner OR explicit milestone owner.
    # No orphan tests can vanish silently from the release decision.
    linked_ids = {ref for feature in features for ref in feature["verification_ids"]}
    gate_tests = matrix.get("gate_required_verification_ids")
    if not isinstance(gate_tests, dict):
        fail("Missing canonical milestone-level test ownership map")
    for gate, refs in gate_tests.items():
        if gate not in gates or not isinstance(refs, list):
            fail(f"Invalid gate-wide test list: {gate}")
        if len(refs) != len(set(refs)):
            fail(f"Duplicate gate-wide test assignment: {gate}")
        for ref in refs:
            if ref not in test_by_id or test_by_id[ref]["gate"] != gate:
                fail(f"Unknown or wrongly assigned milestone test: {gate}/{ref}")
            linked_ids.add(ref)
    if set(test_by_id) != linked_ids:
        fail(f"Unowned verification tests: {sorted(set(test_by_id) - linked_ids)}")

    # Static release isolation is a prerequisite, never a runtime-certification.
    ui = source_text(GAME / "Assets/Scripts/Unity/EvergroveApp.cs")
    builder = source_text(GAME / "Assets/Editor/EvergroveBuild.cs")
    if ui.count("#if UNITY_EDITOR || DEVELOPMENT_BUILD") < 4 or \
            "options = BuildOptions.Development" not in builder:
        fail("Research-only controls lack explicit development-build isolation")
    if not re.search(
        r"#if UNITY_EDITOR \|\| DEVELOPMENT_BUILD[\s\S]*?"
        r"if \(world\.CanSwitchMode\)[\s\S]*?#endif",
        ui
    ):
        fail("Puzzle Only control must be inside development-build guard")
    if not re.search(
        r"#if UNITY_EDITOR \|\| DEVELOPMENT_BUILD[\s\S]*?"
        r'Button\(content, "Playtest consent"[\s\S]*?#endif',
        ui
    ):
        fail("Local playtest consent/export must be gated from consumer UI")
    if 'marker.text = seenEnds == 1 ? "S" : "G"' in ui:
        fail("Nonlocalized S/G board markers regressed")
    if "DrawEndpointIcon(markerNode.transform, spring, markerScale)" not in ui:
        fail("Language-neutral board endpoint markers are missing")
    store = source_text(GAME / "Assets/Scripts/Unity/WorldStore.cs")
    journal = source_text(GAME / "Assets/Scripts/Core/SaveJournal.cs")
    app = source_text(GAME / "Assets/Scripts/Unity/EvergroveApp.cs")
    test_runner = source_text(GAME / "Tests/Program.cs")
    if "evergrove-world-v2.json" not in store:
        fail("Save filename changed without migration contract review")
    for marker in (
        "SaveJournal.TryLoad", "SaveJournal.Commit", "RecoveryBlocked",
        "SaveJournal.HasAnySlot", "public static bool Clear()",
    ):
        if marker not in store:
            fail(f"Save recovery guard missing from Unity adapter: {marker}")
    for marker in ('ReadValid(primary + ".bak"', 'ReadValid(primary + ".tmp"',
                   'ReadValid(primary, accepts, out oldPrimary)', 'File.Copy(primary, backup, true)',
                   'File.Copy(staged, backup, true)', 'if (!stagedValid && HasAnySlot(primary))'):
        if marker not in journal:
            fail(f"Save journal last-good-copy contract drift: {marker}")
    if ("SaveJournalTests.Run();" not in test_runner or
            "SaveJournalTests.cs" not in source_text(DOCS / "SAVE_AND_MIGRATION.md")):
        fail("Save journal interruption tests are untracked")
    if "WorldStore.RecoveryBlocked" not in app or "ConfirmedReset()" not in app:
        fail("Unreadable save lacks a visible explicit reset path")
    # A save warning *inside* scroll content can be invisible when a tile
    # write fails and can remain on screen after a later successful save.
    for marker in (
        'Node("Persistent save warning", scrollNode.transform)',
        "saveWarningBanner.SetActive(saveFailed);",
        "warningViewport.offsetMax = new Vector2(0f, saveFailed ? -SaveBannerHeight() : 0f);",
        'saveWarningCaption.text = GameLocalization.Text("save_warning");',
        "UpdateSaveWarning(); // toggles immediately even without a full UI rebuild",
    ):
        if marker not in app:
            fail(f"Persistent and reversible save-warning UI guard missing: {marker}")
    if "WorldStore.RecoveryBlocked && world.Round == 0" in app:
        fail("Unreadable save reset is hidden for imported legacy progress")

    locale_dir = GAME / "Assets/Resources/Evergrove/Locales"
    for file in locale_dir.glob("*.json"):
        translation = json.loads(file.read_text(encoding="utf-8"))
        first = next(
            (entry.get("value", "") for entry in translation["entries"]
             if entry.get("key") == "first"), ""
        )
        if not first or re.search(r"(?:S\s*=\s*|G\s*=\s*|S＝|G＝)", first):
            fail(f"First puzzle localization still depends on English S/G: {file.name}")
    print(
        f"Evergrove execution contract: {len(features)} feature records, "
        f"{len(tests)} test IDs, {len(SPEC_DOCUMENTS)} linked feature specs, "
        "zero claimed device/human verification unless proof exists."
    )

if __name__ == "__main__":
    try:
        sys.exit(main())
    except (AssertionError, OSError, UnicodeError) as exc:
        print(f"Evergrove documentation contract FAILED: {exc}", file=sys.stderr)
        sys.exit(1)
