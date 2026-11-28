"""Attach each processed build to its 1.0.0 version and submit it for App Review.

Waits for the build to be VALID; skips apps already waiting for or in review.
Export compliance comes from ITSAppUsesNonExemptEncryption=false in the IPA.
Usage: python3 tools/store_catalog/asc_submit.py <apple_apps.json> [slug ...]
"""
from __future__ import annotations

import json
import sys
from pathlib import Path

import asc_client as asc
from catalog import VERSION_NAME, select

OPEN_STATES = ("PREPARE_FOR_SUBMISSION", "DEVELOPER_REJECTED", "REJECTED", "METADATA_REJECTED")


def valid_build(app_id: str) -> str | None:
    builds = asc.get(f"/v1/builds?filter[app]={app_id}&filter[preReleaseVersion.version]={VERSION_NAME}"
                     "&sort=-uploadedDate&limit=5")["data"]
    for b in builds:
        if b["attributes"]["processingState"] == "VALID":
            return b["id"]
    return None


def cancel_pending(app_id: str) -> str:
    """Withdraw an in-flight review submission so a newer build can be attached."""
    subs = asc.get(f"/v1/reviewSubmissions?filter[app]={app_id}&filter[platform]=IOS"
                   "&filter[state]=WAITING_FOR_REVIEW,IN_REVIEW,UNRESOLVED_ISSUES")["data"]
    for sub in subs:
        asc.patch("reviewSubmissions", f"/v1/reviewSubmissions/{sub['id']}", sub["id"], {"canceled": True})
    return f"canceled {len(subs)}"


def submit(app_id: str) -> str:
    versions = asc.get(f"/v1/apps/{app_id}/appStoreVersions")["data"]
    ver = next((v for v in versions if v["attributes"]["appStoreState"] in OPEN_STATES), None)
    if ver is None:
        return "no-open-version " + ",".join(v["attributes"]["appStoreState"] for v in versions)
    build = valid_build(app_id)
    if build is None:
        return "build-not-valid-yet"
    vid = ver["id"]
    asc.patch("appStoreVersions", f"/v1/appStoreVersions/{vid}", vid,
              relationships={"build": asc.rel("builds", build)})
    pending = asc.get(f"/v1/reviewSubmissions?filter[app]={app_id}&filter[platform]=IOS"
                      "&filter[state]=READY_FOR_REVIEW")["data"]
    if pending:
        sub_id = pending[0]["id"]
    else:
        sub_id = asc.post("reviewSubmissions", "/v1/reviewSubmissions", {"platform": "IOS"},
                          {"app": asc.rel("apps", app_id)})["data"]["id"]
        asc.post("reviewSubmissionItems", "/v1/reviewSubmissionItems", {},
                 {"reviewSubmission": asc.rel("reviewSubmissions", sub_id),
                  "appStoreVersion": asc.rel("appStoreVersions", vid)})
    asc.patch("reviewSubmissions", f"/v1/reviewSubmissions/{sub_id}", sub_id, {"submitted": True})
    state = asc.get(f"/v1/reviewSubmissions/{sub_id}")["data"]["attributes"]["state"]
    return f"submitted {sub_id} {state}"


def main(argv: list[str]) -> int:
    cancel = "--cancel" in argv
    argv = [a for a in argv if a != "--cancel"]
    apps = json.loads(Path(argv[0]).read_text())
    for game in select(argv[1:]):
        if cancel:
            try:
                print(game.slug, cancel_pending(apps[game.slug]["apple_id"]), flush=True)
            except asc.AscError as err:
                print(game.slug, "CANCEL FAIL", str(err)[:400], flush=True)
            continue
        try:
            print(game.slug, submit(apps[game.slug]["apple_id"]), flush=True)
        except asc.AscError as err:
            print(game.slug, "FAIL", str(err)[:600], flush=True)
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
