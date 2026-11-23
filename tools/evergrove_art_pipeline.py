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

from PIL import Image, ImageFilter

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


def save(image: Image.Image, name: str) -> None:
    OUT.mkdir(parents=True, exist_ok=True)
    target = OUT / (name + ".png")
    image.save(target, optimize=True)
    meta = OUT / (name + ".png.meta")
    meta.write_text(META.format(guid=guid_for(name)))
    print(f"{target.relative_to(ROOT)}  {image.width}x{image.height}  {image.mode}")


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

    for name in ("banner-stream-flowing", "banner-stream-dry"):
        image = master(name)
        if image:
            save(fit(image.convert("RGBA"), 1536, 1024), name)

    hero = master("hero-village")
    if hero:
        save(fit(hero.convert("RGBA"), 900, 1350), "hero-village")

    vine = master("vine-corner")
    if vine:
        # Keep the corner anchored: crop only right/bottom emptiness.
        rgba = knock_out_paper(vine)
        box = rgba.getchannel("A").point(lambda v: 255 if v > 12 else 0).getbbox()
        if box:
            rgba = rgba.crop((0, 0, min(rgba.width, box[2] + 8), min(rgba.height, box[3] + 8)))
        save(fit(rgba, 512, 512), "vine-corner")

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


if __name__ == "__main__":
    main()
