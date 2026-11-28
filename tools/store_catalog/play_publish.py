"""Publish each catalog game to Google Play through the Android Publisher API.

One edit per app: contact details, en-US listing, icon / feature graphic /
phone screenshots, the signed AAB, and a production release; then commit.
--status draft stages the release; --status completed sends it to review
(managed publishing is off on this account).
Usage: python3 tools/store_catalog/play_publish.py --status draft|completed [slug ...]
"""
from __future__ import annotations

import argparse
import os
import sys
from pathlib import Path

import play_client as play
from catalog import select

LANG = "en-US"


def upload_image(base: str, image_type: str, png: Path) -> None:
    play.call("POST", f"{play.UPLOAD.rsplit('/applications', 1)[0]}/applications/{base}/listings/{LANG}/{image_type}"
              "?uploadType=media", raw=png.read_bytes(), content_type="image/png")


def publish(game, out: Path, status: str, version_code: int) -> str:
    pkg = game.app_id
    meta = game.metadata()["google_play"]
    edit = play.call("POST", f"{play.BASE}/{pkg}/edits", {})["id"]
    base = f"{pkg}/edits/{edit}"
    play.call("PUT", f"{play.BASE}/{base}/details",
              {"defaultLanguage": LANG, "contactEmail": meta["support_email"], "contactWebsite": meta["support_url"]})
    play.call("PUT", f"{play.BASE}/{base}/listings/{LANG}",
              {"language": LANG, "title": meta["title"], "shortDescription": meta["short_description"],
               "fullDescription": meta["full_description"]})
    gfx = out / game.slug / "play"
    for image_type in ("icon", "featureGraphic", "phoneScreenshots"):
        play.call("DELETE", f"{play.BASE}/{base}/listings/{LANG}/{image_type}")
    upload_image(base, "icon", gfx / "icon512.png")
    upload_image(base, "featureGraphic", gfx / "feature.png")
    for png in sorted((out / game.slug / "shots" / "play").glob("*.png")):
        upload_image(base, "phoneScreenshots", png)
    aab = out / game.slug / f"{game.slug}.aab"
    existing = play.call("GET", f"{play.BASE}/{base}/bundles").get("bundles", [])
    if version_code not in [b["versionCode"] for b in existing]:
        bundle = play.call("POST", f"{play.UPLOAD}/{base}/bundles?uploadType=media", raw=aab.read_bytes(),
                           content_type="application/octet-stream")
        if bundle["versionCode"] != version_code:
            raise play.PlayError(f"uploaded versionCode {bundle['versionCode']} != expected {version_code}")
    codes = [version_code]
    release = {"name": "1.0.0", "versionCodes": [str(c) for c in codes], "status": status,
               "releaseNotes": [{"language": LANG, "text": "First release."}]}
    play.call("PUT", f"{play.BASE}/{base}/tracks/production", {"track": "production", "releases": [release]})
    play.call("POST", f"{play.BASE}/{base}:commit")
    track = play.call("GET", f"{play.BASE}/{pkg}/edits/{play.call('POST', f'{play.BASE}/{pkg}/edits', {})['id']}"
                      "/tracks/production")
    return f"committed versionCodes={codes} track={[(r.get('status'), r.get('versionCodes')) for r in track.get('releases', [])]}"


def main(argv: list[str]) -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--status", choices=("draft", "completed"), required=True)
    ap.add_argument("--version-code", type=int, default=1)
    ap.add_argument("slugs", nargs="*")
    args = ap.parse_args(argv)
    out = Path(os.environ["STORE_OUT"])
    for game in select(args.slugs):
        try:
            print(game.slug, publish(game, out, args.status, args.version_code), flush=True)
        except play.PlayError as err:
            print(game.slug, "FAIL", str(err)[:500], flush=True)
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
