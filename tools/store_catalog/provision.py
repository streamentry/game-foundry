"""Register iOS bundle IDs and App Store provisioning profiles via the API.

Installs each profile into ~/Library/MobileDevice/Provisioning Profiles.
Usage: python3 tools/store_catalog/provision.py [slug ...]
"""
from __future__ import annotations

import base64
import plistlib
import subprocess
import sys
from pathlib import Path

import asc_client as asc
from catalog import select

DIST_CERT_ID = "YXVADJ4628"  # iOS Distribution: CLOUDJET (serial 1B059AAF...), in login keychain
PROFILE_DIR = Path.home() / "Library/MobileDevice/Provisioning Profiles"


def ensure_bundle_id(identifier: str, name: str) -> str:
    found = asc.get(f"/v1/bundleIds?filter[identifier]={identifier}&filter[platform]=IOS")["data"]
    exact = [b for b in found if b["attributes"]["identifier"] == identifier]
    if exact:
        return exact[0]["id"]
    made = asc.post("bundleIds", "/v1/bundleIds", {"identifier": identifier, "name": name, "platform": "IOS"})
    return made["data"]["id"]


def ensure_profile(name: str, bundle_pk: str) -> bytes:
    existing = asc.get(f"/v1/bundleIds/{bundle_pk}/profiles?limit=50")["data"]
    for prof in existing:
        a = prof["attributes"]
        if a["name"] == name and a["profileState"] == "ACTIVE" and a["profileType"] == "IOS_APP_STORE":
            return base64.b64decode(a["profileContent"])
    made = asc.post("profiles", "/v1/profiles", {"name": name, "profileType": "IOS_APP_STORE"},
                    {"bundleId": asc.rel("bundleIds", bundle_pk),
                     "certificates": {"data": [{"type": "certificates", "id": DIST_CERT_ID}]}})
    return base64.b64decode(made["data"]["attributes"]["profileContent"])


def install(content: bytes) -> str:
    decoded = subprocess.run(["security", "cms", "-D"], input=content, capture_output=True, check=True).stdout
    uuid = plistlib.loads(decoded)["UUID"]
    PROFILE_DIR.mkdir(parents=True, exist_ok=True)
    (PROFILE_DIR / f"{uuid}.mobileprovision").write_bytes(content)
    return uuid


def main(argv: list[str]) -> int:
    for game in select(argv):
        pk = ensure_bundle_id(game.app_id, game.name)
        uuid = install(ensure_profile(game.profile_name, pk))
        print(f"{game.slug}\t{game.app_id}\t{pk}\t{uuid}")
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
