# BLD-001 evidence packet

test_id: BLD-001
source_commit_sha: 5acbac4c5cba4164a31abf33dedf71ff9bf2f8ba
build_artifact_sha256: bed9375a24216e30cfca2e5265d2cc3f4082398964614e9b3dd9e9be8ca5647b
result: PASS
reviewed_at_utc: 2026-11-23T04:39:13Z
reviewer_role: AI agent (Claude Opus 5.5) local log review; not independent human verification

**Passing criteria (verification matrix):** Editor 6.3 imports, compiles and executes scene/bootstrap smoke without errors

**Artifact:** SHA-256 of the private unity-editor.log for the smoke probe (run-L0JmFm).

**How produced:** `UNITY_BIN=/Volumes/SSD/unity/editors/6000.3.0f1/Unity.app/Contents/MacOS/Unity bash tools/evergrove_unity_local_preflight.sh smoke` on the maintainer Mac (Apple M4) with an activated Unity Personal license. The tracked tree was clean before and after (`provenance_status=TRACKED_INPUTS_ONLY`). Raw Editor logs and outputs stay in the private, git-ignored `.evergrove-evidence/5acbac4c5cba4164a31abf33dedf71ff9bf2f8ba/` directory, per the probe instructions.

**Log lines reviewed:**
- `Unity Editor version:    6000.3.0f1 (d1870ce95baf)`
- `EVERGROVE_UNITY_IMPORT_CHECK_OK editor=6000.3.0f1 puzzles=12 importedLocales=29 snapshotSchema=4`
- `Evergrove Unity Editor compilation, scene setup and puzzle smoke passed.`
- `error CS count: 0`

**Run URL:** none. This is a local run, not a CI workflow run; the private evidence directory takes its place.

**Does not prove:** a signed or installed phone app, physical-device touch, save or interruption behavior, accessibility, mobile privacy, or anything about players (M2–M6 remain open).
