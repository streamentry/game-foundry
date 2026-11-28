"""Generate the static support + privacy site for the puzzle catalog.

Output: sites/puzzles/public/ (deployed as the streamentry-puzzles Worker).
Usage: python3 tools/store_catalog/site.py
"""
from __future__ import annotations

import html
import shutil
from pathlib import Path

from catalog import ROOT, games

OUT = ROOT / "sites" / "puzzles" / "public"
PUBLISHER = "CLOUDJET SOLUTIONS PTE. LTD."
EMAIL = "hi@mosselora.com"
UPDATED = "28 November 2026"

CSS = """
:root{--paper:#f7f3ea;--ink:#1d2430;--muted:#5b6472;--line:#ddd5c4;--accent:#2d5b8a;
--serif:"Iowan Old Style","Palatino Linotype",Palatino,Georgia,serif;--sans:"Avenir Next",Avenir,"Segoe UI",system-ui,sans-serif}
@media (prefers-color-scheme:dark){:root{--paper:#171b22;--ink:#eee8dc;--muted:#a8b0bc;--line:#2f3642;--accent:#8fb8e6}}
*{box-sizing:border-box}body{margin:0;background:var(--paper);color:var(--ink);font:18px/1.6 var(--sans)}
main{max-width:44rem;margin:0 auto;padding:3rem 1rem 4rem}h1{font:600 2.4rem/1.1 var(--serif);margin:.2rem 0 1rem}
h2{font:600 1.35rem/1.3 var(--serif);margin:2.2rem 0 .5rem}a{color:var(--accent)}a:focus-visible{outline:3px solid var(--accent);outline-offset:3px}
.eyebrow{font-size:.8rem;letter-spacing:.12em;text-transform:uppercase;color:var(--muted)}
.icon{width:96px;height:96px;border-radius:22px;box-shadow:0 6px 18px rgba(0,0,0,.18)}
ul.games{list-style:none;padding:0;display:grid;grid-template-columns:repeat(auto-fill,minmax(9.5rem,1fr));gap:1rem}
ul.games a{display:flex;flex-direction:column;align-items:center;gap:.5rem;text-decoration:none;color:var(--ink);padding:.8rem;border-radius:14px}
ul.games a:hover{background:var(--line)}ul.games img{width:72px;height:72px;border-radius:17px}
footer{margin-top:3rem;padding-top:1rem;border-top:1px solid var(--line);color:var(--muted);font-size:.9rem}
"""


def page(title: str, body: str, depth: int) -> str:
    up = "../" * depth
    return f"""<!doctype html><html lang="en"><head><meta charset="utf-8">
<meta name="viewport" content="width=device-width,initial-scale=1"><title>{html.escape(title)}</title>
<link rel="stylesheet" href="{up}style.css"></head><body><main>{body}
<footer>Published by {PUBLISHER} · <a href="{up}privacy/">Privacy policy</a> · <a href="mailto:{EMAIL}">{EMAIL}</a></footer>
</main></body></html>"""


def privacy() -> str:
    return page("Privacy policy — Streamentry Puzzles", f"""
<p class="eyebrow">Streamentry Puzzles</p><h1>Privacy policy</h1>
<p>Last updated {UPDATED}. This policy covers every puzzle game listed on <a href="../">this site</a>,
published by {PUBLISHER} on the App Store and Google Play.</p>
<h2>What we collect</h2><p>Nothing. The games play fully offline. They contain no advertising,
analytics, crash-reporting or tracking software, no account or sign-in, and they do not request
internet access. We do not collect, store, sell or share any personal data.</p>
<h2>Data kept on your device</h2><p>Each game saves your puzzle progress and settings in its own
private storage on your device so you can continue where you left off. That file never leaves your
device. Deleting the app deletes it.</p>
<h2>Children</h2><p>The games are made for adults and are suitable for all ages. Because no data
is collected, no data from children is collected.</p>
<h2>Contact</h2><p>Questions about privacy: <a href="mailto:{EMAIL}">{EMAIL}</a>.
If this policy changes, the new version will be posted here with a new date.</p>""", 1)


def support(game) -> str:
    meta = game.metadata()
    desc = "".join(f"<p>{html.escape(p)}</p>" for p in meta["apple"]["description"].split("\n") if p.strip())
    return page(f"{game.name} — Support", f"""
<img class="icon" src="icon.png" width="96" height="96" alt="">
<p class="eyebrow">Support</p><h1>{html.escape(game.name)}</h1>{desc}
<h2>Get help</h2><p>Email <a href="mailto:{EMAIL}?subject={html.escape(game.name)}">{EMAIL}</a> with your
device model and what happened. We read every message.</p>
<h2>Your progress</h2><p>Progress is saved on your device only. Deleting the app removes it;
there is no cloud backup.</p>""", 1)


def index(all_games) -> str:
    items = "".join(f'<li><a href="{g.slug}/"><img src="{g.slug}/icon.png" width="72" height="72" alt="">'
                    f"<span>{html.escape(g.name)}</span></a></li>" for g in all_games)
    return page("Streamentry Puzzles", f"""
<p class="eyebrow">{PUBLISHER}</p><h1>Streamentry Puzzles</h1>
<p>Calm, offline logic and word puzzles. No ads, no accounts, no data collection.</p>
<ul class="games">{items}</ul>""", 0)


def main() -> None:
    all_games = games()
    if OUT.exists():
        shutil.rmtree(OUT)
    (OUT / "privacy").mkdir(parents=True)
    (OUT / "style.css").write_text(CSS)
    (OUT / "index.html").write_text(index(all_games))
    (OUT / "privacy" / "index.html").write_text(privacy())
    for g in all_games:
        d = OUT / g.slug
        d.mkdir()
        (d / "index.html").write_text(support(g))
        shutil.copy(g.path / "icon.png", d / "icon.png")
    print("pages", len(all_games) + 2)


if __name__ == "__main__":
    main()
