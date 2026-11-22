# Vowel Garden — original independent missing-vowel phrase restoration playtest

**Game Foundry prototype 26 · GF-043 · Godot 4.7.2**, purely a research implementation. No app-store production approval or business validation.

## Game

Reconstruct original short phrases by inserting A/E/I/O/U into a visible consonant skeleton using topic clues.

**Interaction:** Tap a visible underscore, then the large A/E/I/O/U keypad. Undo, Erase and Restart recover choices. **Content:** 6 original authored phrases with one accepted canonical restoration each, not proof of linguistic uniqueness. Each original puzzle is playable on a standalone Godot project with offline saved progress, Undo, Restart, Next, no timer, account, ads, payments or analytics SDK.

## Build and test locally

Open `games/vowel-garden/project.godot` in Godot 4.7.2 standard editor and press F5. For deterministic verification:

```bash
godot --headless --path games/vowel-garden --editor --import --quit
godot --headless --path games/vowel-garden --script res://tests/test_rules.gd
godot --headless --path games/vowel-garden --script res://tests/test_progress.gd
godot --headless --path games/vowel-garden --script res://tests/test_scene.gd
```

The [Vowel Garden Web preview workflow](../../.github/workflows/vowel-garden-preview.yml) exports an independent single-threaded **downloadable** WebGL2 ZIP, bundled with an HTTP `serve.py` helper, not a public website. Download and extract the entire Actions artifact; from inside the extracted folder, run `python3 serve.py` and visit `http://127.0.0.1:8000/`. `python3 serve.py --smoke` verifies the assets. Opening `index.html` through `file://` can cause NetworkError.

## Research status and failure criterion

**Testable advantage:** Do short topic-labeled phrases make missing vowels genuinely pleasant for adults with varied vocabulary? **Strongest criticism:** Consonant-only prompts can admit multiple sensible phrases; a single canonical answer is potentially unfair without broader acceptance. These are small original puzzle fixtures and their scripted tests establish narrow rule/content correctness, **not** that actual players 45+ understand, enjoy or return, that the levels are difficulty-calibrated, or that non-branded App Store search demand exists. Test on physical phones and against direct mature incumbents before adding content.

See [independent visual direction](DESIGN.md), [ten-game research and explicit correctness caveats](../../docs/research/game-experiments-23-to-32-2026-11-22.md) and [browser/network troubleshooting](../../docs/web-networkerror.md).
