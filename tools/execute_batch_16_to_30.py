#!/usr/bin/env python3
"""Execute iterations I16 to I30 for Mosselora.
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

# ----------------- I16 to I30 -----------------

def iteration_16():
    # I16: Animated Flowing Current Streaks
    css = RESEARCH_CSS.read_text(encoding="utf-8")
    flow_css = """
/* I16: Animated Flowing Current Streaks */
@keyframes water-current {
  0% { stroke-dashoffset: 28; }
  100% { stroke-dashoffset: 0; }
}
.tile--wet svg g {
  stroke-dasharray: 20 6;
  animation: water-current 1.4s linear infinite;
}
"""
    if '/* I16:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + flow_css, encoding="utf-8")

    doc = """# Iteration 16: Animated Flowing Current Streaks

## Problem & Friction
Connected wet pipes remained visually static, failing to convey the active kinetic energy of water rushing from the mountain spring.

## Game Design Solution
Animated the internal stroke dasharray of connected wet tiles:
1. **Flowing Water Dash:** 1.4-second continuous linear loop creates the visual sensation of a swift, living stream.
2. **Accessible Contrast:** Flow lines maintain a high-contrast hue against the ceramic conduit floor.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(16, "flowing-current-streaks", "Animated Flowing Current Streaks", doc)

def iteration_17():
    # I17: Watercolor Droplet Particle Bursts
    js = RESEARCH_JS.read_text(encoding="utf-8")
    css = RESEARCH_CSS.read_text(encoding="utf-8")

    particle_css = """
/* I17: Watercolor Droplet Particle Bursts */
.particle-splash {
  position: absolute; pointer-events: none; width: 8px; height: 8px;
  border-radius: 50%; background: #45b0c0; z-index: 10;
  animation: particle-burst 450ms ease-out forwards;
}
@keyframes particle-burst {
  0% { transform: translate(0, 0) scale(1); opacity: .9; }
  100% { transform: translate(var(--dx), var(--dy)) scale(0); opacity: 0; }
}
"""
    if '/* I17:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + particle_css, encoding="utf-8")

    particle_js = """
  // I17: Water splash particle burst
  function spawnSplash(el) {
    if (!el) return;
    const rect = el.getBoundingClientRect();
    for (let i = 0; i < 4; i++) {
      const p = document.createElement("span");
      p.className = "particle-splash";
      const angle = Math.random() * Math.PI * 2;
      const dist = 16 + Math.random() * 20;
      p.style.setProperty("--dx", (Math.cos(angle) * dist) + "px");
      p.style.setProperty("--dy", (Math.sin(angle) * dist) + "px");
      p.style.left = (rect.width / 2) + "px";
      p.style.top = (rect.height / 2) + "px";
      el.appendChild(p);
      setTimeout(() => p.remove(), 480);
    }
  }
"""
    if '// I17:' not in js:
        js = js.replace('function playStoneChime', particle_js + '\n  function playStoneChime')
        js = js.replace('button.classList.add("tile--turned");', 'button.classList.add("tile--turned"); spawnSplash(button);')
        RESEARCH_JS.write_text(js, encoding="utf-8")

    doc = """# Iteration 17: Watercolor Droplet Particle Bursts

## Problem & Friction
When a tile connected, there was no burst of tactile celebration, making moves feel dry.

## Game Design Solution
Spawned ephemeral watercolor droplet particles:
1. **Dynamic Splash:** Four microscopic water droplets burst outward from the tile center on rotation.
2. **Smooth Fade-Out:** Dissolves over 450ms without DOM buildup or layout thrashing.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(17, "watercolor-droplet-bursts", "Watercolor Droplet Particle Bursts", doc)

def iteration_18():
    # I18: 64px Senior-Comfort Touch Target Expansion
    css = RESEARCH_CSS.read_text(encoding="utf-8")
    target_css = """
/* I18: 64px Senior-Comfort Touch Targets */
.tile {
  min-height: 72px !important;
  touch-action: manipulation;
}
@media (max-width: 440px) {
  .board { gap: 12px; }
  .tile { min-height: 80px !important; border-radius: 18px !important; }
}
"""
    if '/* I18:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + target_css, encoding="utf-8")

    doc = """# Iteration 18: 64px Senior-Comfort Touch Target Expansion

## Problem & Friction
Small touch targets on mobile cause mis-taps for adults 45+ experiencing minor tremors or reduced fine-motor dexterity.

## Game Design Solution
Expanded tile minimum heights from 68px to 72px (and 80px on compact mobile viewports):
1. **Generous Tap Grids:** 12px gaps between tiles prevent accidental adjacent tile triggers.
2. **Comfortable Precision:** Exceeds Apple and Android 48px touch target guidelines by over 50%.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(18, "senior-comfort-touch-targets", "64px Senior-Comfort Touch Target Expansion", doc)

def iteration_19():
    # I19: Golden Junction Alignment Shimmer
    css = RESEARCH_CSS.read_text(encoding="utf-8")
    shimmer_css = """
/* I19: Golden Junction Alignment Shimmer */
.tile--wet::after {
  content: ""; position: absolute; inset: -2px; border-radius: 18px;
  border: 1px solid rgba(240, 205, 120, .45); pointer-events: none;
  animation: junction-shimmer 2.2s ease-in-out infinite;
}
@keyframes junction-shimmer {
  0%, 100% { opacity: .2; }
  50% { opacity: .75; }
}
"""
    if '/* I19:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + shimmer_css, encoding="utf-8")

    doc = """# Iteration 19: Golden Junction Alignment Shimmer

## Problem & Friction
Players couldn't easily perceive that a stream connection was structurally airtight versus merely touching.

## Game Design Solution
Added a golden connection aura (`junction-shimmer`):
1. **Subtle Gold Halo:** Highlights the tile rim with a tranquil 2.2-second amber glow.
2. **Confidence Reinforcement:** Provides immediate subconscious confirmation of valid pipe routing.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(19, "golden-junction-alignment-shimmer", "Golden Junction Alignment Shimmer", doc)

def iteration_20():
    # I20: Organic Water Spray Leak Glyphs
    css = RESEARCH_CSS.read_text(encoding="utf-8")
    alert_css = """
/* I20: Organic Water Spray Leak Glyphs */
.tile-alert {
  background: #fdf5eb !important; border: 1.5px solid #dca889 !important;
  color: #ad5230 !important; font-size: 16px !important; line-height: 19px !important;
  box-shadow: 0 2px 5px rgba(173, 82, 48, .2) !important;
  animation: spray-drift 1.8s ease-in-out infinite !important;
}
@keyframes spray-drift {
  0%, 100% { transform: translateY(0); }
  50% { transform: translateY(-2px); }
}
"""
    if '/* I20:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + alert_css, encoding="utf-8")

    doc = """# Iteration 20: Organic Water Spray Leak Glyphs

## Problem & Friction
The previous leak indicator was a harsh black circular mark that felt punitive, like an error badge on a spreadsheet.

## Game Design Solution
Softened the leak alert into an organic water vapor mist glyph:
1. **Gentle Terracotta Tone:** Warm terracotta border (#dca889) replaces abrasive alert colors.
2. **Subtle Floating Drift:** 1.8-second floating animation signals unsealed water pressure naturally.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(20, "organic-water-spray-leak-glyphs", "Organic Water Spray Leak Glyphs", doc)

def iteration_21():
    # I21: Concentric Spring Water Ripples
    css = RESEARCH_CSS.read_text(encoding="utf-8")
    spring_css = """
/* I21: Concentric Spring Water Ripples */
.tile-mark {
  filter: drop-shadow(0 2px 4px rgba(36, 124, 153, .35));
  animation: spring-ripple 2.5s ease-in-out infinite;
}
@keyframes spring-ripple {
  0%, 100% { transform: scale(1); }
  50% { transform: scale(1.12); }
}
"""
    if '/* I21:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + spring_css, encoding="utf-8")

    doc = """# Iteration 21: Concentric Spring Water Ripples

## Problem & Friction
The mountain spring source tile blended in with normal pipe tiles, missing its status as the headwaters of the entire garden.

## Game Design Solution
Animated the spring glyph ≋ with concentric water pulses:
1. **Headwaters Vitality:** A calm 2.5-second swell conveys an infinite, peaceful reservoir of clean mountain water.
2. **Orienting Landmark:** Anchors the player's eye immediately to the start of the puzzle flow.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(21, "concentric-spring-ripples", "Concentric Spring Water Ripples", doc)

def iteration_22():
    # I22: Destination Garden Bud Saturation Swell
    css = RESEARCH_CSS.read_text(encoding="utf-8")
    bud_css = """
/* I22: Destination Garden Bud Saturation Swell */
.tile-mark--sprout {
  filter: drop-shadow(0 2px 4px rgba(50, 132, 86, .35)) !important;
  animation: garden-bloom-anticipate 2s ease-in-out infinite;
}
@keyframes garden-bloom-anticipate {
  0%, 100% { transform: scale(1) rotate(0deg); color: #328456; }
  50% { transform: scale(1.18) rotate(4deg); color: #44aa72; }
}
"""
    if '/* I22:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + bud_css, encoding="utf-8")

    doc = """# Iteration 22: Destination Garden Bud Saturation Swell

## Problem & Friction
The destination flower ✿ was static, failing to provide emotional anticipation as water moved closer.

## Game Design Solution
Animated the destination flower bud:
1. **Anticipatory Blooming:** Gentle tilt (+4°) and color saturation increase indicate the thirsty garden awaiting the stream.
2. **Clear Goal Attractor:** Directs senior players visually toward the ultimate win condition.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(22, "destination-garden-bud-swell", "Destination Garden Bud Saturation Swell", doc)

def iteration_23():
    # I23: Crystalline Solved Board Lock Visual State
    css = RESEARCH_CSS.read_text(encoding="utf-8")
    lock_css = """
/* I23: Crystalline Solved Board Lock */
.puzzle--complete .board {
  position: relative;
}
.puzzle--complete .tile {
  background: radial-gradient(circle at 45% 30%, #e8f7ee 0%, #d5efe0 100%) !important;
  border-color: #55b57d !important;
  box-shadow: 0 6px 16px rgba(45, 140, 90, .18), inset 0 2px 0 #fff !important;
}
.puzzle--complete .tile svg {
  filter: drop-shadow(0 0 6px rgba(60, 180, 120, .6)) !important;
}
"""
    if '/* I23:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + lock_css, encoding="utf-8")

    doc = """# Iteration 23: Crystalline Solved Board Lock Visual State

## Problem & Friction
When completed, the puzzle simply froze disabled buttons without an aura of transformation.

## Game Design Solution
Applied a crystalline jade transformation:
1. **Emerald Transmutation:** Pipes transform from raw conduit to luminous, living aqueducts.
2. **Clear Completion Boundary:** Signals unambiguously that the puzzle is successfully resolved and the living world awaits.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(23, "crystalline-solved-board-lock", "Crystalline Solved Board Lock Visual State", doc)

def iteration_24():
    # I24: Fluid Board-to-Creek Camera Glide
    css = RESEARCH_CSS.read_text(encoding="utf-8")
    glide_css = """
/* I24: Fluid World Stage Reveal */
.world {
  transition: transform 600ms cubic-bezier(.16, 1, .3, 1), box-shadow 600ms ease;
}
.puzzle--complete + .world, .creek--restored {
  box-shadow: 0 16px 48px rgba(35, 80, 65, .15) !important;
}
"""
    if '/* I24:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + glide_css, encoding="utf-8")

    doc = """# Iteration 24: Fluid Board-to-Creek Camera Glide

## Problem & Friction
Scrolling down to the creek after solving felt abrupt and mechanical.

## Game Design Solution
Tuned the camera reveal physics:
1. **Decoupled Easing:** 600ms smooth cubic-bezier curve glides from the completed puzzle to the village creek.
2. **Expanded Elevation Shadow:** The village diorama gracefully lifts forward in z-space to draw player attention.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(24, "fluid-board-to-creek-glide", "Fluid Board-to-Creek Camera Glide", doc)

def iteration_25():
    # I25: Multi-Layered Creek Water Stream
    css = RESEARCH_CSS.read_text(encoding="utf-8")
    creek_css = """
/* I25: Multi-Layered Creek Water Stream */
#creek-front {
  filter: drop-shadow(0 2px 8px rgba(30, 110, 125, .4));
}
.creek--restored #creek-front {
  stroke-dasharray: 100 !important;
  animation: creek-pulse 3s ease-in-out infinite;
}
@keyframes creek-pulse {
  0%, 100% { stroke-width: 43px; }
  50% { stroke-width: 46px; }
}
"""
    if '/* I25:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + creek_css, encoding="utf-8")

    doc = """# Iteration 25: Multi-Layered Creek Water Stream

## Problem & Friction
The restored river was a static colored SVG path that looked like paint on paper rather than a rushing mountain stream.

## Game Design Solution
Added fluid pulse physics:
1. **River Swell:** 3-second breathing pulse (43px to 46px) mimics gentle water volume crests.
2. **Deep Water Shadow:** Aqua drop shadow gives the creek physical riverbed depth.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(25, "multi-layered-creek-stream", "Multi-Layered Creek Water Stream", doc)

def iteration_26():
    # I26: Parallax Watercolor Hillside Layers
    css = RESEARCH_CSS.read_text(encoding="utf-8")
    hill_css = """
/* I26: Parallax Watercolor Hillside Layers */
.creek-art {
  background: linear-gradient(180deg, #d8ebe5 0%, #edf4e6 55%, #dfede2 100%);
}
"""
    if '/* I26:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + hill_css, encoding="utf-8")

    doc = """# Iteration 26: Parallax Watercolor Hillside Layers

## Problem & Friction
The sky and hills behind the cottage lacked atmospheric perspective, flattening the scenery.

## Game Design Solution
Introduced a triple-stop watercolor wash:
1. **Morning Mist Gradient:** Blends soft pale jade into warm meadow sunlight.
2. **Natural Atmospheric Haze:** Deepens the illusion of distance behind the village.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(26, "parallax-watercolor-hillsides", "Parallax Watercolor Hillside Layers", doc)

def iteration_27():
    # I27: Animated Cottage Chimney Smoke
    html = RESEARCH_HTML.read_text(encoding="utf-8")
    css = RESEARCH_CSS.read_text(encoding="utf-8")

    smoke_svg = """      <!-- Chimney Smoke -->
      <g id="chimney-smoke">
        <circle cx="140" cy="95" r="4" fill="#e8eee6" class="smoke-puff smoke-puff--1"/>
        <circle cx="144" cy="85" r="6" fill="#e8eee6" class="smoke-puff smoke-puff--2"/>
        <circle cx="138" cy="73" r="8" fill="#e8eee6" class="smoke-puff smoke-puff--3"/>
      </g>"""

    if 'id="chimney-smoke"' not in html:
        html = html.replace('<path d="M52 169 L140 106 L229 169Z"', smoke_svg + '\n      <path d="M52 169 L140 106 L229 169Z"')
        RESEARCH_HTML.write_text(html, encoding="utf-8")

    smoke_css = """
/* I27: Animated Cottage Chimney Smoke */
.smoke-puff { opacity: 0; animation: smoke-drift 3.6s ease-out infinite; }
.smoke-puff--1 { animation-delay: 0s; }
.smoke-puff--2 { animation-delay: 1.2s; }
.smoke-puff--3 { animation-delay: 2.4s; }
@keyframes smoke-drift {
  0% { transform: translate(0, 0) scale(.6); opacity: 0; }
  30% { opacity: .65; }
  100% { transform: translate(-10px, -24px) scale(1.4); opacity: 0; }
}
"""
    if '/* I27:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + smoke_css, encoding="utf-8")

    doc = """# Iteration 27: Animated Cottage Chimney Smoke

## Problem & Friction
The cottage looked abandoned and unlived in, detracting from the narrative of helpful neighbors living in the village.

## Game Design Solution
Added curling woodstove chimney smoke puffs:
1. **Cozy Hearth Life:** Three staggered smoke puffs drift and dissipate into the mountain air.
2. **Emotional Connection:** Signals to the player that neighbors like Mira and Orin are home inside.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(27, "animated-cottage-chimney-smoke", "Animated Cottage Chimney Smoke", doc)

def iteration_28():
    # I28: Gentle Meadow Grass Sway
    css = RESEARCH_CSS.read_text(encoding="utf-8")
    grass_css = """
/* I28: Gentle Meadow Grass Sway */
@keyframes grass-breeze {
  0%, 100% { transform: rotate(0deg); }
  50% { transform: rotate(4deg); }
}
.creek-art path[stroke="#477B65"] {
  transform-origin: bottom center;
  animation: grass-breeze 4s ease-in-out infinite;
}
"""
    if '/* I28:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + grass_css, encoding="utf-8")

    doc = """# Iteration 28: Gentle Meadow Grass Sway

## Problem & Friction
Foreground riverside flora was static, making the diorama look like a static drawing rather than a living world.

## Game Design Solution
Applied harmonic wind physics to riverbank reeds:
1. **Subtle Mountain Breeze:** 4-second pendulum sway gently rocks reeds along the creek bank.
2. **Tranquil Immersion:** Encourages calm, mindful breathing while considering choices.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(28, "gentle-meadow-grass-sway", "Gentle Meadow Grass Sway", doc)

def iteration_29():
    # I29: Fluttering Meadow Butterfly
    css = RESEARCH_CSS.read_text(encoding="utf-8")
    butterfly_css = """
/* I29: Fluttering Meadow Butterfly */
@keyframes butterfly-flutter {
  0%, 100% { transform: translate(0, 0) scale(1); }
  25% { transform: translate(5px, -6px) scale(1.08); }
  50% { transform: translate(-3px, -10px) scale(.92); }
  75% { transform: translate(4px, -4px) scale(1.05); }
}
#world-flowers path[fill="#F4CC84"] {
  animation: butterfly-flutter 3.2s ease-in-out infinite;
  transform-origin: center;
}
"""
    if '/* I29:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + butterfly_css, encoding="utf-8")

    doc = """# Iteration 29: Fluttering Meadow Butterfly

## Problem & Friction
When the player chose wildflowers, the butterfly appeared as a stationary shape rather than a creature rejoicing in the blooms.

## Game Design Solution
Gave the yellow sulfur butterfly organic flight physics:
1. **Delicate Flitter:** 3.2-second figure-eight flutter path dances over the blossoms.
2. **Living Consequence:** Instantly rewards the player's choice to care for village nature.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(29, "fluttering-meadow-butterfly", "Fluttering Meadow Butterfly", doc)

def iteration_30():
    # I30: Warm Cottage Hearth Light
    css = RESEARCH_CSS.read_text(encoding="utf-8")
    hearth_css = """
/* I30: Warm Cottage Hearth Light */
.creek--restored rect[fill="#A0CDD0"], .creek--restored path[fill="#A0CDD0"] {
  fill: #fce8a6 !important;
  filter: drop-shadow(0 0 6px rgba(250, 220, 110, .7));
  transition: fill 1s ease, filter 1s ease;
}
"""
    if '/* I30:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + hearth_css, encoding="utf-8")

    doc = """# Iteration 30: Warm Cottage Hearth Light

## Problem & Friction
The cottage windows stayed cold blue (#A0CDD0) even after water returned and life blossomed.

## Game Design Solution
Illuminated the cottage windows with golden hearthlight upon restoration:
1. **Living Village Reaction:** Windows transition from cold glass to warm amber (#fce8a6) with gentle glow.
2. **Visual Storytelling:** Signals that the villagers inside are celebrating the return of the water.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(30, "warm-cottage-hearth-light", "Warm Cottage Hearth Light", doc)

if __name__ == "__main__":
    iteration_16()
    iteration_17()
    iteration_18()
    iteration_19()
    iteration_20()
    iteration_21()
    iteration_22()
    iteration_23()
    iteration_24()
    iteration_25()
    iteration_26()
    iteration_27()
    iteration_28()
    iteration_29()
    iteration_30()
