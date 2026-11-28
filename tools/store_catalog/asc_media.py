"""Upload captured iPhone 6.5" screenshots to each app's en-US version via the API.

Screenshots come from $STORE_OUT/<slug>/shots/apple/*.png (1242x2688, rendered
from the real game scene by capture.gd). Existing screenshots in the set are
left alone when the set is already full.
Usage: python3 tools/store_catalog/asc_media.py <apple_apps.json> [slug ...]
"""
from __future__ import annotations

import hashlib
import json
import os
import sys
import urllib.request
from pathlib import Path

import asc_client as asc
from catalog import select

DISPLAY_TYPE = "APP_IPHONE_65"


def en_us_localization(app_id: str) -> str:
    ver = [v for v in asc.get(f"/v1/apps/{app_id}/appStoreVersions")["data"]
           if v["attributes"]["appStoreState"] in ("PREPARE_FOR_SUBMISSION", "DEVELOPER_REJECTED")][0]
    locs = asc.get(f"/v1/appStoreVersions/{ver['id']}/appStoreVersionLocalizations")["data"]
    return [l for l in locs if l["attributes"]["locale"] == "en-US"][0]["id"]


def screenshot_set(loc_id: str) -> tuple[str, int]:
    sets = asc.get(f"/v1/appStoreVersionLocalizations/{loc_id}/appScreenshotSets?include=appScreenshots")
    for s in sets["data"]:
        if s["attributes"]["screenshotDisplayType"] == DISPLAY_TYPE:
            count = len(s["relationships"]["appScreenshots"]["data"])
            return s["id"], count
    made = asc.post("appScreenshotSets", "/v1/appScreenshotSets", {"screenshotDisplayType": DISPLAY_TYPE},
                    {"appStoreVersionLocalization": asc.rel("appStoreVersionLocalizations", loc_id)})
    return made["data"]["id"], 0


def upload(set_id: str, png: Path) -> None:
    data = png.read_bytes()
    made = asc.post("appScreenshots", "/v1/appScreenshots", {"fileName": png.name, "fileSize": len(data)},
                    {"appScreenshotSet": asc.rel("appScreenshotSets", set_id)})["data"]
    for op in made["attributes"]["uploadOperations"]:
        chunk = data[op["offset"]:op["offset"] + op["length"]]
        headers = {h["name"]: h["value"] for h in op["requestHeaders"]}
        req = urllib.request.Request(op["url"], data=chunk, method=op["method"], headers=headers)
        with urllib.request.urlopen(req, timeout=120):
            pass
    asc.patch("appScreenshots", f"/v1/appScreenshots/{made['id']}", made["id"],
              {"uploaded": True, "sourceFileChecksum": hashlib.md5(data).hexdigest()})


def clear_set(set_id: str) -> None:
    for shot in asc.get(f"/v1/appScreenshotSets/{set_id}/appScreenshots?limit=50")["data"]:
        asc.request("DELETE", f"/v1/appScreenshots/{shot['id']}")


def main(argv: list[str]) -> int:
    replace = "--replace" in argv
    argv = [a for a in argv if a != "--replace"]
    apps = json.loads(Path(argv[0]).read_text())
    out = Path(os.environ["STORE_OUT"])
    for game in select(argv[1:]):
        shots = sorted((out / game.slug / "shots" / "apple").glob("*.png"))
        try:
            set_id, existing = screenshot_set(en_us_localization(apps[game.slug]["apple_id"]))
            if replace and existing:
                clear_set(set_id)
                existing = 0
            if existing >= len(shots):
                print(game.slug, "skip", existing, flush=True)
                continue
            for png in shots[existing:]:
                upload(set_id, png)
            print(game.slug, "uploaded", len(shots) - existing, flush=True)
        except (asc.AscError, OSError) as err:
            print(game.slug, "FAIL", str(err)[:300], flush=True)
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
