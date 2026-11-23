# Good Eye — playable Godot prototype

**Status: original offline-first visual-game prototype for user research; not an App Store launch.** See [experiment evidence](../../docs/research/global-45plus-top-three-games-2026-11-21.md), [design](DESIGN.md) and the [playtest protocol](../../docs/playtest.md).

## Play

1. Open `games/good-eye/project.godot` in **Godot 4.7.2 stable** (standard GDScript) and press **F5**.
2. Tap the large shape where you want the cut line. Use **Move − / Turn − / Turn + / Move +** to fine-tune it. Practice gives a short direction cue. Daily Five keeps its first cut unhinted. On a keyboard use arrows and Enter.
3. Press **Cut shape**. See area A, area B, percentage-point error and an eye score (100 = perfect). Percentages stay hidden before cutting.
4. **Practice** lets you retry an identical seeded shape or advance to a new one. **Daily five** offers five reproducible UTC-day shapes and locks each first-attempt score. Progress saves locally.

A web build accepts `?seed=12345` in its URL to open the same challenge in practice mode. The Share button prepares a seeded link **only when the Web export is hosted**; a local file, copied localhost address, or downloaded GitHub Actions artifact does not automatically provide a public shareable site. Clipboard access depends on host browser permissions. No analytics, account, ads, purchases or backend are included.

## Test

Run from the repository root:

```bash
godot --headless --path games/good-eye --editor --import --quit
godot --headless --path games/good-eye --script res://tests/test_geometry.gd
godot --headless --path games/good-eye --script res://tests/test_state.gd
godot --headless --path games/good-eye --script res://tests/test_scene.gd
```

The main CI also runs these tests. The [Good Eye Web Playtest workflow](../../.github/workflows/good-eye-preview.yml) verifies the matching official export templates, exports a single-threaded WebGL2 build and uploads a downloadable `good-eye-web-playtest` artifact. **It does not publish a website.** To serve the exported build locally, extract it and run `python3 -m http.server 8000 --bind 127.0.0.1` inside the folder; open `http://127.0.0.1:8000/` in a WebGL2-capable browser. Do not use `file://`.



**New artifacts contain `serve.py`:** Extract the entire ZIP, open a terminal in the same directory as `index.html`, run `python3 serve.py`, and visit `http://127.0.0.1:8000/`. Do not open the HTML file directly with `file://`. If loading fails, run `python3 serve.py --smoke` and follow [the browser NetworkError guide](../../docs/web-networkerror.md). Actions artifacts are downloads, not live websites.

## Correctness and limits

Each challenge uses an original seeded convex hull. The infinite line clips the polygon into two areas; score is `100 − 2 × abs(50 − percent_A)`. Percentages use actual clipped areas, not the shape's bounding box. Many cuts can be equally good; there is no unique solution. The UTC date avoids different daily seeds across timezones. Local `user://` data may be lost through browser storage eviction or clearing.

The project is not validated for 45+ desirability, organic acquisition, retention, willingness to pay, physically tested mobile UI, signed native exports or store submission. Compare on real phones with an existing 50/50 game before adding a large generator, monetization or SDKs.
