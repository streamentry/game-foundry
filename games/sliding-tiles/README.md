# Sliding Tiles — original independent 8-puzzle sliding blocks playtest

**Game Foundry prototype 31 · GF-068 · Godot 4.7.2**, purely a research implementation. No app-store production approval or business validation.

## Game

Slide numbered tiles orthogonally into the empty square to reconstruct the ordered 1–8 board.

**Interaction:** Tap only a tile adjacent to the blank; Undo and Restart are always available. **Content:** 6 original scrambled starts with verified shortest solution depths of 4,6,8,10,12 and 14 moves. Each original puzzle is playable on a standalone Godot project with offline saved progress, Undo, Restart, Next, no timer, account, ads, payments or analytics SDK.

## Build and test locally

Open `games/sliding-tiles/project.godot` in Godot 4.7.2 standard editor and press F5. For deterministic verification:

```bash
godot --headless --path games/sliding-tiles --editor --import --quit
godot --headless --path games/sliding-tiles --script res://tests/test_rules.gd
godot --headless --path games/sliding-tiles --script res://tests/test_progress.gd
godot --headless --path games/sliding-tiles --script res://tests/test_scene.gd
```

The [Sliding Tiles Web preview workflow](../../.github/workflows/sliding-tiles-preview.yml) exports an independent single-threaded **downloadable** WebGL2 ZIP, bundled with an HTTP `serve.py` helper, not a public website. Download and extract the entire Actions artifact; from inside the extracted folder, run `python3 serve.py` and visit `http://127.0.0.1:8000/`. `python3 serve.py --smoke` verifies the assets. Opening `index.html` through `file://` can cause NetworkError.

## Research status and failure criterion

**Testable advantage:** Do instant one-tap slides and no timer improve comfort enough to be demonstrably preferred? **Strongest criticism:** The 8-puzzle is extremely commoditized; deterministic reachability is engineering hygiene, not a commercial differentiator. These are small original puzzle fixtures and their scripted tests establish narrow rule/content correctness, **not** that actual players 45+ understand, enjoy or return, that the levels are difficulty-calibrated, or that non-branded App Store search demand exists. Test on physical phones and against direct mature incumbents before adding content.

See [independent visual direction](DESIGN.md), [ten-game research and explicit correctness caveats](../../docs/research/game-experiments-23-to-32-2026-11-22.md) and [browser/network troubleshooting](../../docs/web-networkerror.md).
