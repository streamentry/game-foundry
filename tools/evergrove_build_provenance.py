#!/usr/bin/env python3
"""Attest a successful Unity preview export with real content hashes.

Runs ONLY after GameCI reports a build success. This records source and file
provenance, NOT proof that someone installed the game or tested a phone.
Standard library only; no network, credentials, or personal data.
"""
from __future__ import annotations

import argparse
import hashlib
import json
from pathlib import Path
import re
import sys

VALID_PLATFORMS = {"Android", "WebGL", "iOS"}
MANIFEST_NAME = "evergrove-build-provenance.json"


def ensure_expected_output(platform: str, directory: Path) -> None:
    """Reject a successful builder step that produced no usable export tree."""
    if platform not in VALID_PLATFORMS:
        raise ValueError(f"Unsupported platform: {platform}")
    if not directory.is_dir():
        raise FileNotFoundError(f"Unity build directory missing: {directory}")
    markers = {
        "Android": directory / "Evergrove-Playtest.apk",
        "WebGL": directory / "Evergrove-Playtest" / "index.html",
        "iOS": directory / "Evergrove-Xcode" / "Unity-iPhone.xcodeproj" / "project.pbxproj",
    }
    marker = markers[platform]
    if not marker.is_file() or marker.stat().st_size == 0:
        raise FileNotFoundError(f"Unity {platform} export marker missing or empty: {marker}")


def hash_file(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for block in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def create_provenance(
    *,
    platform: str,
    directory: Path,
    source_sha: str,
    workflow_run_url: str,
    project_version_path: Path,
) -> dict:
    """Return deterministic file integrity metadata; do NOT call network APIs."""
    if not re.fullmatch(r"[a-f0-9]{40}", source_sha):
        raise ValueError("source_sha must be a full Git commit SHA")
    if not workflow_run_url.startswith("https://github.com/"):
        raise ValueError("workflow_run_url must be a GitHub Actions HTTPS link")
    ensure_expected_output(platform, directory)
    editor_line = project_version_path.read_text(encoding="utf-8").strip()
    if not editor_line.startswith("m_EditorVersion: "):
        raise ValueError("ProjectVersion.txt is invalid")
    version = editor_line.split(": ", 1)[1]

    items = []
    for candidate in sorted(directory.rglob("*")):
        if candidate.is_symlink():
            raise ValueError(f"Refuse symlinks in upload tree: {candidate}")
        if not candidate.is_file() or candidate.name == MANIFEST_NAME:
            continue
        size = candidate.stat().st_size
        if size == 0:
            # Unity output can legitimately contain empty placeholders; record
            # their actual hash but never accept an entirely empty artifact.
            pass
        items.append({
            "path": candidate.relative_to(directory).as_posix(),
            "size_bytes": size,
            "sha256": hash_file(candidate),
        })
    if not items:
        raise ValueError("No Unity build files found")

    tree = hashlib.sha256()
    for entry in items:
        tree.update(
            (entry["path"] + "\0" + str(entry["size_bytes"]) + "\0" +
             entry["sha256"] + "\n").encode("utf-8")
        )
    return {
        "schema_version": 1,
        "evidence_type": "unity-export-file-hashes",
        "does_not_prove": [
            "Unity Editor end-to-end UI behavior",
            "installed Android/iOS app",
            "phone accessibility, performance or player satisfaction",
            "signed or store-approved release",
        ],
        "source_commit_sha": source_sha,
        "requested_unity_editor_version": version,
        "platform": platform,
        "build_channel": "development-research-only",
        "workflow_run_url": workflow_run_url,
        "files": items,
        "payload_tree_sha256": tree.hexdigest(),
    }


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--platform", choices=sorted(VALID_PLATFORMS), required=True)
    parser.add_argument("--directory", required=True, type=Path)
    parser.add_argument("--source-sha", required=True)
    parser.add_argument("--run-url", required=True)
    parser.add_argument(
        "--project-version",
        type=Path,
        default=Path("games/evergrove/ProjectSettings/ProjectVersion.txt"),
    )
    args = parser.parse_args(argv)
    try:
        result = create_provenance(
            platform=args.platform,
            directory=args.directory,
            source_sha=args.source_sha,
            workflow_run_url=args.run_url,
            project_version_path=args.project_version,
        )
        path = args.directory / MANIFEST_NAME
        path.write_text(json.dumps(result, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
        print(
            f"Unity {args.platform} artifact manifest: {len(result['files'])} files; "
            f"tree SHA256 {result['payload_tree_sha256']} ({path})"
        )
        return 0
    except (ValueError, OSError) as exc:
        print(f"Evergrove export provenance FAILED: {exc}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    sys.exit(main())
