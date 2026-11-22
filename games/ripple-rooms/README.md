# Ripple Rooms — standalone original Ripple Effect playtest

**Game Foundry prototype 20 · GF-022 · Godot 4.7.2**. An unvalidated engineering experiment, not a store release or an approved 45+ opportunity.

## How to play

Fill irregular connected rooms with 1 through the room size, ensuring any repeated k in the same row or column has at least k cells between it.

**Controls:** Tap a square and enter 1–4 using large buttons; each cell displays its room letter and size alongside the number. Includes **6 original irregular-room 4×4 unique puzzles**, offline-only versioned saved progress, Undo/Restart, no timer, ads, account, analytics, purchases or backend. A solved board unlocks the next board.

## Run and verify

Open `games/ripple-rooms/project.godot` in Godot 4.7.2 stable; press **F5**. Or execute:

```bash
godot --headless --path games/ripple-rooms --editor --import --quit
godot --headless --path games/ripple-rooms --script res://tests/test_rules.gd
godot --headless --path games/ripple-rooms --script res://tests/test_progress.gd
godot --headless --path games/ripple-rooms --script res://tests/test_scene.gd
```

**Correctness:** independent exact-rule backtracking/brute-force solution-count tests verify one distinct legal solution per original board. UI smoke tests enter a full first-level solution via the player input route; save tests reject malformed local state. This does **not** establish human difficulty calibration.

The [Ripple Rooms Web Playtest workflow](../../.github/workflows/ripple-rooms-preview.yml) exports one **downloadable** WebGL2 ZIP per successful run—not public hosting. Fully extract it, run `python3 serve.py`, then open **http://127.0.0.1:8000/** on that computer. Run `python3 serve.py --smoke` to verify assets load over HTTP. Direct `file://` opening may cause NetworkError. A hosted build can interpret `?level=4` challenge URLs; no public HTTPS deployment is included.

## Experimental gate

**Hypothesis:** Can room ID and size labels make spatial number-spacing deductions clear without zoom? **Opposition:** An additional room label on every tile creates visual noise and may not be needed after the first few puzzles. Real-device legibility, VoiceOver/TalkBack, age-45+ preference, official Apple keyword popularity, acquisition, retention, payment and commercial viability remain **unmeasured**.

See the [standalone visual design](DESIGN.md), [research decision and stop condition](../../docs/research/game-experiments-18-to-22-2026-11-22.md), and [browser troubleshooting](../../docs/web-networkerror.md).
