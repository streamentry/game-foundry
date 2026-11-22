# Odd Bloom — the different-flower playtest

**Status:** original, independent Godot **4.7.2** game prototype for adult puzzle players; not a released game or validated global market opportunity.

## Play

1. Open `games/odd-bloom/project.godot` in the standard Godot editor and press **F5**.
2. See a grid of 6, 9, 12 or 16 **original vector-drawn flowers**. Exactly one differs structurally from all the others.
3. Tap the different bloom. Incorrect picks get an informative message and a subtle mark; no timer, punishment, ads or forced streaks.
4. After finding the answer, tap **Next blooms** to unlock the next of twelve deterministic rounds. **Try this pattern again** resets the same board; prior rounds remain reachable.
5. Local `user://` save keeps the active round, previous choices and unlocked progress. Malformed saved values are rejected.

Hosted Web builds support reproducible challenge links such as `?round=6` and a **Copy this challenge** control. No public hosting, share attribution, analytics or backend is included. A localhost or downloaded artifact URL cannot recruit distant players.

## Verify

```bash
godot --headless --path games/odd-bloom --editor --import --quit
godot --headless --path games/odd-bloom --script res://tests/test_patterns.gd
godot --headless --path games/odd-bloom --script res://tests/test_progress.gd
godot --headless --path games/odd-bloom --script res://tests/test_scene.gd
```

The tests verify exactly one non-color structural outlier, deterministic seeds, visual-feature diversity, progression locks, tampered save rejection and first-to-next round scene interaction.

The independent [Odd Bloom Web Playtest workflow](../../.github/workflows/odd-bloom-preview.yml) exports a **downloadable** WebGL2 artifact. After extraction, run `python3 -m http.server 8000` and open `http://localhost:8000` in a WebGL2-compatible browser. Do **not** open the generated `index.html` via `file://`.



**New artifacts contain `serve.py`:** Extract the entire ZIP, open a terminal in the same directory as `index.html`, run `python3 serve.py`, and visit `http://127.0.0.1:8000/`. Do not open the HTML file directly with `file://`. If loading fails, run `python3 serve.py --smoke` and follow [the browser NetworkError guide](../../docs/web-networkerror.md). Actions artifacts are downloads, not live websites.

## Research, quality and constraints

This mechanic has many established competitors. Immediate comprehension ≠ sustainable replay; don't scale content without observing real players. Structural distinctions avoid color-only "gotchas" but still require vision. Native accessibility, physical-device contrast and text scaling, adult-45+ fit, App Store search demand, referral outcomes, return and monetization **remain unverified**.

See the [standalone visual design](DESIGN.md) and [next-two decision brief](../../docs/research/next-two-game-experiments-2026-11-22.md).
