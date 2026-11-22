"""Regression tests for complete Godot Web exports and localhost delivery."""
from __future__ import annotations

import hashlib
from pathlib import Path
import sys
import tempfile
import unittest

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
import web_playtest as helper


def fixture(directory: Path, checksums: bool = True) -> None:
    files = {
        "index.html": b"<!DOCTYPE html><html><script src='index.js'></script></html>",
        "index.js": b"console.log('Godot web playtest');\n",
        "index.wasm": b"\x00asm\x01\x00\x00\x00",
        "index.pck": b"GDPC" + b"\x00" * 256,
    }
    for name, data in files.items():
        (directory / name).write_bytes(data)
    if checksums:
        (directory / "SHA256SUMS.txt").write_text(
            "".join(hashlib.sha256(data).hexdigest() + "  " + name + "\n"
                    for name, data in files.items()),
            encoding="utf-8",
        )


class WebPlaytestTests(unittest.TestCase):
    def setUp(self) -> None:
        self.tmp = tempfile.TemporaryDirectory()
        self.addCleanup(self.tmp.cleanup)
        self.directory = Path(self.tmp.name)
        fixture(self.directory)

    def test_complete_verified_bundle(self) -> None:
        self.assertEqual(helper.check_bundle(self.directory), self.directory.resolve())

    def test_full_http_fetch_and_correct_mime(self) -> None:
        helper.smoke_test(self.directory)

    def test_missing_wasm_is_detected(self) -> None:
        (self.directory / "index.wasm").unlink()
        with self.assertRaisesRegex(helper.BundleError, "index.wasm"):
            helper.check_bundle(self.directory)

    def test_corrupted_wasm_is_detected(self) -> None:
        (self.directory / "index.wasm").write_bytes(b"<html>404</html>")
        with self.assertRaisesRegex(helper.BundleError, "WebAssembly"):
            helper.check_bundle(self.directory)

    def test_bad_checksum_is_detected(self) -> None:
        with (self.directory / "index.js").open("ab") as file:
            file.write(b"tamper")
        with self.assertRaisesRegex(helper.BundleError, "Checksum mismatch"):
            helper.check_bundle(self.directory)

    def test_wrong_html_name_reference_is_detected(self) -> None:
        (self.directory / "SHA256SUMS.txt").unlink()
        (self.directory / "index.html").write_text("<h1>Oops</h1>")
        with self.assertRaisesRegex(helper.BundleError, "index.js"):
            helper.check_bundle(self.directory)

    def test_misplaced_extraction_has_actionable_error(self) -> None:
        nested = self.directory / "artifact"
        nested.mkdir()
        fixture(nested)
        for name in helper.ASSETS:
            (self.directory / name).unlink()
        with self.assertRaisesRegex(helper.BundleError, "one folder above"):
            helper.check_bundle(self.directory)

    def test_cli_check_and_smoke_modes(self) -> None:
        self.assertEqual(helper.main(["--directory", str(self.directory), "--check"]), 0)
        self.assertEqual(helper.main(["--directory", str(self.directory), "--smoke"]), 0)


if __name__ == "__main__":
    unittest.main()
