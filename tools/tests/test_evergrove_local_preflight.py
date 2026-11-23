"""Synthetic tests of the LOCAL preflight shell contract, never a Unity build.

Uses a deliberately fake Editor executable with marker text. Tests only that
the wrapper refuses missing artifacts/dirty inputs and accurately flags
untracked Unity-generated metadata; it supplies NO BLD-001/002/003 proof.
"""
from __future__ import annotations

import os
from pathlib import Path
import shutil
import subprocess
import tempfile
import unittest


REPO = Path(__file__).resolve().parents[2]
SCRIPT = REPO / "tools" / "evergrove_unity_local_preflight.sh"

FAKE_UNITY = r"""#!/usr/bin/env bash
set -euo pipefail
project=""
log=""
platform="smoke"
channel="${EVERGROVE_BUILD_CHANNEL:-development}"
while [ "$#" -gt 0 ]; do
  case "$1" in
    -projectPath) project="$2"; shift 2 ;;
    -logFile) log="$2"; shift 2 ;;
    -buildTarget) platform="$2"; shift 2 ;;
    *) shift ;;
  esac
done
[ -n "$project" ] && [ -n "$log" ] || exit 8
mkdir -p "$(dirname "$log")" "$project/Assets/Scripts" "$project/Packages" "$project/ProjectSettings"
echo generated > "$project/Assets/Scripts/Mock.cs.meta"
echo '{"locked":true}' > "$project/Packages/packages-lock.json"
echo settings > "$project/ProjectSettings/ProjectSettings.asset"
echo generated-settings > "$project/ProjectSettings/GeneratedSettings.asset"
if [ "${MOCK_EDIT_TRACKED:-0}" = 1 ]; then
  echo m_EditorVersion: 6000.3.999f1 > "$project/ProjectSettings/ProjectVersion.txt"
fi
if [ "${MOCK_OMIT_LOG:-0}" = 1 ]; then exit 0; fi
cat > "$log" <<EOF
FAKE_UNITY_MARKER_ONLY_NOT_ACTUAL_EDITOR
EVERGROVE_UNITY_IMPORT_CHECK_OK editor=6000.3.0f1
Evergrove Unity Editor compilation, scene setup and puzzle smoke passed.
EOF
if [ "$platform" = "smoke" ]; then exit 0; fi
repo="$(dirname "$(dirname "$project")")"
root="${EVERGROVE_OUTPUT_ROOT:-$repo/build}"
case "$platform" in
  Android)
    if [ "$channel" = "release-candidate" ]; then
      output="$root/Android/Mosselora.aab"
    else
      output="$root/Android/Evergrove-Playtest.apk"
    fi
    marker="$output"
    ;;
  iOS)
    output="$root/iOS/Evergrove-Xcode"
    marker="$output/Unity-iPhone.xcodeproj/project.pbxproj"
    ;;
  WebGL)
    output="$root/WebGL/Evergrove-Playtest"
    marker="$output/index.html"
    ;;
  *) exit 9 ;;
esac
if [ "${MOCK_WRONG_LOG_PATH:-0}" = 1 ]; then
  echo "EVERGROVE_BUILD_SUCCEEDED /old-run/Evergrove-Playtest.apk" >> "$log"
elif [ "$channel" = "release-candidate" ]; then
  echo "EVERGROVE_BUILD_SUCCEEDED $channel $output" >> "$log"
else
  echo "EVERGROVE_BUILD_SUCCEEDED $output" >> "$log"
fi
if [ "${MOCK_OMIT_MARKER:-0}" = 1 ]; then exit 0; fi
mkdir -p "$(dirname "$marker")"
printf "FAKE_ARTIFACT_NOT_A_PLAYER" > "$marker"
"""

class PreflightShellContractTests(unittest.TestCase):
    def setUp(self) -> None:
        self.temp = tempfile.TemporaryDirectory(prefix="evergrove-preflight-mock-")
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.script = self.root / "tools" / "evergrove_unity_local_preflight.sh"
        self.script.parent.mkdir(parents=True)
        shutil.copy2(SCRIPT, self.script)
        proj = self.root / "games" / "evergrove"
        (proj / "ProjectSettings").mkdir(parents=True)
        (proj / "Assets" / "Scripts").mkdir(parents=True)
        (proj / "ProjectSettings" / "ProjectVersion.txt").write_text(
            "m_EditorVersion: 6000.3.0f1\n", encoding="utf-8"
        )
        (proj / "ProjectSettings" / "ProjectSettings.asset").write_text(
            "original settings\n", encoding="utf-8"
        )
        (proj / "Assets" / "Scripts" / "Mock.cs").write_text(
            "// Synthetic tracked source file\n", encoding="utf-8"
        )
        (self.root / ".gitignore").write_text(
            "/build/\n/.evergrove-evidence/\n/games/evergrove/Assets/Scenes/\n",
            encoding="utf-8",
        )
        self._git("init")
        self._git("config", "user.email", "test@localhost.invalid")
        self._git("config", "user.name", "Synthetic Test")
        self._git("add", ".")
        self._git("commit", "-m", "synthetic source")
        self.fake_unity = self.root / "mock-unity.sh"
        self.fake_unity.write_text(FAKE_UNITY, encoding="utf-8")
        self.fake_unity.chmod(0o700)

    def _git(self, *args: str) -> None:
        subprocess.run(
            ["git", *args], cwd=self.root, check=True,
            stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL,
        )

    def _run(self, mode: str = "smoke", **extra: str) -> subprocess.CompletedProcess[str]:
        env = os.environ.copy()
        env["UNITY_BIN"] = str(self.fake_unity)
        env.update(extra)
        return subprocess.run(
            ["bash", str(self.script), mode], cwd=self.root, env=env,
            capture_output=True, text=True, check=False, timeout=15,
        )

    def _single_run(self, sha: str, mode: str) -> Path:
        runs = list((self.root / ".evergrove-evidence" / sha / mode).glob("run-*"))
        self.assertEqual(len(runs), 1, "expected one uniquely isolated invocation")
        return runs[0]

    def test_generated_metadata_is_hashed_not_mistaken_for_clean_commit(self) -> None:
        first = self._run("smoke")
        self.assertEqual(first.returncode, 0, first.stdout + first.stderr)
        sha = subprocess.check_output(
            ["git", "rev-parse", "HEAD"], cwd=self.root, text=True
        ).strip()
        record = self._single_run(sha, "smoke")
        self.assertIn("PROVISIONAL_GENERATED_INPUTS_NOT_IN_GIT",
                      (record / "probe-context.txt").read_text())
        generated = (record / "untracked-unity-inputs.after.sha256").read_text()
        self.assertIn("Mock.cs.meta", generated)
        self.assertIn("packages-lock.json", generated)
        self.assertIn("GeneratedSettings.asset", generated)
        self.assertEqual(
            (self.root / "games" / "evergrove" / "ProjectSettings" / "ProjectSettings.asset").read_text(),
            "original settings\n",
        )
        self.assertTrue((record / "ProjectSettings.asset.effective-build-input").is_file())
        self.assertIn("effective_project_settings_sha256", (record / "probe-context.txt").read_text())
        self.assertIn("WARNING", first.stderr)

        # A real first Unity import generates .meta and project settings:
        # the next Android probe must not die on these known-generated inputs.
        second = self._run("Android")
        self.assertEqual(second.returncode, 0, second.stdout + second.stderr)
        android_run = self._single_run(sha, "Android")
        manifest = android_run / "local-export-manifest.json"
        self.assertTrue(manifest.is_file())
        self.assertIn("effective_project_settings_sha256", manifest.read_text())
        self.assertIn("FAKE", (android_run / "output" / "Android" / "Evergrove-Playtest.apk").read_text())
        self.assertIn(str(android_run / "output"), (android_run / "unity-editor.log").read_text())

    def test_release_candidate_android_uses_aab_and_channel_marker(self) -> None:
        result = self._run("Android", EVERGROVE_BUILD_CHANNEL="release-candidate")
        self.assertEqual(result.returncode, 0, result.stdout + result.stderr)
        sha = subprocess.check_output(
            ["git", "rev-parse", "HEAD"], cwd=self.root, text=True
        ).strip()
        run = self._single_run(sha, "Android")
        artifact = run / "output" / "Android" / "Mosselora.aab"
        self.assertTrue(artifact.is_file())
        self.assertIn("release-candidate", (run / "local-export-manifest.json").read_text())
        self.assertIn("EVERGROVE_BUILD_SUCCEEDED release-candidate", (run / "unity-editor.log").read_text())

    def test_untracked_unrecognized_source_blocks_run(self) -> None:
        file = self.root / "games" / "evergrove" / "Assets" / "Scripts" / "Unexpected.cs"
        file.write_text("// not in source SHA", encoding="utf-8")
        result = self._run()
        self.assertNotEqual(result.returncode, 0)
        self.assertIn("Untracked non-generated Unity input", result.stderr)

    def test_tracked_source_change_blocks_run(self) -> None:
        path = self.root / "games" / "evergrove" / "Assets" / "Scripts" / "Mock.cs"
        path.write_text("// dirty", encoding="utf-8")
        result = self._run()
        self.assertNotEqual(result.returncode, 0)
        self.assertIn("Tracked Evergrove project files differ", result.stderr)

    def test_editor_modifying_tracked_project_blocks_false_provenance(self) -> None:
        result = self._run(MOCK_EDIT_TRACKED="1")
        self.assertNotEqual(result.returncode, 0)
        self.assertIn("Unity modified tracked project inputs", result.stderr)

    def test_success_marker_without_real_output_fails_closed(self) -> None:
        result = self._run("Android", MOCK_OMIT_MARKER="1")
        self.assertNotEqual(result.returncode, 0)
        self.assertIn("Expected nonempty export marker missing", result.stderr)

    def test_stale_apk_cannot_pass_a_new_invocation(self) -> None:
        first = self._run("Android")
        self.assertEqual(first.returncode, 0, first.stdout + first.stderr)
        sha = subprocess.check_output(
            ["git", "rev-parse", "HEAD"], cwd=self.root, text=True
        ).strip()
        existing_run = self._single_run(sha, "Android")
        previous = existing_run / "output" / "Android" / "Evergrove-Playtest.apk"
        self.assertTrue(previous.is_file(), "first attempt must have produced an artifact")
        second = self._run("Android", MOCK_OMIT_MARKER="1")
        self.assertNotEqual(second.returncode, 0, second.stdout + second.stderr)
        self.assertIn("Expected nonempty export marker missing", second.stderr)
        runs = list((self.root / ".evergrove-evidence" / sha / "Android").glob("run-*"))
        self.assertEqual(len(runs), 2, "each preflight attempt must have a fresh directory")
        self.assertTrue(previous.is_file(), "prior artifact remains untouched")

    def test_wrong_editor_log_output_path_cannot_pass(self) -> None:
        result = self._run("Android", MOCK_WRONG_LOG_PATH="1")
        self.assertNotEqual(result.returncode, 0, result.stdout + result.stderr)
        self.assertIn("does not identify this invocation's output", result.stderr)

    def test_no_editor_log_cannot_pass(self) -> None:
        result = self._run("smoke", MOCK_OMIT_LOG="1")
        self.assertNotEqual(result.returncode, 0)
        self.assertIn("without an Editor log", result.stderr)


if __name__ == "__main__":
    unittest.main()
