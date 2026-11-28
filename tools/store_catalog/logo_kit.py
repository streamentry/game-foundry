"""Drawing kit for hand-designed app logos (supersampled PIL vector style).

Matches the generated logo family: rich dark background, raised cream tiles
with soft shadow and top highlight, bold rounded numerals, gold/coral accents.
All coordinates are in a 1024 design space; the canvas renders at 2x.
"""
from __future__ import annotations

import math

from PIL import Image, ImageDraw, ImageFilter, ImageFont

S = 2  # supersample factor
W = 1024 * S
FONT = "/System/Library/Fonts/Avenir Next.ttc"
CREAM = (250, 243, 226)
INK = (34, 38, 52)
GOLD = (236, 184, 76)
CORAL = (234, 104, 84)


def hexc(c: str) -> tuple[int, int, int]:
    c = c.lstrip("#")
    return tuple(int(c[i:i + 2], 16) for i in (0, 2, 4))


def font(px: int, bold: bool = True) -> ImageFont.FreeTypeFont:
    return ImageFont.truetype(FONT, int(px * S), index=0 if bold else 7)


class Logo:
    def __init__(self, top: str, bottom: str) -> None:
        t, b = hexc(top), hexc(bottom)
        self.img = Image.new("RGB", (W, W))
        d = ImageDraw.Draw(self.img)
        for y in range(W):
            k = y / (W - 1)
            d.line([(0, y), (W, y)], fill=tuple(int(t[i] + (b[i] - t[i]) * k) for i in range(3)))
        # soft radial lift in the centre
        glow = Image.new("L", (W, W), 0)
        ImageDraw.Draw(glow).ellipse((W * 0.1, W * 0.05, W * 0.9, W * 0.85), fill=60)
        glow = glow.filter(ImageFilter.GaussianBlur(W * 0.12))
        self.img = Image.composite(Image.new("RGB", (W, W), tuple(min(255, c + 40) for c in t)), self.img, glow)
        self.layer = Image.new("RGBA", (W, W), (0, 0, 0, 0))

    # -- primitives (1024 design space) -------------------------------------
    @staticmethod
    def _b(box):
        return tuple(v * S for v in box)

    def _shadow(self, shape_fn, blur=14, offset=12, alpha=110) -> None:
        sh = Image.new("RGBA", (W, W), (0, 0, 0, 0))
        shape_fn(ImageDraw.Draw(sh), (0, 0, 0, alpha))
        sh = sh.filter(ImageFilter.GaussianBlur(blur * S))
        self.layer.alpha_composite(sh, (0, int(offset * S)))

    def tile(self, box, fill=CREAM, radius=34, shadow=True, highlight=True, outline=None, width=0) -> None:
        b = self._b(box)
        r = radius * S
        if shadow:
            self._shadow(lambda d, c: d.rounded_rectangle(b, r, fill=c))
        d = ImageDraw.Draw(self.layer)
        d.rounded_rectangle(b, r, fill=fill, outline=outline, width=width * S)
        if highlight:
            hl = Image.new("RGBA", (W, W), (0, 0, 0, 0))
            hd = ImageDraw.Draw(hl)
            hd.rounded_rectangle((b[0] + 6 * S, b[1] + 5 * S, b[2] - 6 * S, b[1] + (b[3] - b[1]) * 0.45), r * 0.8,
                                 fill=(255, 255, 255, 34))
            self.layer.alpha_composite(hl)
            bd = ImageDraw.Draw(self.layer)
            bd.rounded_rectangle((b[0], b[3] - 10 * S, b[2], b[3]), r, fill=None)

    def text(self, cx, cy, s, size, fill=INK, bold=True) -> None:
        ImageDraw.Draw(self.layer).text((cx * S, cy * S), s, font=font(size, bold), fill=fill, anchor="mm")

    def circle(self, cx, cy, r, fill, shadow=False, outline=None, width=0) -> None:
        b = self._b((cx - r, cy - r, cx + r, cy + r))
        if shadow:
            self._shadow(lambda d, c: d.ellipse(b, fill=c), blur=10, offset=8)
        ImageDraw.Draw(self.layer).ellipse(b, fill=fill, outline=outline, width=int(width * S))

    def line(self, pts, fill, width, glow=None) -> None:
        p = [(x * S, y * S) for x, y in pts]
        if glow:
            g = Image.new("RGBA", (W, W), (0, 0, 0, 0))
            ImageDraw.Draw(g).line(p, fill=glow, width=int(width * 2.6 * S), joint="curve")
            self.layer.alpha_composite(g.filter(ImageFilter.GaussianBlur(width * 1.2 * S)))
        d = ImageDraw.Draw(self.layer)
        d.line(p, fill=fill, width=int(width * S), joint="curve")
        for x, y in (p[0], p[-1]):
            rr = width * S / 2
            d.ellipse((x - rr, y - rr, x + rr, y + rr), fill=fill)

    def polygon(self, pts, fill, shadow=False) -> None:
        p = [(x * S, y * S) for x, y in pts]
        if shadow:
            self._shadow(lambda d, c: d.polygon(p, fill=c), blur=10, offset=8)
        ImageDraw.Draw(self.layer).polygon(p, fill=fill)

    def star(self, cx, cy, r, fill, points=5, inner=0.45, shadow=True) -> None:
        pts = []
        for i in range(points * 2):
            rr = r if i % 2 == 0 else r * inner
            a = -math.pi / 2 + i * math.pi / points
            pts.append((cx + rr * math.cos(a), cy + rr * math.sin(a)))
        self.polygon(pts, fill, shadow=shadow)

    def leaf(self, cx, cy, size, angle, fill) -> None:
        pts = []
        for t in range(0, 361, 10):
            a = math.radians(t)
            x, y = math.cos(a) * size, math.sin(a) * size * 0.42 * (1 - 0.0 * math.cos(a))
            ca, sa = math.cos(math.radians(angle)), math.sin(math.radians(angle))
            pts.append((cx + x * ca - y * sa, cy + x * sa + y * ca))
        self.polygon(pts, fill)

    def save(self, path) -> None:
        out = self.img.convert("RGBA")
        out.alpha_composite(self.layer)
        out.convert("RGB").resize((1024, 1024), Image.LANCZOS).save(path, optimize=True)


def grid_boxes(n, m, x0, y0, size, gap):
    """Return dict[(r,c)] -> box for an n x m grid of square cells."""
    return {(r, c): (x0 + c * (size + gap), y0 + r * (size + gap),
                     x0 + c * (size + gap) + size, y0 + r * (size + gap) + size)
            for r in range(n) for c in range(m)}


def center(box):
    return (box[0] + box[2]) / 2, (box[1] + box[3]) / 2
