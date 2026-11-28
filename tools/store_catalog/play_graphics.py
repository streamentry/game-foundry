"""Make Google Play's 512px icon and 1024x500 feature graphic for each game.

Uses the same motif as the app icon (icons.py) plus the game name.
Output: $STORE_OUT/<slug>/play/{icon512.png,feature.png}
"""
from __future__ import annotations

import os
import sys
from pathlib import Path

from PIL import Image, ImageDraw

from catalog import select
from icons import _background, _font, _hex, _shadowed_motif
from motifs import MOTIFS


def feature(slug: str, name: str, logo: Path) -> Image.Image:
    w, h = 1024, 500
    src = Image.open(logo).convert("RGB")
    edge = src.getpixel((8, 8))
    dark = tuple(int(c * 0.72) for c in edge)
    spec = {"bg": ("#%02x%02x%02x" % edge, "#%02x%02x%02x" % dark)}
    bg = _background(spec, w).crop((0, (w - h) // 2, w, (w + h) // 2))
    lum = 0.299 * edge[0] + 0.587 * edge[1] + 0.114 * edge[2]
    text_fill = (251, 246, 234) if lum < 150 else (29, 36, 48)
    canvas = Image.new("RGBA", (w, h))
    canvas.paste(bg, (0, 0))
    art = Image.open(logo).convert("RGB").resize((420, 420), Image.LANCZOS)
    mask = Image.new("L", (420, 420), 0)
    ImageDraw.Draw(mask).rounded_rectangle((0, 0, 419, 419), 92, fill=255)
    canvas.paste(art, (40, 40), mask)
    d = ImageDraw.Draw(canvas)
    words = name.split(" ")
    lines = [" ".join(words[: (len(words) + 1) // 2]), " ".join(words[(len(words) + 1) // 2:])] if len(words) > 1 else [name]
    size = 72
    while size > 30 and max(d.textlength(l, font=_font(size)) for l in lines) > 480:
        size -= 2
    y = h / 2 - (len(lines) * size * 1.15) / 2
    for line in lines:
        d.text((500, y), line, font=_font(size), fill=text_fill)
        y += size * 1.15
    return canvas.convert("RGB")


def main(argv: list[str]) -> int:
    out = Path(os.environ["STORE_OUT"])
    for g in select(argv):
        d = out / g.slug / "play"
        d.mkdir(parents=True, exist_ok=True)
        Image.open(g.path / "icon.png").resize((512, 512), Image.LANCZOS).save(d / "icon512.png")
        feature(g.slug, g.name, g.path / "icon.png").save(d / "feature.png")
    print("ok")
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
