# Renzoku Meadow — original independent consecutive Latin squares playtest

**Game Foundry prototype 23 · GF-025 · Godot 4.7.2**, purely a research implementation. No app-store production approval or business validation.

## Game

4×4 full Renzoku: fill 1–4 once per row/column, with marked consecutive edges and unmarked nonconsecutive edges.

**Interaction:** Select a square, enter a digit 1–4; in-cell arrows identify marked right/below edges. **Content:** 6 independently generated Latin/dot layouts with exactly one solution. Each original puzzle is playable on a standalone Godot project with offline saved progress, Undo, Restart, Next, no timer, account, ads, payments or analytics SDK.

## Build and test locally

Open `games/renzoku-meadow/project.godot` in Godot 4.7.2 standard editor and press F5. For deterministic verification:

```bash
godot --headless --path games/renzoku-meadow --editor --import --quit
godot --headless --path games/renzoku-meadow --script res://tests/test_rules.gd
godot --headless --path games/renzoku-meadow --script res://tests/test_progress.gd
godot --headless --path games/renzoku-meadow --script res://tests/test_scene.gd
```

The [Renzoku Meadow Web preview workflow](../../.github/workflows/renzoku-meadow-preview.yml) exports an independent single-threaded **downloadable** WebGL2 ZIP, bundled with an HTTP `serve.py` helper, not a public website. Download and extract the entire Actions artifact; from inside the extracted folder, run `python3 serve.py` and visit `http://127.0.0.1:8000/`. `python3 serve.py --smoke` verifies the assets. Opening `index.html` through `file://` can cause NetworkError.

## Research status and failure criterion

**Testable advantage:** Do explicit edge labels make full positive-and-negative consecutive logic self-explanatory? **Strongest criticism:** Full negative-edge rule may surprise users familiar only with positive-dot Kropki, which is already another studio prototype. These are small original puzzle fixtures and their scripted tests establish narrow rule/content correctness, **not** that actual players 45+ understand, enjoy or return, that the levels are difficulty-calibrated, or that non-branded App Store search demand exists. Test on physical phones and against direct mature incumbents before adding content.

See [independent visual direction](DESIGN.md), [ten-game research and explicit correctness caveats](../../docs/research/game-experiments-23-to-32-2026-11-22.md) and [browser/network troubleshooting](../../docs/web-networkerror.md).
