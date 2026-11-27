#!/usr/bin/env python3
"""Cheap repo-contract check; NOT a substitute for a Unity Editor compile."""
import json
from pathlib import Path

root = Path(__file__).resolve().parents[1]
game = root / "games" / "evergrove"
manifest = json.loads((game / "Packages" / "manifest.json").read_text())
deps = manifest["dependencies"]
# uGUI is a CORE package: Unity chooses its version. Unity 6.3 pins
# com.unity.ugui 2.0.0; 2.6.0 belongs to Unity 6.6, not this editor.
# A real Unity import appends m_EditorVersionWithRevision; only the pin matters.
editor_version = (game / "ProjectSettings" / "ProjectVersion.txt").read_text().strip().splitlines()[0]
assert editor_version == "m_EditorVersion: 6000.3.0f1", (
    "Review Unity core-package versions when changing editor", editor_version
)
assert deps.get("com.unity.ugui") == "2.0.0", (
    "Unity 6.3 ships core uGUI 2.0.0; do not pin uGUI 2.6.0 from Unity 6.6",
    deps.get("com.unity.ugui")
)
assert deps.get("com.unity.inputsystem") == "1.20.1", (
    "Input System package must match the verified Unity 6.3-compatible release"
)
ui = (game / "Assets" / "Scripts" / "Unity" / "EvergroveApp.cs").read_text()
assert "ENABLE_INPUT_SYSTEM" in ui and "InputSystemUIInputModule" in ui
assert "ENABLE_LEGACY_INPUT_MANAGER" in ui and "StandaloneInputModule" in ui
scene_plan = (game / "Assets" / "Scripts" / "Core" / "VillageScenePlan.cs").read_text()
pipe_graphic = (game / "Assets" / "Scripts" / "Unity" / "PipeGraphic.cs").read_text()
illustration = (game / "Assets" / "Scripts" / "Unity" / "VillageIllustration.cs").read_text()
assert "WaterRestored" in scene_plan and "WaterReachPercent" in scene_plan
assert "insight.Flowing[index], world.Solved" in ui
assert "insight.Flowing[i], world.Solved" in ui
assert "activeIllustration.Show" in ui
# Painted skin keeps the same causal inputs: water reach drives the flowing
# creek's alpha, and saved choice counts place painted landmarks.
painted = (game / "Assets" / "Scripts" / "Unity" / "PaintedVillage.cs").read_text()
assert "activePainting.Show" in ui
for marker in ("Mathf.Clamp01(waterReachPercent / 100f)", "restored ? 1f", "TreeSlots, shade", "BridgeSlots, crossings", "FlowerSlots, meadows"):
    assert marker in painted, "painted village lost causal state: " + marker
# Mosselora is player-facing only. Preserve all legacy save keys and package IDs.
theme = (game / "Assets/Scripts/Unity/MosseloraTheme.cs").read_text()
for token in ("Paper", "Ink", "SecondaryInk", "WaterInk", "LeafAction",
              "DryTile", "WetTile", "AnchoredTile", "TileFill"):
    assert token in theme, "Missing shared Mosselora token: " + token
assert ui.count("MosseloraTheme.TileFill") == 2, (
    "Initial puzzle and in-place refresh must use identical visual state rules"
)
assert "choiceIndex == 0" not in ui, (
    "No visually privileged tree choice: the first option biases uncoached tests"
)
assert 'insight.Leaking[index] ? "○"' in ui
assert 'insight.Leaking[i] ? "○"' in ui
assert "AddDisc(vh, stroke * 0.60f, outer)" in pipe_graphic
# Source layout guard: first-tap onboarding precedes the world art, while
# the completed state lands on a real choice anchor rather than page bottom.
first_board = "if (puzzleScreen)\n                BuildPuzzle();"
assert first_board in ui, "Puzzle board lost its board-first position"
assert ui.index(first_board) < ui.index("VillageScenePlan scenePlan = VillageScenePlan.From(world);"), (
    "World art must not push first interactive tile below the title/controls"
)
for marker in ("private RectTransform decisionHeading;",
               "Label(titleRow.transform, \"Choose title\"",
               "decisionHeading = chooseTitle.rectTransform;",
               "content.InverseTransformPoint(worldTop).y",
               "scroll.StopMovement()"):
    assert marker in ui, "Missing anchored post-solve navigation: " + marker
assert "if (scroll != null) scroll.verticalNormalizedPosition = 0f;" not in ui, (
    "Never jump blindly to the page footer after completing a stream"
)
for marker in ("hasWater = sourceConnected || solved;", "Color32 inner = hasWater ?"):
    assert marker in pipe_graphic, "dry/connected water style is missing: " + marker
for marker in ("waterReachPercent", "else if (waterReachPercent > 0)", "Quad(vh, P(0.44f, 0.42f)"):
    assert marker in illustration, "partial first-world water geometry is missing: " + marker
assert "Water(plan.WaterRestored)" in (game / "Assets" / "Scripts" / "Unity" / "VillageDiorama.cs").read_text()
assert "world.Round > 0 || world.Solved" in scene_plan
localized_story = (game / "Assets/Scripts/Core/LocalizedVillage.cs").read_text()
assert 'if (world.Choices.Length == 0 && world.Solved)' in localized_story
assert 'return T("solved");' in localized_story, (
    "Solved first creek must never be described as still waiting for water"
)
assert "GUIUtility.systemCopyBuffer" in ui, "playtest export must be explicit"
study = (game / "Assets" / "Scripts" / "Core" / "LocalPlaytest.cs").read_text().lower()
for forbidden in ("httprequest", "deviceuniqueidentifier", "unityanalytics", "webrequest"):
    assert forbidden not in study, forbidden
diary = (game / "Assets" / "Scripts" / "Core" / "ReturnDiary.cs").read_text().lower()
for forbidden in ("httprequest", "webrequest", "deviceuniqueidentifier", "unityanalytics",
                  "datetime.now", "datetime.utcnow", "datetimeoffset", "guid.", "systeminfo",
                  "unityengine", "playerprefs", "system.net"):
    assert forbidden not in diary, "ReturnDiary must stay pure and offline: " + forbidden
assert "#if UNITY_EDITOR || DEVELOPMENT_BUILD || EVERGROVE_RESEARCH_TOOLS" in (game / "Assets" / "Scripts" / "Core" / "ReturnDiary.cs").read_text()
editor = (game / "Assets" / "Editor" / "EvergroveBuild.cs").read_text()
for required in ("public static void Build()", "BuildPipeline.BuildPlayer", "BuildTarget.Android", "BuildTarget.WebGL", "BuildTarget.iOS", "EvergroveSceneSetup.ValidateImportedProject()"):
    assert required in editor, required
editor_smoke = (game / "Assets/Editor/EvergroveSceneSetup.cs").read_text()
for required in (
    "Application.unityVersion", "JsonUtility.ToJson", "JsonUtility.FromJson",
    "Resources.Load<TextAsset>", "GameLocalization.Register",
    "EVERGROVE_UNITY_IMPORT_CHECK_OK", "ValidateImportedProject()",
):
    assert required in editor_smoke, "Unity import smoke lost a validation: " + required
workflow = (root / ".github" / "workflows" / "evergrove-unity-playtest.yml").read_text()
for required in ("workflow_dispatch:", "game-ci/unity-builder@v4", "buildMethod: Evergrove.UnityClient.Editor.EvergroveBuild.Build", "secrets.UNITY_LICENSE", "secrets.UNITY_SERIAL"):
    assert required in workflow, required
assert "EVERGROVE_OUTPUT_ROOT" in editor, (
    "Local M1 builds must isolate output from old binaries without altering GameCI"
)
local_preflight = (root / "tools" / "evergrove_unity_local_preflight.sh").read_text()
for marker in ('mktemp -d "$EVIDENCE_ROOT/run-XXXXXX"',
               'export EVERGROVE_OUTPUT_ROOT="$EVIDENCE/output"',
               'EVERGROVE_BUILD_SUCCEEDED $OUTPUT'):
    assert marker in local_preflight, "Local M1 stale-build guard missing: " + marker
assert "buildsPath: build" in workflow
assert "tools/evergrove_build_provenance.py" in workflow, (
    "Successful licensed Unity build must upload hashed export provenance"
)
assert "sha256sum" in workflow, "Upload workflow must hash the generated evidence manifest"
print("Evergrove manifest, rendering, consent and manual Unity build contracts: OK (static only)")

# A separate `dotnet run` test exercises the C# translation registry at
# runtime. This static check catches missing or malformed Unity TextAssets.
import re
localization_source = (game / "Assets" / "Scripts" / "Core" / "GameLocalization.cs").read_text()
ids_block = re.search(r"private static readonly string\[\] Ids = \{([\s\S]*?)\};", localization_source)
assert ids_block, "canonical translation IDs not found"
keys = re.findall(r'"([^"]+)"', ids_block.group(1))
assert len(keys) == 127 and len(keys) == len(set(keys)), "canonical locale key set drifted"
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
    assert by_key["title"] == "MOSSELORA", ("Inconsistent player-facing wordmark", code)
    assert set(by_key) == set(keys), ("Translation coverage gap", code)
    for key, value in by_key.items():
        assert isinstance(value, str) and value.strip(), ("Empty translation", code, key)
    assert all(f"{{{i}}}" in by_key["journal_entry"] for i in range(3)), code
    assert all(f"{{{i}}}" in by_key["progress"] for i in range(3)), code
    assert all(f"{{{i}}}" in by_key["stream"] for i in range(2)), code
locale_runtime = (game / "Assets/Scripts/Unity/LocaleRuntime.cs").read_text()
shipping_policy = (game / "Assets/Scripts/Core/LocaleShippingPolicy.cs").read_text()
for required in ("LocaleShippingPolicy.Resolve", "LocaleShippingPolicy.Allows",
                 "LocaleShippingPolicy.Selectable", "#if UNITY_EDITOR || DEVELOPMENT_BUILD"):
    assert required in locale_runtime, "Missing consumer locale-channel isolation: " + required
for required in ('new GameLanguage("en", "English")', "researchBuild ? GameLocalization.Available",
                 "GameLocalization.Fallback"):
    assert required in shipping_policy, "Consumer locale policy must require review to expand: " + required
assert "foreach (GameLanguage choice in LocaleRuntime.SelectableLanguages)" in ui
# One settings row now hosts the language control on every screen.
assert ui.count("LocaleRuntime.SelectableLanguages.Count > 1") == 1
assert "LocaleRuntime.Initialize();" in ui and "BuildLanguagePicker()" in ui
assert "LocalizedVillage." in ui and "GameLocalization.Text(" in ui
# Garden file: clipboard export/import wired to the settings UI with a second confirmation.
assert "GardenTransfer.Export(world" in ui and "GardenTransfer.TryImport(" in ui
assert "GUIUtility.systemCopyBuffer" in ui and "Confirm garden restore" in ui
assert "Cancel garden restore" in ui and "SaveWorld(); // same persistence path" in ui
print(f"Evergrove language assets: English source + {len(supported)} complete offline catalogs, {len(keys)} keys each")

# BLD-007 release-candidate source guard: every research/dev-only identifier in
# Assets/ must sit inside an approved research guard (its #else branch does not count).
# Approved guards are the only ones that compile out of a release-candidate build
# (no DEVELOPMENT_BUILD, Debug.isDebugBuild false).
APPROVED_GUARDS = {
    "#if UNITY_EDITOR || DEVELOPMENT_BUILD",
    "#if UNITY_EDITOR || DEVELOPMENT_BUILD || EVERGROVE_RESEARCH_TOOLS",
}
DEV_ONLY_TOKENS = [
    r"\bDevQaTour\b", r"\bDevScreenshot\b", r"\bTextSpike\b", r"\bLocalPlaytest\b",
    r"\bReturnDiary\b", r"\bReturnStatus\b",
    r"\bFacilitatorHold\b", r"\bparticipantView\b",
    r"\bStartParticipantView\b", r"\bEndParticipantView\b",
    r"\bVillageDiorama\b", r"\bQaReplaceWorld\b", r"\bQaWorld\b", r"\bLogForQa\b",
    r"mosseloraQaTour", r"mosseloraScreenshot", r"mosseloraTextSpike", r"mosseloraTextScale",
    r"matched control", r"Matched control", r"Playtest consent", r"Diorama visual comparison",
    r"Copy playtest", r"Living mode", r"Puzzle control", r"Miniature diorama",
    r"MOSSELORA_QA_TOUR_DONE", r"MOSSELORA_SR_NODES", r"MOSSELORA_TEXT_SCALE_OVERRIDE",
]
DEV_ONLY_PATTERN = re.compile("|".join(DEV_ONLY_TOKENS))


def dev_only_violations(source):
    """Return (line_number, text) for dev-only tokens outside an approved guard."""
    stack = []  # each frame: [approved, in_else]
    found = []
    for number, line in enumerate(source.splitlines(), start=1):
        stripped = line.strip()
        if stripped.startswith("#if"):
            stack.append([stripped in APPROVED_GUARDS, False])
            continue
        if stripped.startswith("#elif") or stripped.startswith("#else"):
            if stack:
                stack[-1][1] = True
            continue
        if stripped.startswith("#endif"):
            if stack:
                stack.pop()
            continue
        if DEV_ONLY_PATTERN.search(line) and not any(a and not e for a, e in stack):
            found.append((number, stripped))
    return found


# Self-test so a broken scanner cannot silently pass the real tree.
_sample = "#if UNITY_EDITOR || DEVELOPMENT_BUILD\nclass A { DevQaTour x; }\n#else\nDevQaTour y;\n#endif\nDevScreenshot z;\n"
assert [n for n, _ in dev_only_violations(_sample)] == [4, 6], "dev-only guard scanner self-test failed"

assets_root = game / "Assets"
leaks = []
for path in sorted(assets_root.rglob("*.cs")):
    if "/Library/" in str(path):
        continue
    for number, text in dev_only_violations(path.read_text(encoding="utf-8")):
        leaks.append(f"{path.relative_to(game)}:{number}: {text}")
assert not leaks, "Dev-only identifiers outside an approved research guard:\n" + "\n".join(leaks)
print("Evergrove release-candidate dev-control source guard: OK (no unguarded dev-only identifiers)")
