# Evergrove — product design system / Chapter One

**Current spec and engineering handoff:** [Evergrove handbook](docs/README.md).

## One promise

**A little world that changes because you were here.**

The visual identity is **tender, grounded, alive**, without exaggeration, gamified anxiety or faux scientific claims. The story is shown through consequence first, prose second.

## Aesthetic system

**Painted storybook skin (2026-11-23).** Warm parchment page, original watercolor art (garden stream strip, vine corners, wordmark, illustrated choice icons, ending painting; provenance in [art/README.md](art/README.md)), rounded parchment tiles with copper (dry) and teal (flowing) pipes, and toned pill actions. Art is decoration around the puzzle, never behind instructions; every painted element is non-interactive.

| Token (`MosseloraTheme`) | Value | Role |
| --- | --- | --- |
| Paper | `#F9F2E3` | Page, matched to the painted strip's sky |
| Ink | `#183E34` | Primary text (10.4:1 on Paper) |
| SecondaryInk | `#49645A` | Secondary text |
| DryChannel | `#9C5A38` | Copper pipe, dashed stripe (≥3.39:1 on every tile state) |
| WaterChannel | `#18717F` | Spring-connected teal pipe, solid stripe (≥3.6:1) |
| DryTile / WetTile / SolvedTile | `#F4ECDA` / `#E1F0EC` / `#E0EDD9` | Rounded tile fills, `#D5C6A4` edge |
| CardFill / CardEdge | `#FBF7EC` / `#D9CCAB` | Choice, memory, journal and ending cards |
| SagePanel | `#E3EBD6` | Stream pill and settings panel |
| ForestAction / WaterAction / EarthAction | `#1F5A43` / `#1C6F7A` / `#A4583A` | Undo, Guide me, Restart; white bold captions ≥5.2:1 |
| QuietAction | `#EFE6D2` | Secondary actions with ink caption and drawn edge |

Typography: the device's Avenir Next (iOS) or Roboto (Android) for English when installed, bold for headings and actions; locale fonts unchanged for other languages. No font is bundled.

### Icon language (2026-11-24)

- Frequent, universally understood actions are icon-only: Undo (curved arrow, forest), Guide me (lightbulb, teal; a 1/2 or 2/2 badge shows hint depth), Restart stream (circular arrow, terracotta), Continue (arrow), Change choice (two opposite straight arrows, deliberately unlike the circular Restart), journal (open book), settings row of language (globe), text size (small/large A, badge shows 125%/150%), board size (magnifier over a grid, badge 1.2x) and chime (speaker with waves / speaker with X; a bell read as "notifications").
- Garden file copy/restore and "explore another garden" are icon-only; words appear only on the erase confirmation, beside an icon-only cancel. The unreadable-save recovery flow keeps words (rare, data-loss risk).
- After a solve, the board holds about 1.25 s on the finished creek (bloom, full droplet meter), then glides to the choices; the small "revisit water" undo sits below the choices, not between the payoff and the decision.
- Neighbors are painted portrait medallions with their name beneath; the current speaker gets a leaf-green ring.
- Every icon button keeps its localized label as a hidden child Text named "Caption", which ScreenReaderBridge speaks; names used by the QA tour are unchanged.
- Controls squash to 94% under the finger (PressSquash); a tapped pipe swings 90 degrees into place (about 160 ms) and undo swings back; the painted creek fades toward the measured water reach instead of popping. Restart and multi-step jumps snap.
- Open risk: icon-only controls have not been tested with adults 40+; watch for hesitation on Guide me, text size and board size in the first-session test.

Type hierarchy (approximate logical px): title **25**, game decisions **15–17**, guidance **13–15**, metadata **11–12**. Use built-in typography for reproducible builds; commission distinctive typography only after verifying packaging and rights.

**Layout:** Safe-area-aware scroll, first move visible quickly, 3×3 cells ~90px, 4×4 cells ~67px at 414px reference width, 52–77px actions, 17px side margins. Verify on 320px–430px phones, larger displays and iPad; do not infer accessibility from logical size alone.

**Signature interaction:** a pipe rotates; **only spring-connected channels turn water blue**, while dry channels stay earthen and retain readable shapes; the upstream creek advances with actual graph reach; a complete route restores the whole village creek; a choice changes the village and its next puzzle. Feedback is causal, never a fake progress animation.

**Current playable art (2D baseline):** a manually composed, original 2D vector illustration with quiet hills, water, shelter, bridge and growing flowers. Three people remain in the scene. Flower clusters, shade, bridge, walkers and butterflies respond to persisted choices; environmental colors subtly change with chapter. No stock art, borrowed icons, generic magic glow or asset-heavy UI.

**Long-term art hypothesis (2.5D):** a fixed-camera miniature with 2D puzzle UI is **now implemented as an opt-in procedural renderer** in `VillageDiorama.cs`; high-quality authored assets and Unity device QA remain unverified. The [visual ADR](../../docs/decisions/2026-11-22-evergrove-visual-direction-2p5d.md) covers 2D alternatives. Do not scale 3D production without evidence.

**Sound and motion:** default silent, optional soft procedural completion chime. UI transitions are intentionally restrained and functional; introduce subtle animation only after device/reduced-motion testing. Haptic and background music are excluded until they have user value.

## Interaction constraints

- **First 10 seconds:** the Unity first puzzle comes immediately after the concise title, **before** the 2D/2.5D world illustration; see a water pipe, understand tap-to-rotate and take a valid action on a compact phone. Real pixels/timing still require device verification.
- **During play:** plain-language connected-tile and leak feedback, undo, restart, optional one-connection help, no timer or penalty.
- **After solving:** jump to the **actual** choice heading, not the page bottom where footer/settings can hide the decision. Retain preceding context to communicate full creek restoration, then offer three understandable environment choices with a written consequence preview.
- **After choice:** a dedicated *memory moment* confirms which NPC and environmental relationship changed, then a voluntary Continue.
- **Later sessions:** journal reconstructs the sequence; multiple choices combine into new places.
- **Ending:** preserve choices in one of several authored conclusions, with a two-step confirmation before erasing the garden.
- **Comparison mode:** same puzzle only in chapter 1; subsequent Living boards are rotated and anchored while Puzzle Only uses original fixtures. This confounds narrative-only A/B. See [validation](docs/VALIDATION.md).

## Screen and test checklist

- [ ] Unity Editor UI compiles and launches with uGUI.
- [ ] Real-device iOS and Android touch, safe-area, keyboard focus and 4×4 cell size verified.
- [ ] Color contrast measured and revised where necessary.
- [ ] Screen reader / accessibility tree semantics verified on mobile.
- [ ] Rotating, undo, hint, recovery after force-kill and v1 save migration tested on device.
- [ ] Artwork readability and feeling of causal agency observed with relevant adults.
- [ ] User-perceived quality benchmarked head-to-head versus a polished established puzzle game.
- [ ] Device sound behavior tested, including mute/ringer and consent.

## The meaningful second-order loop — ecological watercourses

Following a restored stream, the player chooses the next priority: shade trees, a crossing or wildflowers. **In the next chapter** the chosen structure changes the actual grid topology and a marked connection is fixed in place. A tree holds its roots, a bridge holds a foundation, a meadow protects a habitat. The player routes the water around the preserved feature; tapping it cannot rotate it.

The prototype implements 3 deterministic rotated branches per next chapter, with a mathematically verified unique solution. Rotation is a controlled *content-efficient experiment*, not a deep simulation by itself. Do not claim emergent ecology until independent systems (growth, water availability, dependencies, costs and repair) actually affect each other.

**Research caveat:** This changes mechanical complexity and the amount of puzzle assistance. For the pure-vs-living A/B, the controls no longer see identical post-chapter-1 puzzles. A valid test of the *world narrative layer alone* must hold board rules constant; test this new systemic mode as a **separate condition** or restrict analysis to chapter one. Never attribute changes in retention purely to narrative when topology and fixed tiles also change.

**Data durability:** v3 persists the selected board variant and the previously completed variant; v1/v2 sessions resume unchanged until the next committed choice. The player can reconsider a choice without reopening a different board.

## First 30 seconds: visible causality

- At start: the **downstream creek is still dry**, with a partial upstream trickle corresponding to the spring's already connected channels.
- First action: a pipe rotates instantly; when that connects another channel to the spring, the channel becomes blue and the upstream trickle extends. Wrong-direction turns may **retract** water honestly.
- First solution: the fully connected, leak-free route restores the whole creek in 2D and 2.5D, giving stronger environmental proof than partial progress.
- First choice: plant shade, repair a crossing or grow flowers, and let the next chapter reflect it.
- No guilt, streaks, penalties, fake personalization, mandatory dialogue or hidden reward schedules.

`VillageScenePlan.WaterRestored` remains false until the first actual solve; `WaterReachPercent` tracks how much of the first board is spring-connected. Pure C# tests verify first-tap increase, Undo retraction and full restoration. The 2D storybook reflects partial water on every tap without a view rebuild; the optional 2.5D experiment still changes on solve only. Neither render path has been evaluated in a real Unity Editor or on phones yet.

## Release and evidence mechanics

Unity 6.3 uses an **Input System UI Input Module**, not an unconditional legacy Standalone Input Module. The project ships `com.unity.inputsystem` and an editor-matched uGUI package; the editor must resolve them before any app claim. A *manual*, license-gated GameCI workflow runs Unity's own C# compiler and can produce an Android test APK or WebGL artifact. **This has not been run yet.**

An optional **in-memory playtest report** is user-initiated, never uploads, has no identifiers and can be copied only by explicit user action. This is for qualitative first-session observation, not a substitute for D1/D7 cohort research.

## Two rendering languages, one authoritative world

The current **2D storybook** remains the inexpensive baseline. The new opt-in **2.5D miniature** uses a fixed orthographic camera aimed at a low-poly island with an original generated stream, a cottage and three villagers. These are two visual presentations of the **same world events**, not separate games and not an engine rewrite.

- A pure `VillageScenePlan` reads the state once and declares trees, crossing, visitors, butterflies, neighbor gathering and chapter phase.
- The 2.5D renderer creates only those visible objects. A fixed camera avoids touch-navigation demands; all water puzzles remain 2D controls.
- World illustrations use a reused unlit material with per-object property blocks, no dynamic shadows, a bounded offscreen texture and no 3D physics simulation. Avoid costly UGC assets, uncontrolled model generation or large-stage wandering.
- The 2.5D view defaults to **stillness**; an opt-in low-frequency animation can gently move water glimmers, butterflies and villagers. The rendering loop is disabled when the 2D mode is shown.
- All three neighbors now have explicit **Talk** buttons and hand-authored conversations grounded in past decisions. They do not retain chat histories or imply human-like consciousness.
- Art controls appear below first-play gameplay. The first tappable water tile stays prioritized.

**Research warning:** The miniature's low-poly forms are an *engineering proof of composition and causality*, not finished character art. Do not claim commercial-grade animation, style preference or accessibility until verified in Unity and tested with players.

**Not a production certificate:** C# unit tests prove deterministic game logic, not taste or device readiness.

## Insanely Great Design: 100-Iteration Transformation (2026-11-24)

Mosselora was upgraded through 100 dedicated design iterations to fulfill `vision.md` as an immersive, tactile garden sanctuary for adults 45+:
1. **Icon-First Navigation:** Replaced text-heavy buttons with tactile vector pills (`↶ Undo`, `💧 Hint`, `↺ Restart`) and a collapsible Smart Floating Menu Drawer (`☰`).
2. **Immersive Game Stage:** Removed all web-like masthead banners, text footers, and article styling in favor of an organic porcelain tile stage and living watercolor diorama.
3. **Living Diorama:** Flowing river currents, curling chimney smoke, drifting autumn blossom petals, swaying meadow reeds, and swimming koi fish.
4. **Procedural Pentatonic Audio:** Zero-dependency Web Audio API soundscape with hand-tuned singing bowl stone chimes, water ripples, and harmonious C-major fanfare.
5. **Senior-First Ergonomics:** Generous 64px–88px touch targets, WCAG AAA 7:1 contrast ratios, tap confirmation on restart, and high-visibility focus rings.
