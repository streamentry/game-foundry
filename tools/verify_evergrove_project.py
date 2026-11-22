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
