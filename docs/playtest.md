# Browser playtest — from code to real user feedback

**Purpose:** share an original, verified game vertical slice with a few players without iOS signing, Android certificates, login, analytics SDKs or publishing to a store.

## Produce a preview

- Use [GitHub Actions → Browser Playtest Build](https://github.com/streamentry/game-foundry/actions/workflows/web-preview.yml) and select **Run workflow**, or open a PR touching `games/takuzu-lab/`.
- CI imports the Godot 4.7.2 project, runs rules/uniqueness/save/scene tests, installs the **official matching export template** with a SHA-512 checksum verification and exports a **single-threaded** WebGL2 build.
- Download the `takuzu-lab-web-playtest` artifact from the successful workflow run (retained for 7 days). It includes the exported `index.html`, `index.js`, `index.wasm`, `index.pck`, build provenance and checksums.
- Extract the artifact and host it locally:

  ```bash
  cd path/to/extracted/artifact
  python3 -m http.server 8000 --bind 127.0.0.1
  ```

  Open `http://127.0.0.1:8000/index.html` in a WebGL2-capable browser. Do not double-click the HTML file and expect WASM to load over `file://`.

**This workflow creates a downloadable build only.** It does **not** deploy a website, create a public playable URL, collect player analytics, or submit anything to an App Store. Any external hosting needs an explicitly approved destination and privacy review.

## First five player sessions (fast qualitative signal)

Recruit people who already enjoy small logic puzzles—not only your friends or colleagues. Don't explain the controls before watching.

Record one row per participant, without collecting unnecessary identifying details:

| Field | Observation |
| --- | --- |
| Scenario | Daily 4×4 or new 6×6, device/browser |
| Time to first correct move | Measured seconds, not recalled impression |
| Could state the rules | Yes / no; exact misunderstanding |
| Number of mis-taps or layout failures | Observed count |
| Completed a puzzle without assistance | Yes / no |
| Used a hint, undo, restart | Observed |
| Wants to play another level | Asked behavior, not just courtesy |
| Main friction | Verbatim description |
| Would recommend / return | Stated intent **not proof** of actual retention |

Ask **what they expected to happen**, not whether they like the colors. Watch for forced sideways scrolling on narrow phones, focus/touch conflicts, unclear clue status and impossible puzzle frustration.

## Decision gate (provisional)

- **Engineering:** no invalid/unsolvable puzzle reported; progress survives refresh; all tap targets work on phones and desktop.
- **Usability:** at least **4 of 5** first-time puzzle players can make a valid first move without help (a small-sample heuristic, not a population estimate).
- **Product:** player asks for more puzzles unprompted or voluntarily starts another round; compliments are insufficient.
- **Distribution:** this playtest says nothing about App Store demand or organic ranking. Keep [market research](research/discovery-playbook.md) separate and do not ship Takuzu solely because the demo works.

**Stop/repair:** if users don't understand the first interaction or the browser crashes, fix that before adding levels, streaks, ads or a second game. If users love the prototype but non-branded search demand is weak, test alternate positioning and discovery before making 20 more puzzles.

## Technical caveats

Godot Web exports use WebAssembly and WebGL2; browsers without adequate graphics support may fail. Thread support is disabled to avoid cross-origin isolation headers, at a possible cost to performance. The local save uses Godot's `user://` storage mapped to browser storage and is **not guaranteed durable across incognito mode, cache clearing or storage eviction**. This is not a native iOS/Android distribution test.

References: [Godot 4.7 Web export](https://docs.godotengine.org/en/4.7/tutorials/export/exporting_for_web.html) · [Godot export automation](https://docs.godotengine.org/en/4.7/tutorials/export/exporting_projects.html).


## Good Eye: first global visual experiment

Good Eye is a separate, original Godot project under `games/good-eye/`. Its [design](../games/good-eye/DESIGN.md), [run instructions](../games/good-eye/README.md) and [market test plan](research/global-45plus-top-three-games-2026-11-21.md) are independent from Takuzu Lab.

- Trigger **GitHub Actions → Good Eye Web Playtest** via `workflow_dispatch` or a PR affecting `games/good-eye/`.
- Download the `good-eye-web-playtest` artifact from a successful run, extract it and serve it over localhost with the same Python HTTP server command above.
- When hosted as an actual HTTP(S) page, `?seed=12345` opens a reproducible practice silhouette. The Share button prepares a URL with the current seed where browser clipboard access permits. A localhost link does not recruit another person.
- For initial research, compare first-play comprehension and optional replay with a direct 50/50 competitor using real phones. Do not label the artifact public, production-ready or clinically beneficial.
- The GitHub workflow saves an artifact only. Do **not** assume a public challenge domain, App Store app, real-world referral metrics or physical-device accessibility checks exists.


## Shikaku Garden — rectangle logic pilot

The independent [Shikaku Garden prototype](../games/shikaku-garden/README.md) can be exported with [Shikaku Garden Web Playtest](../.github/workflows/shikaku-garden-preview.yml) to a downloadable artifact. To run it, extract it, serve over local HTTP and open in a WebGL2 browser. No public deployment, iOS signing, app-store publication or analytics is performed.

Watch the first tap and second-corner intent, note number/area confusion, record puzzle completion versus assistance and compare optional replay with a direct Shikaku competitor. The authored levels must pass uniqueness tests before recruitment. Do not treat a completed rectangle as a proven paid-product feature.
