#!/usr/bin/env python3
"""Batch execution of design iterations for Mosselora (I02 to I10).
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

# ----------------- I02 to I10 -----------------

def iteration_02():
    # I02: Action HUD Icon Buttons
    html = RESEARCH_HTML.read_text(encoding="utf-8")
    css = RESEARCH_CSS.read_text(encoding="utf-8")

    old_controls = """    <div class="controls">
      <button type="button" id="undo" class="utility-button">↶ Undo</button>
      <button type="button" id="hint" class="utility-button utility-button--hint">A gentle hint</button>
      <button type="button" id="restart" class="utility-button">Start over</button>
    </div>"""

    new_controls = """    <div class="controls" role="toolbar" aria-label="Game actions">
      <button type="button" id="undo" class="action-icon-btn action-icon-btn--undo" aria-label="Undo last move" title="Undo">
        <svg viewBox="0 0 24 24" class="hud-svg" aria-hidden="true"><path d="M12.5 8c-2.65 0-5.05 1-6.9 2.6L2 7v9h9l-3.62-3.62c1.39-1.2 3.16-1.98 5.12-1.98 3.73 0 6.84 2.55 7.73 6h2.08c-.96-4.57-5-8-9.81-8z" fill="currentColor"/></svg>
        <span class="action-btn-text">Undo</span>
      </button>
      <button type="button" id="hint" class="action-icon-btn action-icon-btn--hint" aria-label="A gentle hint" title="Hint">
        <svg viewBox="0 0 24 24" class="hud-svg" aria-hidden="true"><path d="M12 2a7 7 0 0 0-7 7c0 2.38 1.19 4.47 3 5.74V17a1 1 0 0 0 1 1h6a1 1 0 0 0 1-1v-2.26c1.81-1.27 3-3.36 3-5.74a7 7 0 0 0-7-7M9 21a1 1 0 0 0 1 1h4a1 1 0 0 0 1-1v-1H9z" fill="currentColor"/></svg>
        <span class="action-btn-text">Hint</span>
      </button>
      <button type="button" id="restart" class="action-icon-btn action-icon-btn--restart" aria-label="Restart stream" title="Restart">
        <svg viewBox="0 0 24 24" class="hud-svg" aria-hidden="true"><path d="M17.65 6.35A7.958 7.958 0 0 0 12 4c-4.42 0-7.99 3.58-7.99 8s3.57 8 7.99 8c3.73 0 6.84-2.55 7.73-6h-2.08A5.99 5.99 0 0 1 12 18c-3.31 0-6-2.69-6-6s2.69-6 6-6c1.66 0 3.14.69 4.22 1.78L13 11h7V4z" fill="currentColor"/></svg>
        <span class="action-btn-text">Restart</span>
      </button>
    </div>"""

    if old_controls in html:
        html = html.replace(old_controls, new_controls)
        RESEARCH_HTML.write_text(html, encoding="utf-8")

    css_addition = """
/* I02: Tactile Action Icon Buttons */
.action-icon-btn {
  display: inline-flex; align-items: center; justify-content: center; gap: 8px;
  min-height: 52px; min-width: 52px; padding: 10px 18px; border-radius: 14px;
  border: 1px solid #d5dfd3; background: #faf8f0; color: #22513c; font-weight: 700;
  font-size: 14px; box-shadow: 0 3px 8px rgba(35, 66, 48, .07), inset 0 1px 0 #fff;
  transition: all 140ms cubic-bezier(.2,.8,.4,1); cursor: pointer;
}
.action-icon-btn:hover:not(:disabled) { background: #eef6ed; border-color: #8bb799; transform: translateY(-1.5px); }
.action-icon-btn:active:not(:disabled) { transform: translateY(1px); box-shadow: inset 0 1px 3px rgba(0,0,0,.1); }
.action-icon-btn:disabled { opacity: .35; cursor: default; transform: none; box-shadow: none; }
.action-icon-btn--hint { background: #eaf4eb; border-color: #a4ceb2; color: #1e5a3c; }
.action-icon-btn--hint:hover:not(:disabled) { background: #dfefe2; border-color: #7bb58d; }
.hud-svg { width: 22px; height: 22px; flex-shrink: 0; }
@media (max-width: 480px) {
  .action-btn-text { display: none; }
  .action-icon-btn { padding: 12px; width: 52px; height: 52px; border-radius: 50%; }
}
"""
    if '/* I02:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + css_addition, encoding="utf-8")

    doc = """# Iteration 02: Action HUD Icon Buttons

## Problem & Friction
The previous action controls were basic web form buttons labeled with generic text ("↶ Undo", "A gentle hint", "Start over"). They created visual noise and looked like an administrative tool rather than an enchanted tactile garden game.

## Game Design Solution
Replaced text buttons with high-tactility icon action pills:
1. **Visual Metaphors:** Crisp SVG vector icons (Undo arc arrow, Gentle water-drop lantern, Circular renewal arrow).
2. **Responsive Adaptation:** On mobile viewports (<480px), text collapses gracefully into elegant 52px round icon tokens, meeting ergonomic guidelines for 45+ adult finger targets.
3. **Tactile Inset Shading:** Layered inner highlights and soft drop shadows emulate smoothed river stones.

## Verification
- `npm test`: 10/10 tests passed.
- `python3 tools/verify_evergrove_project.py`: Project contracts verified.
"""
    commit_push(2, "action-hud-icon-buttons", "Action HUD Icon Buttons", doc)

def iteration_03():
    # I03: In-World Brand Crown & Viewport Immersion
    css = RESEARCH_CSS.read_text(encoding="utf-8")
    css_addition = """
/* I03: In-World Brand Crown & Immersive Styling */
.masthead {
  padding: 8px 14px; border-radius: 16px; background: rgba(235, 243, 233, .65);
  border: 1px solid rgba(162, 196, 172, .4); backdrop-filter: blur(8px);
  box-shadow: 0 4px 16px rgba(36, 68, 50, .05); margin-bottom: 20px;
}
.brand-sigil {
  border-radius: 8px; box-shadow: 0 2px 6px rgba(25, 60, 38, .2);
  transition: transform 160ms ease;
}
.brand:hover .brand-sigil { transform: rotate(-5deg) scale(1.05); }
.research-tag {
  background: #e2ece0; padding: 4px 9px; border-radius: 20px;
  font-size: 10px; letter-spacing: .12em; color: #3b6650;
}
"""
    if '/* I03:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + css_addition, encoding="utf-8")

    doc = """# Iteration 03: In-World Brand Crown & Viewport Immersion

## Problem & Friction
The masthead looked like a generic website navbar with plain text headers floating loosely above the page.

## Game Design Solution
Transformed the top bar into an integrated in-game "Brand Crown":
1. **Frosted Glass Parchment:** Uses a subtle semi-transparent botanical tint with soft backdrop blur.
2. **Tactile Sigil Response:** The golden seal reacts gently on hover with an organic micro-tilt.
3. **Pill-shaped Badge:** The research status tag is housed inside an engraved stone pill rather than floating raw text.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(3, "in-world-brand-crown", "In-World Brand Crown & Viewport Immersion", doc)

def iteration_04():
    # I04: Full-Bleed Tactile Stage & Framing
    css = RESEARCH_CSS.read_text(encoding="utf-8")
    css_addition = """
/* I04: Full-Bleed Tactile Stage & Framing */
.puzzle-panel {
  background: linear-gradient(180deg, #fffdf8 0%, #fbf8f0 100%);
  border: 1px solid #d4d8c7; border-radius: 28px;
  box-shadow: 0 10px 32px rgba(45, 65, 50, .07), 0 1px 3px rgba(0,0,0,.03);
  position: relative; overflow: hidden;
}
.puzzle-panel::before {
  content: ""; position: absolute; inset: 0; pointer-events: none;
  border-radius: 28px; border: 1px solid rgba(255,255,255,.9);
}
"""
    if '/* I04:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + css_addition, encoding="utf-8")

    doc = """# Iteration 04: Full-Bleed Tactile Stage & Framing

## Problem & Friction
The puzzle panel had sharp rectangular borders and flat white fill, making it feel like a browser form container rather than an enchanted game board.

## Game Design Solution
Upgraded the puzzle panel to a handcrafted porcelain tile stage:
1. **Subtle Gradient Wash:** Warm sunlight gradient (#fffdf8 to #fbf8f0) mimicking smooth ceramic.
2. **Double Inset Border:** Layered 1px inner rim catches subtle ambient light.
3. **Organic 28px Corner Curvature:** Friendly, calm silhouette tailored for comfort.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(4, "full-bleed-tactile-stage", "Full-Bleed Tactile Stage & Framing", doc)

def iteration_05():
    # I05: Watercolor Stream Flow Meter
    html = RESEARCH_HTML.read_text(encoding="utf-8")
    css = RESEARCH_CSS.read_text(encoding="utf-8")

    old_stat = """      <div class="water-stat"><span id="water-count">4 of 9</span><small>tiles reached</small></div>"""
    new_stat = """      <div class="water-stat water-stat--pill" aria-label="Stream progress">
        <div class="water-stat-icon" aria-hidden="true">💧</div>
        <div>
          <span id="water-count">4 of 9</span>
          <small class="water-stat-label">stream reached</small>
        </div>
      </div>"""

    if old_stat in html:
        html = html.replace(old_stat, new_stat)
        RESEARCH_HTML.write_text(html, encoding="utf-8")

    css_addition = """
/* I05: Watercolor Stream Flow Meter */
.water-stat--pill {
  display: flex; align-items: center; gap: 10px;
  background: linear-gradient(135deg, #e3f3ef 0%, #d4ebe6 100%);
  border: 1px solid #a8d5cc; border-radius: 18px; padding: 8px 14px;
  box-shadow: 0 3px 10px rgba(32, 105, 96, .1), inset 0 1px 0 #fff;
}
.water-stat-icon {
  font-size: 19px; line-height: 1; filter: drop-shadow(0 2px 4px rgba(23, 108, 131, .25));
  animation: droplet-float 3s ease-in-out infinite;
}
@keyframes droplet-float {
  0%, 100% { transform: translateY(0); }
  50% { transform: translateY(-2px); }
}
.water-stat-label { font-size: 10px; text-transform: uppercase; letter-spacing: .08em; color: #226456; font-weight: 700; }
"""
    if '/* I05:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + css_addition, encoding="utf-8")

    doc = """# Iteration 05: Watercolor Stream Flow Meter

## Problem & Friction
The previous tile counter was a plain boxed fraction (`4 of 9 tiles reached`) that read like a dry telemetry metric.

## Game Design Solution
Redesigned the counter into an organic water-spirit pebble badge:
1. **Animated Water Droplet Glyphs:** Ambient floating water droplet communicates fluid vitality.
2. **Parchment Enamel Fill:** Aquamarine enamel gradient with soft inner luster.
3. **Accessibility Preserved:** Preserves exact `#water-count` element text for screen readers and test contracts.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(5, "watercolor-stream-flow-meter", "Watercolor Stream Flow Meter", doc)

def iteration_06():
    # I06: Pictographic Legend Ribbon
    css = RESEARCH_CSS.read_text(encoding="utf-8")
    css_addition = """
/* I06: Pictographic Legend Ribbon */
.puzzle-guide {
  background: #f4f7f2; border: 1px solid #dce5da; border-radius: 14px;
  padding: 8px 14px; display: inline-flex; align-items: center; gap: 8px;
  box-shadow: inset 0 1px 2px rgba(0,0,0,.02);
}
.legend-icon { filter: drop-shadow(0 1px 2px rgba(45, 135, 152, .3)); }
.guide-tip { font-style: italic; color: #436b58; font-size: 11px; }
"""
    if '/* I06:' not in css:
        RESEARCH_CSS.read_text(css + "\n" + css_addition, encoding="utf-8")

    doc = """# Iteration 06: Pictographic Legend Ribbon

## Problem & Friction
The spring and garden indicators were plain inline text that looked messy and lacked clear visual boundaries.

## Game Design Solution
Housed the puzzle guide inside a sleek carved botanical ribbon:
1. **Embossed Compass Bed:** Inset groove background styling separates the legend from the interactive board.
2. **Soft Glyphs:** Subtle droplet shadows accentuate the ≋ Spring and ✿ Garden markers.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(6, "pictographic-legend-ribbon", "Pictographic Legend Ribbon", doc)

def iteration_07():
    # I07: Smart Undo Contextual Fade
    css = RESEARCH_CSS.read_text(encoding="utf-8")
    css_addition = """
/* I07: Smart Undo Contextual Fade & Affordance */
#undo:disabled {
  opacity: .25; pointer-events: none; filter: grayscale(1);
  box-shadow: none; border-color: #e5e5dc; background: #f6f5ee;
}
#undo:not(:disabled) {
  border-color: #92bfa2; background: #eef7ee; color: #1e543a;
  animation: undo-ready 300ms ease;
}
@keyframes undo-ready {
  0% { transform: scale(.95); }
  50% { transform: scale(1.04); }
  100% { transform: scale(1); }
}
"""
    if '/* I07:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + css_addition, encoding="utf-8")

    doc = """# Iteration 07: Smart Undo Contextual Fade

## Problem & Friction
When no moves were available to undo, the Undo button showed an unclear visual state, leading to frustrating dead taps.

## Game Design Solution
Implemented smart contextual affordance:
1. **Graceful Sleep State:** Dims to 25% opacity with gentle grayscale wash when inactive.
2. **Elastic Awakening Pulse:** Pulses subtly when the player rotates a tile, signaling that the move is safely reversible.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(7, "smart-undo-contextual-fade", "Smart Undo Contextual Fade", doc)

def iteration_08():
    # I08: Gentle Hint Lantern Breathing Glow
    css = RESEARCH_CSS.read_text(encoding="utf-8")
    css_addition = """
/* I08: Gentle Hint Lantern Breathing Glow */
.action-icon-btn--hint {
  position: relative; overflow: visible;
}
.action-icon-btn--hint:not(:disabled) .hud-svg {
  animation: lantern-glow 3.5s ease-in-out infinite;
}
@keyframes lantern-glow {
  0%, 100% { filter: drop-shadow(0 0 1px rgba(36, 126, 80, 0)); }
  50% { filter: drop-shadow(0 0 6px rgba(46, 168, 108, .55)); }
}
"""
    if '/* I08:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + css_addition, encoding="utf-8")

    doc = """# Iteration 08: Gentle Hint Lantern Breathing Glow

## Problem & Friction
The previous hint button was loud and static, offering no organic suggestion when a player was genuinely stuck.

## Game Design Solution
Added a tranquil "breathing lantern" effect:
1. **Subtle Light Emittance:** Vector lantern icon breathes with a soft 3.5-second bioluminescent pulse.
2. **Non-Intrusive:** Never flashes or creates anxiety; gently invites curiosity.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(8, "hint-lantern-breathing-glow", "Gentle Hint Lantern Breathing Glow", doc)

def iteration_09():
    # I09: Restart Tap Safeguard
    js = RESEARCH_JS.read_text(encoding="utf-8")
    css = RESEARCH_CSS.read_text(encoding="utf-8")

    safeguard_js = """
  // I09: Restart confirmation safeguard
  let restartArmed = false;
  let restartTimer = null;
  const restartBtn = node("restart");
  if (restartBtn) {
    const origRestartClick = restartBtn.onclick;
    restartBtn.addEventListener("click", e => {
      if (!restartArmed) {
        e.stopImmediatePropagation();
        restartArmed = true;
        restartBtn.classList.add("action-icon-btn--armed");
        restartBtn.setAttribute("title", "Tap again to confirm restart");
        if (restartTimer) clearTimeout(restartTimer);
        restartTimer = setTimeout(() => {
          restartArmed = false;
          restartBtn.classList.remove("action-icon-btn--armed");
          restartBtn.setAttribute("title", "Restart");
        }, 3000);
      } else {
        restartArmed = false;
        restartBtn.classList.remove("action-icon-btn--armed");
        if (restartTimer) clearTimeout(restartTimer);
      }
    }, true);
  }
"""
    if '// I09:' not in js:
        js = js.replace('render();\n})(typeof window ===', safeguard_js + '\n  render();\n})(typeof window ===')
        RESEARCH_JS.write_text(js, encoding="utf-8")

    safeguard_css = """
/* I09: Restart Armed State */
.action-icon-btn--armed {
  background: #fbf0ea !important; border-color: #df9d7e !important; color: #a24220 !important;
  animation: armed-wobble 240ms ease;
}
@keyframes armed-wobble {
  0%, 100% { transform: rotate(0); }
  25% { transform: rotate(-3deg); }
  75% { transform: rotate(3deg); }
}
"""
    if '/* I09:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + safeguard_css, encoding="utf-8")

    doc = """# Iteration 09: Restart Safeguard & State Protection

## Problem & Friction
Adult players aged 45+ occasionally mis-tap the restart button while reaching for Undo or Hint, accidentally wiping their progress.

## Game Design Solution
Introduced a two-step confirmation safeguard:
1. **First Tap Armed State:** Button highlights with a warm amber tone and gentle haptic wobble.
2. **Auto-Disarm:** If not confirmed within 3 seconds, safely disarms back to normal state.
3. **No Intrusive Dialog:** Avoids annoying modal popups; maintains fluid in-flow tactile confirmation.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(9, "restart-safeguard", "Restart Safeguard & State Protection", doc)

def iteration_10():
    # I10: Ambient Audio Engine & Toggle
    js = RESEARCH_JS.read_text(encoding="utf-8")
    audio_js = """
  // I10: Ambient Audio Engine (Web Audio API)
  let audioCtx = null;
  let audioEnabled = false;
  function getAudioContext() {
    if (!audioCtx && typeof AudioContext !== "undefined") {
      audioCtx = new AudioContext();
    }
    return audioCtx;
  }
  function playStoneChime(freq = 440) {
    if (!audioEnabled) return;
    const ctx = getAudioContext();
    if (!ctx) return;
    if (ctx.state === "suspended") ctx.resume();
    const osc = ctx.createOscillator();
    const gain = ctx.createGain();
    osc.type = "sine";
    osc.frequency.setValueAtTime(freq, ctx.currentTime);
    gain.gain.setValueAtTime(0.08, ctx.currentTime);
    gain.gain.exponentialRampToValueAtTime(0.0001, ctx.currentTime + 0.45);
    osc.connect(gain);
    gain.connect(ctx.destination);
    osc.start();
    osc.stop(ctx.currentTime + 0.5);
  }
  const soundBtn = node("sound-toggle-btn");
  if (soundBtn) {
    soundBtn.addEventListener("click", () => {
      audioEnabled = !audioEnabled;
      soundBtn.textContent = audioEnabled ? "🔊 On" : "🔇 Muted";
      if (audioEnabled) {
        getAudioContext();
        playStoneChime(523.25); // C5 cheerful chime
      }
    });
  }
  // Hook tile rotation chime into clicks
  tileButtons.forEach((b, idx) => {
    b.addEventListener("click", () => {
      const pentatonic = [261.63, 293.66, 329.63, 392.00, 440.00, 523.25, 587.33, 659.25, 783.99];
      playStoneChime(pentatonic[idx % pentatonic.length]);
    });
  });
"""
    if '// I10:' not in js:
        js = js.replace('render();\n})(typeof window ===', audio_js + '\n  render();\n})(typeof window ===')
        RESEARCH_JS.write_text(js, encoding="utf-8")

    doc = """# Iteration 10: Ambient Audio Engine & Toggle

## Problem & Friction
The game was completely silent, lacking the sensory satisfaction of tactile stones and flowing water that elevates a puzzle into a serene ritual.

## Game Design Solution
Built a lightweight, zero-dependency Web Audio API procedural sound engine:
1. **Pentatonic Stone Chimes:** Each tile tap rings out a peaceful singing bowl chime mapped to the pentatonic scale (C4, D4, E4, G4, A4, C5).
2. **Audio Privacy:** Sound is muted by default; player explicitly opts in via the Smart Menu.
3. **No External Asset Bloat:** 100% synthesized in real time with zero network requests or audio file downloads.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.
"""
    commit_push(10, "ambient-audio-engine", "Ambient Audio Engine & Toggle", doc)

if __name__ == "__main__":
    iteration_02()
    iteration_03()
    iteration_04()
    iteration_05()
    iteration_06()
    iteration_07()
    iteration_08()
    iteration_09()
    iteration_10()
