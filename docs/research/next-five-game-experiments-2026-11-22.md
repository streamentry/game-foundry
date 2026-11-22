# Next five independently playable game prototypes

**Decision date: 2026-11-22. Status:** implementation experiment at user's request; **no market/retention validation**. The existing 30-query ASO protocol and Kakuro production gates remain in force.

| Game | Existing hypothesis | Core experiment | Strongest counterargument |
| --- | --- | --- | --- |
| [Futoshiki Terrace](../../games/futoshiki-terrace/README.md) | GF-004 | Do adults who enjoy logic puzzles understand inequalities faster with persistent local relationship explanations? | Incumbent Futoshiki apps already cover the same rules; an inspector could be unnecessary. |
| [Star Atlas](../../games/star-atlas/README.md) | GF-005 | Can letter-labeled regions and untimed no-contact logic produce legible, satisfying short play? | Rotations are equivalent layouts, and a three-base-board catalog is too small for sustained play. |
| [Bridge Isles](../../games/bridge-isles/README.md) | GF-007 | Do visual bridge lines and direct letters make Hashi navigation easier on smaller phones? | Hashi is mature, and rendering bridges is not a defensible distribution advantage. |
| [Loop Lantern](../../games/loop-lantern/README.md) | GF-008 | Can explicit side controls remove precision-drawing friction while retaining satisfying deduction? | The discrete side controls may feel slower than direct tracing, and classic competitors already exist. |
| [Hitori Grove](../../games/hitori-grove/README.md) | GF-009 | Does clear text feedback about duplicate removal and white connectivity improve understanding versus existing Hitori apps? | 4×4 grids and aesthetic polish are not enough to stand out in a familiar puzzle category. |

## Engineering and distribution boundaries

Each is a separate Godot 4.7.2/GDScript project with original small fixture boards, solver-verifiable solutions, touch-first gameplay, undo/restart, offline versioned save/recovery and a downloadable WebGL2 ZIP that verifies WASM/PCK/JS/HTML delivery over localhost. A **green Web export is not a hosted site**, signed mobile build or real-phone usability result. The games are distinct rule engines; shared conventions do not imply a generic mass-produced app ready for App Review 4.3.

**Known facts:** these genres exist in the pre-existing discovery queue; `GF-004`, `GF-007`, `GF-008`, and `GF-009` have historical listing anchors while `GF-005` remains a hypothesis-only keyword. A listing anchor is *not* installs, revenue, store search volume or a 45+ age breakdown. The original levels are designed for correctness and first-play experiments; the Star Atlas content is specifically **three distinct connected region base maps plus three deterministic rotations**, not six independent solved designs.

## Cheapest disconfirmation

Recruit 6–10 outside adult puzzle players with a deliberate spread of ages 45–54, 55–64 and 65+ where possible. Alternate order of the five new prototypes and direct incumbent games. Record unaided first correct action, observed misunderstandings, perceived effort, whether they **voluntarily** start another level, and unprompted return on a later day. Keep denominators and board versions fixed; separate expressed intention from actual behavior. This is a formative pilot, not a population estimate.

**Continue** a game into a larger test only if at least three participants independently choose more than one additional board **and** users articulate and demonstrate a concrete comparative improvement absent in the incumbent; these are conservative **proposed pilot gates**, not benchmark conversion rates. **Stop or park** after one targeted UI revision if there is no comparative preference, people cannot understand the rules without intervention, or only contrived achievement prompts drive continuation.

## Priority warning

Building **12** prototypes can create a false sense of progress. Technical feasibility and automated solver correctness are known; discoverability, differentiation, healthy retention, payment, full accessibility and 45+ appeal are **unknown**. The most valuable next action is an accessible public browser playtest on real phones, not automatically expanding the catalog. The user's express instruction authorizes small prototypes, **not** public App Store release, purchases, new analytics collection, or replacing the evidence gates.
