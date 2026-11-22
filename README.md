# Game Foundry

**Research-first, Godot-powered mobile games.** We don't mass-clone winners. We identify search intent that existing games serve poorly, ship distinctive experiences, and let organic market feedback decide what deserves investment.

## Status — 2026-11-22

- **Engine:** Godot **4.7.2 stable** (pinned; Godot 4.8 is beta).
- **First gated ASO experiment:** **Kakuro**, U.S. adults 50-75 first; U.K. and Japan follow-up hypotheses. [Global ASO recheck](docs/research/kakuro-global-aso-45plus-2026-11-21.md) identifies **direct competitors already claiming explainable hints**. No production greenlight until demand, storefront rank, and comparative player tests pass.
- **Prototype 22:** [Kuromasu Windows](games/kuromasu-windows/) — Shade black cells so black cells never share an edge, all white cells stay connected, and each numbered white square sees exactly its indicated white squares in its row and column. 6 original uniquely determined 4×4 visibility boards; original engineer-tested playtest only. **45+ preference and organic acquisition unmeasured.**
- **Prototype 21:** [Battleship Harbor](games/battleship-harbor/) — Find a fleet of exactly one length-3, one length-2, and one single-cell ship on a 5×5 chart, with no ship touching another even diagonally and exact line totals. 6 original 5×5 unique fleet layouts; original engineer-tested playtest only. **45+ preference and organic acquisition unmeasured.**
- **Prototype 20:** [Ripple Rooms](games/ripple-rooms/) — Fill irregular connected rooms with 1 through the room size, ensuring any repeated k in the same row or column has at least k cells between it. 6 original irregular-room 4×4 unique puzzles; original engineer-tested playtest only. **45+ preference and organic acquisition unmeasured.**
- **Prototype 19:** [Kropki Pairs](games/kropki-pairs/) — Fill 1–4 once per row and column; white dots mean consecutive digits, black dots exact 2:1 ratios. Undotted edges are unconstrained. 6 unique original 4×4 positive-dot Latin boards; original engineer-tested playtest only. **45+ preference and organic acquisition unmeasured.**
- **Prototype 18:** [Numberlink Paths](games/numberlink-paths/) — Two original orthogonal routes cover an entire 4×4 grid without crossing or branching; each connects a same-letter endpoint pair. 6 full-coverage two-pair boards with unique route assignments; original engineer-tested playtest only. **45+ preference and organic acquisition unmeasured.**
- **Prototype 17:** [Cage Hearth](games/cage-hearth/) — Fill 1–4 once per row and column while solving connected cages labeled with sums, products, differences, exact quotients or single values. six original uniquely solvable 4×4 arithmetic-cage grids. **Engineering playtest only; organic demand/45+ retention unverified.**
- **Prototype 16:** [Thermometer Studio](games/thermometer-studio/) — Fill liquid from each tube's bulb toward its tip only. Row and column totals constrain all eight disjoint two-cell thermometers. six original uniquely solvable 4×4 thermometer layouts. **Engineering playtest only; organic demand/45+ retention unverified.**
- **Prototype 15:** [Skyline Steps](games/skyline-steps/) — Fill tower heights 1–4 without repeating any height in a row or column. Perimeter numbers count how many buildings are visible from that direction. six original uniquely solvable 4×4 visibility puzzles. **Engineering playtest only; organic demand/45+ retention unverified.**
- **Prototype 14:** [Hidato Walk](games/hidato-walk/) — Place numbers 1–16 exactly once so every consecutive pair touches horizontally, vertically or diagonally, while preserving given numbers. six original uniquely solvable 4×4 walking paths. **Engineering playtest only; organic demand/45+ retention unverified.**
- **Prototype 13:** [Nurikabe Bay](games/nurikabe-bay/) — Classify every square as sea or island. Numbered islands have exact sizes; each island contains one clue, the sea is connected, and there can be no 2×2 all-sea block. six original uniquely solvable 4×4 bay puzzles. **Engineering playtest only; organic demand/45+ retention unverified.**
- **Prototype 12:** [Hitori Grove](games/hitori-grove/) — Shade repeated numbers, ensuring no horizontally or vertically adjacent shaded squares and one connected group of unshaded squares. six playtest boards. **Unvalidated market opportunity.**
- **Prototype 11:** [Loop Lantern](games/loop-lantern/) — Draw a single closed nonbranching Slitherlink loop using cell clues that specify how many surrounding edges are selected. six playtest boards. **Unvalidated market opportunity.**
- **Prototype 10:** [Bridge Isles](games/bridge-isles/) — Connect visible neighboring numbered islands with zero, one or two straight bridges, without crossings, and keep the network connected. six playtest boards. **Unvalidated market opportunity.**
- **Prototype 9:** [Star Atlas](games/star-atlas/) — Place exactly one star in each row, column and labeled connected region, without allowing stars to touch even diagonally. six (three original connected-region layouts plus three rotated variations) playtest boards. **Unvalidated market opportunity.**
- **Prototype 8:** [Futoshiki Terrace](games/futoshiki-terrace/) — Fill a 4×4 Latin grid using 1–4 exactly once in each row and column, while satisfying directional inequalities. six playtest boards. **Unvalidated market opportunity.**
- **Seventh game prototype:** [Quiet Camp](games/quiet-camp/) — eight unique Tents & Trees clearings with accessible tap cycling, actual one-to-one tent/tree matching, no-touch constraints, row/column totals and offline save. **GF-006 experimental prototype, not validated demand.**
- **Sixth game prototype:** [Kakuro Compass](games/kakuro-compass/) — eight uniqueness-tested beginner cross-sum puzzles with a visible across/down candidate inspector, large digit keypad, undo and offline progress. **A research prototype for GF-001, not an ASO/production greenlight.**
- **Fifth game prototype:** [Odd Bloom](games/odd-bloom/) — twelve deterministic visual-difference rounds, single-structural-anomaly validation, large tap controls and offline save/replay. **No age-specific demand or retention verified.**
- **Fourth game prototype:** [Shikaku Garden](games/shikaku-garden/) — eight independently authored, uniqueness-checked rectangle-partition puzzles with corner-to-corner controls, undo and offline progression. **Not market-validated; the GF-018 ASO research gate remains open.**
- **Third visual playtest:** [One Path](games/one-path/) — eight verified Euler-trail graphs, non-junction crossings, tap/drag and letter-button entry, undo and offline resume. **Prototype only; user demand untested.**
- **Second visual playtest:** [Quiet Shot](games/quiet-shot/) — eight original ricochet boards, deterministic ray–mirror physics, large aim controls, offline progress and a verified browser export. **Playtest only, not market validated.**
- **First playable new game:** [Good Eye](games/good-eye/) — an original, offline-first 50/50 shape-cutting playtest with accurate polygon-area scoring, practice, five UTC daily challenges, local progress, and deterministic challenge links. **Prototype only; not launched or market-validated.**
- **Global visual playtest shortlist:** **1. Good Eye → 2. Quiet Shot → 3. One Path.** [Strategy and first three experiments](docs/research/global-45plus-top-three-games-2026-11-21.md) document the mechanics, direct competitors, proposed seven-day test and stop conditions. This is a separate exploratory ranking; acquisition, 45+ fit and economics are unvalidated, and the ASO production gate remains in force.
- **Production games:** 0. The included **Takuzu Lab** is a playable engineering reference, **not** a validated game opportunity or App Store release.
- **Opportunity hypotheses:** 100 in [docs/ideas/candidates.csv](docs/ideas/candidates.csv). Search demand is **unmeasured** until imported from a primary source.
- **Backend:** none; local-first is the default.
- **Distribution:** iOS first, Android where economics justify it.

- **Mobile export:** physical iOS/Android builds remain unverified; see the [dated toolchain and SDK assessment](docs/mobile-validation.md). The reference currently has only a Web export preset.

## Get started

1. Install [Godot 4.7.2](https://godotengine.org/download/archive/) (standard, non-.NET).
2. Open `games/takuzu-lab/project.godot` in Godot; press **F6** on the main scene or **F5** to play.
3. Run headless tests:

   ```bash
   godot --headless --path games/takuzu-lab --editor --import --quit
   godot --headless --path games/takuzu-lab --script res://tests/test_rules.gd
   python3 tools/validate_ideas.py docs/ideas/candidates.csv
   python3 -m unittest discover -s tools/tests -v
   ```

4. Read [AGENTS.md](AGENTS.md), [DESIGN.md](DESIGN.md), [docs/architecture.md](docs/architecture.md), and [docs/research/README.md](docs/research/README.md).
5. Run an offline, read-only App Store competitor catalog request preview:

   ```bash
   python3 tools/scan_storefront.py --country US --max-keywords 5 --dry-run
   ```

   See [observed 2026-11-15 US competitor snapshot](docs/research/catalog-snapshot-2026-11-15.md). Apple's public catalog matches are NOT search volume or official organic keyword ranks.

## Repository

```text
games/takuzu-lab/          Playable Godot reference (independent project)
games/good-eye/            First playable original visual-judgment experiment
games/quiet-shot/          Second visual-planning experiment (one planned shot)
games/one-path/            Third visual logic experiment (Euler trail)
games/shikaku-garden/      Fourth experiment (numbered rectangular partitions)
games/odd-bloom/           Fifth experiment (one different flower)
games/kakuro-compass/      Sixth experiment (guided crossing sums)
games/quiet-camp/          Seventh experiment (original Tents & Trees deduction)
games/futoshiki-terrace/ Futoshiki Terrace playtest
games/star-atlas/       Star Atlas playtest
games/bridge-isles/     Bridge Isles playtest
games/loop-lantern/     Loop Lantern playtest
games/hitori-grove/     Hitori Grove playtest
games/nurikabe-bay/        Nurikabe Bay independent experiment
games/hidato-walk/         Hidato Walk independent experiment
games/skyline-steps/       Skyline Steps independent experiment
games/thermometer-studio/  Thermometer Studio independent experiment
games/cage-hearth/         Cage Hearth independent experiment
games/numberlink-paths/     Numberlink Paths independent prototype
games/kropki-pairs/         Kropki Pairs independent prototype
games/ripple-rooms/         Ripple Rooms independent prototype
games/battleship-harbor/    Battleship Harbor independent prototype
games/kuromasu-windows/     Kuromasu Windows independent prototype
tools/                     Research-data validation and deterministic checks
docs/ideas/                100 research candidates and a source-backed shortlist
docs/research/             Demand, competition and review research protocol
docs/                      Architecture, quality bar, roadmap, release checklist
.github/workflows/         Godot headless + research-data checks
```

Each actual shipped game gets its **own Godot project, identity, app-store listing, and design system**. Shared systems emerge from repeated, tested needs—not premature abstractions. Reuse engine code, not indistinguishable App Store submissions.

## Market discovery (source-first, not idea-first)

Explore the **actual** most-searched game terms in Apple's Ads Insights API, then filter for specialist puzzles. This avoids over-indexing on our invented list of 100 niche queries.

```bash
# Offline, no credentials, no network:
python3 tools/fetch_apple_popularity.py --mode genre --month 2026-10 --countries US,GB,AU --dry-run

# Authenticated, read-only Apple Ads account:
export APPLE_ADS_ACCESS_TOKEN="YOUR_OAUTH_TOKEN"
export APPLE_ADS_ACCOUNT_ID="YOUR_ACCOUNT_ID"
python3 tools/fetch_apple_popularity.py --mode genre --month 2026-10 --countries US,GB,AU --page-size 200 --max-pages 3 --output .apple-results/genre-2026-10.json
python3 tools/discover_ideas.py --snapshot .apple-results/genre-2026-10.json --output .apple-results/opportunity-queue-2026-10.csv
```

For specialist subgenres, consider `--mode contains --patterns puzzle,word,logic`, with careful attention to API **top-500-per-genre** coverage. See [the discovery playbook](docs/research/discovery-playbook.md).

## Play Good Eye locally

Open `games/good-eye/project.godot` in Godot 4.7.2 and press **F5**, or run its [four local validation commands](games/good-eye/README.md). The separate [Good Eye Web Playtest](.github/workflows/good-eye-preview.yml) workflow can produce a **downloadable** WebGL2 build; it does not publish a public URL. Practice and daily modes are playable without an account or network connection.

## Play Quiet Shot locally

Open `games/quiet-shot/project.godot` in Godot 4.7.2 and press **F5**. Its [game README](games/quiet-shot/README.md) describes validation and the independent [Quiet Shot browser export workflow](.github/workflows/quiet-shot-preview.yml). A GitHub Actions artifact does not publish a playable public website.

## Play One Path locally

Open `games/one-path/project.godot` in Godot 4.7.2 and press **F5**. Follow the [One Path playtest instructions](games/one-path/README.md) or generate a downloadable, verified WebGL2 artifact with [One Path Web Playtest](.github/workflows/one-path-preview.yml).

## Play Shikaku Garden locally

Open `games/shikaku-garden/project.godot` in Godot 4.7.2 and press **F5**, or follow the [game instructions](games/shikaku-garden/README.md). The [Shikaku Garden Web Playtest](.github/workflows/shikaku-garden-preview.yml) workflow exports a downloadable web build, **not a public website**.

## Play Odd Bloom locally

Open `games/odd-bloom/project.godot` in Godot 4.7.2 and press **F5**, or follow the [game instructions](games/odd-bloom/README.md). The [Odd Bloom Web Playtest](.github/workflows/odd-bloom-preview.yml) workflow exports a **downloadable WebGL2 artifact**, not a hosted game.

## Fix “NetworkError when attempting to fetch resource”

The GitHub Actions ZIP is **not** a hosted website: GitHub Pages is not enabled for this repository. Opening an extracted `index.html` directly using `file://` prevents browsers from fetching the matching WebAssembly and PCK files. **Use the complete extracted ZIP**, not a copied HTML file.

New playtest artifacts include `serve.py`. Open a Terminal in the extracted folder (where `index.html`, `index.js`, `index.wasm` and `index.pck` are present) and run:

```bash
python3 serve.py
```

Open the printed **http://127.0.0.1:8000/** URL on that computer. Run `python3 serve.py --check` to verify completeness and file hashes, or `python3 serve.py --smoke` to check HTTP 200, MIME types and full byte delivery. For older artifacts without `serve.py`, use `python3 -m http.server 8000 --bind 127.0.0.1` from the same directory. The workflows now test this HTTP path before uploading. This does **not** amount to real-browser/WebGL verification or public deployment.

See [the exact troubleshooting guide](docs/web-networkerror.md). If it still fails, provide the browser's failing request and HTTP status from DevTools → Network.

## Play Kakuro Compass locally

Open `games/kakuro-compass/project.godot` in Godot 4.7.2 and press **F5**. The [game README](games/kakuro-compass/README.md) explains its intersection inspector, deterministic puzzle tests and [downloadable browser preview](.github/workflows/kakuro-compass-preview.yml). All browser artifacts include `serve.py` to avoid `file://` asset-fetch errors.

## Play Quiet Camp locally

Open `games/quiet-camp/project.godot` in Godot 4.7.2 and press **F5**. Its [independent README](games/quiet-camp/README.md) describes gameplay, test commands, and the [downloadable WebGL2 preview](.github/workflows/quiet-camp-preview.yml). The artifact includes `serve.py` to avoid unsupported `file://` loading.

## Play prototypes 13–17

Each of the five new games has a separate Godot 4.7.2 project and its own verified-download workflow. Open a `games/<slug>/project.godot` in Godot and press F5. Browser artifacts include `serve.py`: extract every file, run `python3 serve.py`, then visit `http://127.0.0.1:8000/`. A ZIP artifact is **not** a hosted public website. See the [new five-game decision](docs/research/game-experiments-13-to-17-2026-11-22.md).

## Play prototypes 18–22 locally

Each is an independent Godot 4.7.2 project at `games/<slug>/project.godot`; open one in Godot and press **F5**. Each successful per-game GitHub Action produces a downloadable ZIP containing `serve.py`: extract the ZIP, run `python3 serve.py`, and open `http://127.0.0.1:8000/`. This is **not public hosting**. Review the [experiment decision](docs/research/game-experiments-18-to-22-2026-11-22.md) before investing in broader content.

## Share a tested browser-playtest build

The [Browser Playtest Build workflow](.github/workflows/web-preview.yml) exports Takuzu Lab to WebGL2 from pinned Godot 4.7.2 after running gameplay integrity tests. It uploads a downloadable artifact; **it does not publish a public website or App Store app**.

See [the 5-player playtest protocol](docs/playtest.md). This is the shortest route from working code to observing whether people can play and enjoy it.

## Launch gate

No app is greenlit on ratings, Google Trends, or a developer's gut feeling alone. Require **measured non-branded App Store demand**, a documented competitor gap, a distinctive product promise, and a plausible organic distribution route. Don't mistake **lower competition** for **unmet demand**.

## Principles

- Product and distribution are the same experiment. Reserve time for ASO, analytics and retention validation.
- Respect creators' copyright, trademarks and assets; reimplement generic mechanics with original design and content.
- Respect [App Review Guidelines 4.3](https://developer.apple.com/app-store/review/guidelines/): do not submit spammy, near-identical clones.
- No ads, tracking SDK or purchases until legally and technically reviewed and instrumented. No secrets in git.
- Do not assume an open-source engine gives you a working iOS signing environment; native exports need a Mac, Xcode, provisioning, and plugin verification.

## License

No license is granted by this repository at present. The engine is separately MIT-licensed; repository code, artwork, generated content and brand need an explicit owner licensing decision before third-party reuse.
