# Nurikabe Bay — independent Nurikabe puzzle playtest

**Game Foundry prototype 13 · GF-010 · Godot 4.7.2**. User-requested engineering experiment, not an App Store release or proven business.

## Play

Classify every square as sea or island. Numbered islands have exact sizes; each island contains one clue, the sea is connected, and there can be no 2×2 all-sea block.

**Controls:** Tap a blank cell to cycle SEA → LAND → EMPTY. Fixed clue cells are uneditable. There are six original uniquely solvable 4×4 bay puzzles, with no timer, forced hints, login, analytics, ads or network backend. Solve a level to unlock the next; local versioned JSON stores the current level and entries.

**Start:** open `games/nurikabe-bay/project.godot` with the standard Godot 4.7.2 editor; press **F5**.

## Verify the independent rules, save and real scene

```bash
godot --headless --path games/nurikabe-bay --editor --import --quit
godot --headless --path games/nurikabe-bay --script res://tests/test_rules.gd
godot --headless --path games/nurikabe-bay --script res://tests/test_progress.gd
godot --headless --path games/nurikabe-bay --script res://tests/test_scene.gd
```

The solver is required to show **exactly one valid solution** on every board; scene tests solve the first board using real tap/keypad actions. This establishes rule correctness only, not optimal difficulty or real-device usability.

[Nurikabe Bay Web Preview](../../.github/workflows/nurikabe-bay-preview.yml) exports a *downloadable* single-threaded WebGL2 ZIP. Download and fully extract the artifact from a green Actions run; run `python3 serve.py` in the extracted folder and visit **http://127.0.0.1:8000/** on the same computer. `python3 serve.py --smoke` checks all four binary assets over HTTP. Direct `file://` can trigger NetworkError. The downloadable ZIP is not a public HTTPS playtest link. Hosted builds support `?level=3` and a copy-challenge button.

## Evidence and stop rule

**Testable advantage:** A live explanation of sea connectivity and island area makes the first board clearer than a classic minimalist Nurikabe app. **Strongest objection:** The genre is established; small grids and pretty shores are not a reason to switch or return. Real iOS/Android visual/contrast/VoiceOver QA, age-45+ preferences, voluntary return, direct competitor comparison, official search demand, organic installs and conversion **remain unverified**. One measured real-player comparison is worth more than creating 100 extra levels.

See [standalone design system](DESIGN.md), [research decision and stopping gates](../../docs/research/game-experiments-13-to-17-2026-11-22.md), and [browser troubleshooting](../../docs/web-networkerror.md).
