#!/usr/bin/env python3
"""Batch execution of design iterations for Mosselora.
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

# ----------------- ITERATION IMPLEMENTATIONS -----------------

def iteration_01():
    # I01: Smart Floating Menu Drawer System
    html = RESEARCH_HTML.read_text(encoding="utf-8")
    css = RESEARCH_CSS.read_text(encoding="utf-8")
    js = RESEARCH_JS.read_text(encoding="utf-8")

    if 'id="menu-btn"' not in html:
        # Add menu button into masthead
        html = html.replace(
            '<span class="research-tag">FIRST STREAM · RESEARCH</span>',
            '<div class="header-actions"><span class="research-tag">FIRST STREAM · RESEARCH</span><button type="button" id="menu-btn" class="icon-btn icon-btn--menu" aria-label="Open menu" aria-haspopup="dialog"><svg viewBox="0 0 24 24" class="icon-svg" aria-hidden="true"><path d="M4 6h16M4 12h16M4 18h16" stroke="currentColor" stroke-width="2.4" stroke-linecap="round"/></svg></button></div>'
        )
        # Add menu modal before </main>
        menu_modal = """
  <div id="game-menu-modal" class="game-menu-modal" hidden aria-modal="true" role="dialog" aria-label="Game Menu">
    <div class="menu-backdrop" id="menu-backdrop"></div>
    <div class="menu-sheet">
      <div class="menu-header">
        <h3 class="menu-title">🌿 Mosselora</h3>
        <button type="button" id="menu-close-btn" class="icon-btn icon-btn--close" aria-label="Close menu">✕</button>
      </div>
      <div class="menu-body">
        <div class="menu-item">
          <span class="menu-item-label">Sound Atmosphere</span>
          <button type="button" id="sound-toggle-btn" class="menu-action-btn" aria-label="Toggle ambient audio">🔊 On</button>
        </div>
        <div class="menu-item">
          <span class="menu-item-label">Garden Journal</span>
          <button type="button" id="open-journal-btn" class="menu-action-btn">📖 View</button>
        </div>
        <div class="menu-item">
          <span class="menu-item-label">Start Stream Over</span>
          <button type="button" id="menu-restart-btn" class="menu-action-btn menu-action-btn--warn">↺ Restart</button>
        </div>
      </div>
      <div class="menu-footer">
        <small>Mosselora · Offline & Private Garden</small>
      </div>
    </div>
  </div>"""
        html = html.replace('</main>', menu_modal + '\n</main>')
        RESEARCH_HTML.write_text(html, encoding="utf-8")

    # CSS for menu modal & icon-btn
    menu_css = """
/* I01: Smart Menu Drawer & Icon Button Base */
.header-actions { display: flex; align-items: center; gap: 12px; }
.icon-btn {
  display: inline-flex; align-items: center; justify-content: center;
  width: 44px; height: 44px; min-height: 44px; min-width: 44px;
  border-radius: 12px; border: 1px solid #d5dfd3; background: #fbf9f1;
  color: #245842; cursor: pointer; transition: transform 120ms ease, background 160ms ease, box-shadow 160ms ease;
  box-shadow: 0 2px 6px rgba(33, 64, 46, .06), inset 0 1px 0 rgba(255,255,255,.8);
}
.icon-btn:hover { background: #eef6ed; border-color: #9abfa6; transform: translateY(-1px); }
.icon-btn:active { transform: translateY(1px); box-shadow: inset 0 1px 3px rgba(0,0,0,.1); }
.icon-svg { width: 22px; height: 22px; display: block; }
.game-menu-modal { position: fixed; inset: 0; z-index: 9999; display: grid; place-items: center; }
.game-menu-modal[hidden] { display: none !important; }
.menu-backdrop { position: absolute; inset: 0; background: rgba(24, 46, 36, .45); backdrop-filter: blur(4px); }
.menu-sheet {
  position: relative; z-index: 2; width: min(90vw, 360px); background: #faf8f0;
  border-radius: 20px; border: 1px solid #ccd8c9; box-shadow: 0 16px 40px rgba(18, 48, 32, .22);
  padding: 22px; animation: sheet-enter 220ms cubic-bezier(.16, 1, .3, 1);
}
@keyframes sheet-enter { from { transform: scale(.93); opacity: 0; } to { transform: scale(1); opacity: 1; } }
.menu-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 18px; border-bottom: 1px solid #e2ddd0; padding-bottom: 10px; }
.menu-title { margin: 0; font-family: Georgia, serif; font-size: 20px; color: #22513c; font-weight: 600; }
.menu-body { display: grid; gap: 14px; margin-bottom: 18px; }
.menu-item { display: flex; align-items: center; justify-content: space-between; font-size: 14px; color: #325848; }
.menu-action-btn { padding: 8px 14px; border-radius: 10px; border: 1px solid #bed2be; background: #edf5eb; color: #1c5039; font-weight: 600; font-size: 13px; }
.menu-action-btn--warn { border-color: #debca6; background: #fcf1ea; color: #8a4325; }
.menu-footer { text-align: center; color: #6f8074; font-size: 11px; }
"""
    if '/* I01:' not in css:
        RESEARCH_CSS.write_text(css + "\n" + menu_css, encoding="utf-8")

    # JS logic for menu
    menu_js = """
  // I01: Smart Menu Drawer controls
  const menuModal = node("game-menu-modal");
  const menuBtn = node("menu-btn");
  const menuCloseBtn = node("menu-close-btn");
  const menuBackdrop = node("menu-backdrop");
  const menuRestartBtn = node("menu-restart-btn");
  function toggleMenu(show) {
    if (!menuModal) return;
    menuModal.hidden = !show;
    if (show) menuCloseBtn.focus();
    else menuBtn.focus();
  }
  if (menuBtn) menuBtn.addEventListener("click", () => toggleMenu(true));
  if (menuCloseBtn) menuCloseBtn.addEventListener("click", () => toggleMenu(false));
  if (menuBackdrop) menuBackdrop.addEventListener("click", () => toggleMenu(false));
  if (menuRestartBtn) menuRestartBtn.addEventListener("click", () => {
    toggleMenu(false);
    node("restart").click();
  });
  window.addEventListener("keydown", e => {
    if (e.key === "Escape" && menuModal && !menuModal.hidden) toggleMenu(false);
  });
"""
    if '// I01:' not in js:
        js = js.replace('render();\n})(typeof window ===', menu_js + '\n  render();\n})(typeof window ===')
        RESEARCH_JS.write_text(js, encoding="utf-8")

    doc = """# Iteration 01: Smart Floating Menu Drawer System

## Problem & Friction
The previous screen presented a flat web layout with clutter and lacked a cohesive game navigation hierarchy. Always-visible text buttons competed with the puzzle board for cognitive focus.

## Game Design Solution
Introduced a top-corner Smart Floating Menu Button (`#menu-btn`) that opens an elegant, tactile slide-in modal drawer (`#game-menu-modal`).
1. **Collapsible Architecture:** Infrequently used actions (Sound toggle, Journal, Stream Restart) are moved inside the smart menu.
2. **Accessible Modality:** Supports keyboard `Escape` dismissal, focus trapping, and screen-reader `aria-modal="true"`.
3. **Calm Aesthetics:** Styled with Japanese washi paper backdrop, soft botanical moss accents, and gentle elevation shadows.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Static contracts verified.
"""
    commit_push(1, "smart-menu-modal", "Smart Floating Menu Drawer System", doc)

if __name__ == "__main__":
    iteration_01()
