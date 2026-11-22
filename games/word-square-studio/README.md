# Word Square Studio — original independent symmetric word squares playtest

**Game Foundry prototype 25 · GF-038 · Godot 4.7.2**, purely a research implementation. No app-store production approval or business validation.

## Game

Arrange three 3-letter English words so each vertical column matches the corresponding horizontal row.

**Interaction:** Tap the unplaced word cards in top-to-bottom order; the board shows row positions immediately. **Content:** 6 original word-card arrangements with a single valid permutation each. Each original puzzle is playable on a standalone Godot project with offline saved progress, Undo, Restart, Next, no timer, account, ads, payments or analytics SDK.

## Build and test locally

Open `games/word-square-studio/project.godot` in Godot 4.7.2 standard editor and press F5. For deterministic verification:

```bash
godot --headless --path games/word-square-studio --editor --import --quit
godot --headless --path games/word-square-studio --script res://tests/test_rules.gd
godot --headless --path games/word-square-studio --script res://tests/test_progress.gd
godot --headless --path games/word-square-studio --script res://tests/test_scene.gd
```

The [Word Square Studio Web preview workflow](../../.github/workflows/word-square-studio-preview.yml) exports an independent single-threaded **downloadable** WebGL2 ZIP, bundled with an HTTP `serve.py` helper, not a public website. Download and extract the entire Actions artifact; from inside the extracted folder, run `python3 serve.py` and visit `http://127.0.0.1:8000/`. `python3 serve.py --smoke` verifies the assets. Opening `index.html` through `file://` can cause NetworkError.

## Research status and failure criterion

**Testable advantage:** Can a three-word symmetry puzzle create a fast satisfying first-play loop without word-definition hints? **Strongest criticism:** Three-word puzzles may be too trivial for practiced word-game players without adding larger verified word squares. These are small original puzzle fixtures and their scripted tests establish narrow rule/content correctness, **not** that actual players 45+ understand, enjoy or return, that the levels are difficulty-calibrated, or that non-branded App Store search demand exists. Test on physical phones and against direct mature incumbents before adding content.

See [independent visual direction](DESIGN.md), [ten-game research and explicit correctness caveats](../../docs/research/game-experiments-23-to-32-2026-11-22.md) and [browser/network troubleshooting](../../docs/web-networkerror.md).
