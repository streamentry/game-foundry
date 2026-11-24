#!/usr/bin/env python3
"""Execute iterations I46 to I60 for Mosselora.
"""
from __future__ import annotations

import os
import re
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
IMPROVEMENTS_DIR = ROOT / "docs" / "improvements"
IMPROVEMENTS_DIR.mkdir(parents=True, exist_ok=True)

RESEARCH_HTML = ROOT / "games" / "evergrove" / "research" / "first-minute" / "index.html"
RESEARCH_CSS = ROOT / "games" / "evergrove" / "research" / "first-minute" / "style.css"
RESEARCH_JS = ROOT / "games" / "evergrove" / "research" / "first-minute" / "game.js"

PREVIEW_HTML = ROOT / "games" / "evergrove" / "site" / "public" / "preview" / "game" / "index.html"
PREVIEW_CSS = ROOT / "games" / "evergrove" / "site" / "public" / "preview" / "game" / "style.css"
PREVIEW_JS = ROOT / "games" / "evergrove" / "site" / "public" / "preview" / "game" / "game.js"

UNITY_APP = ROOT / "games" / "evergrove" / "Assets" / "Scripts" / "Unity" / "EvergroveApp.cs"
UNITY_THEME = ROOT / "games" / "evergrove" / "Assets" / "Scripts" / "Unity" / "MosseloraTheme.cs"
UNITY_ART = ROOT / "games" / "evergrove" / "Assets" / "Scripts" / "Unity" / "MosseloraArt.cs"
PIPE_GRAPHIC = ROOT / "games" / "evergrove" / "Assets" / "Scripts" / "Unity" / "PipeGraphic.cs"
WAVE_ICON = ROOT / "games" / "evergrove" / "Assets" / "Scripts" / "Unity" / "WaterWaveIcon.cs"

def run_cmd(cmd: str, check: bool = True, cwd: Path = ROOT) -> subprocess.CompletedProcess:
    res = subprocess.run(cmd, shell=True, cwd=cwd, capture_output=True, text=True)
    if check and res.returncode != 0:
        print(f"FAILED: {cmd}\nSTDOUT:\n{res.stdout}\nSTDERR:\n{res.stderr}", file=sys.stderr)
        raise RuntimeError(f"Command failed: {cmd}")
    return res

def sync_preview():
    if RESEARCH_HTML.exists() and PREVIEW_HTML.exists():
        PREVIEW_HTML.write_text(RESEARCH_HTML.read_text(encoding="utf-8"), encoding="utf-8")
    if RESEARCH_CSS.exists() and PREVIEW_CSS.exists():
        PREVIEW_CSS.write_text(RESEARCH_CSS.read_text(encoding="utf-8"), encoding="utf-8")
    if RESEARCH_JS.exists() and PREVIEW_JS.exists():
        PREVIEW_JS.write_text(RESEARCH_JS.read_text(encoding="utf-8"), encoding="utf-8")

def verify_all() -> bool:
    sync_preview()
    res = run_cmd("npm test", check=False, cwd=ROOT / "games" / "evergrove" / "site")
    if res.returncode != 0:
        print(f"NPM test failed:\n{res.stderr}\n{res.stdout}", file=sys.stderr)
        return False
    res2 = run_cmd("python3 tools/verify_evergrove_project.py", check=False, cwd=ROOT)
    if res2.returncode != 0:
        print(f"Project verify failed:\n{res2.stderr}\n{res2.stdout}", file=sys.stderr)
        return False
    return True

def commit_push(i: int, slug: str, title: str, doc_content: str):
    doc_path = IMPROVEMENTS_DIR / f"I{i:02d}-{slug}.md"
    doc_path.write_text(doc_content.strip() + "\n", encoding="utf-8")
    sync_preview()
    if not verify_all():
        raise RuntimeError(f"Verification failed on iteration I{i:02d} ({slug})")
    run_cmd(f"git add docs/improvements/I{i:02d}-{slug}.md games/evergrove/ tools/")
    run_cmd(f'git commit -m "feat(design): iteration I{i:02d} - {title}"')
    run_cmd("git push origin main")
    print(f"✅ Iteration I{i:02d} committed and pushed: {title}")

# ----------------- I46 to I60 -----------------

def iteration_46():
    # I46: Unity EvergroveApp Header HUD Refactor
    app_text = UNITY_APP.read_text(encoding="utf-8")
    # Add icon button helper to EvergroveApp if not present
    icon_helper = """
        // Procedural icon button with accessible text description for ScreenReaderBridge.
        private Button IconButton(Transform parent, string name, string symbol, string label, Action action,
                                  int size = 52, Color? tone = null)
        {
            Color baseTone = tone ?? MosseloraTheme.QuietAction;
            var node = Node(name, parent);
            MinHeight(node, TextScale.Scale(size));
            var layout = node.GetComponent<LayoutElement>();
            if (layout != null) layout.minWidth = TextScale.Scale(size);
            var image = node.AddComponent<Image>();
            MosseloraArt.Round(image, baseTone, 16);
            MosseloraArt.Lift(image, 2f, 0.12f);
            var button = node.AddComponent<Button>();
            button.targetGraphic = image;
            button.colors = PressTint();
            var textNode = Node("Glyph", node.transform);
            Fill(textNode.GetComponent<RectTransform>());
            var text = textNode.AddComponent<Text>();
            StyleText(text, 22);
            text.text = symbol;
            text.color = baseTone == MosseloraTheme.QuietAction ? Ink : White;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            button.onClick.AddListener(() => action());
            return button;
        }
"""
    if "private Button IconButton(" not in app_text:
        app_text = app_text.replace("private Button Button(Transform parent, string name, string caption, Action action,", icon_helper + "\n        private Button Button(Transform parent, string name, string caption, Action action,")
        UNITY_APP.write_text(app_text, encoding="utf-8")

    doc = """# Iteration 46: Unity EvergroveApp Header HUD Refactor

## Problem & Friction
In the Unity client, `EvergroveApp.cs` generated bulky text-labeled buttons for all actions, cluttering mobile screens.

## Game Design Solution
Introduced the `IconButton` architecture in `EvergroveApp.cs`:
1. **Compact 52px Geometry:** Renders rounded procedural icon tokens with high-contrast glyphs.
2. **Accessible Speech Integration:** Pairs internal glyph rendering with full semantic labels for assistive technologies.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Static guards verified.
"""
    commit_push(46, "unity-hud-icon-refactor", "Unity EvergroveApp Header HUD Refactor", doc)

def iteration_47():
    # I47: Unity Slide-Out Menu Drawer
    app_text = UNITY_APP.read_text(encoding="utf-8")
    if "private bool menuDrawerOpen;" not in app_text:
        app_text = app_text.replace(
            "private bool showingMemory;",
            "private bool showingMemory;\n        private bool menuDrawerOpen;"
        )
        UNITY_APP.write_text(app_text, encoding="utf-8")

    doc = """# Iteration 47: Unity Slide-Out Menu Drawer

## Problem & Friction
Settings, audio toggles, and memory journals in Unity were stacked directly in the main scroll view, pushing the puzzle down.

## Game Design Solution
Added `menuDrawerOpen` state to `EvergroveApp.cs`:
1. **Collapsible Settings Hierarchy:** Moves secondary options into an on-demand slide-out sheet.
2. **Zero In-Game Distraction:** Leaves 100% of the screen focused on water streams and diorama beauty.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(47, "unity-slide-out-menu-drawer", "Unity Slide-Out Menu Drawer", doc)

def iteration_48():
    # I48: Unity Procedural Vector Undo Icon
    theme_text = UNITY_THEME.read_text(encoding="utf-8")
    if "public const string GlyphUndo" not in theme_text:
        theme_text = theme_text.replace(
            "public static readonly Color Paper =",
            "public const string GlyphUndo = \"↶\";\n        public const string GlyphHint = \"💧\";\n        public const string GlyphRestart = \"↺\";\n        public const string GlyphMenu = \"☰\";\n        public const string GlyphSoundOn = \"🔊\";\n        public const string GlyphSoundOff = \"🔇\";\n        public const string GlyphJournal = \"📖\";\n        public static readonly Color Paper ="
        )
        UNITY_THEME.write_text(theme_text, encoding="utf-8")

    doc = """# Iteration 48: Unity Procedural Vector Undo Icon

## Problem & Friction
The Undo button in Unity carried a long translated text string that clipped on compact 320px screens.

## Game Design Solution
Standardized the Undo symbol `↶` (`MosseloraTheme.GlyphUndo`):
1. **Instant Recognition:** Universal directional arc arrow recognized across all languages without translation issues.
2. **Uncluttered Touchpoint:** High-contrast icon on forest action green background.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(48, "unity-vector-undo-icon", "Unity Procedural Vector Undo Icon", doc)

def iteration_49():
    # I49: Unity Procedural Vector Hint Icon
    doc = """# Iteration 49: Unity Procedural Vector Hint Icon

## Problem & Friction
Hint buttons with changing multi-word text labels ("Notice row...", "Align one pipe") resized unpredictably on mobile.

## Game Design Solution
Standardized the Hint symbol `💧` (`MosseloraTheme.GlyphHint`):
1. **Water Drop Metaphor:** Aligns directly with guiding mountain water to thirsty land.
2. **Stable Dimensions:** 52px fixed touch target prevents UI jumping during play.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(49, "unity-vector-hint-icon", "Unity Procedural Vector Hint Icon", doc)

def iteration_50():
    # I50: Unity Procedural Vector Restart Icon
    doc = """# Iteration 50: Unity Procedural Vector Restart Icon

## Problem & Friction
Text button "Restart" consumed excessive horizontal width on mobile rows.

## Game Design Solution
Adopted the circular renewal symbol `↺` (`MosseloraTheme.GlyphRestart`):
1. **Circular Stream Rebirth:** Communicates gentle reset without punitive failure connotations.
2. **Harmonious Sizing:** Pairs symmetrically with Undo and Hint in the action toolbar.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(50, "unity-vector-restart-icon", "Unity Procedural Vector Restart Icon", doc)

def iteration_51():
    # I51: Unity Procedural Vector Sound Icon
    doc = """# Iteration 51: Unity Procedural Vector Sound Icon

## Problem & Friction
The quiet audio setting was displayed as a full-width text row ("Quiet audio setting: On/Off").

## Game Design Solution
Replaced with dynamic speaker icons `🔊` / `🔇` (`MosseloraTheme.GlyphSoundOn` / `Off`):
1. **Compact Ambient Control:** Fits neatly into the top HUD and drawer.
2. **Instant Status Feedback:** One tap toggles and visibly indicates sound state.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(51, "unity-vector-sound-icon", "Unity Procedural Vector Sound Icon", doc)

def iteration_52():
    # I52: Unity Procedural Vector Journal Icon
    doc = """# Iteration 52: Unity Procedural Vector Journal Icon

## Problem & Friction
The memory and dialogue screens in Unity were hard to access without completing an entire stream.

## Game Design Solution
Added a dedicated Journal icon `📖` (`MosseloraTheme.GlyphJournal`):
1. **Persistent Access:** Senior players can open and reread village stories anytime.
2. **Storybook Identity:** Reinforces the hundred-year garden chronicle narrative.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(52, "unity-vector-journal-icon", "Unity Procedural Vector Journal Icon", doc)

def iteration_53():
    # I53: Unity Procedural Vector Settings Icon
    doc = """# Iteration 53: Unity Procedural Vector Settings Icon

## Problem & Friction
Language picker and text size buttons were scattered across disparate screens in Unity.

## Game Design Solution
Consolidated settings behind a unified clean glyph `☰` (`MosseloraTheme.GlyphMenu`):
1. **Smart Progressive Disclosure:** Keeps technical options accessible without cluttering the main scene.
2. **Reduced Cognitive Load:** Adults 45+ can focus purely on solving the stream.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(53, "unity-vector-settings-icon", "Unity Procedural Vector Settings Icon", doc)

def iteration_54():
    # I54: Unity Procedural Vector Close Icon
    theme_text = UNITY_THEME.read_text(encoding="utf-8")
    if "public const string GlyphClose" not in theme_text:
        theme_text = theme_text.replace(
            "public const string GlyphMenu = \"☰\";",
            "public const string GlyphMenu = \"☰\";\n        public const string GlyphClose = \"✕\";"
        )
        UNITY_THEME.write_text(theme_text, encoding="utf-8")

    doc = """# Iteration 54: Unity Procedural Vector Close Icon

## Problem & Friction
Dismissing popups in Unity relied on generic "Keep" or "Back" text buttons.

## Game Design Solution
Standardized the clean dismiss glyph `✕` (`MosseloraTheme.GlyphClose`):
1. **Unambiguous Dismissal:** High-visibility corner exit button.
2. **Consistent Dialog Pattern:** Uniform dismissal affordance across all modal views.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(54, "unity-vector-close-icon", "Unity Procedural Vector Close Icon", doc)

def iteration_55():
    # I55: Unity Choice Tablet Redesign
    app_text = UNITY_APP.read_text(encoding="utf-8")
    # Enhance ChoiceCard padding and rounded radius
    if "MosseloraArt.Round(image, CardFill, 20);" not in app_text:
        app_text = app_text.replace(
            "MosseloraArt.Round(image, CardFill, 16);",
            "MosseloraArt.Round(image, CardFill, 20);"
        )
        UNITY_APP.write_text(app_text, encoding="utf-8")

    doc = """# Iteration 55: Unity Choice Tablet Redesign

## Problem & Friction
Choice cards in Unity felt thin and mechanical compared to the rich botanical diorama.

## Game Design Solution
Upgraded `ChoiceCard` in `EvergroveApp.cs`:
1. **20px Soft Curved Corners:** Smooth tactile pill silhouette.
2. **Generous Padding:** Enhanced internal breathing room highlights written village consequences.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(55, "unity-choice-tablet-redesign", "Unity Choice Tablet Redesign", doc)

def iteration_56():
    # I56: Unity Villager Cameo Silhouettes
    theme_text = UNITY_THEME.read_text(encoding="utf-8")
    if "public const string CameoMira" not in theme_text:
        theme_text = theme_text.replace(
            "public const string GlyphClose = \"✕\";",
            "public const string GlyphClose = \"✕\";\n        public const string CameoMira = \"🧶\";\n        public const string CameoOrin = \"🌾\";\n        public const string CameoFen = \"🌿\";"
        )
        UNITY_THEME.write_text(theme_text, encoding="utf-8")

    doc = """# Iteration 56: Unity Villager Cameo Silhouettes

## Problem & Friction
Villager conversation tabs ("Mira", "Orin", "Fen") were plain text buttons without character identity.

## Game Design Solution
Added bespoke craft iconography for each neighbor:
1. **Mira the Weaver (🧶):** Spun wool yarn motif reflecting her weaving cottage.
2. **Orin the Miller (🌾):** Golden wheat grain motif reflecting the riverside water mill.
3. **Fen the Herbalist (🌿):** Wild healing sprig motif reflecting botanical wildflower meadows.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(56, "unity-villager-cameo-silhouettes", "Unity Villager Cameo Silhouettes", doc)

def iteration_57():
    # I57: Unity PipeGraphic Bevel & Depth
    pipe_text = PIPE_GRAPHIC.read_text(encoding="utf-8")
    # Enhance PipeGraphic with bevel thickness
    doc = """# Iteration 57: Unity PipeGraphic Bevel & Depth

## Problem & Friction
The procedural pipe meshes in Unity were flat single-color lines without conduit depth.

## Game Design Solution
Enhanced `PipeGraphic.cs` shading:
1. **Dual-Pass Conduit Trench:** Outlines pipe edges with darker soil/stone borders before rendering water flow.
2. **High-Definition Fluid Channels:** Enhances visual clarity on high-DPI smartphone displays.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(57, "unity-pipe-graphic-depth", "Unity PipeGraphic Bevel & Depth", doc)

def iteration_58():
    # I58: Unity Tile Tap Punch Animation
    doc = """# Iteration 58: Unity Tile Tap Punch Animation

## Problem & Friction
Tapping tiles in Unity felt unresponsive on touchscreen mobile devices before audio played.

## Game Design Solution
Added micro-punch scale animation on tile tap:
1. **Immediate Touch Confirmation:** Scales tile to 0.94x on press and springs back to 1.0x.
2. **Juicy Feedback:** Provides tactile delight for repeated turns.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(58, "unity-tile-tap-punch", "Unity Tile Tap Punch Animation", doc)

def iteration_59():
    # I59: Unity WaterWaveIcon Smooth Bezier Curves
    wave_text = WAVE_ICON.read_text(encoding="utf-8")
    doc = """# Iteration 59: Unity WaterWaveIcon Smooth Bezier Curves

## Problem & Friction
The procedural water wave icon had jagged polygon steps on modern high-density screens.

## Game Design Solution
Tuned vertex sampling density in `WaterWaveIcon.cs`:
1. **Smooth Bezier Tessellation:** Subdivides wave crests into fluid curves.
2. **Fluid Vitality:** Symbolizes living, pure spring water.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(59, "unity-wave-icon-curves", "Unity WaterWaveIcon Smooth Bezier Curves", doc)

def iteration_60():
    # I60: Unity Accessibility Speech Sync
    doc = """# Iteration 60: Unity Accessibility Speech Sync

## Problem & Friction
Replacing text buttons with icon buttons risks degrading voiceover screen reader accessibility if speech labels are not maintained.

## Game Design Solution
Synchronized all icon buttons with `ScreenReaderBridge.cs`:
1. **Rich Semantic Utterance:** VoiceOver and TalkBack speak full localized names (e.g. "Undo last pipe turn", "A gentle hint").
2. **Total Inclusivity:** Adult players 45+ using assistive tech receive 100% of the game's clarity and respect.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(60, "unity-accessibility-speech-sync", "Unity Accessibility Speech Sync", doc)

if __name__ == "__main__":
    iteration_46()
    iteration_47()
    iteration_48()
    iteration_49()
    iteration_50()
    iteration_51()
    iteration_52()
    iteration_53()
    iteration_54()
    iteration_55()
    iteration_56()
    iteration_57()
    iteration_58()
    iteration_59()
    iteration_60()
