# Shikaku Garden — an original rectangle-logic playtest

**Status:** a standalone Godot **4.7.2 stable** prototype, not a launched game. Based on generic Shikaku mathematical rules; eight original numbered boards were generated and audited for exactly one full solution.

## Play

1. Open `games/shikaku-garden/project.godot` in the standard Godot editor and press **F5**.
2. Tap a grid cell to select a **first corner**, then another cell to choose the **opposite corner** of the desired rectangle.
3. A rectangle is accepted only if it is inside the board, overlaps no earlier region, contains **exactly one numbered clue**, and its total number of squares equals that number.
4. Fill the entire board. Use **Undo** and **Restart** freely, with no timer. The next garden unlocks when the rules are satisfied.
5. Partial progress and unlocked levels are saved in versioned local `user://` JSON, with invalid saved rectangles discarded safely.

Hosted Web exports support `?level=3` for a reproducible specific garden; the **Copy this challenge** button prepares the level link if running over HTTP(S). A downloaded build or localhost URL is not a public share link.

## Verification

```bash
godot --headless --path games/shikaku-garden --editor --import --quit
godot --headless --path games/shikaku-garden --script res://tests/test_rules.gd
godot --headless --path games/shikaku-garden --script res://tests/test_progress.gd
godot --headless --path games/shikaku-garden --script res://tests/test_scene.gd
```

The tests verify **eight exact-cover unique solutions**, playable reference rectangles, invalid moves, save/resume and the actual screen. [Shikaku Garden Web Playtest](../../.github/workflows/shikaku-garden-preview.yml) exports the single-threaded WebGL2 build as a downloadable GitHub Actions artifact; it does not publish a website. Extract and serve the artifact using `python3 -m http.server 8000`, then open `http://localhost:8000`.

## Limits and evidence

This is a prototype from the GF-018 research backlog, not measured ASO demand or a production selection. Differences from established Shikaku titles remain hypotheses. Phone ergonomics, target-age comprehension, store acquisition, return, willingness to pay and native iOS/Android signing **are unverified**. Compare user behavior against a direct incumbent before building content automation.

See [original design system](DESIGN.md) and [global research gate](../../docs/research/global-45plus-top-three-games-2026-11-21.md).
