"""Localized store listings: load translations and push them via the store APIs.

Translations live in store/metadata/<slug>/i18n/<lang>.json; the language ->
store-locale map is store/metadata/store-locales.json. Product names stay in
English on every locale.

Usage:
  python3 tools/store_catalog/i18n_store.py apple <apple_apps.json> [slug ...]
  python3 tools/store_catalog/i18n_store.py play [slug ...]
Apple requires the app's version to be editable (not waiting for review).
"""
from __future__ import annotations

import json
import sys
from pathlib import Path

from catalog import ROOT, Game, select

LOCALES = json.loads((ROOT / "store" / "metadata" / "store-locales.json").read_text())
LOCALES.pop("_note", None)


def translations(game: Game) -> dict[str, dict]:
    out = {}
    for lang in LOCALES:
        p = game.path.parent.parent / "store" / "metadata" / game.slug / "i18n" / f"{lang}.json"
        if p.exists():
            out[lang] = json.loads(p.read_text(encoding="utf-8"))
    return out


def apple(game: Game, app_id: str) -> str:
    import asc_client as asc
    meta = game.metadata()
    name = meta["apple"]["name"]
    info = [i for i in asc.get(f"/v1/apps/{app_id}/appInfos")["data"]
            if i["attributes"].get("state") != "READY_FOR_DISTRIBUTION"][0]["id"]
    ver = [v for v in asc.get(f"/v1/apps/{app_id}/appStoreVersions")["data"]
           if v["attributes"]["appStoreState"] in ("PREPARE_FOR_SUBMISSION", "DEVELOPER_REJECTED",
                                                    "REJECTED", "METADATA_REJECTED")][0]["id"]
    have_info = {l["attributes"]["locale"]: l["id"] for l in
                 asc.get(f"/v1/appInfos/{info}/appInfoLocalizations?limit=200")["data"]}
    have_ver = {l["attributes"]["locale"]: l["id"] for l in
                asc.get(f"/v1/appStoreVersions/{ver}/appStoreVersionLocalizations?limit=200")["data"]}
    done, failed = 0, []
    for lang, t in translations(game).items():
        for loc in LOCALES[lang]["apple"]:
            try:
                ia = {"name": name, "subtitle": t["subtitle"], "privacyPolicyUrl": meta["apple"]["privacy_policy_url"]}
                if loc in have_info:
                    asc.patch("appInfoLocalizations", f"/v1/appInfoLocalizations/{have_info[loc]}", have_info[loc], ia)
                else:
                    asc.post("appInfoLocalizations", "/v1/appInfoLocalizations", {"locale": loc, **ia},
                             {"appInfo": asc.rel("appInfos", info)})
                va = {"description": t["description"], "keywords": t["keywords"],
                      "supportUrl": meta["apple"]["support_url"],
                      "marketingUrl": "https://streamentry-puzzles.4rqnf2gvxf.workers.dev/"}
                if loc in have_ver:
                    asc.patch("appStoreVersionLocalizations", f"/v1/appStoreVersionLocalizations/{have_ver[loc]}",
                              have_ver[loc], va)
                else:
                    asc.post("appStoreVersionLocalizations", "/v1/appStoreVersionLocalizations", {"locale": loc, **va},
                             {"appStoreVersion": asc.rel("appStoreVersions", ver)})
                done += 1
            except asc.AscError as err:
                failed.append(f"{loc}: {str(err)[:160]}")
    return f"apple locales={done} failed={len(failed)} {failed[:3]}"


def play(game: Game) -> str:
    import play_client as pc
    meta = game.metadata()["google_play"]
    pkg = game.app_id
    edit = pc.call("POST", f"{pc.BASE}/{pkg}/edits", {})["id"]
    base = f"{pc.BASE}/{pkg}/edits/{edit}"
    done, failed = 0, []
    for lang, t in translations(game).items():
        for loc in LOCALES[lang]["play"]:
            try:
                pc.call("PUT", f"{base}/listings/{loc}",
                        {"language": loc, "title": meta["title"], "shortDescription": t["short_description"],
                         "fullDescription": t["description"]})
                done += 1
            except pc.PlayError as err:
                failed.append(f"{loc}: {str(err)[:160]}")
    pc.call("POST", f"{base}:commit")
    return f"play locales={done} failed={len(failed)} {failed[:3]}"


def main(argv: list[str]) -> int:
    store = argv[0]
    if store == "apple":
        apps = json.loads(Path(argv[1]).read_text())
        for g in select(argv[2:]):
            print(g.slug, apple(g, apps[g.slug]["apple_id"]), flush=True)
    else:
        import play_client as pc
        for g in select(argv[1:]):
            try:
                print(g.slug, play(g), flush=True)
            except pc.PlayError as err:
                print(g.slug, "FAIL", str(err)[:300], flush=True)
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
