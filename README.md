# Game Foundry

**Research-first, Godot-powered mobile games.** We don't mass-clone winners. We identify search intent that existing games serve poorly, ship distinctive experiences, and let organic market feedback decide what deserves investment.

## Status — 2026-11-21

- **Engine:** Godot **4.7.2 stable** (pinned; Godot 4.8 is beta).
- **First gated ASO experiment:** **Kakuro**, U.S. adults 50-75 first; U.K. and Japan follow-up hypotheses. [Global ASO recheck](docs/research/kakuro-global-aso-45plus-2026-11-21.md) identifies **direct competitors already claiming explainable hints**. No production greenlight until demand, storefront rank, and comparative player tests pass.
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
