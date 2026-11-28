"""Fill each catalog app's App Store Connect record through the API.

Sets content rights, category, age rating (all none), subtitle, privacy URL,
version 1.0.0 text, copyright, automatic release, review contact, free price
and availability. Owner-authorized declarations (2026-11-28): no third-party
content, age rating all none.
Usage: python3 tools/store_catalog/asc_listing.py <apple_apps.json> [slug ...]
Env: ASC_* plus REVIEW_SOURCE_APP (an app whose review contact is reused).
"""
from __future__ import annotations

import json
import os
import sys
from pathlib import Path

import asc_client as asc
from catalog import VERSION_NAME, Game, select

WORD_GAMES = {"vowel-garden", "word-ladder-lane", "word-search-shores", "word-square-studio"}
COPYRIGHT = "2026 CLOUDJET SOLUTIONS PTE. LTD."
SITE = "https://streamentry-puzzles.4rqnf2gvxf.workers.dev/"
EXCLUDED_TERRITORIES = {"CHN"}  # needs security assessment + AIGC filings first
AGE_ENUMS = ["alcoholTobaccoOrDrugUseOrReferences", "contests", "gamblingSimulated", "gunsOrOtherWeapons",
             "horrorOrFearThemes", "matureOrSuggestiveThemes", "medicalOrTreatmentInformation",
             "profanityOrCrudeHumor", "sexualContentGraphicAndNudity", "sexualContentOrNudity",
             "violenceCartoonOrFantasy", "violenceRealistic", "violenceRealisticProlongedGraphicOrSadistic"]
AGE_BOOLS = ["advertising", "ageAssurance", "gambling", "healthOrWellnessTopics", "lootBox", "messagingAndChat",
             "parentalControls", "unrestrictedWebAccess", "userGeneratedContent", "datingAndRelationships",
             "sharedLocation", "socialMedia", "socialMediaAgeRestricted"]
REVIEW_NOTES = ("Offline single-player puzzle game. No sign-in, network access, ads, purchases, analytics "
                "or data collection. Tap a cell or control to play; Undo and Restart are always available. "
                "Rules are shown on the first screen.")


def review_contact() -> dict:
    src = os.environ["REVIEW_SOURCE_APP"]
    ver = asc.get(f"/v1/apps/{src}/appStoreVersions?limit=1")["data"][0]["id"]
    a = asc.get(f"/v1/appStoreVersions/{ver}/appStoreReviewDetail")["data"]["attributes"]
    return {k: a[k] for k in ("contactFirstName", "contactLastName", "contactPhone", "contactEmail")}


def set_app_info(game: Game, app_id: str, meta: dict) -> None:
    asc.patch("apps", f"/v1/apps/{app_id}", app_id, {"contentRightsDeclaration": "DOES_NOT_USE_THIRD_PARTY_CONTENT"})
    info = [i for i in asc.get(f"/v1/apps/{app_id}/appInfos")["data"]
            if i["attributes"].get("state") != "READY_FOR_DISTRIBUTION"][0]
    sub = "GAMES_WORD" if game.slug in WORD_GAMES else "GAMES_PUZZLE"
    asc.patch("appInfos", f"/v1/appInfos/{info['id']}", info["id"], relationships={
        "primaryCategory": asc.rel("appCategories", "GAMES"),
        "primarySubcategoryOne": asc.rel("appCategories", sub)})
    loc = [l for l in asc.get(f"/v1/appInfos/{info['id']}/appInfoLocalizations")["data"]
           if l["attributes"]["locale"] == "en-US"][0]
    asc.patch("appInfoLocalizations", f"/v1/appInfoLocalizations/{loc['id']}", loc["id"],
              {"subtitle": meta["apple"]["subtitle"], "privacyPolicyUrl": meta["apple"]["privacy_policy_url"]})
    age = asc.get(f"/v1/appInfos/{info['id']}/ageRatingDeclaration")["data"]
    attrs = {k: "NONE" for k in AGE_ENUMS if k in age["attributes"]}
    attrs.update({k: False for k in AGE_BOOLS if k in age["attributes"]})
    asc.patch("ageRatingDeclarations", f"/v1/ageRatingDeclarations/{age['id']}", age["id"], attrs)


def set_version(app_id: str, meta: dict, contact: dict) -> str:
    ver = [v for v in asc.get(f"/v1/apps/{app_id}/appStoreVersions")["data"]
           if v["attributes"]["appStoreState"] in ("PREPARE_FOR_SUBMISSION", "DEVELOPER_REJECTED")][0]
    vid = ver["id"]
    asc.patch("appStoreVersions", f"/v1/appStoreVersions/{vid}", vid,
              {"versionString": VERSION_NAME, "copyright": COPYRIGHT, "releaseType": "AFTER_APPROVAL"})
    loc = [l for l in asc.get(f"/v1/appStoreVersions/{vid}/appStoreVersionLocalizations")["data"]
           if l["attributes"]["locale"] == "en-US"][0]
    a = meta["apple"]
    asc.patch("appStoreVersionLocalizations", f"/v1/appStoreVersionLocalizations/{loc['id']}", loc["id"],
              {"description": a["description"], "keywords": a["keywords"], "supportUrl": a["support_url"],
               "marketingUrl": SITE})
    detail = asc.request("GET", f"/v1/appStoreVersions/{vid}/appStoreReviewDetail", ok=(200, 404)).get("data")
    attrs = {**contact, "demoAccountRequired": False, "notes": REVIEW_NOTES}
    if detail:
        asc.patch("appStoreReviewDetails", f"/v1/appStoreReviewDetails/{detail['id']}", detail["id"], attrs)
    else:
        asc.post("appStoreReviewDetails", "/v1/appStoreReviewDetails", attrs,
                 {"appStoreVersion": asc.rel("appStoreVersions", vid)})
    return vid


def set_free_price(app_id: str) -> None:
    points = asc.get(f"/v1/apps/{app_id}/appPricePoints?filter[territory]=USA&limit=200")["data"]
    free = [p for p in points if float(p["attributes"]["customerPrice"]) == 0.0][0]
    body = {"data": {"type": "appPriceSchedules", "relationships": {
                "app": asc.rel("apps", app_id),
                "baseTerritory": asc.rel("territories", "USA"),
                "manualPrices": {"data": [{"type": "appPrices", "id": "${price}"}]}}},
            "included": [{"type": "appPrices", "id": "${price}", "attributes": {"startDate": None},
                          "relationships": {"appPricePoint": asc.rel("appPricePoints", free["id"])}}]}
    asc.request("POST", "/v1/appPriceSchedules", body)


def set_availability(app_id: str, territories: list[str]) -> None:
    refs = [{"type": "territoryAvailabilities", "id": f"${{{t}}}"} for t in territories]
    included = [{"type": "territoryAvailabilities", "id": f"${{{t}}}",
                 "attributes": {"available": t not in EXCLUDED_TERRITORIES},
                 "relationships": {"territory": asc.rel("territories", t)}} for t in territories]
    body = {"data": {"type": "appAvailabilities", "attributes": {"availableInNewTerritories": True},
                     "relationships": {"app": asc.rel("apps", app_id),
                                       "territoryAvailabilities": {"data": refs}}},
            "included": included}
    asc.request("POST", "/v2/appAvailabilities", body)


def main(argv: list[str]) -> int:
    apps = json.loads(Path(argv[0]).read_text())
    contact = review_contact()
    territories = [t["id"] for t in asc.get("/v1/territories?limit=200")["data"]]
    for game in select(argv[1:]):
        app_id = apps[game.slug]["apple_id"]
        meta = game.metadata()
        steps = [("info", lambda: set_app_info(game, app_id, meta)),
                 ("version", lambda: set_version(app_id, meta, contact)),
                 ("price", lambda: set_free_price(app_id)),
                 ("availability", lambda: set_availability(app_id, territories))]
        result = []
        for name, fn in steps:
            try:
                fn()
                result.append(f"{name}=ok")
            except asc.AscError as err:
                result.append(f"{name}=FAIL {str(err)[:300]}")
        print(game.slug, " ".join(result), flush=True)
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
