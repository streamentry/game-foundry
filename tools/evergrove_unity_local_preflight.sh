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
BUILD_CHANNEL="${EVERGROVE_BUILD_CHANNEL:-development}"
case "$BUILD_CHANNEL" in
  development|release-candidate) ;;
  *) echo "EVERGROVE_BUILD_CHANNEL must be development or release-candidate." >&2; exit 2 ;;
esac

ROOT="$(CDPATH= cd -- "$(dirname "$0")/.." && pwd)"
cd "$ROOT"
if ! git diff --quiet HEAD -- games/evergrove tools/evergrove_unity_local_preflight.sh; then
  echo "Tracked Evergrove project files differ from HEAD. Commit or discard them before collecting source-SHA evidence." >&2
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
# Every invocation gets a fresh, isolated directory. A successful old APK,
# Xcode export or Unity log must NEVER satisfy a new build's gate.
EVIDENCE_ROOT="$ROOT/.evergrove-evidence/$SOURCE_SHA/$MODE"
mkdir -p "$EVIDENCE_ROOT"
EVIDENCE="$(mktemp -d "$EVIDENCE_ROOT/run-XXXXXX")"
export EVERGROVE_OUTPUT_ROOT="$EVIDENCE/output"

# Unity legitimately creates .meta files, package lockfiles and fresh
# ProjectSettings/*.asset when importing this minimal scaffold. Do not let
# those generated files break subsequent probes or silently claim that HEAD
# alone describes every input. Hash and flag every generated input instead.
record_untracked_inputs() {
  local phase="$1"
  local record="$EVIDENCE/untracked-unity-inputs.$phase.sha256"
  : > "$record"
  while IFS= read -r file; do
    case "$file" in
      games/evergrove/Assets/*.meta|\
      games/evergrove/Packages/packages-lock.json|\
      games/evergrove/Packages/*.meta|\
      games/evergrove/ProjectSettings/*.asset|\
      games/evergrove/ProjectSettings/*.meta)
        if [ ! -f "$file" ]; then
          echo "Unexpected non-file generated Unity input: $file" >&2
          return 1
        fi
        shasum -a 256 "$file" >> "$record"
        ;;
      *)
        echo "Untracked non-generated Unity input blocks source-SHA evidence: $file" >&2
        return 1
        ;;
    esac
  done < <(git ls-files --others --exclude-standard -- \
    games/evergrove/Assets games/evergrove/Packages games/evergrove/ProjectSettings)
}

record_untracked_inputs before || exit 1
LOG="$EVIDENCE/unity-editor.log"
STDOUT="$EVIDENCE/editor-stdout.log"
PROJECT_SETTINGS_BUILD_INPUT_SHA=""

ARGS=(-batchmode -nographics -quit -projectPath "$PROJECT")
if [ "$MODE" = "smoke" ]; then
  ARGS+=(-executeMethod Evergrove.UnityClient.Editor.EvergroveSceneSetup.VerifyProject)
else
  ARGS+=(-buildTarget "$MODE" -executeMethod Evergrove.UnityClient.Editor.EvergroveBuild.Build)
fi
ARGS+=(-logFile "$LOG")

echo "Evergrove local probe: $MODE/$BUILD_CHANNEL (requested Editor $EXPECTED_VERSION, source $SOURCE_SHA)"
if ! "$UNITY_BIN" "${ARGS[@]}" >"$STDOUT" 2>&1; then
  echo "Unity exited with failure. Inspect the private log: $LOG" >&2
  tail -n 35 "$LOG" >&2 || true
  exit 1
fi
if [ ! -f "$LOG" ]; then
  echo "Unity returned without an Editor log; no build proof." >&2
  exit 1
fi
# Unity writes PlayerSettings (product name, app ID, version and SDK choice)
# into this tracked asset while building. Capture the exact effective file,
# then restore only that expected generated edit. Any other tracked change
# still blocks provenance and is left untouched for review.
if ! git diff --quiet HEAD -- games/evergrove tools/evergrove_unity_local_preflight.sh; then
  CHANGED_PATHS="$(git diff --name-only HEAD -- games/evergrove tools/evergrove_unity_local_preflight.sh)"
  if [ "$CHANGED_PATHS" != "games/evergrove/ProjectSettings/ProjectSettings.asset" ]; then
    echo "Unity modified tracked project inputs outside the expected PlayerSettings asset: $CHANGED_PATHS" >&2
    exit 1
  fi
  cp "$PROJECT/ProjectSettings/ProjectSettings.asset" "$EVIDENCE/ProjectSettings.asset.effective-build-input"
  shasum -a 256 "$EVIDENCE/ProjectSettings.asset.effective-build-input" > "$EVIDENCE/ProjectSettings.asset.effective-build-input.sha256"
  PROJECT_SETTINGS_BUILD_INPUT_SHA="$(awk '{print $1}' "$EVIDENCE/ProjectSettings.asset.effective-build-input.sha256")"
  git show HEAD:games/evergrove/ProjectSettings/ProjectSettings.asset > "$PROJECT/ProjectSettings/ProjectSettings.asset"
  if ! git diff --quiet HEAD -- games/evergrove tools/evergrove_unity_local_preflight.sh; then
    echo "Restoring the captured PlayerSettings asset did not return tracked project files to HEAD." >&2
    exit 1
  fi
fi
record_untracked_inputs after || exit 1
# The importer itself validates actual Unity JsonUtility and 29 bundled
# locale TextAssets. Exit code alone is insufficient if that probe was skipped.
if ! grep -Fq "EVERGROVE_UNITY_IMPORT_CHECK_OK editor=$EXPECTED_VERSION" "$LOG"; then
  echo "Pinned-version Unity import/serialization marker missing from this run." >&2
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
  OUTDIR="$EVERGROVE_OUTPUT_ROOT/$MODE"
  case "$MODE" in
    Android)
      if [ "$BUILD_CHANNEL" = "release-candidate" ]; then
        OUTPUT="$OUTDIR/Mosselora.aab"
      else
        OUTPUT="$OUTDIR/Evergrove-Playtest.apk"
      fi
      MARKER="$OUTPUT"
      ;;
    iOS)
      OUTPUT="$OUTDIR/Evergrove-Xcode"
      MARKER="$OUTPUT/Unity-iPhone.xcodeproj/project.pbxproj"
      ;;
    WebGL)
      OUTPUT="$OUTDIR/Evergrove-Playtest"
      MARKER="$OUTPUT/index.html"
      ;;
  esac
  # Check the EXACT unique path that this invocation requested, not a
  # generic success log from a different output directory.
  if [ "$BUILD_CHANNEL" = "release-candidate" ]; then
    EXPECTED_MARKER="EVERGROVE_BUILD_SUCCEEDED release-candidate $OUTPUT"
  else
    EXPECTED_MARKER="EVERGROVE_BUILD_SUCCEEDED $OUTPUT"
  fi
  if ! grep -Fq "$EXPECTED_MARKER" "$LOG"; then
    echo "Unity success marker does not identify this invocation's output: $OUTPUT" >&2
    exit 1
  fi
  if [ ! -s "$MARKER" ]; then
    echo "Expected nonempty export marker missing: $MARKER" >&2
    exit 1
  fi
  shasum -a 256 "$MARKER" > "$EVIDENCE/marker.sha256"
  python3 - "$OUTDIR" "$EVIDENCE" "$SOURCE_SHA" "$MODE" "$EXPECTED_VERSION" "$BUILD_CHANNEL" "$PROJECT_SETTINGS_BUILD_INPUT_SHA" <<'PY'
import hashlib
import json
from pathlib import Path
import sys

root, evidence = Path(sys.argv[1]), Path(sys.argv[2])
sha, platform, requested_version, build_channel, project_settings_sha = sys.argv[3:8]
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
    "build_channel": build_channel,
    "effective_project_settings_sha256": project_settings_sha or None,
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
printf 'source_commit_sha=%s\nrequested_unity_editor_version=%s\nmode=%s\nbuild_channel=%s\nlocal_run_dir=%s\n' \
  "$SOURCE_SHA" "$EXPECTED_VERSION" "$MODE" "$BUILD_CHANNEL" "$EVIDENCE" > "$EVIDENCE/probe-context.txt"
if [ -n "$PROJECT_SETTINGS_BUILD_INPUT_SHA" ]; then
  printf 'effective_project_settings_sha256=%s\neffective_project_settings_file=%s\n' \
    "$PROJECT_SETTINGS_BUILD_INPUT_SHA" "$EVIDENCE/ProjectSettings.asset.effective-build-input" >> "$EVIDENCE/probe-context.txt"
fi
if [ -s "$EVIDENCE/untracked-unity-inputs.after.sha256" ]; then
  printf 'provenance_status=PROVISIONAL_GENERATED_INPUTS_NOT_IN_GIT\n' >> "$EVIDENCE/probe-context.txt"
  echo "WARNING: imported Unity .meta / settings / package-lock files are untracked." >&2
  echo "Hashes are in $EVIDENCE/untracked-unity-inputs.after.sha256" >&2
  echo "Review and commit reproducible Unity metadata before treating the source SHA as a complete build-input manifest." >&2
else
  printf 'provenance_status=TRACKED_INPUTS_ONLY\n' >> "$EVIDENCE/probe-context.txt"
fi
echo "LOCAL PROBE COMPLETE. Review logs before changing any verification status."
echo "Private evidence directory: $EVIDENCE"
echo "Do not commit raw Editor logs, license data, signatures, or test-device identities."
