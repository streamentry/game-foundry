"""The Godot puzzle catalog: slugs, display names and store identifiers."""
from __future__ import annotations

import json
import re
from dataclasses import dataclass
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
EXCLUDED = {"evergrove"}  # Mosselora ships through its own Unity pipeline.
TEAM_ID = "7MBXZKYSY4"
VERSION_NAME = "1.0.0"


@dataclass(frozen=True)
class Game:
    slug: str
    name: str

    @property
    def app_id(self) -> str:
        return "com.streamentry." + self.slug.replace("-", "")

    @property
    def path(self) -> Path:
        return ROOT / "games" / self.slug

    @property
    def profile_name(self) -> str:
        return f"{self.name} App Store"

    def metadata(self) -> dict:
        return json.loads((ROOT / "store" / "metadata" / self.slug / "en-US.json").read_text())


def games() -> list[Game]:
    out = []
    for project in sorted((ROOT / "games").glob("*/project.godot")):
        slug = project.parent.name
        if slug in EXCLUDED:
            continue
        name = re.search(r'^config/name="(.*)"', project.read_text(), re.M).group(1)
        out.append(Game(slug, name))
    return out


def select(slugs: list[str]) -> list[Game]:
    allg = games()
    return [g for g in allg if not slugs or g.slug in slugs]
