#!/usr/bin/env python3
"""Check and serve an extracted Godot 4 Web build safely over localhost.

Run from an extracted Actions artifact:
    python3 serve.py

For CI or diagnostics:
    python3 tools/web_playtest.py --directory build/<game>-web --smoke
    python3 serve.py --check

This checks local files and HTTP delivery. It does not claim WebGL rendering
or actual browser/device compatibility.
"""
from __future__ import annotations

import argparse
import functools
import hashlib
import http.server
import os
from pathlib import Path
import re
import socket
import sys
import threading
import urllib.request

ASSETS = ("index.html", "index.js", "index.wasm", "index.pck")
MIME = {
    ".html": "text/html",
    ".js": "application/javascript",
    ".wasm": "application/wasm",
    ".pck": "application/octet-stream",
}


class BundleError(ValueError):
    pass


def check_bundle(directory: Path) -> Path:
    directory = directory.expanduser().resolve()
    if not directory.is_dir():
        raise BundleError(f"Build directory not found: {directory}")
    for name in ASSETS:
        path = directory / name
        if not path.is_file() or path.stat().st_size == 0:
            alternatives = list(directory.glob("*/index.html"))
            suffix = (
                " You may be one folder above the extracted bundle; "
                "try the directory containing index.html."
                if alternatives else ""
            )
            raise BundleError(f"Missing or empty {name} in {directory}.{suffix}")
    with (directory / "index.wasm").open("rb") as handle:
        if handle.read(4) != b"\x00asm":
            raise BundleError("index.wasm has no WebAssembly magic header; export may be damaged.")
    # Godot pack begins with GDPC; prevents a server's HTML 404 page from
    # accidentally being treated as a valid binary pack.
    with (directory / "index.pck").open("rb") as handle:
        if handle.read(4) != b"GDPC":
            raise BundleError("index.pck is not a plain Godot PCK; check the export.")
    if b"index.js" not in (directory / "index.html").read_bytes():
        raise BundleError("index.html does not reference index.js; filenames may have changed.")

    checksums = directory / "SHA256SUMS.txt"
    if checksums.exists():
        seen: set[str] = set()
        for line in checksums.read_text(encoding="utf-8").splitlines():
            match = re.fullmatch(r"([0-9a-fA-F]{64})\s+\*?(.+)", line.strip())
            if not match:
                raise BundleError("Malformed SHA256SUMS.txt.")
            digest, name = match.groups()
            if name not in ASSETS or name in seen:
                raise BundleError(f"Unexpected/duplicate entry in SHA256SUMS.txt: {name}")
            seen.add(name)
            real = _hash_file(directory / name)
            if real.lower() != digest.lower():
                raise BundleError(f"Checksum mismatch for {name}; download/extraction is incomplete.")
        if set(ASSETS) != seen:
            raise BundleError("SHA256SUMS.txt does not cover every required Web asset.")
    return directory


def _hash_file(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as source:
        for block in iter(lambda: source.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


class WebHandler(http.server.SimpleHTTPRequestHandler):
    def guess_type(self, path: str) -> str:
        suffix = Path(path).suffix.lower()
        if suffix in MIME:
            return MIME[suffix]
        return super().guess_type(path)

    def log_message(self, format: str, *args: object) -> None:
        if not getattr(self.server, "quiet", False):
            super().log_message(format, *args)


def _server(directory: Path, port: int, quiet: bool = False) -> http.server.ThreadingHTTPServer:
    handler = functools.partial(WebHandler, directory=str(directory))
    instance = http.server.ThreadingHTTPServer(("127.0.0.1", port), handler)
    instance.daemon_threads = True
    instance.quiet = quiet
    return instance


def smoke_test(directory: Path) -> None:
    directory = check_bundle(directory)
    server = _server(directory, 0, quiet=True)
    thread = threading.Thread(target=server.serve_forever, daemon=True)
    thread.start()
    try:
        root = f"http://127.0.0.1:{server.server_address[1]}/"
        for name in ("", *ASSETS):
            url = root + name
            with urllib.request.urlopen(url, timeout=15) as response:
                if response.status != 200:
                    raise BundleError(f"HTTP {response.status} loading {name or '/'}")
                suffix = ".html" if name == "" else Path(name).suffix.lower()
                content_type = response.headers.get_content_type()
                if content_type != MIME[suffix]:
                    raise BundleError(f"Wrong Content-Type for {name or '/'}: {content_type}")
                digest = hashlib.sha256()
                while chunk := response.read(1024 * 1024):
                    digest.update(chunk)
            expected = _hash_file(directory / (name or "index.html"))
            if digest.hexdigest() != expected:
                raise BundleError(f"Incomplete HTTP response for {name or '/'}")
        print(f"PASS: HTTP 200, correct MIME, and byte-perfect GET for all {len(ASSETS)} Web assets.")
        print("Note: full browser WebGL2 rendering and external hosting are not tested.")
    finally:
        server.shutdown()
        thread.join(timeout=5)
        server.server_close()


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description="Serve or diagnose an extracted Godot Web playtest.")
    parser.add_argument("--directory", type=Path, default=Path.cwd(),
                        help="Folder containing index.html, index.js, index.wasm and index.pck.")
    parser.add_argument("--port", type=int, default=8000,
                        help="Localhost TCP port (default 8000).")
    action = parser.add_mutually_exclusive_group()
    action.add_argument("--check", action="store_true", help="Validate build assets and optional SHA-256 checksums.")
    action.add_argument("--smoke", action="store_true", help="Validate the build and download every asset over localhost HTTP.")
    args = parser.parse_args(argv)
    try:
        directory = check_bundle(args.directory)
        if args.smoke:
            smoke_test(directory)
        elif args.check:
            print(f"PASS: complete matching Web assets in {directory}")
        else:
            if args.port < 1 or args.port > 65535:
                raise BundleError("Port must be between 1 and 65535.")
            try:
                server = _server(directory, args.port)
            except OSError as exc:
                raise BundleError(
                    f"Cannot listen on 127.0.0.1:{args.port}: {exc}. "
                    "Try --port 8010."
                ) from exc
            print(f"Serving {directory}", flush=True)
            print(f"Open http://127.0.0.1:{args.port}/ (NOT index.html via file://)", flush=True)
            print("Press Ctrl+C to stop. This URL works only on this computer.", flush=True)
            try:
                server.serve_forever()
            except KeyboardInterrupt:
                print("\nServer stopped.")
            finally:
                server.server_close()
        return 0
    except (BundleError, OSError, urllib.error.URLError) as exc:
        print(f"WEB BUILD ERROR: {exc}", file=sys.stderr)
        print(
            "Use the extracted Actions artifact, not an individual HTML file. "
            "If the browser still fails over HTTP, open DevTools > Network and "
            "look for the failing .wasm or .pck request.",
            file=sys.stderr,
        )
        return 1


if __name__ == "__main__":
    sys.exit(main())
