#!/usr/bin/env bash
# Local, license-owning maintainer M1 probe. Never publishes, signs or uploads.
# Its output is LOCAL evidence only, not a Unity/device certification.
set -euo pipefail

usage() {
  echo "Usage: [UNITY_BIN=/path/to/Unity] tools/evergrove_unity_local_preflight.sh smoke|Android|iOS|WebGL" >&2
  exit 2
}

if [ "$#" -ne 1 ]; then usage; fi
MODE="$1"
case "$MODE" in
  smoke|Android|iOS|WebGL) ;;
  *) usage ;;
esac

ROOT="$(CDPATH= cd -- "$(dirname "$0")/.." && pwd)"
cd "$ROOT"
if ! git diff --quiet HEAD -- games/evergrove tools/evergrove_unity_local_preflight.sh; then
  echo "Tracked Evergrove project files differ from HEAD. Commit or discard them before collecting source-SHA evidence." >&2
  exit 1
fi
if git ls-files --others --exclude-standard -- games/evergrove/Assets games/evergrove/Packages games/evergrove/ProjectSettings | grep -q .; then
  echo "Untracked scripts or package/settings files are present; source-SHA evidence would be ambiguous." >&2
  exit 1
fi

SOURCE_SHA="$(git rev-parse HEAD)"
PROJECT="$ROOT/games/evergrove"
EXPECTED_VERSION="$(sed -n 's/^m_EditorVersion: //p' "$PROJECT/ProjectSettings/ProjectVersion.txt")"
if [ -z "$EXPECTED_VERSION" ]; then
  echo "ProjectVersion.txt has no editor version." >&2
  exit 1
fi

UNITY_BIN="${UNITY_BIN:-/Applications/Unity/Hub/Editor/$EXPECTED_VERSION/Unity.app/Contents/MacOS/Unity}"
if [ ! -x "$UNITY_BIN" ]; then
  echo "A licensed Unity Editor is not available at: $UNITY_BIN" >&2
  echo "Install the exact project editor and required platform module with Unity Hub, or set UNITY_BIN." >&2
  exit 2
fi
EVIDENCE="$ROOT/.evergrove-evidence/$SOURCE_SHA/$MODE"
mkdir -p "$EVIDENCE"
LOG="$EVIDENCE/unity-editor.log"
STDOUT="$EVIDENCE/editor-stdout.log"

ARGS=(-batchmode -nographics -quit -projectPath "$PROJECT")
if [ "$MODE" = "smoke" ]; then
  ARGS+=(-executeMethod Evergrove.UnityClient.Editor.EvergroveSceneSetup.VerifyProject)
else
  ARGS+=(-buildTarget "$MODE" -executeMethod Evergrove.UnityClient.Editor.EvergroveBuild.Build)
fi
ARGS+=(-logFile "$LOG")

echo "Evergrove local probe: $MODE (requested Editor $EXPECTED_VERSION, source $SOURCE_SHA)"
if ! "$UNITY_BIN" "${ARGS[@]}" >"$STDOUT" 2>&1; then
  echo "Unity exited with failure. Inspect the private log: $LOG" >&2
  tail -n 35 "$LOG" >&2 || true
  exit 1
fi
if [ ! -f "$LOG" ]; then
  echo "Unity returned without an Editor log; no build proof." >&2
  exit 1
fi
if [ "$MODE" = "smoke" ]; then
  if ! grep -Fq "Evergrove Unity Editor compilation, scene setup and puzzle smoke passed." "$LOG"; then
    echo "Expected scene/bootstrap success marker absent; inspect the Editor log." >&2
    exit 1
  fi
else
  if ! grep -Fq "EVERGROVE_BUILD_SUCCEEDED" "$LOG"; then
    echo "Expected Unity BuildResult success marker absent; inspect the Editor log." >&2
    exit 1
  fi
  OUTDIR="$ROOT/build/$MODE"
  case "$MODE" in
    Android) MARKER="$OUTDIR/Evergrove-Playtest.apk" ;;
    iOS) MARKER="$OUTDIR/Evergrove-Xcode/Unity-iPhone.xcodeproj/project.pbxproj" ;;
    WebGL) MARKER="$OUTDIR/Evergrove-Playtest/index.html" ;;
  esac
  if [ ! -s "$MARKER" ]; then
    echo "Expected nonempty export marker missing: $MARKER" >&2
    exit 1
  fi
  shasum -a 256 "$MARKER" > "$EVIDENCE/marker.sha256"
  python3 - "$OUTDIR" "$EVIDENCE" "$SOURCE_SHA" "$MODE" "$EXPECTED_VERSION" <<'PY'
import hashlib
import json
from pathlib import Path
import sys

root, evidence = Path(sys.argv[1]), Path(sys.argv[2])
sha, platform, requested_version = sys.argv[3:6]
items = []
for file in sorted(root.rglob("*")):
    if file.is_symlink():
        raise SystemExit("Refusing a symlink inside local Unity output: " + str(file))
    if not file.is_file():
        continue
    digest = hashlib.sha256()
    with file.open("rb") as handle:
        for part in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(part)
    items.append({
        "path": file.relative_to(root).as_posix(),
        "size_bytes": file.stat().st_size,
        "sha256": digest.hexdigest(),
    })
if not items:
    raise SystemExit("Unity output directory is empty")
manifest = {
    "evidence_type": "LOCAL_UNVERIFIED_EXPORT_FILE_MANIFEST",
    "source_commit_sha": sha,
    "requested_unity_editor_version": requested_version,
    "platform": platform,
    "build_channel": "development-research-only",
    "does_not_prove": [
        "actual Editor version without log review",
        "signed or installed phone app",
        "touch, save, accessibility or mobile privacy behavior",
    ],
    "files": items,
}
(evidence / "local-export-manifest.json").write_text(
    json.dumps(manifest, indent=2) + "\n", encoding="utf-8")
print("Local export file hashes recorded:", len(items))
PY
fi

shasum -a 256 "$LOG" > "$EVIDENCE/editor-log.sha256"
printf 'source_commit_sha=%s\nrequested_unity_editor_version=%s\nmode=%s\n' \
  "$SOURCE_SHA" "$EXPECTED_VERSION" "$MODE" > "$EVIDENCE/probe-context.txt"
echo "LOCAL PROBE COMPLETE. Review logs before changing any verification status."
echo "Private evidence directory: $EVIDENCE"
echo "Do not commit raw Editor logs, license data, signatures, or test-device identities."
