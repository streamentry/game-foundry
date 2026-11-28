"""Validate translated store listings: required keys and store length limits.

Usage: python3 tools/store_catalog/check_i18n.py [lang ...]
"""
import glob
import json
import sys

LIMITS = {"subtitle": 30, "keywords": 100, "short_description": 80, "description": 4000}


def main(argv):
    langs = set(argv)
    bad, n = [], 0
    for p in sorted(glob.glob("store/metadata/*/i18n/*.json")):
        lang = p.rsplit("/", 1)[1][:-5]
        if langs and lang not in langs:
            continue
        n += 1
        try:
            d = json.load(open(p, encoding="utf-8"))
        except ValueError as e:
            bad.append(f"{p}: invalid JSON {e}")
            continue
        for k, lim in LIMITS.items():
            v = d.get(k)
            if not v or not isinstance(v, str):
                bad.append(f"{p}: missing {k}")
            elif len(v) > lim or (k == "keywords" and len(v.encode('utf-8')) > 100):
                bad.append(f"{p}: {k} too long ({len(v)} chars / {len(v.encode('utf-8'))} bytes)")
    print(f"checked {n} files, {len(bad)} problems")
    for b in bad[:80]:
        print(b)
    return 1 if bad else 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
