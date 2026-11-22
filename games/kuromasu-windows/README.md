# Kuromasu Windows — standalone original Kuromasu / Kurodoko playtest

**Game Foundry prototype 22 · GF-028 · Godot 4.7.2**. An unvalidated engineering experiment, not a store release or an approved 45+ opportunity.

## How to play

Shade black cells so black cells never share an edge, all white cells stay connected, and each numbered white square sees exactly its indicated white squares in its row and column.

**Controls:** Tap a blank square black → white → unknown. Numbered clues remain fixed white; text reports sight-line and connectivity violations. Includes **6 original uniquely determined 4×4 visibility boards**, offline-only versioned saved progress, Undo/Restart, no timer, ads, account, analytics, purchases or backend. A solved board unlocks the next board.

## Run and verify

Open `games/kuromasu-windows/project.godot` in Godot 4.7.2 stable; press **F5**. Or execute:

```bash
godot --headless --path games/kuromasu-windows --editor --import --quit
godot --headless --path games/kuromasu-windows --script res://tests/test_rules.gd
godot --headless --path games/kuromasu-windows --script res://tests/test_progress.gd
godot --headless --path games/kuromasu-windows --script res://tests/test_scene.gd
```

**Correctness:** independent exact-rule backtracking/brute-force solution-count tests verify one distinct legal solution per original board. UI smoke tests enter a full first-level solution via the player input route; save tests reject malformed local state. This does **not** establish human difficulty calibration.

The [Kuromasu Windows Web Playtest workflow](../../.github/workflows/kuromasu-windows-preview.yml) exports one **downloadable** WebGL2 ZIP per successful run—not public hosting. Fully extract it, run `python3 serve.py`, then open **http://127.0.0.1:8000/** on that computer. Run `python3 serve.py --smoke` to verify assets load over HTTP. Direct `file://` opening may cause NetworkError. A hosted build can interpret `?level=4` challenge URLs; no public HTTPS deployment is included.

## Experimental gate

**Hypothesis:** Do visible white/black labels clarify sight-line deduction more than abstract shading? **Opposition:** A full white/black/unknown mark per square adds tapping and could be tiring; readability must be tested on phones. Real-device legibility, VoiceOver/TalkBack, age-45+ preference, official Apple keyword popularity, acquisition, retention, payment and commercial viability remain **unmeasured**.

See the [standalone visual design](DESIGN.md), [research decision and stop condition](../../docs/research/game-experiments-18-to-22-2026-11-22.md), and [browser troubleshooting](../../docs/web-networkerror.md).
