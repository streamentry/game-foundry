# Evergrove — product design system / Chapter One

## One promise

**A little world that changes because you were here.**

The visual identity is **tender, grounded, alive**, without exaggeration, gamified anxiety or faux scientific claims. The story is shown through consequence first, prose second.

## Aesthetic system

| Token | Value | Role |
| --- | --- | --- |
| Paper | `#F7F4E9` | Low-glare background |
| Evergreen ink | `#23443C` | Primary content |
| Moss | `#52685F` | Secondary content |
| Leaf | `#276D54` | Primary actions |
| Pale | `#DFEBDD` | Tactile buttons |
| Sand | `#F2E7D4` | Story cards |
| River | `#DCEDEF` | Memory journal background |
| Water blue | `#6FB4BA` | Connection motif |

Type hierarchy (approximate logical px): title **25**, game decisions **15–17**, guidance **13–15**, metadata **11–12**. Use built-in typography for reproducible builds; commission distinctive typography only after verifying packaging and rights.

**Layout:** Safe-area-aware scroll, first move visible quickly, 3×3 cells ~90px, 4×4 cells ~67px at 414px reference width, 52–77px actions, 17px side margins. Verify on 320px–430px phones, larger displays and iPad; do not infer accessibility from logical size alone.

**Signature interaction:** a pipe rotates, the reachable water updates, the stream completes, a deliberate choice changes the illustration, and the world explains what changed. Every element reinforces one causal loop.

**Current playable art (2D baseline):** a manually composed, original 2D vector illustration with quiet hills, water, shelter, bridge and growing flowers. Three people remain in the scene. Flower clusters, shade, bridge, walkers and butterflies respond to persisted choices; environmental colors subtly change with chapter. No stock art, borrowed icons, generic magic glow or asset-heavy UI.

**Long-term art hypothesis (2.5D):** a miniature, lightweight stylized 3D village under a mostly fixed elevated **orthographic camera**, with expressive but modest NPC animation and a crisp **2D puzzle UI**. Persistent events should visibly change paths, shade, flowering and visitor movement. This is **not implemented** in the current illustrated Unity prototype. [Full visual-direction ADR](../../docs/decisions/2026-11-22-evergrove-visual-direction-2p5d.md) documents the strongest 2D alternative, phone constraints, separate 2D vs 2.5D comparison and reversal conditions. **Do not prematurely replace the validated deterministic state core or build a 3D asset factory.**

**Sound and motion:** default silent, optional soft procedural completion chime. UI transitions are intentionally restrained and functional; introduce subtle animation only after device/reduced-motion testing. Haptic and background music are excluded until they have user value.

## Interaction constraints

- **First 10 seconds:** see the water puzzle, understand tap-to-rotate and take a valid action.
- **During play:** plain-language connected-tile and leak feedback, undo, restart, optional one-connection help, no timer or penalty.
- **After solving:** three understandable environment choices with a written consequence preview.
- **After choice:** a dedicated *memory moment* confirms which NPC and environmental relationship changed, then a voluntary Continue.
- **Later sessions:** journal reconstructs the sequence; multiple choices combine into new places.
- **Ending:** preserve choices in one of several authored conclusions, with a two-step confirmation before erasing the garden.
- **Comparison mode:** same deterministic puzzle content without world layer. Do not mistake a prettier world for causal evidence without a control.

## Screen and test checklist

- [ ] Unity Editor UI compiles and launches with uGUI.
- [ ] Real-device iOS and Android touch, safe-area, keyboard focus and 4×4 cell size verified.
- [ ] Color contrast measured and revised where necessary.
- [ ] Screen reader / accessibility tree semantics verified on mobile.
- [ ] Rotating, undo, hint, recovery after force-kill and v1 save migration tested on device.
- [ ] Artwork readability and feeling of causal agency observed with relevant adults.
- [ ] User-perceived quality benchmarked head-to-head versus a polished established puzzle game.
- [ ] Device sound behavior tested, including mute/ringer and consent.

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
