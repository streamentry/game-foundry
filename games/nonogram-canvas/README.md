# Nonogram Canvas — original independent picture cross / Nonogram playtest

**Game Foundry prototype 28 · GF-056 · Godot 4.7.2**, purely a research implementation. No app-store production approval or business validation.

## Game

Fill a 5×5 image using exact row/column runs separated by at least one empty cell.

**Interaction:** Tap a cell filled → crossed empty → unknown. Ten row/column clue lines stay visible. **Content:** 6 original 5×5 pixel designs with uniquely determined row and column clues. Each original puzzle is playable on a standalone Godot project with offline saved progress, Undo, Restart, Next, no timer, account, ads, payments or analytics SDK.

## Build and test locally

Open `games/nonogram-canvas/project.godot` in Godot 4.7.2 standard editor and press F5. For deterministic verification:

```bash
godot --headless --path games/nonogram-canvas --editor --import --quit
godot --headless --path games/nonogram-canvas --script res://tests/test_rules.gd
godot --headless --path games/nonogram-canvas --script res://tests/test_progress.gd
godot --headless --path games/nonogram-canvas --script res://tests/test_scene.gd
```

The [Nonogram Canvas Web preview workflow](../../.github/workflows/nonogram-canvas-preview.yml) exports an independent single-threaded **downloadable** WebGL2 ZIP, bundled with an HTTP `serve.py` helper, not a public website. Download and extract the entire Actions artifact; from inside the extracted folder, run `python3 serve.py` and visit `http://127.0.0.1:8000/`. `python3 serve.py --smoke` verifies the assets. Opening `index.html` through `file://` can cause NetworkError.

## Research status and failure criterion

**Testable advantage:** Are visible per-line completion counts and large tap targets enough to earn preference over mature Nonograms? **Strongest criticism:** A 5×5 tutorial can demonstrate correctness but may provide little replay value, and clue headers may crowd narrow screens. These are small original puzzle fixtures and their scripted tests establish narrow rule/content correctness, **not** that actual players 45+ understand, enjoy or return, that the levels are difficulty-calibrated, or that non-branded App Store search demand exists. Test on physical phones and against direct mature incumbents before adding content.

See [independent visual direction](DESIGN.md), [ten-game research and explicit correctness caveats](../../docs/research/game-experiments-23-to-32-2026-11-22.md) and [browser/network troubleshooting](../../docs/web-networkerror.md).
