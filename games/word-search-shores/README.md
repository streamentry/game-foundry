# Word Search Shores — original independent offline word search playtest

**Game Foundry prototype 27 · GF-048 · Godot 4.7.2**, purely a research implementation. No app-store production approval or business validation.

## Game

Find three target words in a 6×6 original letter grid by tapping their start and end along a straight horizontal, vertical or diagonal line.

**Interaction:** Tap the first and last letters of a target word, in either direction; found words become visibly highlighted. **Content:** 6 original 6×6 boards; each target word appears exactly once in the accepted straight-line directions. Each original puzzle is playable on a standalone Godot project with offline saved progress, Undo, Restart, Next, no timer, account, ads, payments or analytics SDK.

## Build and test locally

Open `games/word-search-shores/project.godot` in Godot 4.7.2 standard editor and press F5. For deterministic verification:

```bash
godot --headless --path games/word-search-shores --editor --import --quit
godot --headless --path games/word-search-shores --script res://tests/test_rules.gd
godot --headless --path games/word-search-shores --script res://tests/test_progress.gd
godot --headless --path games/word-search-shores --script res://tests/test_scene.gd
```

The [Word Search Shores Web preview workflow](../../.github/workflows/word-search-shores-preview.yml) exports an independent single-threaded **downloadable** WebGL2 ZIP, bundled with an HTTP `serve.py` helper, not a public website. Download and extract the entire Actions artifact; from inside the extracted folder, run `python3 serve.py` and visit `http://127.0.0.1:8000/`. `python3 serve.py --smoke` verifies the assets. Opening `index.html` through `file://` can cause NetworkError.

## Research status and failure criterion

**Testable advantage:** Can explicit endpoint taps make offline word search more comfortable without drag precision? **Strongest criticism:** Direct competitors have much more content; requiring two endpoint taps may be less natural than a continuous swipe for some players. These are small original puzzle fixtures and their scripted tests establish narrow rule/content correctness, **not** that actual players 45+ understand, enjoy or return, that the levels are difficulty-calibrated, or that non-branded App Store search demand exists. Test on physical phones and against direct mature incumbents before adding content.

See [independent visual direction](DESIGN.md), [ten-game research and explicit correctness caveats](../../docs/research/game-experiments-23-to-32-2026-11-22.md) and [browser/network troubleshooting](../../docs/web-networkerror.md).
