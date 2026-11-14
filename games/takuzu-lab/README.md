# Takuzu Lab — engine reference, not validated opportunity

Open `project.godot` in Godot **4.7.2 stable** and run the project. A single 4×4 Takuzu puzzle is playable with tap-to-cycle cells, Undo, Restart and live constraint feedback.

The five givens form an independently enumerated **unique solution** among valid 4×4 Takuzu grids (72 completed solutions were enumerated for this test in a separate Python check). This claim does not mean the demo includes a general-purpose puzzle generator. The game has no monetization, native export configuration, persistence, localization or analytics yet.

```bash
godot --headless --path games/takuzu-lab --editor --import --quit
godot --headless --path games/takuzu-lab --script res://tests/test_rules.gd
```

Future actual products should have original design specs and seeded puzzle corpora with measured difficulty.
