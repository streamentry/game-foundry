#!/usr/bin/env python3
"""Cheap repo-contract check; NOT a substitute for a Unity Editor compile."""
import json
from pathlib import Path

root = Path(__file__).resolve().parents[1]
game = root / "games" / "evergrove"
manifest = json.loads((game / "Packages" / "manifest.json").read_text())
deps = manifest["dependencies"]
assert deps.get("com.unity.ugui"), "Unity uGUI dependency is required"
assert deps.get("com.unity.inputsystem"), "modern Unity touch input package required"
ui = (game / "Assets" / "Scripts" / "Unity" / "EvergroveApp.cs").read_text()
assert "ENABLE_INPUT_SYSTEM" in ui and "InputSystemUIInputModule" in ui
assert "ENABLE_LEGACY_INPUT_MANAGER" in ui and "StandaloneInputModule" in ui
assert "WaterRestored" in (game / "Assets" / "Scripts" / "Core" / "VillageScenePlan.cs").read_text()
assert "Water(plan.WaterRestored)" in (game / "Assets" / "Scripts" / "Unity" / "VillageDiorama.cs").read_text()
assert "world.Round > 0 || world.Solved" in ui
assert "GUIUtility.systemCopyBuffer" in ui, "playtest export must be explicit"
study = (game / "Assets" / "Scripts" / "Core" / "LocalPlaytest.cs").read_text().lower()
for forbidden in ("httprequest", "deviceuniqueidentifier", "unityanalytics", "webrequest"):
    assert forbidden not in study, forbidden
editor = (game / "Assets" / "Editor" / "EvergroveBuild.cs").read_text()
for required in ("public static void Build()", "BuildPipeline.BuildPlayer", "BuildTarget.Android", "BuildTarget.WebGL", "BuildTarget.iOS"):
    assert required in editor, required
workflow = (root / ".github" / "workflows" / "evergrove-unity-playtest.yml").read_text()
for required in ("workflow_dispatch:", "game-ci/unity-builder@v4", "buildMethod: Evergrove.UnityClient.Editor.EvergroveBuild.Build", "secrets.UNITY_LICENSE", "secrets.UNITY_SERIAL"):
    assert required in workflow, required
assert "buildsPath: build" in workflow
print("Evergrove manifest, rendering, consent and manual Unity build contracts: OK (static only)")

# A separate `dotnet run` test exercises the C# translation registry at
# runtime. This static check catches missing or malformed Unity TextAssets.
import re
localization_source = (game / "Assets" / "Scripts" / "Core" / "GameLocalization.cs").read_text()
ids_block = re.search(r"private static readonly string\[\] Ids = \{([\s\S]*?)\};", localization_source)
assert ids_block, "canonical translation IDs not found"
keys = re.findall(r'"([^"]+)"', ids_block.group(1))
assert len(keys) == 73 and len(keys) == len(set(keys)), "canonical locale key set drifted"
bundle_dir = game / "Assets" / "Resources" / "Evergrove" / "Locales"
supported = {
    "zh-Hans", "zh-Hant", "es", "pt-BR", "fr", "de", "ja", "ko", "ar",
    "hi", "ru", "id", "vi", "tr", "th", "it", "nl", "pl", "uk", "fa",
    "he", "ms", "fil", "bn", "ur", "ta", "te", "sw", "sv"
}
locale_files = {file.stem for file in bundle_dir.glob("*.json")}
assert locale_files == supported, ("Missing/unexpected locale files", sorted(supported ^ locale_files))
for code in sorted(supported):
    contents = json.loads((bundle_dir / f"{code}.json").read_text(encoding="utf-8"))
    assert contents["code"] == code, ("Wrong language tag", code)
    translations = contents["entries"]
    by_key = {item["key"]: item["value"] for item in translations}
    assert len(translations) == len(keys) == len(by_key), ("Duplicate/missing keys", code)
    assert set(by_key) == set(keys), ("Translation coverage gap", code)
    for key, value in by_key.items():
        assert isinstance(value, str) and value.strip(), ("Empty translation", code, key)
    assert all(f"{{{i}}}" in by_key["journal_entry"] for i in range(3)), code
    assert all(f"{{{i}}}" in by_key["progress"] for i in range(3)), code
    assert all(f"{{{i}}}" in by_key["stream"] for i in range(2)), code
assert "LocaleRuntime.Initialize();" in ui and "BuildLanguagePicker()" in ui
assert "LocalizedVillage." in ui and "GameLocalization.Text(" in ui
print(f"Evergrove language assets: English source + {len(supported)} complete offline catalogs, {len(keys)} keys each")
