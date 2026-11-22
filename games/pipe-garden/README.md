# Pipe Garden — original independent pipe rotation playtest

**Game Foundry prototype 30 · GF-064 · Godot 4.7.2**, purely a research implementation. No app-store production approval or business validation.

## Game

Rotate nine pipes on a 3×3 board into a single continuous, leak-free route from one endpoint to the other.

**Interaction:** Tap a tile to rotate; curved and end pieces have four orientations, straights have two. **Content:** 6 original 3×3 pipe layouts with one distinct valid port-direction solution each. Each original puzzle is playable on a standalone Godot project with offline saved progress, Undo, Restart, Next, no timer, account, ads, payments or analytics SDK.

## Build and test locally

Open `games/pipe-garden/project.godot` in Godot 4.7.2 standard editor and press F5. For deterministic verification:

```bash
godot --headless --path games/pipe-garden --editor --import --quit
godot --headless --path games/pipe-garden --script res://tests/test_rules.gd
godot --headless --path games/pipe-garden --script res://tests/test_progress.gd
godot --headless --path games/pipe-garden --script res://tests/test_scene.gd
```

The [Pipe Garden Web preview workflow](../../.github/workflows/pipe-garden-preview.yml) exports an independent single-threaded **downloadable** WebGL2 ZIP, bundled with an HTTP `serve.py` helper, not a public website. Download and extract the entire Actions artifact; from inside the extracted folder, run `python3 serve.py` and visit `http://127.0.0.1:8000/`. `python3 serve.py --smoke` verifies the assets. Opening `index.html` through `file://` can cause NetworkError.

## Research status and failure criterion

**Testable advantage:** Can a one-tap reversible rotation system deliver satisfying untimed spatial deduction? **Strongest criticism:** Rotating small pipes can become mechanical trial-and-error rather than strategic play, and 3×3 grids are beginner-only. These are small original puzzle fixtures and their scripted tests establish narrow rule/content correctness, **not** that actual players 45+ understand, enjoy or return, that the levels are difficulty-calibrated, or that non-branded App Store search demand exists. Test on physical phones and against direct mature incumbents before adding content.

See [independent visual direction](DESIGN.md), [ten-game research and explicit correctness caveats](../../docs/research/game-experiments-23-to-32-2026-11-22.md) and [browser/network troubleshooting](../../docs/web-networkerror.md).
