# Akari Evening — original independent Light Up / Akari playtest

**Game Foundry prototype 29 · GF-062 · Godot 4.7.2**, purely a research implementation. No app-store production approval or business validation.

## Game

Place lamps that illuminate all open cells without any pair seeing each other, while honoring numbered wall adjacency.

**Interaction:** Tap an open cell to cycle lamp → cross note → empty; the board highlights lit cells. **Content:** 6 original 4×4 numbered-wall boards with one valid lamp arrangement each. Each original puzzle is playable on a standalone Godot project with offline saved progress, Undo, Restart, Next, no timer, account, ads, payments or analytics SDK.

## Build and test locally

Open `games/akari-evening/project.godot` in Godot 4.7.2 standard editor and press F5. For deterministic verification:

```bash
godot --headless --path games/akari-evening --editor --import --quit
godot --headless --path games/akari-evening --script res://tests/test_rules.gd
godot --headless --path games/akari-evening --script res://tests/test_progress.gd
godot --headless --path games/akari-evening --script res://tests/test_scene.gd
```

The [Akari Evening Web preview workflow](../../.github/workflows/akari-evening-preview.yml) exports an independent single-threaded **downloadable** WebGL2 ZIP, bundled with an HTTP `serve.py` helper, not a public website. Download and extract the entire Actions artifact; from inside the extracted folder, run `python3 serve.py` and visit `http://127.0.0.1:8000/`. `python3 serve.py --smoke` verifies the assets. Opening `index.html` through `file://` can cause NetworkError.

## Research status and failure criterion

**Testable advantage:** Will immediate illumination cues reduce the need for rule explanations and encourage further play? **Strongest criticism:** Small boards and colored light feedback may be aesthetically appealing but insufficient to teach illumination to first-time players. These are small original puzzle fixtures and their scripted tests establish narrow rule/content correctness, **not** that actual players 45+ understand, enjoy or return, that the levels are difficulty-calibrated, or that non-branded App Store search demand exists. Test on physical phones and against direct mature incumbents before adding content.

See [independent visual direction](DESIGN.md), [ten-game research and explicit correctness caveats](../../docs/research/game-experiments-23-to-32-2026-11-22.md) and [browser/network troubleshooting](../../docs/web-networkerror.md).
