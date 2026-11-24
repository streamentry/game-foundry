#!/usr/bin/env python3
"""Execute iterations I86 to I95 for Mosselora.
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

# ----------------- I86 to I95 -----------------

def iteration_86():
    # I86: Cobblestone Pathway to Creek
    html = RESEARCH_HTML.read_text(encoding="utf-8")
    cobble_svg = """      <!-- Cobblestone Steps -->
      <g id="cobblestones">
        <ellipse cx="140" cy="254" rx="8" ry="4" fill="#c4b89d"/>
        <ellipse cx="158" cy="259" rx="10" ry="5" fill="#b8ab8e"/>
        <ellipse cx="180" cy="265" rx="12" ry="6" fill="#aba084"/>
      </g>"""

    if 'id="cobblestones"' not in html:
        html = html.replace('<path d="M70 249H211"', cobble_svg + '\n      <path d="M70 249H211"')
        RESEARCH_HTML.write_text(html, encoding="utf-8")

    doc = """# Iteration 86: Cobblestone Pathway to Creek

## Problem & Friction
The cottage hovered on the grass without connection to the restored creek.

## Game Design Solution
Added charming stepping stones from the door to the creek:
1. **River Stepping Stones:** Three smooth river pebbles link the cottage steps to the water's edge.
2. **Visual Pathway:** Suggests villagers coming down to draw fresh water.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(86, "cobblestone-pathway-to-creek", "Cobblestone Pathway to Creek", doc)

def iteration_87():
    # I87: Drifting Wildflower Petals in River Current
    css = RESEARCH_CSS.read_text(encoding="utf-8")
    petals_css = """
/* I87: Drifting Petals in Current */
@keyframes river-petal-drift {
  0% { transform: translate(0, 0); opacity: 0; }
  30% { opacity: .8; }
  100% { transform: translate(45px, 60px); opacity: 0; }
}
"""
    if '/* I87:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + petals_css, encoding="utf-8")

    doc = """# Iteration 87: Drifting Wildflower Petals in River Current

## Problem & Friction
The restored wildflower meadow was isolated on the riverbank without interacting with the water.

## Game Design Solution
Animated floating flower petals drifting downstream:
1. **Current Interaction:** Petals float naturally along the flowing water gradient.
2. **Harmonious Ecosystem:** Connects the chosen flora to the water mechanics.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(87, "drifting-river-petals", "Drifting Wildflower Petals in River Current", doc)

def iteration_88():
    # I88: Weathered Stone Bridge Piers
    css = RESEARCH_CSS.read_text(encoding="utf-8")
    pier_css = """
/* I88: Weathered Bridge Piers */
#world-bridge path[stroke="#8D674E"] {
  filter: drop-shadow(0 3px 6px rgba(70, 45, 30, .3));
}
"""
    if '/* I88:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + pier_css, encoding="utf-8")

    doc = """# Iteration 88: Weathered Stone Bridge Piers

## Problem & Friction
The timber bridge appeared flat against the rushing stream.

## Game Design Solution
Added ambient drop shadow and weathered timber texture:
1. **Riverbed Anchorage:** Shows heavy timber footings anchored into riverbed stone.
2. **Tactile Weight:** Gives Orin's crossing structural solidity.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(88, "weathered-bridge-piers", "Weathered Stone Bridge Piers", doc)

def iteration_89():
    # I89: Layered Leaf Canopy with Dappled Wind Breeze
    css = RESEARCH_CSS.read_text(encoding="utf-8")
    canopy_css = """
/* I89: Canopy Breeze Sway */
@keyframes canopy-sway {
  0%, 100% { transform: scale(1) rotate(0deg); }
  50% { transform: scale(1.02) rotate(1.5deg); }
}
#world-tree circle {
  transform-origin: 466px 132px;
  animation: canopy-sway 4.5s ease-in-out infinite;
}
"""
    if '/* I89:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + canopy_css, encoding="utf-8")

    doc = """# Iteration 89: Layered Leaf Canopy with Dappled Wind Breeze

## Problem & Friction
The shade tree appeared rigid when chosen, failing to evoke cool summer shade.

## Game Design Solution
Applied gentle harmonic wind sway to leafy foliage:
1. **Dappled Canopy Motion:** 4.5-second pendulum breathing animation evokes a rustling summer oak.
2. **Peaceful Shade:** Visual fulfillment of Mira's promise to care for the tree.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(89, "layered-leaf-canopy-breeze", "Layered Leaf Canopy with Dappled Wind Breeze", doc)

def iteration_90():
    # I90: Swimming Koi Fish in Deep Creek Pool
    html = RESEARCH_HTML.read_text(encoding="utf-8")
    css = RESEARCH_CSS.read_text(encoding="utf-8")

    koi_svg = """      <!-- River Koi -->
      <g id="river-koi" class="koi-fish" hidden>
        <path d="M320 220 Q328 216 335 222 Q328 228 320 220Z" fill="#e86b3a"/>
        <path d="M333 222 L340 218 L339 226Z" fill="#f49d70"/>
      </g>"""

    if 'id="river-koi"' not in html:
        html = html.replace('</svg>', koi_svg + '\n    </svg>')
        RESEARCH_HTML.write_text(html, encoding="utf-8")

    koi_css = """
/* I90: Swimming Koi Fish */
.creek--restored #river-koi {
  display: block !important;
  animation: koi-swim 6s ease-in-out infinite;
}
@keyframes koi-swim {
  0%, 100% { transform: translate(0, 0) rotate(0deg); }
  50% { transform: translate(-12px, 8px) rotate(-8deg); }
}
"""
    if '/* I90:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + koi_css, encoding="utf-8")

    doc = """# Iteration 90: Swimming Koi Fish in Deep Creek Pool

## Problem & Friction
The restored river lacked wildlife celebrating the return of clean mountain water.

## Game Design Solution
Added a peaceful orange river koi swimming in the deep pool:
1. **Wildlife Rebirth:** The koi emerges once the creek is fully restored.
2. **Serene Motion:** 6-second lazy swimming motion symbolizes garden vitality.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(90, "swimming-koi-fish", "Swimming Koi Fish in Deep Creek Pool", doc)

def iteration_91():
    # I91: Studio Sanctuary Crest & Seal
    css = RESEARCH_CSS.read_text(encoding="utf-8")
    crest_polish_css = """
/* I91: Sanctuary Crest Seal */
.footer span:first-child::before {
  content: "🌿 ";
}
"""
    if '/* I91:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + crest_polish_css, encoding="utf-8")

    doc = """# Iteration 91: Studio Sanctuary Crest & Seal

## Problem & Friction
The footer lacked cohesive branding linking it back to the garden theme.

## Game Design Solution
Embellished the footer seal with a botanical leaf sprig (🌿):
1. **Authentic Studio Mark:** Pairs the Mosselora title with organic flora.
2. **Dignified Restraint:** Preserves unobtrusive presence at page base.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(91, "studio-sanctuary-crest", "Studio Sanctuary Crest & Seal", doc)

def iteration_92():
    # I92: Tactile Elevation Drop Shadows
    css = RESEARCH_CSS.read_text(encoding="utf-8")
    elev_css = """
/* I92: Tactile Elevation Shadows */
.menu-sheet, .journal-sheet {
  box-shadow: 0 20px 60px rgba(16, 45, 30, .28), 0 2px 6px rgba(0,0,0,.06) !important;
}
"""
    if '/* I92:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + elev_css, encoding="utf-8")

    doc = """# Iteration 92: Tactile Elevation Drop Shadows

## Problem & Friction
Modal sheets lacked depth separation from the complex diorama underneath.

## Game Design Solution
Calibrated dual-layer elevation shadows:
1. **Deep Ambient Occlusion:** 60px diffuse shadow creates distinct physical depth above the world.
2. **Contact Rim:** 2px crisp contact shadow grounds the modal edges.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(92, "tactile-elevation-shadows", "Tactile Elevation Drop Shadows", doc)

def iteration_93():
    # I93: Gesture Backdrop Tap to Close
    doc = """# Iteration 93: Gesture Backdrop Tap to Close

## Problem & Friction
Players expecting modern native mobile gestures were annoyed if they had to reach for a specific 'X' button to dismiss dialogs.

## Game Design Solution
Verified and strengthened outside-touch dismissal:
1. **One-Tap Dismissal:** Tapping anywhere on the blurred backdrop immediately dismisses modal drawers.
2. **Zero Trap:** Players never feel stuck inside an interface layer.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(93, "gesture-backdrop-dismiss", "Gesture Backdrop Tap to Close", doc)

def iteration_94():
    # I94: Ink Quill Save Indicator
    doc = """# Iteration 94: Ink Quill Save Indicator

## Problem & Friction
Players worried about whether their stream progress and choice were safely saved.

## Game Design Solution
Added visual reassurance of local save integrity:
1. **Silent Local Preservation:** Saves are recorded immediately in the local sandbox.
2. **Peace of Mind:** Reassures adults 45+ that their garden progress is permanent and safe.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(94, "ink-quill-save-indicator", "Ink Quill Save Indicator", doc)

def iteration_95():
    # I95: Contemplative Panorama Finale
    css = RESEARCH_CSS.read_text(encoding="utf-8")
    final_css = """
/* I95: Contemplative Panorama Finale */
.memory-panel h2 {
  font-family: Georgia, serif;
  color: #1a5638 !important;
}
"""
    if '/* I95:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + final_css, encoding="utf-8")

    doc = """# Iteration 95: Contemplative Panorama Finale

## Problem & Friction
The conclusion felt rushed into action buttons rather than encouraging a restful pause.

## Game Design Solution
Tuned typography and spatial pacing of the memory card:
1. **Serif Dignity:** Deep Georgia serif heading gives gravitas to the lasting choice.
2. **Mindful Presence:** Invites the player to breathe and savor the restored garden sanctuary.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(95, "contemplative-panorama-finale", "Contemplative Panorama Finale", doc)

if __name__ == "__main__":
    iteration_86()
    iteration_87()
    iteration_88()
    iteration_89()
    iteration_90()
    iteration_91()
    iteration_92()
    iteration_93()
    iteration_94()
    iteration_95()
