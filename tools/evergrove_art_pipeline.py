#!/usr/bin/env python3
"""Turn Mosselora's generated art masters into runtime sprites.

Masters (Codex built-in image_gen output) live in games/evergrove/art/generated.
This script slices sprite sheets, trims transparent margins, removes a flat
paper background when a master came back opaque, downsizes to phone-appropriate
resolutions and writes Unity sprite import settings, so the Unity project
imports them as uncropped, non-mipmapped UI sprites.

Usage: python3 tools/evergrove_art_pipeline.py
"""
from __future__ import annotations

import hashlib
from pathlib import Path

from PIL import Image, ImageChops, ImageFilter

ROOT = Path(__file__).resolve().parents[1]
GAME = ROOT / "games" / "evergrove"
MASTERS = GAME / "art" / "generated"
OUT = GAME / "Assets" / "Resources" / "Evergrove" / "Art"

META = """fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 0
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: 100
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 0
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  swizzle: 50462976
  cookieLightType: 0
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 1
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    customData:
    physicsShape: []
    bones: []
    spriteID: 5e97eb03825dee720800000000000000
    internalID: 0
    vertices: []
    indices:
    edges: []
    weights: []
    secondaryTextures: []
    spriteCustomDataEntries: []
    nameFileIdTable: {{}}
  mipmapLimitGroupName:
  pSDRemoveMatte: 0
  userData:
  assetBundleName:
  assetBundleVariant:
"""


def guid_for(name: str) -> str:
    return hashlib.md5(("mosselora-art/" + name).encode()).hexdigest()


def knock_out_paper(image: Image.Image, tolerance: int = 26) -> Image.Image:
    """Opaque master: make the flat corner-sampled background transparent, softly."""
    rgba = image.convert("RGBA")
    if rgba.getextrema()[3][0] < 250:
        return rgba  # already has real transparency
    w, h = rgba.size
    corners = [rgba.getpixel(p)[:3] for p in ((2, 2), (w - 3, 2), (2, h - 3), (w - 3, h - 3))]
    bg = tuple(sum(c[i] for c in corners) // 4 for i in range(3))
    pixels = rgba.load()
    for y in range(h):
        for x in range(w):
            r, g, b, a = pixels[x, y]
            d = max(abs(r - bg[0]), abs(g - bg[1]), abs(b - bg[2]))
            if d <= tolerance:
                pixels[x, y] = (r, g, b, 0)
            elif d <= tolerance * 2:
                pixels[x, y] = (r, g, b, int(255 * (d - tolerance) / tolerance))
    alpha = rgba.getchannel("A").filter(ImageFilter.GaussianBlur(0.6))
    rgba.putalpha(alpha)
    return rgba


def knock_out_border(image: Image.Image, tolerance: int = 22) -> Image.Image:
    """Remove only the background connected to the image border.

    For the brand logo: its cream letters and white flowers are close to the
    white backdrop in colour, so a global knockout would punch holes in them.
    """
    from PIL import ImageDraw
    rgba = image.convert("RGBA")
    w, h = rgba.size
    corners = [rgba.getpixel(p)[:3] for p in ((2, 2), (w - 3, 2), (2, h - 3), (w - 3, h - 3))]
    bg = tuple(sum(c[i] for c in corners) // 4 for i in range(3))
    rgb = rgba.convert("RGB")
    dist = Image.new("L", (w, h))
    src, out = rgb.load(), dist.load()
    for y in range(h):
        for x in range(w):
            r, g, b = src[x, y]
            out[x, y] = min(255, max(abs(r - bg[0]), abs(g - bg[1]), abs(b - bg[2])))
    near = dist.point(lambda d: 255 if d <= tolerance * 2 else 0)
    for x in range(0, w, 4):
        for y in (0, h - 1):
            if near.getpixel((x, y)) == 255:
                ImageDraw.floodfill(near, (x, y), 128)
    for y in range(0, h, 4):
        for x in (0, w - 1):
            if near.getpixel((x, y)) == 255:
                ImageDraw.floodfill(near, (x, y), 128)
    alpha = Image.new("L", (w, h), 255)
    a, n, dd = alpha.load(), near.load(), dist.load()
    for y in range(h):
        for x in range(w):
            if n[x, y] == 128:
                d = dd[x, y]
                a[x, y] = 0 if d <= tolerance else int(255 * (d - tolerance) / tolerance)
    rgba.putalpha(alpha.filter(ImageFilter.GaussianBlur(0.7)))
    return rgba


def trim(image: Image.Image, pad: int = 8) -> Image.Image:
    alpha = image.getchannel("A").point(lambda v: 255 if v > 12 else 0)
    box = alpha.getbbox()
    if not box:
        return image
    l, t, r, b = box
    return image.crop((max(0, l - pad), max(0, t - pad),
                       min(image.width, r + pad), min(image.height, b + pad)))


def fit(image: Image.Image, max_w: int, max_h: int) -> Image.Image:
    scale = min(1.0, max_w / image.width, max_h / image.height)
    if scale >= 1.0:
        return image
    return image.resize((round(image.width * scale), round(image.height * scale)), Image.LANCZOS)


def fade_top(image: Image.Image, share: float = 0.42) -> Image.Image:
    """Painted sky melts into the page: alpha ramps from 0 at the top edge."""
    rgba = image.convert("RGBA")
    ramp = max(1, int(rgba.height * share))
    alpha = rgba.getchannel("A")
    mask = Image.new("L", rgba.size, 255)
    for y in range(ramp):
        t = y / ramp
        mask.paste(int(255 * t * t * (3 - 2 * t)), (0, y, rgba.width, y + 1))
    rgba.putalpha(ImageChops.multiply(alpha, mask))
    return rgba


def save(image: Image.Image, name: str, border: int = 0) -> None:
    OUT.mkdir(parents=True, exist_ok=True)
    target = OUT / (name + ".png")
    image.save(target, optimize=True)
    meta = OUT / (name + ".png.meta")
    text = META.format(guid=guid_for(name))
    if border:
        text = text.replace("spriteBorder: {x: 0, y: 0, z: 0, w: 0}",
                            f"spriteBorder: {{x: {border}, y: {border}, z: {border}, w: {border}}}")
    meta.write_text(text)
    print(f"{target.relative_to(ROOT)}  {image.width}x{image.height}  {image.mode}")


UI_ICONS = ("ui-undo", "ui-hint", "ui-restart", "ui-language",
            "ui-text-size", "ui-board-size", "ui-chime-on", "ui-chime-off",
            "ui-journal", "ui-continue", "ui-change", "ui-copy-file",
            "ui-restore-file", "ui-new-garden", "ui-confirm", "ui-cancel")


SUPERSEDED = {"ui-board-size", "ui-chime-on", "ui-chime-off", "ui-change"}


def grid(sheet: Image.Image, columns: int, rows: int) -> list[Image.Image]:
    w, h = sheet.size
    cw, ch = w // columns, h // rows
    return [sheet.crop((c * cw, r * ch, (c + 1) * cw, (r + 1) * ch))
            for r in range(rows) for c in range(columns)]


def white_glyph(cell: Image.Image, size: int = 128) -> Image.Image:
    """Dark glyph -> tintable white glyph on a square transparent canvas."""
    rgba = knock_out_paper(cell)
    alpha = rgba.getchannel("A")
    # Ink darkness, not paper, carries the shape when the master is opaque.
    luminance = rgba.convert("L").point(lambda v: 255 - v)
    alpha = ImageChops.multiply(alpha, luminance.point(lambda v: min(255, v * 2)))
    glyph = Image.new("RGBA", rgba.size, (255, 255, 255, 0))
    glyph.putalpha(alpha)
    glyph = trim(glyph, 2)
    side = max(glyph.width, glyph.height)
    square = Image.new("RGBA", (side, side), (255, 255, 255, 0))
    square.alpha_composite(glyph, ((side - glyph.width) // 2, (side - glyph.height) // 2))
    return square.resize((size, size), Image.LANCZOS)


def paper_tile(paper: Image.Image, size: int = 192, radius: int = 24,
               gain: float = 3.0) -> Image.Image:
    """Rounded, near-white, tintable tile carrying the watercolor paper grain."""
    from PIL import ImageDraw, ImageStat
    gray = paper.convert("L").resize((size, size), Image.LANCZOS)
    mean = ImageStat.Stat(gray).mean[0]
    grain = gray.point(lambda v: max(0, min(255, int(250 - (mean - v) * gain))))
    mask = Image.new("L", (size * 4, size * 4), 0)
    ImageDraw.Draw(mask).rounded_rectangle((0, 0, size * 4 - 1, size * 4 - 1), radius * 4, fill=255)
    mask = mask.resize((size, size), Image.LANCZOS)
    tile = Image.merge("RGBA", (grain, grain, grain, mask))
    return tile


def page_grain(paper: Image.Image, size: int = 512, gain: float = 2.6) -> Image.Image:
    """Seamless near-white watercolor grain for the whole page (I94).

    The Codex paper master is mirrored 2x2 so the tile has no seams, then
    flattened to near-white so the page's own Paper tint shows through.
    """
    from PIL import ImageOps, ImageStat
    side = min(paper.size)
    square = paper.convert("L").crop((0, 0, side, side)).resize((size, size), Image.LANCZOS)
    mirrored = Image.new("L", (size * 2, size * 2))
    mirrored.paste(square, (0, 0))
    mirrored.paste(ImageOps.mirror(square), (size, 0))
    mirrored.paste(ImageOps.flip(square), (0, size))
    mirrored.paste(ImageOps.flip(ImageOps.mirror(square)), (size, size))
    tile = mirrored.resize((size, size), Image.LANCZOS)
    mean = ImageStat.Stat(tile).mean[0]
    grain = tile.point(lambda v: max(0, min(255, int(252 - (mean - v) * gain))))
    return Image.merge("RGBA", (grain, grain, grain, Image.new("L", grain.size, 255)))


def quadrants(sheet: Image.Image) -> list[Image.Image]:
    w, h = sheet.size
    return [sheet.crop((x, y, x + w // 2, y + h // 2))
            for y in (0, h // 2) for x in (0, w // 2)]


def main() -> None:
    def master(name: str) -> Image.Image | None:
        path = MASTERS / (name + ".png")
        if not path.exists():
            print("missing master:", path.relative_to(ROOT))
            return None
        return Image.open(path)

    for name in ("banner-stream-flowing", "banner-stream-dry", "banner-autumn-flowing",
                 "banner-autumn-dry", "banner-winter-flowing", "banner-winter-dry"):
        image = master(name)
        if image:
            save(fade_top(fit(image.convert("RGBA"), 1536, 1024)), name)

    hero = master("hero-village")
    if hero:
        save(fit(hero.convert("RGBA"), 900, 1350), "hero-village")

    # iOS launch image: parchment, vine corners, wordmark (copied into the
    # exported Xcode project by Assets/Editor/MosseloraLaunchScreen.cs).
    mark = master("wordmark")
    vine = master("vine-corner")
    brand_logo = GAME / "brand" / "mosselora-logo.png"
    if (mark or brand_logo.exists()) and vine:
        sheet = Image.new("RGBA", (1242, 2208), (249, 242, 227, 255))
        # The repo's brand logo, as in game and on the site (I102).
        word = (fit(trim(knock_out_border(Image.open(brand_logo)), 4), 980, 420)
                if brand_logo.exists() else fit(trim(knock_out_paper(mark), 4), 760, 300))
        sheet.alpha_composite(word, ((1242 - word.width) // 2, 1104 - word.height // 2))
        corner = fit(knock_out_paper(vine), 420, 420)
        sheet.alpha_composite(corner, (0, 0))
        sheet.alpha_composite(corner.transpose(Image.FLIP_LEFT_RIGHT), (1242 - corner.width, 0))
        out = GAME / "brand" / "launch-portrait.png"
        sheet.convert("RGB").save(out, optimize=True)
        print(out.relative_to(ROOT))

    for kind in ("tree", "bridge", "flowers"):
        vignette = master("vignette-" + kind)
        if vignette:
            save(fit(vignette.convert("RGBA"), 720, 480), "vignette-" + kind)

    bed = master("board-bed")
    if bed:
        save(fit(knock_out_paper(bed), 768, 768), "board-bed")

    festival = master("ending-festival")
    if festival:
        save(fit(festival.convert("RGBA"), 900, 1350), "ending-festival")

    vine = master("vine-corner")
    if vine:
        # Keep the corner anchored: crop only right/bottom emptiness.
        rgba = knock_out_paper(vine)
        box = rgba.getchannel("A").point(lambda v: 255 if v > 12 else 0).getbbox()
        if box:
            rgba = rgba.crop((0, 0, min(rgba.width, box[2] + 8), min(rgba.height, box[3] + 8)))
        save(fit(rgba, 512, 512), "vine-corner")

    # The in-game wordmark is the repo's brand logo (games/evergrove/brand/
    # mosselora-logo.png), the same art as the app icon and the web site.
    brand_logo = GAME / "brand" / "mosselora-logo.png"
    if brand_logo.exists():
        save(fit(trim(knock_out_border(Image.open(brand_logo)), 4), 1024, 400), "wordmark")
    else:
        mark = master("wordmark")
        if mark:
            save(fit(trim(knock_out_paper(mark), 4), 1024, 256), "wordmark")

    icons = master("icons-choices")
    if icons:
        names = ("icon-shade-tree", "icon-bridge", "icon-wildflowers", "icon-foundation")
        for name, part in zip(names, quadrants(knock_out_paper(icons))):
            save(fit(trim(part), 384, 384), name)

    small = master("icons-small")
    if small:
        names = ("glyph-sprout", "glyph-droplet", "glyph-sprig", "glyph-waves")
        for name, part in zip(names, quadrants(knock_out_paper(small))):
            limit = (384, 128) if name == "glyph-sprig" else (160, 160)
            save(fit(trim(part, 4), *limit), name)

    ui = master("icons-ui")
    if ui:
        for name, cell in zip(UI_ICONS, grid(ui, 4, 4)):
            if name in SUPERSEDED:
                continue  # replaced by clearer glyphs from icons-ui-2.png
            save(white_glyph(cell), name)

    ui2 = master("icons-ui-2")
    if ui2:
        for name, cell in zip(("ui-exchange", "ui-sound-on", "ui-sound-off", "ui-zoom-board"),
                              grid(ui2, 2, 2)):
            save(white_glyph(cell), name)

    ui3 = master("icons-ui-3")
    if ui3:
        for name, cell in zip(("ui-menu", "ui-settings", "ui-neighbors", "ui-home"), grid(ui3, 2, 2)):
            save(white_glyph(cell), name)

    ui4 = master("icons-ui-4")
    if ui4:
        for name, cell in zip(("ui-motion-on", "ui-motion-off", "ui-path", "ui-chat"), grid(ui4, 2, 2)):
            save(white_glyph(cell), name)

    ui5 = master("icons-ui-5")
    if ui5:
        for name, cell in zip(("ui-tap", "ui-turn", "ui-rooted", "ui-sun"), grid(ui5, 2, 2)):
            save(white_glyph(cell), name)

    ui6 = master("icons-ui-6")
    if ui6:
        # Four candidates for "choose again"; a cold-read picks one for IconChange (I84).
        for name, cell in zip(("ui-choose-sprout", "ui-choose-pencil", "ui-choose-cards", "ui-choose-seed"),
                              grid(ui6, 2, 2)):
            save(white_glyph(cell), name)

    stones = master("stones")
    if stones:
        # Painted stepping stones for the stream path (I119): ahead in each season,
        # walked (wet), and "you are here" with a sprout. Not yet wired into the UI.
        for name, cell in zip(("stone-summer", "stone-autumn", "stone-winter", "stone-wet", "stone-here"),
                              grid(stones.convert("RGBA"), 5, 1)):
            save(fit(trim(cell, 4), 96, 96), name)

    plaques = master("button-plaques")
    if plaques:
        for name, cell in zip(("plaque-forest", "plaque-teal", "plaque-parchment"),
                              grid(plaques.convert("RGBA"), 3, 1)):
            part = fit(trim(cell, 2), 300, 150)
            save(part, name, border=int(part.height * 0.45))

    medallions = master("medallions")
    if medallions:
        # Round tokens for the board's Undo and Hint (I129).
        for name, cell in zip(("medallion-forest", "medallion-teal", "medallion-parchment"),
                              grid(medallions.convert("RGBA"), 3, 1)):
            save(fit(trim(cell, 2), 192, 192), name)

    butterfly = master("butterfly")
    if butterfly:
        for name, cell in zip(("butterfly-open", "butterfly-closed"),
                              grid(knock_out_paper(butterfly), 2, 1)):
            save(fit(trim(cell, 4), 160, 160), name)

    paper = master("paper-grain")
    if paper:
        save(paper_tile(paper), "tile-paper", border=26)
        save(page_grain(paper), "page-grain")
        grain_meta = OUT / "page-grain.png.meta"  # tiled Image needs a full-rect mesh
        grain_meta.write_text(grain_meta.read_text().replace("spriteMeshType: 0", "spriteMeshType: 1"))

    neighbors = master("neighbors")
    if neighbors:
        for name, cell in zip(("portrait-mira", "portrait-orin", "portrait-fen"),
                              grid(neighbors.convert("RGBA"), 3, 1)):
            save(fit(trim(knock_out_paper(cell), 2), 256, 256), name)


if __name__ == "__main__":
    main()
