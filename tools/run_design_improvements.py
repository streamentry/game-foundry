#!/usr/bin/env python3
"""Run design improvements iterations for Mosselora.
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
DESIGN_SYSTEM = ROOT / "games" / "evergrove" / "DESIGN-SYSTEM.md"

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

def record_and_commit(iteration: int, slug: str, title: str, description_md: str):
    doc_path = IMPROVEMENTS_DIR / f"I{iteration:02d}-{slug}.md"
    doc_path.write_text(description_md.strip() + "\n", encoding="utf-8")
    sync_preview()
    if not verify_all():
        raise RuntimeError(f"Verification failed on iteration I{iteration:02d}")
    run_cmd(f"git add docs/improvements/I{iteration:02d}-{slug}.md games/evergrove/ tools/")
    msg = f"feat(design): iteration I{iteration:02d} - {title}"
    run_cmd(f'git commit -m "{msg}"')
    run_cmd("git push origin main")
    print(f"==> Committed & pushed I{iteration:02d}: {title}")

print("Loaded base helper.")
