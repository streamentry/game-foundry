"""Hand-designed logos for the games whose generated logo is missing.

Usage: python3 tools/store_catalog/logo_designs.py <out_dir> [slug ...]
Writes <out_dir>/<slug>.png (1024x1024 RGB). Skips slugs that already exist
unless --force is passed.
"""
from __future__ import annotations

import math
import sys
from pathlib import Path

from logo_kit import CORAL, CREAM, GOLD, INK, Logo, center, grid_boxes, hexc


def nonogram_canvas(L: Logo) -> None:
    L.tile((212, 212, 812, 812), fill=CREAM, radius=40)
    heart = ["01010", "11111", "11111", "01110", "00100"]
    cells = grid_boxes(5, 5, 312, 312, 88, 8)
    for (r, c), b in cells.items():
        on = heart[r][c] == "1"
        L.tile(b, fill=CORAL if on else hexc("#E9E1CC"), radius=14, shadow=False, highlight=on)
    for c, clue in enumerate(["1", "4", "4", "4", "1"]):
        L.text(center(cells[(0, c)])[0], 270, clue, 34, fill=hexc("#5A6070"))
    for r, clue in enumerate(["1 1", "5", "5", "3", "1"]):
        L.text(266, center(cells[(r, 0)])[1], clue, 30, fill=hexc("#5A6070"))


def nurikabe_bay(L: Logo) -> None:
    sea, sand = hexc("#1E5F86"), hexc("#F1DDB0")
    layout = ["SS#", "#S#", "#S2"]
    layout = [["3", ".", "#"], ["#", "#", "."], ["#", "2", "#"]]
    for (r, c), b in grid_boxes(3, 3, 222, 222, 180, 20).items():
        v = layout[r][c]
        if v == "#":
            L.tile(b, fill=sea, radius=30, highlight=False)
        else:
            L.tile(b, fill=sand, radius=30)
            if v != ".":
                L.text(*center(b), v, 110, fill=hexc("#16405C"))


def odd_bloom(L: Logo) -> None:
    def flower(cx, cy, r, petals, petal, core):
        for i in range(petals):
            a = 2 * math.pi * i / petals - math.pi / 2
            L.circle(cx + math.cos(a) * r * 0.55, cy + math.sin(a) * r * 0.55, r * 0.42, petal)
        L.circle(cx, cy, r * 0.3, core)
    for (r, c), b in grid_boxes(3, 3, 222, 222, 180, 20).items():
        L.tile(b, fill=CREAM, radius=36)
        cx, cy = center(b)
        if (r, c) == (1, 2):
            flower(cx, cy, 70, 6, CORAL, GOLD)
        else:
            flower(cx, cy, 70, 5, hexc("#F0C978"), hexc("#B5643C"))


def one_path(L: Logo) -> None:
    pts = [(312, 300), (712, 300), (812, 512), (712, 724), (312, 724), (212, 512)]
    order = [0, 1, 2, 3, 4, 5, 0, 3]
    L.line([pts[i] for i in order], fill=GOLD, width=34, glow=(236, 184, 76, 120))
    for x, y in pts:
        L.circle(x, y, 46, CREAM, shadow=True)
        L.circle(x, y, 18, hexc("#1F4A52"))


def pipe_garden(L: Logo) -> None:
    for (r, c), b in grid_boxes(3, 3, 222, 222, 180, 20).items():
        L.tile(b, fill=hexc("#E8E0C8"), radius=30)
    copper = hexc("#C97B45")
    path = [(122, 312), (312, 312), (312, 512), (512, 512), (512, 712), (712, 712)]
    L.line(path, fill=hexc("#8E4E25"), width=78)
    L.line(path, fill=copper, width=58)
    L.line([(x, y - 12) for x, y in path], fill=(236, 170, 120), width=14)
    L.leaf(760, 660, 62, -50, hexc("#5FAE5C"))
    L.leaf(790, 700, 46, 10, hexc("#82C46B"))


def quiet_camp(L: Logo) -> None:
    cells = grid_boxes(3, 3, 222, 222, 180, 20)
    for b in cells.values():
        L.tile(b, fill=hexc("#EEE6CF"), radius=30)

    def tree(b):
        cx, cy = center(b)
        L.tile((cx - 10, cy + 20, cx + 10, cy + 70), fill=hexc("#7A5232"), radius=6, shadow=False, highlight=False)
        L.circle(cx, cy - 8, 58, hexc("#3E8A4B"), shadow=True)
        L.circle(cx - 18, cy - 26, 22, hexc("#58A660"))

    def tent(b):
        cx, cy = center(b)
        L.polygon([(cx - 72, cy + 52), (cx, cy - 64), (cx + 72, cy + 52)], hexc("#E39A4D"), shadow=True)
        L.polygon([(cx - 22, cy + 52), (cx, cy + 4), (cx + 22, cy + 52)], hexc("#7C3F1E"))
    tree(cells[(0, 0)]); tent(cells[(0, 1)]); tent(cells[(2, 1)]); tree(cells[(2, 2)])


def quiet_shot(L: Logo) -> None:
    L.tile((780, 180, 830, 844), fill=CREAM, radius=24)
    for i, r in enumerate((120, 84, 48)):
        L.circle(372, 286, r, CREAM if i % 2 == 0 else hexc("#2B3A5C"))
    L.circle(372, 286, 20, CORAL)
    dash = [(270, 790), (760, 520), (372, 286)]
    for (x0, y0), (x1, y1) in zip(dash, dash[1:]):
        n = 9
        for k in range(n):
            if k % 2 == 0:
                a, b = k / n, (k + 1) / n
                L.line([(x0 + (x1 - x0) * a, y0 + (y1 - y0) * a), (x0 + (x1 - x0) * b, y0 + (y1 - y0) * b)],
                       fill=hexc("#F6C27A"), width=16)
    L.circle(270, 790, 58, CORAL, shadow=True)
    L.circle(252, 770, 18, (255, 196, 180))


def renzoku_meadow(L: Logo) -> None:
    cells = grid_boxes(2, 2, 252, 252, 240, 40)
    for (r, c), b in cells.items():
        L.tile(b, fill=CREAM, radius=40)
        L.text(*center(b), "34 21"[r * 3 + c], 150)
    yb = hexc("#F2CE3E")
    L.tile((492, 340, 532, 444), fill=yb, radius=16, shadow=False)
    L.tile((492, 620, 532, 724), fill=yb, radius=16, shadow=False)
    L.tile((580, 492, 684, 532), fill=yb, radius=16, shadow=False)


def ripple_rooms(L: Logo) -> None:
    for i, r in enumerate((420, 340, 260)):
        L.circle(512, 512, r, None, outline=(160, 214, 240, 70 + i * 40), width=10)
    cells = grid_boxes(2, 2, 292, 292, 210, 20)
    vals = {(0, 0): "1", (0, 1): "2", (1, 1): "1"}
    for k, b in cells.items():
        L.tile(b, fill=CREAM if k in vals else hexc("#8ED0F0"), radius=36)
        if k in vals:
            L.text(*center(b), vals[k], 140, fill=hexc("#17406A"))


def shikaku_garden(L: Logo) -> None:
    rects = [((212, 212, 532, 432), hexc("#A9CF83"), "4"), ((552, 212, 812, 532), hexc("#F1DDA4"), "4"),
             ((212, 452, 532, 812), hexc("#E9C46A"), "6"), ((552, 552, 812, 812), hexc("#CFE3B0"), "2")]
    for box, fill, num in rects:
        L.tile(box, fill=fill, radius=36)
        L.text(*center(box), num, 120, fill=hexc("#2E4A22"))


def skyline_steps(L: Logo) -> None:
    heights = [300, 480, 380, 560]
    for i, h in enumerate(heights):
        x0 = 232 + i * 150
        top = 812 - h
        L.tile((x0, top, x0 + 130, 812), fill=hexc("#F08A4B") if i != 3 else GOLD, radius=22)
        for wy in range(top + 40, 790, 70):
            L.tile((x0 + 38, wy, x0 + 92, wy + 30), fill=hexc("#FFE3B8"), radius=8, shadow=False, highlight=False)
    L.tile((196, 830, 828, 846), fill=CREAM, radius=8, shadow=False)
    L.circle(268, 300, 64, CREAM, shadow=True)
    L.text(268, 300, "4", 84)


def sliding_tiles(L: Logo) -> None:
    wood, dark = hexc("#E8C99A"), hexc("#7A4E2C")
    L.tile((196, 196, 828, 828), fill=hexc("#5A3A22"), radius=48, highlight=False)
    nums = ["123", "456", "78 "]
    for (r, c), b in grid_boxes(3, 3, 226, 226, 184, 16).items():
        n = nums[r][c]
        if n == " ":
            L.tile(b, fill=hexc("#4A2F1B"), radius=26, shadow=False, highlight=False)
            continue
        if n == "8":
            b = (b[0] + 40, b[1], b[2] + 40, b[3])
        L.tile(b, fill=wood, radius=28)
        L.text(*center(b), n, 104, fill=dark)


def star_atlas(L: Logo) -> None:
    cells = grid_boxes(4, 4, 232, 232, 130, 10)
    for b in cells.values():
        L.tile(b, fill=hexc("#1C2654"), radius=18, shadow=False, highlight=False,
               outline=(120, 140, 210), width=2)
    for x0, y0, x1, y1 in ((232, 232, 512, 512), (512, 232, 792, 372), (512, 372, 792, 792), (232, 512, 512, 792)):
        L.tile((x0 + 4, y0 + 4, x1 - 4, y1 - 4), fill=None, radius=22, shadow=False, highlight=False,
               outline=(246, 214, 120), width=8)
    for r, c in ((0, 1), (1, 3), (2, 0), (3, 2)):
        L.star(*center(cells[(r, c)]), 50, hexc("#F7D774"))


def takuzu_lab(L: Logo) -> None:
    pattern = ["ABAB", "BAAB", "ABBA", "BABA"]
    for (r, c), b in grid_boxes(4, 4, 222, 222, 130, 16).items():
        L.tile(b, fill=hexc("#EDE7DA"), radius=24)
        cx, cy = center(b)
        L.circle(cx, cy, 42, CORAL if pattern[r][c] == "A" else hexc("#2F4858"))


def thermometer_studio(L: Logo) -> None:
    for i, fill in enumerate((0.85, 0.5, 0.7)):
        x = 312 + i * 200
        L.tile((x - 50, 210, x + 50, 700), fill=CREAM, radius=50)
        L.circle(x, 730, 92, CREAM, shadow=True)
        top = 700 - (700 - 260) * fill
        L.tile((x - 26, top, x + 26, 720), fill=hexc("#F2869A"), radius=26, shadow=False, highlight=False)
        L.circle(x, 730, 66, hexc("#F2869A"))
        L.circle(x - 22, 708, 16, (255, 200, 210))


def vowel_garden(L: Logo) -> None:
    letters = "BLOOM"
    for i, ch in enumerate(letters):
        x0 = 142 + i * 152
        vowel = ch in "AEIOU"
        L.tile((x0, 420, x0 + 136, 556), fill=GOLD if vowel else CREAM, radius=28)
        L.text(x0 + 68, 490, ch, 92, fill=hexc("#1F4D33"))
        if vowel:
            L.line([(x0 + 68, 420), (x0 + 68, 330)], fill=hexc("#5FAE5C"), width=14)
            L.leaf(x0 + 96, 350, 40, -30, hexc("#7CC46A"))
            L.leaf(x0 + 40, 372, 34, 210, hexc("#5FAE5C"))
    L.tile((120, 600, 904, 630), fill=hexc("#8B5E34"), radius=14, shadow=False, highlight=False)


def word_ladder_lane(L: Logo) -> None:
    rail = hexc("#C48A4C")
    L.tile((208, 150, 240, 874), fill=rail, radius=16)
    L.tile((784, 150, 816, 874), fill=rail, radius=16)
    words = [("COLD", None), ("CORD", 2), ("WORD", 0)]
    for i, (w, hi) in enumerate(words):
        y0 = 216 + i * 214
        for j, ch in enumerate(w):
            x0 = 262 + j * 130
            L.tile((x0, y0, x0 + 112, y0 + 150), fill=GOLD if j == hi else CREAM, radius=24)
            L.text(x0 + 56, y0 + 78, ch, 88, fill=hexc("#4A2A12"))


def word_search_shores(L: Logo) -> None:
    rows = ["SEAW", "TAVE", "ORSN", "MDUE"]
    cells = grid_boxes(4, 4, 232, 232, 130, 10)
    L.tile((222, 222, 802, 372), fill=hexc("#F3C77A"), radius=75, shadow=False, highlight=False)
    for (r, c), b in cells.items():
        L.text(*center(b), rows[r][c], 84, fill=hexc("#0F3D4C") if r == 0 else CREAM)
    L.line([(150, 860), (300, 830), (450, 860), (600, 830), (750, 860), (874, 836)], fill=(160, 220, 230), width=12)


def word_square_studio(L: Logo) -> None:
    rows = ["ART", "RUE", "TEN"]
    for (r, c), b in grid_boxes(3, 3, 222, 222, 180, 20).items():
        L.tile(b, fill=GOLD if r == c else CREAM, radius=34)
        L.text(*center(b), rows[r][c], 112, fill=hexc("#3A1C5C"))


DESIGNS = {
    "nonogram-canvas": ("#2E3A46", "#1A222B", nonogram_canvas),
    "nurikabe-bay": ("#2A86B0", "#103E5C", nurikabe_bay),
    "odd-bloom": ("#8E3358", "#4E1530", odd_bloom),
    "one-path": ("#2A5A62", "#102E33", one_path),
    "pipe-garden": ("#3D7A4C", "#1A3D25", pipe_garden),
    "quiet-camp": ("#5B7D38", "#2A3F18", quiet_camp),
    "quiet-shot": ("#3B4E7A", "#1A2340", quiet_shot),
    "renzoku-meadow": ("#7A8A2C", "#3D4712", renzoku_meadow),
    "ripple-rooms": ("#2E6EA6", "#103154", ripple_rooms),
    "shikaku-garden": ("#4E7D33", "#223E14", shikaku_garden),
    "skyline-steps": ("#2E3360", "#11132C", skyline_steps),
    "sliding-tiles": ("#7A5236", "#3E2716", sliding_tiles),
    "star-atlas": ("#1A2560", "#070B2A", star_atlas),
    "takuzu-lab": ("#3C5668", "#1A2A36", takuzu_lab),
    "thermometer-studio": ("#A33445", "#5A1220", thermometer_studio),
    "vowel-garden": ("#3D8A5C", "#1A4A2E", vowel_garden),
    "word-ladder-lane": ("#8A5528", "#45270E", word_ladder_lane),
    "word-search-shores": ("#237488", "#0C3A48", word_search_shores),
    "word-square-studio": ("#6A34A0", "#2E1252", word_square_studio),
}


def main(argv: list[str]) -> int:
    force = "--force" in argv
    argv = [a for a in argv if a != "--force"]
    out = Path(argv[0])
    for slug in argv[1:] or sorted(DESIGNS):
        target = out / f"{slug}.png"
        if target.exists() and not force:
            continue
        top, bottom, fn = DESIGNS[slug]
        logo = Logo(top, bottom)
        fn(logo)
        logo.save(target)
        print("logo", slug)
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
