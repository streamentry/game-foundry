# Browser playtest — from code to real user feedback

**Purpose:** share an original, verified game vertical slice with a few players without iOS signing, Android certificates, login, analytics SDKs or publishing to a store.

## Produce a preview

- Use [GitHub Actions → Browser Playtest Build](https://github.com/streamentry/game-foundry/actions/workflows/web-preview.yml) and select **Run workflow**, or open a PR touching `games/takuzu-lab/`.
- CI imports the Godot 4.7.2 project, runs rules/uniqueness/save/scene tests, installs the **official matching export template** with a SHA-512 checksum verification, exports a **single-threaded** WebGL2 build, and smoke-tests every exported asset over loopback HTTP.
- Download the `takuzu-lab-web-playtest` artifact from the successful workflow run (retained for 7 days). It includes the exported `index.html`, `index.js`, `index.wasm`, `index.pck`, build provenance and checksums.
- Extract the artifact and host it locally:

  ```bash
  cd path/to/extracted/artifact
  python3 serve.py
  ```

  Open `http://127.0.0.1:8000/` in a WebGL2-capable browser. **New artifacts** include the local `serve.py` launcher, which checks all required files and optional SHA-256 hashes. Run `python3 serve.py --smoke` to test the same asset URLs the browser requests. Older artifacts without `serve.py` can still be run with `python3 -m http.server 8000 --bind 127.0.0.1`. Do not double-click `index.html` or load it via `file://`. See [the NetworkError troubleshooting guide](web-networkerror.md).

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


## Odd Bloom — one-tap visual comparison pilot

The independent [Odd Bloom prototype](../games/odd-bloom/README.md) has a downloadable WebGL2 artifact via [Odd Bloom Web Playtest](../.github/workflows/odd-bloom-preview.yml). The artifact is not a public playable URL; extract and serve over local HTTP to test.

Try six-target rounds before introducing the denser 4×4 grid. Record obvious vs genuinely subtle differences, accidental taps, whether older players can comfortably see the petal/center contrasts, and which game they voluntarily continue playing when compared with an existing odd-one-out competitor. Do not assume non-color structural differences automatically make the game accessible to every player.


## Kakuro Compass — guided intersection comparison

The [standalone Kakuro Compass prototype](../games/kakuro-compass/README.md) has eight authored unique-solution cross-sum boards and a persistent explanation of locally feasible digits for both intersecting clue runs. [Kakuro Compass Web Playtest](../.github/workflows/kakuro-compass-preview.yml) exports the complete WebGL2 artifact with `serve.py`, not a live link.

Compare against a current Kakuro incumbent **on actual player phones**: can players discover the first entry, understand the arrows, and use the visible inspector correctly? Does the inspector make the deduction clearer without just giving the answer? All gameplay checks are code tests, not evidence of age-specific usability or preference.


## Quiet Camp — Tents & Trees spatial deduction

The independent [Quiet Camp](../games/quiet-camp/README.md) project has eight uniquely-solvable tent/tree layouts and original vector cells; its [Web Playtest](../.github/workflows/quiet-camp-preview.yml) is a complete downloadable WebGL2 ZIP with `serve.py`, **not a publicly hosted URL**.

Observe players' first tap, awareness of row/column totals, diagonal tent adjacency, and understanding of one tent per tree. Compare with a direct incumbent and watch voluntary additional play, not polite compliments. Verify touch comfort on compact iPhones and Android handsets; CI cannot substitute for that.


## Five additional logic playtests — small comparative study

- **[Futoshiki Terrace](../games/futoshiki-terrace/README.md):** Fill a 4×4 Latin grid using 1–4 exactly once in each row and column, while satisfying directional inequalities. Compare the official browser artifact against a direct incumbent, preferably on participants' own phones. Log actual first-play friction and unaided continuation; an engine test does not establish 45+ fit.
- **[Star Atlas](../games/star-atlas/README.md):** Place exactly one star in each row, column and labeled connected region, without allowing stars to touch even diagonally. Compare the official browser artifact against a direct incumbent, preferably on participants' own phones. Log actual first-play friction and unaided continuation; an engine test does not establish 45+ fit.
- **[Bridge Isles](../games/bridge-isles/README.md):** Connect visible neighboring numbered islands with zero, one or two straight bridges, without crossings, and keep the network connected. Compare the official browser artifact against a direct incumbent, preferably on participants' own phones. Log actual first-play friction and unaided continuation; an engine test does not establish 45+ fit.
- **[Loop Lantern](../games/loop-lantern/README.md):** Draw a single closed nonbranching Slitherlink loop using cell clues that specify how many surrounding edges are selected. Compare the official browser artifact against a direct incumbent, preferably on participants' own phones. Log actual first-play friction and unaided continuation; an engine test does not establish 45+ fit.
- **[Hitori Grove](../games/hitori-grove/README.md):** Shade repeated numbers, ensuring no horizontally or vertically adjacent shaded squares and one connected group of unshaded squares. Compare the official browser artifact against a direct incumbent, preferably on participants' own phones. Log actual first-play friction and unaided continuation; an engine test does not establish 45+ fit.

All five GitHub Actions exports bundle `serve.py`; extract the **whole** archive, run `python3 serve.py` inside its directory, and visit http://127.0.0.1:8000/ on the same computer. Do not open `index.html` directly over `file://`. Actual public hosting and signed mobile builds remain incomplete.


## Prototypes 13–17 — independent controls and comparative playtest

- [Nurikabe Bay](../games/nurikabe-bay/README.md): Tap a blank cell to cycle SEA → LAND → EMPTY. Fixed clue cells are uneditable. Compare with a direct Nurikabe incumbent on actual adult-45+ phones; note unaided first action, visual comfort, a second voluntary round, and later unsponsored return. No onboarding/market claim is supported by an automated test.
- [Hidato Walk](../games/hidato-walk/README.md): Tap an editable cell, choose a number on the 4×4 keypad, or use Undo, Erase and Restart. Compare with a direct Hidato incumbent on actual adult-45+ phones; note unaided first action, visual comfort, a second voluntary round, and later unsponsored return. No onboarding/market claim is supported by an automated test.
- [Skyline Steps](../games/skyline-steps/README.md): Select a square and enter 1–4; outside edge clues are displayed around the grid. Undo, Erase and Restart are always available. Compare with a direct Skyscrapers incumbent on actual adult-45+ phones; note unaided first action, visual comfort, a second voluntary round, and later unsponsored return. No onboarding/market claim is supported by an automated test.
- [Thermometer Studio](../games/thermometer-studio/README.md): Tap a bulb to fill its first segment or a tip to fill both; tap the same target again to empty the tube. Undo and Restart recover mistakes. Compare with a direct Thermometers incumbent on actual adult-45+ phones; note unaided first action, visual comfort, a second voluntary round, and later unsponsored return. No onboarding/market claim is supported by an automated test.
- [Cage Hearth](../games/cage-hearth/README.md): Tap a colored cage cell, enter 1–4 on the large keypad, or use Undo, Erase and Restart. Each cage includes a visible arithmetic target. Compare with a direct arithmetic cages (KenKen-style) incumbent on actual adult-45+ phones; note unaided first action, visual comfort, a second voluntary round, and later unsponsored return. No onboarding/market claim is supported by an automated test.

Each downloadable WebGL2 ZIP includes `serve.py` and verified HTTP `.wasm`/`.pck` delivery; the ZIP is **not** public hosting. Never open the extracted HTML through `file://`. Actual phone/assistive-technology tests remain open.


## Five new logic studies, prototypes 18–22

- [Numberlink Paths](../games/numberlink-paths/README.md): Tap blank cells A → B → empty; fixed endpoints are marked with circles, and each route has its own letter and color. **Compare against a direct Numberlink alternative** on actual target-player phones and record first unassisted correct action, friction, voluntary second puzzle and any next-day unsponsored return.
- [Kropki Pairs](../games/kropki-pairs/README.md): Tap an editable square, then a digit on the large four-key pad. In-square →W and ↓B labels plus an inspector identify dot relations. **Compare against a direct Kropki alternative** on actual target-player phones and record first unassisted correct action, friction, voluntary second puzzle and any next-day unsponsored return.
- [Ripple Rooms](../games/ripple-rooms/README.md): Tap a square and enter 1–4 using large buttons; each cell displays its room letter and size alongside the number. **Compare against a direct Ripple Effect alternative** on actual target-player phones and record first unassisted correct action, friction, voluntary second puzzle and any next-day unsponsored return.
- [Battleship Harbor](../games/battleship-harbor/README.md): Tap a square ship → optional water note → blank; current/target row and column totals remain visible; only six ship cells need placing. **Compare against a direct Battleships logic alternative** on actual target-player phones and record first unassisted correct action, friction, voluntary second puzzle and any next-day unsponsored return.
- [Kuromasu Windows](../games/kuromasu-windows/README.md): Tap a blank square black → white → unknown. Numbered clues remain fixed white; text reports sight-line and connectivity violations. **Compare against a direct Kuromasu / Kurodoko alternative** on actual target-player phones and record first unassisted correct action, friction, voluntary second puzzle and any next-day unsponsored return.

Each per-game workflow produces a full downloadable WebGL2 ZIP with `serve.py` and verified HTTP asset loading. None is publicly hosted or physically phone-verified. Extract the entire ZIP first; opening `index.html` via `file://` may produce a NetworkError.


## Ten new prototype playtests (23–32)

- [Renzoku Meadow](../games/renzoku-meadow/README.md): Select a square, enter a digit 1–4; in-cell arrows identify marked right/below edges. Compare unassisted clarity and voluntary additional play to a direct consecutive Latin squares alternative on a real phone.
- [Word Ladder Lane](../games/word-ladder-lane/README.md): Tap an eligible next-word card; each move shows the current ladder, goal and step count. Compare unassisted clarity and voluntary additional play to a direct four-letter word transformations alternative on a real phone.
- [Word Square Studio](../games/word-square-studio/README.md): Tap the unplaced word cards in top-to-bottom order; the board shows row positions immediately. Compare unassisted clarity and voluntary additional play to a direct symmetric word squares alternative on a real phone.
- [Vowel Garden](../games/vowel-garden/README.md): Tap a visible underscore, then the large A/E/I/O/U keypad. Undo, Erase and Restart recover choices. Compare unassisted clarity and voluntary additional play to a direct missing-vowel phrase restoration alternative on a real phone.
- [Word Search Shores](../games/word-search-shores/README.md): Tap the first and last letters of a target word, in either direction; found words become visibly highlighted. Compare unassisted clarity and voluntary additional play to a direct offline word search alternative on a real phone.
- [Nonogram Canvas](../games/nonogram-canvas/README.md): Tap a cell filled → crossed empty → unknown. Ten row/column clue lines stay visible. Compare unassisted clarity and voluntary additional play to a direct picture cross / Nonogram alternative on a real phone.
- [Akari Evening](../games/akari-evening/README.md): Tap an open cell to cycle lamp → cross note → empty; the board highlights lit cells. Compare unassisted clarity and voluntary additional play to a direct Light Up / Akari alternative on a real phone.
- [Pipe Garden](../games/pipe-garden/README.md): Tap a tile to rotate; curved and end pieces have four orientations, straights have two. Compare unassisted clarity and voluntary additional play to a direct pipe rotation alternative on a real phone.
- [Sliding Tiles](../games/sliding-tiles/README.md): Tap only a tile adjacent to the blank; Undo and Restart are always available. Compare unassisted clarity and voluntary additional play to a direct 8-puzzle sliding blocks alternative on a real phone.
- [Number Pyramid](../games/number-pyramid/README.md): Select a bottom stone and enter a digit 1–9; upper stones show live derived sums and fixed clue labels. Compare unassisted clarity and voluntary additional play to a direct number sum pyramid alternative on a real phone.

The ten independent downloadable WebGL2 builds include an HTTP `serve.py` smoke test; these are **not publicly deployed**, signed app-store releases or age-45+ user studies. Their seven-day GitHub Actions retention may expire: preserve findings in research docs, not in ephemeral download URLs.
