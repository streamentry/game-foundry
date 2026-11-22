"""Synthetic export-tree tests; do not pretend Unity was executed here."""
import hashlib
from pathlib import Path
import sys
import tempfile
import unittest

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from evergrove_build_provenance import (
    MANIFEST_NAME,
    create_provenance,
    ensure_expected_output,
    hash_file,
)

SHA = "a" * 40
URL = "https://github.com/streamentry/game-foundry/actions/runs/1"


class UnityBuildProvenanceTests(unittest.TestCase):
    def setUp(self):
        self.tmp = tempfile.TemporaryDirectory()
        self.addCleanup(self.tmp.cleanup)
        self.root = Path(self.tmp.name)
        self.output = self.root / "build"
        self.output.mkdir()
        self.version = self.root / "ProjectVersion.txt"
        self.version.write_text("m_EditorVersion: 6000.3.0f1\n", encoding="utf-8")

    def run_manifest(self, platform):
        return create_provenance(
            platform=platform,
            directory=self.output,
            source_sha=SHA,
            workflow_run_url=URL,
            project_version_path=self.version,
        )

    def test_android_real_bytes_and_sha(self):
        apk = self.output / "Evergrove-Playtest.apk"
        apk.write_bytes(b"synthetic-unity-artifact")
        got = self.run_manifest("Android")
        self.assertEqual(len(got["files"]), 1)
        self.assertEqual(got["files"][0]["sha256"], hashlib.sha256(apk.read_bytes()).hexdigest())
        self.assertEqual(got["requested_unity_editor_version"], "6000.3.0f1")
        self.assertEqual(got["build_channel"], "development-research-only")
        self.assertIn("installed Android/iOS app", got["does_not_prove"])
        self.assertEqual(got["source_commit_sha"], SHA)
        self.assertRegex(got["payload_tree_sha256"], r"^[a-f0-9]{64}$")

    def test_webgl_marker_and_individual_files(self):
        folder = self.output / "Evergrove-Playtest"
        folder.mkdir()
        (folder / "index.html").write_text("<html>test</html>", encoding="utf-8")
        (folder / "app.js").write_bytes(b"JS")
        got = self.run_manifest("WebGL")
        self.assertEqual([f["path"] for f in got["files"]],
                         ["Evergrove-Playtest/app.js", "Evergrove-Playtest/index.html"])

    def test_ios_requires_actual_xcode_project(self):
        proj = self.output / "Evergrove-Xcode" / "Unity-iPhone.xcodeproj"
        proj.mkdir(parents=True)
        with self.assertRaises(FileNotFoundError):
            self.run_manifest("iOS")
        (proj / "project.pbxproj").write_text("project", encoding="utf-8")
        self.assertEqual(self.run_manifest("iOS")["platform"], "iOS")

    def test_empty_or_missing_export_fails_closed(self):
        for platform in ("Android", "WebGL", "iOS"):
            with self.subTest(platform=platform), self.assertRaises(FileNotFoundError):
                ensure_expected_output(platform, self.output)

    def test_bad_source_and_run_identity_fail_closed(self):
        (self.output / "Evergrove-Playtest.apk").write_bytes(b"file")
        for sha in ("", "deadbeef", "z" * 40):
            with self.assertRaises(ValueError):
                create_provenance(
                    platform="Android", directory=self.output, source_sha=sha,
                    workflow_run_url=URL, project_version_path=self.version
                )
        with self.assertRaises(ValueError):
            create_provenance(
                platform="Android", directory=self.output, source_sha=SHA,
                workflow_run_url="http://example.com/anything",
                project_version_path=self.version,
            )

    def test_symlink_not_allowed_and_manifest_not_self_hashed(self):
        (self.output / "Evergrove-Playtest.apk").write_bytes(b"apk")
        (self.output / MANIFEST_NAME).write_text("previous manifest", encoding="utf-8")
        got = self.run_manifest("Android")
        self.assertEqual(len(got["files"]), 1)
        self.assertNotEqual(got["files"][0]["path"], MANIFEST_NAME)
        link = self.output / "external-link"
        try:
            link.symlink_to(self.version)
        except (OSError, NotImplementedError):
            self.skipTest("symlinks unavailable")
        with self.assertRaises(ValueError):
            self.run_manifest("Android")


if __name__ == "__main__":
    unittest.main()
