# BLD-002 evidence packet

test_id: BLD-002
source_commit_sha: 5acbac4c5cba4164a31abf33dedf71ff9bf2f8ba
build_artifact_sha256: 2897d95432122775cac3c178eb8e69bdce79825755e62f0e1cea809dc0baa2a7
result: PASS
reviewed_at_utc: 2026-11-23T04:39:13Z
reviewer_role: AI agent (Claude Opus 5.5) local log review; not independent human verification

**Passing criteria (verification matrix):** Unsigned Android APK artifact built by manual workflow with recorded SHA

**Artifact:** SHA-256 of Evergrove-Playtest.apk (44,447,020 bytes; unsigned development build; run-19TUuk).

**How produced:** `UNITY_BIN=/Volumes/SSD/unity/editors/6000.3.0f1/Unity.app/Contents/MacOS/Unity bash tools/evergrove_unity_local_preflight.sh Android` on the maintainer Mac (Apple M4) with an activated Unity Personal license. The tracked tree was clean before and after (`provenance_status=TRACKED_INPUTS_ONLY`). Raw Editor logs and outputs stay in the private, git-ignored `.evergrove-evidence/5acbac4c5cba4164a31abf33dedf71ff9bf2f8ba/` directory, per the probe instructions.

**Log lines reviewed:**
- `Unity Editor version:    6000.3.0f1 (d1870ce95baf)`
- `EVERGROVE_BUILD_SUCCEEDED <run-19TUuk>/output/Android/Evergrove-Playtest.apk`
- `editor log sha256: aed98c34c751ce259d0d18191c6ff2eb0e7623f60ab4665b9ee64905a36e7015`
- `error CS count: 0`

**Run URL:** none. This is a local run, not a CI workflow run; the private evidence directory takes its place.

**Does not prove:** a signed or installed phone app, physical-device touch, save or interruption behavior, accessibility, mobile privacy, or anything about players (M2–M6 remain open).
