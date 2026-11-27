#!/usr/bin/env python3
"""Summarise a Mosselora tour's touch-scan.tsv (checklist 91).

Each row of touch-scan.tsv is: shot, element name ("<element> < <parent>"), size ("WxH" in
canvas points). Rows are grouped by element kind (digits replaced by N), with the smallest and
largest short side and the first shot each kind appears in, largest groups first.

usage: tools/evergrove_touch_scan_summary.py <tour-dir>/touch-scan.tsv [more.tsv ...]
"""
import collections
import re
import sys


def summarise(path):
    kinds = collections.defaultdict(list)
    with open(path, encoding="utf-8") as rows:
        for line in rows:
            parts = line.rstrip("\n").split("\t")
            if len(parts) != 3 or "x" not in parts[2]:
                continue
            shot, name, size = parts
            try:
                width, height = (float(v) for v in size.split("x"))
            except ValueError:
                continue
            kinds[re.sub(r"\d+", "N", name)].append((min(width, height), shot))
    print(f"== {path}: {sum(len(v) for v in kinds.values())} rows, {len(kinds)} kinds")
    for kind, seen in sorted(kinds.items(), key=lambda item: -len(item[1])):
        shots = sorted({shot for _, shot in seen})
        more = f" (+{len(shots) - 1} shots)" if len(shots) > 1 else ""
        sides = [side for side, _ in seen]
        print(f"{len(seen):5}  {min(sides):3.0f}-{max(sides):3.0f} pt  {kind}  e.g. {shots[0]}{more}")


def main(argv):
    if len(argv) < 2:
        print(__doc__.strip())
        return 2
    for path in argv[1:]:
        summarise(path)
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
