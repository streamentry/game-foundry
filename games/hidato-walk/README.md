# Hidato Walk — independent Hidato puzzle playtest

**Game Foundry prototype 14 · GF-013 · Godot 4.7.2**. User-requested engineering experiment, not an App Store release or proven business.

## Play

Place numbers 1–16 exactly once so every consecutive pair touches horizontally, vertically or diagonally, while preserving given numbers.

**Controls:** Tap an editable cell, choose a number on the 4×4 keypad, or use Undo, Erase and Restart. There are six original uniquely solvable 4×4 walking paths, with no timer, forced hints, login, analytics, ads or network backend. Solve a level to unlock the next; local versioned JSON stores the current level and entries.

**Start:** open `games/hidato-walk/project.godot` with the standard Godot 4.7.2 editor; press **F5**.

## Verify the independent rules, save and real scene

```bash
godot --headless --path games/hidato-walk --editor --import --quit
godot --headless --path games/hidato-walk --script res://tests/test_rules.gd
godot --headless --path games/hidato-walk --script res://tests/test_progress.gd
godot --headless --path games/hidato-walk --script res://tests/test_scene.gd
```

The solver is required to show **exactly one valid solution** on every board; scene tests solve the first board using real tap/keypad actions. This establishes rule correctness only, not optimal difficulty or real-device usability.

[Hidato Walk Web Preview](../../.github/workflows/hidato-walk-preview.yml) exports a *downloadable* single-threaded WebGL2 ZIP. Download and fully extract the artifact from a green Actions run; run `python3 serve.py` in the extracted folder and visit **http://127.0.0.1:8000/** on the same computer. `python3 serve.py --smoke` checks all four binary assets over HTTP. Direct `file://` can trigger NetworkError. The downloadable ZIP is not a public HTTPS playtest link. Hosted builds support `?level=3` and a copy-challenge button.

## Evidence and stop rule

**Testable advantage:** A generous number pad and explicit diagonal adjacency feedback reduce Hidato's first-entry friction. **Strongest objection:** Sixteen buttons plus sixteen cells may require scrolling on small screens; only actual device observation can confirm comfort. Real iOS/Android visual/contrast/VoiceOver QA, age-45+ preferences, voluntary return, direct competitor comparison, official search demand, organic installs and conversion **remain unverified**. One measured real-player comparison is worth more than creating 100 extra levels.

See [standalone design system](DESIGN.md), [research decision and stopping gates](../../docs/research/game-experiments-13-to-17-2026-11-22.md), and [browser troubleshooting](../../docs/web-networkerror.md).
