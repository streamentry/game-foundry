"""Install the generated logo (Codex image generation) as each game's app icon.

Source: $STORE_OUT/logos/<slug>.png (1024x1024, opaque).
Writes games/<slug>/icon.png (1024, RGB), icon_bg.png (432 adaptive
background = full logo; content is centered inside the launcher safe zone)
and icon_fg.png (432 transparent foreground).
Usage: python3 tools/store_catalog/apply_logos.py [slug ...]
"""
from __future__ import annotations

import os
import sys
from pathlib import Path

from PIL import Image

from catalog import select


def main(argv: list[str]) -> int:
    logos = Path(os.environ["STORE_OUT"]) / "logos"
    for g in select(argv):
        src = logos / f"{g.slug}.png"
        if not src.exists():
            print(g.slug, "missing")
            continue
        img = Image.open(src).convert("RGB").resize((1024, 1024), Image.LANCZOS)
        img.save(g.path / "icon.png", optimize=True)
        img.resize((432, 432), Image.LANCZOS).save(g.path / "icon_bg.png", optimize=True)
        Image.new("RGBA", (432, 432), (0, 0, 0, 0)).save(g.path / "icon_fg.png")
        print(g.slug, "ok")
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
