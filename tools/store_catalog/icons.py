"""Generate original 1024x1024 app icons for the Godot puzzle catalog.

Each icon is an opaque square (no alpha, as Apple requires) with a deep
background and a small motif drawn from the game's own rules: a grid of
cells, digits or letters, plus optional path segments. No third-party art.

Usage: python3 tools/store_catalog/icons.py [slug ...]
Writes games/<slug>/icon.png (1024) and games/<slug>/icon_fg.png
(432 adaptive foreground) and games/<slug>/icon_bg.png (432 background).
"""
from __future__ import annotations

import sys
from pathlib import Path

from PIL import Image, ImageDraw, ImageFilter, ImageFont

from motifs import MOTIFS

ROOT = Path(__file__).resolve().parents[2]
FONT = "/System/Library/Fonts/Avenir Next.ttc"
SIZE = 1024


def _hex(c: str) -> tuple[int, int, int]:
    c = c.lstrip("#")
    return tuple(int(c[i:i + 2], 16) for i in (0, 2, 4))


def _font(px: int) -> ImageFont.FreeTypeFont:
    # Index 0 is Avenir Next Bold in the system collection.
    return ImageFont.truetype(FONT, px, index=0)


def _background(spec: dict, size: int) -> Image.Image:
    top, bottom = _hex(spec["bg"][0]), _hex(spec["bg"][1])
    img = Image.new("RGB", (size, size), top)
    d = ImageDraw.Draw(img)
    for y in range(size):
        t = y / (size - 1)
        d.line([(0, y), (size, y)], fill=tuple(int(top[i] + (bottom[i] - top[i]) * t) for i in range(3)))
    return img


def _cell_box(r: int, c: int, n: int, origin: float, span: float) -> tuple[float, float, float, float]:
    step = span / n
    pad = step * 0.07
    x0 = origin + c * step + pad
    y0 = origin + r * step + pad
    return x0, y0, x0 + step - 2 * pad, y0 + step - 2 * pad


def _draw_motif(img: Image.Image, spec: dict, origin: float, span: float) -> None:
    rows = spec["grid"]
    n = max(len(rows), max(len(r) for r in rows))
    step0 = span / n
    origin_x = origin + (n - max(len(r) for r in rows)) * step0 / 2
    origin_y = origin + (n - len(rows)) * step0 / 2
    fg, accent, ink = _hex(spec["fg"]), _hex(spec["accent"]), _hex(spec.get("ink", spec["bg"][1]))
    d = ImageDraw.Draw(img)
    step = span / n
    radius = step * 0.16
    font = _font(int(step * 0.56))
    for r, row in enumerate(rows):
        for c, ch in enumerate(row):
            if ch == " ":
                continue
            box = _cell_box(r, c, n, 0, span)
            box = (box[0] + origin_x, box[1] + origin_y, box[2] + origin_x, box[3] + origin_y)
            cx, cy = (box[0] + box[2]) / 2, (box[1] + box[3]) / 2
            if ch in "<>^v":
                d.text((cx, cy), ch, font=font, fill=accent, anchor="mm")
            elif ch == ":":
                rr = step * 0.13
                d.ellipse((cx - rr, cy - rr, cx + rr, cy + rr), fill=accent)
            elif ch == "|":
                d.rounded_rectangle((cx - step * 0.08, cy - step * 0.3, cx + step * 0.08, cy + step * 0.3), step * 0.06, fill=accent)
            elif ch == "#":
                d.rounded_rectangle(box, radius, fill=accent)
            elif ch == "=":
                d.rounded_rectangle(box, radius, fill=fg)
                inset = step * 0.2
                d.rounded_rectangle((box[0] + inset, box[1] + inset, box[2] - inset, box[3] - inset), radius, fill=accent)
            elif ch == "o":
                d.rounded_rectangle(box, radius, fill=fg)
                rr = step * 0.22
                d.ellipse((cx - rr, cy - rr, cx + rr, cy + rr), fill=accent)
            elif ch == "x":
                d.rounded_rectangle(box, radius, fill=fg)
                k = step * 0.18
                w = max(4, int(step * 0.07))
                d.line((cx - k, cy - k, cx + k, cy + k), fill=ink, width=w)
                d.line((cx - k, cy + k, cx + k, cy - k), fill=ink, width=w)
            elif ch == "/":
                d.rounded_rectangle(box, radius, fill=accent)
                d.line((box[0] + radius * 0.4, box[1] + radius * 0.4, box[2] - radius * 0.4, box[3] - radius * 0.4),
                       fill=fg, width=max(4, int(step * 0.05)))
            elif ch == ".":
                d.rounded_rectangle(box, radius, fill=fg)
            elif ch == "_":
                d.rounded_rectangle(box, radius, outline=fg, width=max(4, int(step * 0.06)))
            elif ch.isdigit() and spec.get("circles"):
                rr = step * 0.4
                d.ellipse((cx - rr, cy - rr, cx + rr, cy + rr), fill=fg)
                d.text((cx, cy), ch, font=font, fill=ink, anchor="mm")
            else:
                filled = ch.islower() and ch.isalpha()
                d.rounded_rectangle(box, radius, fill=accent if filled else fg)
                d.text((cx, cy + step * 0.02), ch.upper(), font=font, fill=fg if filled else ink, anchor="mm")
    for seg in spec.get("paths", []):
        pts = []
        for r, c in seg:
            pts.append((origin_x + (c + 0.5) * step, origin_y + (r + 0.5) * step))
        w = int(step * spec.get("path_width", 0.16))
        d.line(pts, fill=_hex(spec.get("path_color", spec["accent"])), width=w, joint="curve")
        for p in (pts[0], pts[-1]):
            d.ellipse((p[0] - w * 0.5, p[1] - w * 0.5, p[0] + w * 0.5, p[1] + w * 0.5),
                      fill=_hex(spec.get("path_color", spec["accent"])))


def _shadowed_motif(spec: dict, size: int, origin: float, span: float, background: Image.Image) -> Image.Image:
    layer = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    _draw_motif(layer, spec, origin, span)
    alpha = layer.split()[3]
    shadow = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    shadow.putalpha(alpha.point(lambda a: int(a * 0.35)))
    shadow = shadow.filter(ImageFilter.GaussianBlur(size * 0.012))
    out = background.convert("RGBA")
    out.alpha_composite(shadow, (0, int(size * 0.012)))
    out.alpha_composite(layer)
    return out


def render(slug: str) -> None:
    spec = MOTIFS[slug]
    game = ROOT / "games" / slug
    bg = _background(spec, SIZE)
    # Motif occupies the central 66% so iOS corner masking never clips it.
    icon = _shadowed_motif(spec, SIZE, SIZE * 0.17, SIZE * 0.66, bg).convert("RGB")
    icon.save(game / "icon.png", optimize=True)
    # Android adaptive: 432 px layers, keep motif inside the 66% safe zone.
    a = 432
    _background(spec, a).save(game / "icon_bg.png", optimize=True)
    fg = Image.new("RGBA", (a, a), (0, 0, 0, 0))
    fg = _shadowed_motif(spec, a, a * 0.24, a * 0.52, fg)
    fg.save(game / "icon_fg.png", optimize=True)


def main(argv: list[str]) -> int:
    slugs = argv or sorted(MOTIFS)
    for slug in slugs:
        render(slug)
        print("icon", slug)
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
