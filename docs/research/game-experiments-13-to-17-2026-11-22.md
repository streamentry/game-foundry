# Five separate research prototypes, games 13–17

**Dated:** 2026-11-22 · **Status:** user-requested engineering implementation, **no production approval or measured user response**.

| Prototype | Existing keyword hypothesis | Differentiation to challenge | Strongest counterargument |
| --- | --- | --- | --- |
| [Nurikabe Bay](../../games/nurikabe-bay/README.md) | GF-010: Nurikabe | A live explanation of sea connectivity and island area makes the first board clearer than a classic minimalist Nurikabe app. | The genre is established; small grids and pretty shores are not a reason to switch or return. |
| [Hidato Walk](../../games/hidato-walk/README.md) | GF-013: Hidato | A generous number pad and explicit diagonal adjacency feedback reduce Hidato's first-entry friction. | Sixteen buttons plus sixteen cells may require scrolling on small screens; only actual device observation can confirm comfort. |
| [Skyline Steps](../../games/skyline-steps/README.md) | GF-019: Skyscrapers | Placing every directional clue visibly around the board makes abstract sight lines intelligible without a lengthy tutorial. | This resembles classic Sudoku-style input; a small 4×4 version may not reveal a meaningful commercial wedge. |
| [Thermometer Studio](../../games/thermometer-studio/README.md) | GF-021: Thermometers | Two-segment instruments with current/target labels teach prefix fill logic in a short, tactile session. | Two-cell thermometers are an intentionally simplified mechanic; real puzzles may need varied tube lengths to stay interesting. |
| [Cage Hearth](../../games/cage-hearth/README.md) | GF-023: arithmetic cages (KenKen-style) | Warm, visually grouped cages help players parse arithmetic constraints without a separate instructions screen. | Arithmetic puzzles have incumbents and may overlap the studio's Futoshiki/Kakuro audiences; a new title alone adds no differentiated distribution. |

## What each implementation actually establishes

Each project has a separate Godot 4.7.2 source tree and brand; six independently authored 4×4 puzzles; a pure, deterministic rule engine that enforces the standard puzzle mathematics (including nonlocal connectivity where needed); a solver to verify exact uniqueness; full player controls with recovery; versioned local save; and a downloadable WebGL2 artifact built using official pinned templates. The artifact workflow tests **actual HTTP delivery** for `index.html`, `index.js`, `index.wasm` and `index.pck`, but is not HTTPS hosting or a graphical browser/device pass.

No puzzle is claimed to have a calibrated difficulty curve. Thermometers deliberately use **eight two-cell tubes**, a restricted beginner interpretation rather than the full length variety of classic puzzles. All work uses independently generated rule fixtures and original interface treatments. Do not imply novelty or reuse any external art, levels or brand assets.

**Observed versus unknown:** GF-010 has a historical US App Store listing anchor in the 100-row research queue, which establishes only that a Nurikabe title existed at the time of the scan. GF-013, GF-019, GF-021 and GF-023 remain hypothesis-only search terms. **None** provides official Apple keyword popularity, 45+ evidence, direct competitor superiority, retention, referral conversion or monetization evidence. The primary U.S. Kakuro proof and its gates remain untouched.

## Cheapest falsification and stop gates

Recruit outside puzzle players across 45–54, 55–64 and 65+ when feasible. Randomize/rotate order, compare **at least one direct incumbent per mechanic**, and collect denominators and observed first correct move, unaided explanations, optional second and third puzzles, a next-day unprompted return, and whether they actually prefer ours. Record version, device, accessibility issues and prior familiarity to control selection bias. Do not conflate click, opening, expressed intention, or developer satisfaction with retention.

**Proposed qualitative pilot gate (not an industry benchmark):** 6–10 relevant adults per shortlisted mechanic. Only build more content for a game if at least three volunteers independently choose additional levels **and** there is a repeatable observation of meaningful competitor advantage. Give exactly one documented UX correction for comprehension failure, then park if the gap remains. Stop building new prototypes as a substitute for external feedback.

## Risks and next real action

With 17 functioning prototypes, **portfolio count is now an anti-metric** absent distribution. Additional untested catalogs increase QA, compliance and Apple App Review 4.3 exposure without changing the underlying uncertainty. User-requested prototypes should not be published as a flood of separate, undifferentiated apps. Next real action: host a small opt-in browser playtest at a public HTTPS URL (with explicit approval and privacy review), then directly observe users 45+ playing on their own phones. Do not invent a release, analytics source, or automatic user recruitment.
