"""Write iOS (App Store) and Android (AAB) export presets for each game.

Keeps the existing Web Playtest preset as preset.0 and replaces any native
presets. Signing secrets are not written here: the Android keystore comes
from GODOT_ANDROID_KEYSTORE_RELEASE_* environment variables at export time.
Usage: python3 tools/store_catalog/presets.py <profiles.tsv> [slug ...]
"""
from __future__ import annotations

import re
import sys
from pathlib import Path

from catalog import TEAM_ID, VERSION_NAME, Game, select

BUILD_NUMBER = 2

COMMON = '''runnable=false
advanced_options=false
dedicated_server=false
custom_features=""
export_filter="all_resources"
include_filter=""
exclude_filter="tests/*, icon_fg.png, icon_bg.png"
export_path=""
encrypt_pck=false
encrypt_directory=false
script_export_mode=2'''


def ios_preset(game: Game, profile_uuid: str) -> str:
    return f'''[preset.1]

name="iOS"
platform="iOS"
{COMMON}

[preset.1.options]

custom_template/debug=""
custom_template/release=""
architectures/arm64=true
application/app_store_team_id="{TEAM_ID}"
application/export_method_debug=1
application/export_method_release=0
application/bundle_identifier="{game.app_id}"
application/signature=""
application/short_version="{VERSION_NAME}"
application/version="{BUILD_NUMBER}"
application/targeted_device_family=0
application/export_project_only=false
application/code_sign_identity_release="iPhone Distribution"
application/provisioning_profile_uuid_release="{profile_uuid}"
application/additional_plist_content="<key>ITSAppUsesNonExemptEncryption</key><false/>"
capabilities/access_wifi=false
icons/icon_1024x1024="res://icon.png"
'''


def android_preset(game: Game) -> str:
    return f'''[preset.2]

name="Android"
platform="Android"
{COMMON}

[preset.2.options]

custom_template/debug=""
custom_template/release=""
gradle_build/use_gradle_build=true
gradle_build/export_format=1
gradle_build/min_sdk=""
gradle_build/target_sdk="36"
architectures/armeabi-v7a=true
architectures/arm64-v8a=true
architectures/x86=false
architectures/x86_64=false
version/code={BUILD_NUMBER}
version/name="{VERSION_NAME}"
package/unique_name="{game.app_id}"
package/name="{game.name}"
package/signed=true
package/app_category=2
package/retain_data_on_uninstall=false
launcher_icons/main_192x192="res://icon.png"
launcher_icons/adaptive_foreground_432x432="res://icon_fg.png"
launcher_icons/adaptive_background_432x432="res://icon_bg.png"
permissions/internet=false
'''


def write(game: Game, profile_uuid: str) -> None:
    cfg = game.path / "export_presets.cfg"
    text = cfg.read_text()
    web = re.split(r"\n(?=\[preset\.1\])", text)[0].rstrip() + "\n"
    cfg.write_text(web + "\n" + ios_preset(game, profile_uuid) + "\n" + android_preset(game))
    proj = game.path / "project.godot"
    ptext = proj.read_text()
    if "config/icon=" not in ptext:
        ptext = ptext.replace("[application]\n", '[application]\n\nconfig/icon="res://icon.png"\n', 1)
        ptext = re.sub(r"\[application\]\n\n", "[application]\n", ptext, count=1)
    if "config/version=" not in ptext:
        ptext = ptext.replace("[application]\n", f'[application]\nconfig/version="{VERSION_NAME}"\n', 1)
    if "import_etc2_astc=true" not in ptext:
        if "[rendering]\n" in ptext:
            ptext = ptext.replace("[rendering]\n", "[rendering]\ntextures/vram_compression/import_etc2_astc=true\n", 1)
        else:
            ptext = ptext.rstrip() + "\n\n[rendering]\ntextures/vram_compression/import_etc2_astc=true\n"
    proj.write_text(ptext)


def main(argv: list[str]) -> int:
    uuids = {}
    for line in Path(argv[0]).read_text().splitlines():
        slug, _app_id, _pk, uuid = line.split("\t")
        uuids[slug] = uuid
    for game in select(argv[1:]):
        write(game, uuids[game.slug])
        print("preset", game.slug)
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
