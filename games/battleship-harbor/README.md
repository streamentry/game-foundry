# Battleship Harbor — standalone original Battleships logic playtest

**Game Foundry prototype 21 · GF-026 · Godot 4.7.2**. An unvalidated engineering experiment, not a store release or an approved 45+ opportunity.

## How to play

Find a fleet of exactly one length-3, one length-2, and one single-cell ship on a 5×5 chart, with no ship touching another even diagonally and exact line totals.

**Controls:** Tap a square ship → optional water note → blank; current/target row and column totals remain visible; only six ship cells need placing. Includes **6 original 5×5 unique fleet layouts**, offline-only versioned saved progress, Undo/Restart, no timer, ads, account, analytics, purchases or backend. A solved board unlocks the next board.

## Run and verify

Open `games/battleship-harbor/project.godot` in Godot 4.7.2 stable; press **F5**. Or execute:

```bash
godot --headless --path games/battleship-harbor --editor --import --quit
godot --headless --path games/battleship-harbor --script res://tests/test_rules.gd
godot --headless --path games/battleship-harbor --script res://tests/test_progress.gd
godot --headless --path games/battleship-harbor --script res://tests/test_scene.gd
```

**Correctness:** independent exact-rule backtracking/brute-force solution-count tests verify one distinct legal solution per original board. UI smoke tests enter a full first-level solution via the player input route; save tests reject malformed local state. This does **not** establish human difficulty calibration.

The [Battleship Harbor Web Playtest workflow](../../.github/workflows/battleship-harbor-preview.yml) exports one **downloadable** WebGL2 ZIP per successful run—not public hosting. Fully extract it, run `python3 serve.py`, then open **http://127.0.0.1:8000/** on that computer. Run `python3 serve.py --smoke` to verify assets load over HTTP. Direct `file://` opening may cause NetworkError. A hosted build can interpret `?level=4` challenge URLs; no public HTTPS deployment is included.

## Experimental gate

**Hypothesis:** Can exact fleet rules and optional water notes make a no-guess logic game satisfying in brief sessions? **Opposition:** There are mature fleet-logic products; row/col counters alone establish no competitive preference. Real-device legibility, VoiceOver/TalkBack, age-45+ preference, official Apple keyword popularity, acquisition, retention, payment and commercial viability remain **unmeasured**.

See the [standalone visual design](DESIGN.md), [research decision and stop condition](../../docs/research/game-experiments-18-to-22-2026-11-22.md), and [browser troubleshooting](../../docs/web-networkerror.md).
