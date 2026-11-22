# Word Ladder Lane — original independent four-letter word transformations playtest

**Game Foundry prototype 24 · GF-031 · Godot 4.7.2**, purely a research implementation. No app-store production approval or business validation.

## Game

Change exactly one letter at a time, using only available word cards, to reach a given goal in an exact shortest move budget.

**Interaction:** Tap an eligible next-word card; each move shows the current ladder, goal and step count. **Content:** 6 original small curated word graphs with one accepted shortest path each. Each original puzzle is playable on a standalone Godot project with offline saved progress, Undo, Restart, Next, no timer, account, ads, payments or analytics SDK.

## Build and test locally

Open `games/word-ladder-lane/project.godot` in Godot 4.7.2 standard editor and press F5. For deterministic verification:

```bash
godot --headless --path games/word-ladder-lane --editor --import --quit
godot --headless --path games/word-ladder-lane --script res://tests/test_rules.gd
godot --headless --path games/word-ladder-lane --script res://tests/test_progress.gd
godot --headless --path games/word-ladder-lane --script res://tests/test_scene.gd
```

The [Word Ladder Lane Web preview workflow](../../.github/workflows/word-ladder-lane-preview.yml) exports an independent single-threaded **downloadable** WebGL2 ZIP, bundled with an HTTP `serve.py` helper, not a public website. Download and extract the entire Actions artifact; from inside the extracted folder, run `python3 serve.py` and visit `http://127.0.0.1:8000/`. `python3 serve.py --smoke` verifies the assets. Opening `index.html` through `file://` can cause NetworkError.

## Research status and failure criterion

**Testable advantage:** Can four-letter small-lexicon choice ladders reduce frustrating typing while preserving inference? **Strongest criticism:** Limited card dictionaries lower vocabulary burden but may feel too guided; free-entry word ladders have different appeal. These are small original puzzle fixtures and their scripted tests establish narrow rule/content correctness, **not** that actual players 45+ understand, enjoy or return, that the levels are difficulty-calibrated, or that non-branded App Store search demand exists. Test on physical phones and against direct mature incumbents before adding content.

See [independent visual direction](DESIGN.md), [ten-game research and explicit correctness caveats](../../docs/research/game-experiments-23-to-32-2026-11-22.md) and [browser/network troubleshooting](../../docs/web-networkerror.md).
