# Cage Hearth — independent arithmetic cages (KenKen-style) puzzle playtest

**Game Foundry prototype 17 · GF-023 · Godot 4.7.2**. User-requested engineering experiment, not an App Store release or proven business.

## Play

Fill 1–4 once per row and column while solving connected cages labeled with sums, products, differences, exact quotients or single values.

**Controls:** Tap a colored cage cell, enter 1–4 on the large keypad, or use Undo, Erase and Restart. Each cage includes a visible arithmetic target. There are six original uniquely solvable 4×4 arithmetic-cage grids, with no timer, forced hints, login, analytics, ads or network backend. Solve a level to unlock the next; local versioned JSON stores the current level and entries.

**Start:** open `games/cage-hearth/project.godot` with the standard Godot 4.7.2 editor; press **F5**.

## Verify the independent rules, save and real scene

```bash
godot --headless --path games/cage-hearth --editor --import --quit
godot --headless --path games/cage-hearth --script res://tests/test_rules.gd
godot --headless --path games/cage-hearth --script res://tests/test_progress.gd
godot --headless --path games/cage-hearth --script res://tests/test_scene.gd
```

The solver is required to show **exactly one valid solution** on every board; scene tests solve the first board using real tap/keypad actions. This establishes rule correctness only, not optimal difficulty or real-device usability.

[Cage Hearth Web Preview](../../.github/workflows/cage-hearth-preview.yml) exports a *downloadable* single-threaded WebGL2 ZIP. Download and fully extract the artifact from a green Actions run; run `python3 serve.py` in the extracted folder and visit **http://127.0.0.1:8000/** on the same computer. `python3 serve.py --smoke` checks all four binary assets over HTTP. Direct `file://` can trigger NetworkError. The downloadable ZIP is not a public HTTPS playtest link. Hosted builds support `?level=3` and a copy-challenge button.

## Evidence and stop rule

**Testable advantage:** Warm, visually grouped cages help players parse arithmetic constraints without a separate instructions screen. **Strongest objection:** Arithmetic puzzles have incumbents and may overlap the studio's Futoshiki/Kakuro audiences; a new title alone adds no differentiated distribution. Real iOS/Android visual/contrast/VoiceOver QA, age-45+ preferences, voluntary return, direct competitor comparison, official search demand, organic installs and conversion **remain unverified**. One measured real-player comparison is worth more than creating 100 extra levels.

See [standalone design system](DESIGN.md), [research decision and stopping gates](../../docs/research/game-experiments-13-to-17-2026-11-22.md), and [browser troubleshooting](../../docs/web-networkerror.md).
