# Number Pyramid — original independent number sum pyramid playtest

**Game Foundry prototype 32 · GF-079 · Godot 4.7.2**, purely a research implementation. No app-store production approval or business validation.

## Game

Reconstruct four bottom digits 1–9 from scattered clues; each of the six upper numbers equals the sum of the two directly below.

**Interaction:** Select a bottom stone and enter a digit 1–9; upper stones show live derived sums and fixed clue labels. **Content:** 6 independent original pyramid clues with exactly one four-digit base solution each. Each original puzzle is playable on a standalone Godot project with offline saved progress, Undo, Restart, Next, no timer, account, ads, payments or analytics SDK.

## Build and test locally

Open `games/number-pyramid/project.godot` in Godot 4.7.2 standard editor and press F5. For deterministic verification:

```bash
godot --headless --path games/number-pyramid --editor --import --quit
godot --headless --path games/number-pyramid --script res://tests/test_rules.gd
godot --headless --path games/number-pyramid --script res://tests/test_progress.gd
godot --headless --path games/number-pyramid --script res://tests/test_scene.gd
```

The [Number Pyramid Web preview workflow](../../.github/workflows/number-pyramid-preview.yml) exports an independent single-threaded **downloadable** WebGL2 ZIP, bundled with an HTTP `serve.py` helper, not a public website. Download and extract the entire Actions artifact; from inside the extracted folder, run `python3 serve.py` and visit `http://127.0.0.1:8000/`. `python3 serve.py --smoke` verifies the assets. Opening `index.html` through `file://` can cause NetworkError.

## Research status and failure criterion

**Testable advantage:** Can large visual sum stones make short arithmetic inference engaging without speed pressure? **Strongest criticism:** Without scaffolded deduction explanations, this may feel like arithmetic guesswork rather than an enjoyable logic game. These are small original puzzle fixtures and their scripted tests establish narrow rule/content correctness, **not** that actual players 45+ understand, enjoy or return, that the levels are difficulty-calibrated, or that non-branded App Store search demand exists. Test on physical phones and against direct mature incumbents before adding content.

See [independent visual direction](DESIGN.md), [ten-game research and explicit correctness caveats](../../docs/research/game-experiments-23-to-32-2026-11-22.md) and [browser/network troubleshooting](../../docs/web-networkerror.md).
