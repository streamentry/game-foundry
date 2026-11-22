# Numberlink Paths — standalone original Numberlink playtest

**Game Foundry prototype 18 · GF-014 · Godot 4.7.2**. An unvalidated engineering experiment, not a store release or an approved 45+ opportunity.

## How to play

Two original orthogonal routes cover an entire 4×4 grid without crossing or branching; each connects a same-letter endpoint pair.

**Controls:** Tap blank cells A → B → empty; fixed endpoints are marked with circles, and each route has its own letter and color. Includes **6 full-coverage two-pair boards with unique route assignments**, offline-only versioned saved progress, Undo/Restart, no timer, ads, account, analytics, purchases or backend. A solved board unlocks the next board.

## Run and verify

Open `games/numberlink-paths/project.godot` in Godot 4.7.2 stable; press **F5**. Or execute:

```bash
godot --headless --path games/numberlink-paths --editor --import --quit
godot --headless --path games/numberlink-paths --script res://tests/test_rules.gd
godot --headless --path games/numberlink-paths --script res://tests/test_progress.gd
godot --headless --path games/numberlink-paths --script res://tests/test_scene.gd
```

**Correctness:** independent exact-rule backtracking/brute-force solution-count tests verify one distinct legal solution per original board. UI smoke tests enter a full first-level solution via the player input route; save tests reject malformed local state. This does **not** establish human difficulty calibration.

The [Numberlink Paths Web Playtest workflow](../../.github/workflows/numberlink-paths-preview.yml) exports one **downloadable** WebGL2 ZIP per successful run—not public hosting. Fully extract it, run `python3 serve.py`, then open **http://127.0.0.1:8000/** on that computer. Run `python3 serve.py --smoke` to verify assets load over HTTP. Direct `file://` opening may cause NetworkError. A hosted build can interpret `?level=4` challenge URLs; no public HTTPS deployment is included.

## Experimental gate

**Hypothesis:** Can letter-first, large-tap path assignment beat precise drag gestures for short play? **Opposition:** Full-board coverage is a variant of Numberlink; tap-to-fill squares may be slower and less tactile than drawing lines. Real-device legibility, VoiceOver/TalkBack, age-45+ preference, official Apple keyword popularity, acquisition, retention, payment and commercial viability remain **unmeasured**.

See the [standalone visual design](DESIGN.md), [research decision and stop condition](../../docs/research/game-experiments-18-to-22-2026-11-22.md), and [browser troubleshooting](../../docs/web-networkerror.md).
