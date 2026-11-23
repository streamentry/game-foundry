# BLD-003 evidence packet

test_id: BLD-003
source_commit_sha: 5acbac4c5cba4164a31abf33dedf71ff9bf2f8ba
build_artifact_sha256: fe60275ecaad330825470c3fea6793fe2424ae69737c3828803d26b114adc186
result: PASS
reviewed_at_utc: 2026-11-23T04:39:13Z
reviewer_role: AI agent (Claude Opus 5.5) local log review; not independent human verification

**Passing criteria (verification matrix):** iOS Xcode project exported with compile log; signing remains separate

**Artifact:** SHA-256 of local-export-manifest.json listing 2,870 hashed files of the exported Evergrove-Xcode project (run-oeRNJe); unsigned device-SDK export.

**How produced:** `UNITY_BIN=/Volumes/SSD/unity/editors/6000.3.0f1/Unity.app/Contents/MacOS/Unity bash tools/evergrove_unity_local_preflight.sh iOS` on the maintainer Mac (Apple M4) with an activated Unity Personal license. The tracked tree was clean before and after (`provenance_status=TRACKED_INPUTS_ONLY`). Raw Editor logs and outputs stay in the private, git-ignored `.evergrove-evidence/5acbac4c5cba4164a31abf33dedf71ff9bf2f8ba/` directory, per the probe instructions.

**Log lines reviewed:**
- `Unity Editor version:    6000.3.0f1 (d1870ce95baf)`
- `EVERGROVE_BUILD_SUCCEEDED <run-oeRNJe>/output/iOS/Evergrove-Xcode`
- `editor log sha256: 55d783aadc1bcbffd4d528c2fe1066299b379c5ffeffdb4394ac156b1c05fb92`
- `error CS count: 0`

**Run URL:** none. This is a local run, not a CI workflow run; the private evidence directory takes its place.

**Does not prove:** a signed or installed phone app, physical-device touch, save or interruption behavior, accessibility, mobile privacy, or anything about players (M2–M6 remain open).
