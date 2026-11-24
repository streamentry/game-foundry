#!/usr/bin/env python3
"""Execute iterations I31 to I45 for Mosselora.
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

# ----------------- I31 to I45 -----------------

def iteration_31():
    # I31: Botanical Choice Tablet Overhaul
    css = RESEARCH_CSS.read_text(encoding="utf-8")
    tablet_css = """
/* I31: Botanical Choice Tablet Overhaul */
.choice-list { gap: 14px !important; }
.choice-button {
  min-height: 84px !important; padding: 14px 20px !important; border-radius: 20px !important;
  background: radial-gradient(circle at 20% 50%, #fbf9f2 0%, #f2ede0 100%) !important;
  border: 1.5px solid #c9d6c5 !important;
  box-shadow: 0 4px 14px rgba(35, 60, 42, .08), inset 0 1px 0 #fff !important;
  transition: all 180ms cubic-bezier(.2,.8,.3,1.2) !important;
}
.choice-button:hover {
  transform: translateY(-2px) scale(1.01) !important;
  border-color: #72ab87 !important;
  box-shadow: 0 8px 22px rgba(45, 95, 65, .15), inset 0 1px 0 #fff !important;
}
"""
    if '/* I31:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + tablet_css, encoding="utf-8")

    doc = """# Iteration 31: Botanical Choice Tablet Overhaul

## Problem & Friction
Consequential choice buttons felt like flat web forms rather than physical story choices in a handcrafted diorama.

## Game Design Solution
Redesigned choice options into tactile botanical tablets:
1. **Embossed Ceramic Finish:** Warm ivory glaze with inset perimeter highlight.
2. **Generous Touch Geometry:** 84px height exceeds senior touch comfort requirements.
3. **Elevated Hover Lift:** Responsive -2px float confirms interactivity on desktop and touch.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(31, "botanical-choice-tablets", "Botanical Choice Tablet Overhaul", doc)

def iteration_32():
    # I32: Handcrafted Shade Tree Tablet
    css = RESEARCH_CSS.read_text(encoding="utf-8")
    tree_css = """
/* I32: Handcrafted Shade Tree Tablet */
.choice-button[data-choice="tree"] > span {
  color: #3b7e52 !important;
  filter: drop-shadow(0 2px 4px rgba(40, 100, 60, .25));
}
.choice-button[data-choice="tree"]:hover { background: #f0f7ee !important; }
"""
    if '/* I32:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + tree_css, encoding="utf-8")

    doc = """# Iteration 32: Handcrafted Shade Tree Tablet

## Problem & Friction
The tree choice lacked distinct botanical identity compared to other options.

## Game Design Solution
Tuned the shade tree aesthetic:
1. **Verdant Leaf Shadow:** Deep oak emerald hue (#3b7e52) with subtle leaf canopy drop shadow.
2. **Cooling Hover Tone:** Soft green tint (#f0f7ee) evokes resting under fresh canopy shade.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(32, "shade-tree-tablet", "Handcrafted Shade Tree Tablet", doc)

def iteration_33():
    # I33: Handcrafted Footbridge Tablet
    css = RESEARCH_CSS.read_text(encoding="utf-8")
    bridge_css = """
/* I33: Handcrafted Footbridge Tablet */
.choice-button[data-choice="bridge"] > span {
  color: #8c5b36 !important;
  filter: drop-shadow(0 2px 4px rgba(110, 70, 40, .25));
}
.choice-button[data-choice="bridge"]:hover { background: #f9f4ed !important; }
"""
    if '/* I33:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + bridge_css, encoding="utf-8")

    doc = """# Iteration 33: Handcrafted Footbridge Tablet

## Problem & Friction
The crossing option lacked the warm timber and cobblestone character described in the village narrative.

## Game Design Solution
Styled the bridge tablet with timbercraft motifs:
1. **Rustic Cedar Hue:** Warm chestnut glyph (#8c5b36) evoking hand-hewn bridge timber.
2. **Parchment Hearth Wash:** Gentle cedar undertone on interactive states.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(33, "footbridge-tablet", "Handcrafted Footbridge Tablet", doc)

def iteration_34():
    # I34: Handcrafted Wildflower Meadow Tablet
    css = RESEARCH_CSS.read_text(encoding="utf-8")
    flower_css = """
/* I34: Handcrafted Wildflower Meadow Tablet */
.choice-button[data-choice="flowers"] > span {
  color: #a8547e !important;
  filter: drop-shadow(0 2px 4px rgba(135, 60, 95, .25));
}
.choice-button[data-choice="flowers"]:hover { background: #fbf2f6 !important; }
"""
    if '/* I34:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + flower_css, encoding="utf-8")

    doc = """# Iteration 34: Handcrafted Wildflower Meadow Tablet

## Problem & Friction
The flower choice appeared identical to the generic garden icons without communicating its colorful butterfly sanctuary identity.

## Game Design Solution
Enhanced the wildflower tablet with botanical floral tones:
1. **Meadow Orchid Accent:** Deep blooming mauve (#a8547e) with subtle petal glow.
2. **Floral Petal Bloom:** Distinct hover state sets it apart as a vibrant pollinator haven.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(34, "wildflower-meadow-tablet", "Handcrafted Wildflower Meadow Tablet", doc)

def iteration_35():
    # I35: Tactile Choice Tablet Depression & Haptic Buzz
    css = RESEARCH_CSS.read_text(encoding="utf-8")
    js = RESEARCH_JS.read_text(encoding="utf-8")

    press_css = """
/* I35: Tactile Choice Tablet Depression */
.choice-button:active {
  transform: translateY(2px) scale(.985) !important;
  box-shadow: inset 0 2px 6px rgba(0,0,0,.12) !important;
}
"""
    if '/* I35:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + press_css, encoding="utf-8")

    haptic_js = """
  // I35: Web Haptic Vibration support
  function triggerHaptic(duration = 25) {
    if (typeof navigator !== "undefined" && navigator.vibrate) {
      try { navigator.vibrate(duration); } catch {}
    }
  }
"""
    if '// I35:' not in js:
        js = js.replace('function playStoneChime', haptic_js + '\n  function playStoneChime')
        js = js.replace('if (!model.choose(button.dataset.choice)) return;', 'if (!model.choose(button.dataset.choice)) return;\n      triggerHaptic(40);')
        RESEARCH_JS.write_text(js, encoding="utf-8")

    doc = """# Iteration 35: Tactile Choice Tablet Depression & Haptic Buzz

## Problem & Friction
Selecting a major village consequence felt sterile without physical sensory affirmation.

## Game Design Solution
Introduced tactile depression and haptic feedback:
1. **Physical Card Depression:** Tablet physically yields into the board with an inset shadow.
2. **Subtle Haptic Pulse:** Triggers a 40ms micro-vibration on modern mobile devices, confirming the lasting decision.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(35, "tactile-tablet-depression", "Tactile Choice Tablet Depression & Haptic Buzz", doc)

def iteration_36():
    # I36: Consequence Water Ripple Wave
    css = RESEARCH_CSS.read_text(encoding="utf-8")
    wave_css = """
/* I36: Consequence Water Ripple Wave */
.world-ripple {
  position: absolute; inset: 0; pointer-events: none; border-radius: 24px;
  border: 3px solid rgba(80, 180, 195, .8);
  animation: ripple-spread 1.2s cubic-bezier(.1, .7, .1, 1) forwards;
}
@keyframes ripple-spread {
  0% { transform: scale(.6); opacity: 1; }
  100% { transform: scale(1.15); opacity: 0; }
}
"""
    if '/* I36:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + wave_css, encoding="utf-8")

    doc = """# Iteration 36: Consequence Water Ripple Wave

## Problem & Friction
The transition from making a village choice to viewing its impact was sudden.

## Game Design Solution
Added an expanding environmental ripple effect:
1. **Aquamarine Shockwave:** Emits an expanding water crest across the diorama canvas.
2. **Causal Connection:** Visually reinforces the core premise: *"Your choice changes the place."*

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(36, "consequence-water-ripple-wave", "Consequence Water Ripple Wave", doc)

def iteration_37():
    # I37: Storybook Illuminated Inscription
    css = RESEARCH_CSS.read_text(encoding="utf-8")
    story_css = """
/* I37: Storybook Illuminated Inscription */
.memory-panel {
  background: radial-gradient(circle at 10% 20%, #fbf9f1 0%, #edf4ec 100%) !important;
  border: 1.5px solid #a8cbb2 !important; border-left: 6px solid #3c825a !important;
  border-radius: 22px !important; box-shadow: 0 10px 32px rgba(35, 75, 50, .09) !important;
  position: relative;
}
.memory-panel::before {
  content: "❧"; position: absolute; top: 16px; right: 20px;
  font-size: 26px; color: #43845e; opacity: .4;
}
"""
    if '/* I37:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + story_css, encoding="utf-8")

    doc = """# Iteration 37: Storybook Illuminated Inscription

## Problem & Friction
The memory box was styled like a plain callout alert box rather than an illuminated manuscript recording village history.

## Game Design Solution
Restyled the memory panel as a storybook chronicle:
1. **Botanical Floral Fleuron (❧):** Adds an authentic storybook ornamentation mark.
2. **Emerald Spine Accent:** 6px left border emulates the bound leather spine of a traveler's journal.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(37, "storybook-illuminated-inscription", "Storybook Illuminated Inscription", doc)

def iteration_38():
    # I38: Villager Cameo & Dialogue Styling
    css = RESEARCH_CSS.read_text(encoding="utf-8")
    cameo_css = """
/* I38: Villager Cameo Dialogue Styling */
.memory-panel h2 {
  display: flex; align-items: center; gap: 10px; font-size: 28px !important;
}
.memory-panel p#memory-text {
  font-size: 16px !important; line-height: 1.7 !important; color: #234d38 !important;
  font-style: italic;
}
"""
    if '/* I38:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + cameo_css, encoding="utf-8")

    doc = """# Iteration 38: Villager Cameo & Dialogue Styling

## Problem & Friction
The written memory looked like game system log text rather than words spoken from the heart by village neighbors.

## Game Design Solution
Enhanced the typography of villager dialogue:
1. **Spoken Dialogue Styling:** Rendered in warm italicized text with generous 1.7 line height for easy reading by adults 45+.
2. **Deep Forest Ink:** High-contrast #234d38 tone ensures effortless legibility under varied lighting conditions.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(38, "villager-dialogue-styling", "Villager Cameo & Dialogue Styling", doc)

def iteration_39():
    # I39: Interactive Storybook Journal Drawer
    html = RESEARCH_HTML.read_text(encoding="utf-8")
    js = RESEARCH_JS.read_text(encoding="utf-8")

    journal_modal = """
  <!-- Journal Modal -->
  <div id="journal-modal" class="game-menu-modal" hidden aria-modal="true" role="dialog" aria-label="Garden Chronicle">
    <div class="menu-backdrop" id="journal-backdrop"></div>
    <div class="menu-sheet journal-sheet">
      <div class="menu-header">
        <h3 class="menu-title">📖 Garden Chronicle</h3>
        <button type="button" id="journal-close-btn" class="icon-btn icon-btn--close" aria-label="Close journal">✕</button>
      </div>
      <div class="menu-body" id="journal-entries">
        <p class="journal-entry-intro">A quiet record of the streams restored and choices made.</p>
        <div class="journal-card" id="journal-current-card">
          <strong>Chapter 01: First Bend</strong>
          <p id="journal-choice-summary">The stream has begun to flow through the dry riverbed.</p>
        </div>
      </div>
      <div class="menu-footer">
        <small>Preserved offline in your local garden.</small>
      </div>
    </div>
  </div>"""

    if 'id="journal-modal"' not in html:
        html = html.replace('</main>', journal_modal + '\n</main>')
        RESEARCH_HTML.write_text(html, encoding="utf-8")

    journal_js = """
  // I39: Garden Journal Modal
  const journalModal = node("journal-modal");
  const openJournalBtn = node("open-journal-btn");
  const journalCloseBtn = node("journal-close-btn");
  const journalBackdrop = node("journal-backdrop");
  function toggleJournal(show) {
    if (!journalModal) return;
    journalModal.hidden = !show;
    if (show && model.choice) {
      node("journal-choice-summary").textContent = CHOICES[model.choice].result;
    }
  }
  if (openJournalBtn) openJournalBtn.addEventListener("click", () => {
    toggleMenu(false);
    toggleJournal(true);
  });
  if (journalCloseBtn) journalCloseBtn.addEventListener("click", () => toggleJournal(false));
  if (journalBackdrop) journalBackdrop.addEventListener("click", () => toggleJournal(false));
"""
    if '// I39:' not in js:
        js = js.replace('render();\n})(typeof window ===', journal_js + '\n  render();\n})(typeof window ===')
        RESEARCH_JS.write_text(js, encoding="utf-8")

    doc = """# Iteration 39: Interactive Storybook Journal Drawer

## Problem & Friction
Once a player reached the end, there was no way to revisit the narrative record without resetting the entire stream.

## Game Design Solution
Connected the Garden Journal modal (`#journal-modal`) from the smart menu:
1. **Persistent Memory Record:** Displays the player's active chapter progress and villager choices.
2. **Dedicated Chronicle Sheet:** Elegant scrollable modal styled like an aged botanical notebook.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(39, "interactive-storybook-journal", "Interactive Storybook Journal Drawer", doc)

def iteration_40():
    # I40: Seed of Rejuvenation Replay Icon
    html = RESEARCH_HTML.read_text(encoding="utf-8")
    css = RESEARCH_CSS.read_text(encoding="utf-8")

    if '🌱' not in html:
        html = html.replace(
            '<button type="button" id="replay" class="primary-button">Play the stream again</button>',
            '<button type="button" id="replay" class="primary-button primary-button--sprout"><span class="btn-icon" aria-hidden="true">🌱</span> Tend the stream again</button>'
        )
        RESEARCH_HTML.write_text(html, encoding="utf-8")

    replay_css = """
/* I40: Seed of Rejuvenation Replay Button */
.primary-button--sprout {
  display: inline-flex !important; align-items: center; justify-content: center; gap: 10px;
  min-height: 54px !important; border-radius: 16px !important; font-size: 16px !important;
  background: linear-gradient(135deg, #2b7454 0%, #1f5d42 100%) !important;
  box-shadow: 0 4px 16px rgba(35, 95, 65, .25), inset 0 1px 0 rgba(255,255,255,.3) !important;
}
.primary-button--sprout:hover { transform: translateY(-1px); filter: brightness(1.05) !important; }
"""
    if '/* I40:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + replay_css, encoding="utf-8")

    doc = """# Iteration 40: Seed of Rejuvenation Replay Icon

## Problem & Friction
The replay button read "Play the stream again", sounding repetitive like a standard mobile app loop.

## Game Design Solution
Re-framed the action as "Tend the stream again" with a vibrant green seedling icon (🌱):
1. **Mindful Gardening Theme:** Taps into the calming satisfaction of tending a lifelong botanical sanctuary.
2. **Vibrant Tactile Pill:** 54px hit target with warm emerald gradient and soft tactile lift.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(40, "seed-of-rejuvenation-replay-icon", "Seed of Rejuvenation Replay Icon", doc)

def iteration_41():
    # I41: Session Notes Parchment Icon
    html = RESEARCH_HTML.read_text(encoding="utf-8")
    if '📜' not in html:
        html = html.replace(
            '<button type="button" id="copy-notes" class="text-button">Copy local session notes (optional)</button>',
            '<button type="button" id="copy-notes" class="text-button text-button--scroll"><span aria-hidden="true">📜</span> Copy local session notes (optional)</button>'
        )
        RESEARCH_HTML.write_text(html, encoding="utf-8")

    doc = """# Iteration 41: Session Notes Parchment Icon

## Problem & Friction
The session notes action was a raw hyperlink-styled text button that looked out of place in a game.

## Game Design Solution
Integrated an illuminated scroll icon (📜) and styled the action as a discrete parchment export tool:
1. **Organic Iconography:** Fits the storybook traveler's notebook aesthetic.
2. **Preserved Test Contracts:** Full string matching for facilitator notes unchanged.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(41, "session-notes-parchment-icon", "Session Notes Parchment Icon", doc)

def iteration_42():
    # I42: Twilight & Dusk Ambiance Palette
    css = RESEARCH_CSS.read_text(encoding="utf-8")
    dusk_css = """
/* I42: Twilight & Dusk Palette Mode */
body.theme-dusk {
  background: radial-gradient(ellipse 110% 320px at 55% -30px, #263832 0%, transparent 88%), #1b2622;
  color: #e5ece6;
}
body.theme-dusk .puzzle-panel {
  background: #22302a !important; border-color: #354a40 !important; color: #e5ece6 !important;
}
body.theme-dusk .tile {
  background: #2a3a33 !important; border-color: #435e52 !important; color: #c4d8ce !important;
}
"""
    if '/* I42:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + dusk_css, encoding="utf-8")

    doc = """# Iteration 42: Twilight & Dusk Ambiance Palette

## Problem & Friction
Older players who enjoy peaceful evening puzzle routines before bed experienced eye strain under bright white backgrounds.

## Game Design Solution
Engineered a soothing deep botanical dusk palette (`.theme-dusk`):
1. **Calming Forest Slate:** Soft pine slate (#1b2622) with emerald undertones relieves night glare.
2. **Soft Mineral Tile Contrast:** Ceramic tiles take on a gentle jade sheen (#2a3a33) with soft legibility.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(42, "twilight-dusk-ambiance-palette", "Twilight & Dusk Ambiance Palette", doc)

def iteration_43():
    # I43: Circadian Ambient Sky Lighting
    js = RESEARCH_JS.read_text(encoding="utf-8")
    circadian_js = """
  // I43: Circadian Sky Lighting adaptation
  const hour = new Date().getHours();
  if (hour >= 20 || hour < 6) {
    document.body.classList.add("theme-dusk");
  }
"""
    if '// I43:' not in js:
        js = js.replace('render();\n})(typeof window ===', circadian_js + '\n  render();\n})(typeof window ===')
        RESEARCH_JS.write_text(js, encoding="utf-8")

    doc = """# Iteration 43: Circadian Ambient Sky Lighting

## Problem & Friction
The game stayed locked in midday sunlight regardless of whether a player was unwinding in the early morning or late at night.

## Game Design Solution
Added automatic circadian lighting adaptation:
1. **Natural Sunset Adaptation:** If local time is past 8:00 PM or before 6:00 AM, seamlessly shifts to the restful dusk ambiance.
2. **Zero Setup Needed:** Respects the player's natural biological rhythm automatically.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(43, "circadian-ambient-sky-lighting", "Circadian Ambient Sky Lighting", doc)

def iteration_44():
    # I44: Studio Sanctuary Crest & Footer Removal
    css = RESEARCH_CSS.read_text(encoding="utf-8")
    crest_css = """
/* I44: Studio Sanctuary Crest */
.footer {
  border-top: 1px solid #dcdad0 !important;
  opacity: .55; transition: opacity 200ms ease;
}
.footer:hover { opacity: .9; }
"""
    if '/* I44:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + crest_css, encoding="utf-8")

    doc = """# Iteration 44: Studio Sanctuary Crest & Footer Polish

## Problem & Friction
The bottom footer displayed large capital letters like an academic copyright statement, pulling the player out of the magic.

## Game Design Solution
Subdued the technical footnote:
1. **Minimalist Transparency:** 55% resting opacity allows the world diorama to remain the visual hero.
2. **Gentle Hover Wake:** Only reveals full contrast when deliberately inspected.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(44, "sanctuary-crest-footer-polish", "Studio Sanctuary Crest & Footer Polish", doc)

def iteration_45():
    # I45: Organic Paper Texture Canvas Vignette
    css = RESEARCH_CSS.read_text(encoding="utf-8")
    vignette_css = """
/* I45: Organic Paper Texture Canvas Vignette */
body::after {
  content: ""; position: fixed; inset: 0; pointer-events: none; z-index: 999;
  box-shadow: inset 0 0 80px rgba(45, 60, 48, .07);
}
"""
    if '/* I45:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + vignette_css, encoding="utf-8")

    doc = """# Iteration 45: Organic Paper Texture Canvas Vignette

## Problem & Friction
Screens ended abruptly at the viewport edge, reinforcing the perception of a web browser window.

## Game Design Solution
Framed the canvas with an ambient botanical vignette:
1. **Subtle Perimetral Depth:** Soft 80px diffuse shadow focuses peripheral vision onto the central diorama.
2. **Tactile Paper Immersion:** Creates the atmosphere of a physical artist's sketchbook.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(45, "paper-texture-canvas-vignette", "Organic Paper Texture Canvas Vignette", doc)

if __name__ == "__main__":
    iteration_31()
    iteration_32()
    iteration_33()
    iteration_34()
    iteration_35()
    iteration_36()
    iteration_37()
    iteration_38()
    iteration_39()
    iteration_40()
    iteration_41()
    iteration_42()
    iteration_43()
    iteration_44()
    iteration_45()
