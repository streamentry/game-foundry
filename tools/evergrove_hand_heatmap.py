#!/usr/bin/env python3
"""Draw a one-hand thumb-reach heatmap per hand from a Mosselora dev tour log (checklist 5).

Reads the MOSSELORA_HANDMAP lines written by the hand tour (games/evergrove DevQaTour.Hand.cs):
  MOSSELORA_HANDMAP <left|right> <name> <x> <y> <w> <h>
where x, y is the control's centre and w, h the screen size, all in canvas points (origin top-left).
For each hand it writes one PNG beside the log, named <log>-hand-<left|right>.png, showing the screen,
the two thumb-reach zones from the ThumbReach model (shaded), and each logged control as a dot:
green when reachable, red when out of reach.

The zone constants mirror games/evergrove/Assets/Scripts/Core/ThumbReach.cs; a test in
games/evergrove/Tests/HandednessTests.cs checks they still match.

Usage: python3 tools/evergrove_hand_heatmap.py <tour-stdout.txt>
Needs Pillow only.
"""
import pathlib
import re
import sys

from PIL import Image, ImageDraw

LINE = re.compile(r"MOSSELORA_HANDMAP (left|right) (\S+) (-?[\d.]+) (-?[\d.]+) ([\d.]+) ([\d.]+)")
PORTRAIT_TOP_SHARE = 1 / 3   # ThumbReach.PortraitTopShare
LANDSCAPE_TOP_SHARE = 1 / 4  # ThumbReach.LandscapeTopShare
LANDSCAPE_SIDE_SHARE = 0.45  # ThumbReach.LandscapeSideShare
CANVAS_WIDTH = 960
MARGIN = 40
REACH_FILL = (60, 150, 110, 70)
OUT_FILL = (0, 0, 0, 0)
GREEN = (34, 139, 84, 255)
RED = (190, 45, 40, 255)
INK = (40, 40, 40, 255)


def reach_rects(w, h):
    """The reachable zones as rectangles (x0, y0, x1, y1), mirroring ThumbReach.Reachable."""
    if h > w:
        return [(0, h * PORTRAIT_TOP_SHARE, w, h)]
    top = h * LANDSCAPE_TOP_SHARE
    return [(0, top, w * LANDSCAPE_SIDE_SHARE, h), (w * (1 - LANDSCAPE_SIDE_SHARE), top, w, h)]


def reachable(x, y, w, h):
    """True when the centre sits in a reach zone; mirrors ThumbReach.Reachable (right-hand flag does not change it)."""
    if not (0 <= x <= w and 0 <= y <= h):
        return False
    return any(x0 <= x <= x1 and y0 <= y <= y1 for x0, y0, x1, y1 in reach_rects(w, h))


def read_log(path):
    hands = {"left": [], "right": []}
    for line in path.read_text(errors="replace").splitlines():
        match = LINE.search(line)
        if match:
            side, name, x, y, w, h = match.groups()
            hands[side].append((name, float(x), float(y), float(w), float(h)))
    return hands


def draw_hand(side, entries, out_path):
    _, _, _, w, h = entries[0]
    scale = (CANVAS_WIDTH - 2 * MARGIN) / w
    screen_w = int(round(w * scale))
    screen_h = int(round(h * scale))
    height = screen_h + 2 * MARGIN + 60
    image = Image.new("RGBA", (CANVAS_WIDTH, height), (250, 246, 236, 255))
    overlay = Image.new("RGBA", image.size, OUT_FILL)
    draw_over = ImageDraw.Draw(overlay)
    origin = (MARGIN, MARGIN + 40)
    for x0, y0, x1, y1 in reach_rects(w, h):
        draw_over.rectangle(
            [origin[0] + x0 * scale, origin[1] + y0 * scale, origin[0] + x1 * scale, origin[1] + y1 * scale],
            fill=REACH_FILL,
        )
    image = Image.alpha_composite(image, overlay)
    draw = ImageDraw.Draw(image)
    draw.rectangle([origin[0], origin[1], origin[0] + screen_w, origin[1] + screen_h], outline=INK, width=2)
    reached = 0
    for name, x, y, _, _ in entries:
        ok = reachable(x, y, w, h)
        reached += ok
        cx, cy = origin[0] + x * scale, origin[1] + y * scale
        draw.ellipse([cx - 9, cy - 9, cx + 9, cy + 9], fill=GREEN if ok else RED, outline=INK)
    title = f"{side.capitalize()} hand: {reached} reachable, {len(entries) - reached} out, screen {int(w)}x{int(h)} pt"
    draw.text((MARGIN, 14), title, fill=INK)
    draw.text((MARGIN, origin[1] + screen_h + 12), "Shaded: one-thumb reach (ThumbReach model). Green in reach, red out.", fill=INK)
    image.convert("RGB").save(out_path)
    return reached, len(entries)


def main(argv):
    if len(argv) != 2:
        print("Usage: python3 tools/evergrove_hand_heatmap.py <tour-stdout.txt>", file=sys.stderr)
        return 2
    log = pathlib.Path(argv[1])
    hands = read_log(log)
    if not any(hands.values()):
        print(f"No MOSSELORA_HANDMAP lines in {log}", file=sys.stderr)
        return 1
    for side in ("left", "right"):
        if not hands[side]:
            print(f"No {side} hand lines in {log}; skipped", file=sys.stderr)
            continue
        out_path = log.with_name(f"{log.stem}-hand-{side}.png")
        reached, total = draw_hand(side, hands[side], out_path)
        print(f"{side}: {reached}/{total} controls reachable -> {out_path}")
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
