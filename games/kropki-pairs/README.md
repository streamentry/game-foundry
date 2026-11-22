# Kropki Pairs — standalone original Kropki playtest

**Game Foundry prototype 19 · GF-020 · Godot 4.7.2**. An unvalidated engineering experiment, not a store release or an approved 45+ opportunity.

## How to play

Fill 1–4 once per row and column; white dots mean consecutive digits, black dots exact 2:1 ratios. Undotted edges are unconstrained.

**Controls:** Tap an editable square, then a digit on the large four-key pad. In-square →W and ↓B labels plus an inspector identify dot relations. Includes **6 unique original 4×4 positive-dot Latin boards**, offline-only versioned saved progress, Undo/Restart, no timer, ads, account, analytics, purchases or backend. A solved board unlocks the next board.

## Run and verify

Open `games/kropki-pairs/project.godot` in Godot 4.7.2 stable; press **F5**. Or execute:

```bash
godot --headless --path games/kropki-pairs --editor --import --quit
godot --headless --path games/kropki-pairs --script res://tests/test_rules.gd
godot --headless --path games/kropki-pairs --script res://tests/test_progress.gd
godot --headless --path games/kropki-pairs --script res://tests/test_scene.gd
```

**Correctness:** independent exact-rule backtracking/brute-force solution-count tests verify one distinct legal solution per original board. UI smoke tests enter a full first-level solution via the player input route; save tests reject malformed local state. This does **not** establish human difficulty calibration.

The [Kropki Pairs Web Playtest workflow](../../.github/workflows/kropki-pairs-preview.yml) exports one **downloadable** WebGL2 ZIP per successful run—not public hosting. Fully extract it, run `python3 serve.py`, then open **http://127.0.0.1:8000/** on that computer. Run `python3 serve.py --smoke` to verify assets load over HTTP. Direct `file://` opening may cause NetworkError. A hosted build can interpret `?level=4` challenge URLs; no public HTTPS deployment is included.

## Experimental gate

**Hypothesis:** Do explicit directional dot labels make ratio/consecutive reasoning more legible than tiny colored dots? **Opposition:** This beginner positive-dot-only variant simplifies classic full Kropki, and dense clue labels can crowd a small phone. Real-device legibility, VoiceOver/TalkBack, age-45+ preference, official Apple keyword popularity, acquisition, retention, payment and commercial viability remain **unmeasured**.

See the [standalone visual design](DESIGN.md), [research decision and stop condition](../../docs/research/game-experiments-18-to-22-2026-11-22.md), and [browser troubleshooting](../../docs/web-networkerror.md).
