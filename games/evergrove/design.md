# Mosselora — Living Garden Design System
> **Status (2026-11-23):** partially implemented in the Unity/C# and offline browser source. Artwork and visual behavior still require Unity/device QA and uncoached player validation.  
> **Scope:** Mosselora (formerly Evergrove), mobile-first, offline-first living-garden puzzle.  
> **Canonical design proposal:** this file. Existing implementation reference: [DESIGN.md](DESIGN.md), [player experience](docs/PLAYER_EXPERIENCE.md), and [browser research slice](research/first-minute/README.md).  
> **Brand decision:** Mosselora is a working name pending formal trademark clearance; do not silently rename shipped assets, save identifiers, packages, namespaces or public URLs.

## 1. The one sentence

**A garden that remembers your touch.**

A player should feel *“I helped this place become itself”*, not *“I completed another level.”* The garden is the protagonist; the player's care is the verb.

The design is not a fantasy-themed productivity dashboard. It is a **playable ecological storybook**: tactile puzzles, visible cause and effect, quiet moments, and a place whose history is legible in its landscape.

### Four design laws

1. **The world answers before the UI speaks.** A connected channel fills with water; a newly shaded path changes what a neighbor does. Only then does concise language explain.
2. **Beauty follows causality.** No glowing reward for a false connection, no animated growth that contradicts saved state, no fake simulation.
3. **The player controls the pace.** No countdowns, streaks, guilt, energy meters, artificial urgency, coercive rewards or forced notifications.
4. **Every detail survives subtraction.** If a decorative element obscures a pipe, reduces contrast, delays the first action or weakens recognition at small size, remove it.

## 2. The unconventional signature: the garden is the interface

Conventional mobile games divide the screen into a game board, HUD, rewards, inventory, dialogue and a separate decorative world. Mosselora should feel like **one continuous place**.

**Proposed interaction grammar:**
- **Water is progress.** The board's actual connected graph determines visible water reach. Avoid a generic XP bar.
- **Roots are constraints.** A protected tree root, bridge footing or meadow habitat becomes a visibly anchored puzzle tile in later chapters. Constraint is story, not arbitrary difficulty.
- **Growth is memory.** The world changes from committed choices; the journal records *what happened* rather than awarding badges.
- **Stillness is a feature.** On return, let the player observe the world before requesting another task. This is a hypothesis to test, not an excuse to delay the first tap.
- **A mistake is information.** A wrong rotation may withdraw water; show it truthfully and let Undo restore it. Never scold.
- **Choices are ecological verbs.** “Plant shade,” “Connect the banks,” “Make room for flowers,” each with a concrete consequence preview.

**Novelty must be earned.** Do not invent complex gestures or hide essential controls to appear unconventional. The radical part is the coherence of world, puzzle, memory and emotional pacing.

## 3. Two-layer visual language: Wonder outside, clarity inside

The approved **brand direction** is an illustrated golden, moss-covered ornamental **M**, surrounded by a stream, flowers, butterfly and lit cottage. It conveys warmth, shelter and renewal. The **app icon must be full-bleed square art**, with no baked-in rounded white corners; the operating system applies its own mask. The illustrated brand is a concept, not yet a production asset specification or trademark clearance.

**Brand layer — the invitation:** painterly moss, botanical textures, warm gold, water-blue light, imperfect natural silhouettes, a tiny welcoming home. Use for icon, title moment, chapter covers and rare celebrations.

**Play layer — the instrument:** restrained surfaces, unmistakable pipe shapes, high contrast, minimal textures, predictable buttons. A readable 3×3 board at 320 px matters more than elaborate scenery.

**Never** put high-detail foliage beneath a puzzle cell, simulate unreadable glass, use gold text on pale cream, or rely on a butterfly/glow to communicate a state.

### Palette tokens (proposed, not contrast-certified)

| Token | Hex | Purpose |
| --- | --- | --- |
| `soil.paper` | `#F7F4E9` | Calm, low-glare background |
| `ink.forest` | `#183E34` | Primary text and strong outlines |
| `ink.secondary` | `#49645A` | Secondary text, pending contrast verification |
| `leaf.action` | `#276D54` | Primary action and committed growth |
| `leaf.soft` | `#DFEBDD` | Light action surfaces |
| `water.deep` | `#176C83` | Connected-water channels and focus accent |
| `water.light` | `#DCEDEF` | Water surface and journal wash |
| `earth.dry` | `#9B8266` | Dry channels, with distinct non-color geometry |
| `gold.lantern` | `#E8B958` | Rare warm emphasis; never essential small text |
| `flower.lilac` | `#9A78AE` | Optional botanical accent |
| `danger.ink` | `#87402F` | Destructive confirmation and real save failures |
| `surface.light` | `#FFFDF6` | Readable puzzle/choice panels |

**Color allocation:** ~75% quiet neutrals and garden greens, ~20% ecological state colors, at most ~5% gold/lilac accents. These are art-direction ratios, not enforced analytics. Validate all text and control-state combinations with actual contrast calculations; target WCAG AA (4.5:1 normal text, 3:1 large text and relevant non-text UI) where applicable. Color must never be the only signal.

**Day/night:** no mandatory real-time clock mechanic. If time-of-day artwork is explored later, keep gameplay contrast and puzzle semantics constant; do not punish players who visit at a different hour.

## 4. Typography and copy

**Display:** a warm, slightly eccentric storybook serif for the Mosselora wordmark and occasional chapter titles; verify license, font rendering and all target scripts before embedding. Avoid using an ornamental serif for game instructions.

**Functional:** legible platform/system sans, familiar numerals, generous spacing. Use native fallback for Vietnamese diacritics, CJK, Arabic and other supported locales. Do not assume English wordmark lettering can become body text in 30 languages.

| Role | Starting mobile size | Rule |
| --- | --- | --- |
| Brand/chapter title | 30–42 logical px | Max two lines; never pushes first move below fold |
| Main task | 22–28 | One plain-language verb |
| Primary choice | 16–18 | Clear verb + consequence |
| Body/help | 15–17 | Comfortable line length and spacing |
| Secondary labels | 13–15 | Never tiny for required information |
| Decorative eyebrow | 11–12 | Optional only; never sole instruction |

All sizes are **starting hypotheses**, not fixed caps: support platform text scaling, clipping checks and localization expansion.

**Voice:** kind, precise, concrete. Not saccharine, mystical or congratulatory by default.

| Instead of | Say |
| --- | --- |
| “AMAZING! YOU WON!” | “The stream reaches the garden.” |
| “Oops! Wrong move!” | “This turn breaks the connection.” |
| “Claim your reward” | “Choose what grows here.” |
| “You haven't played in 3 days” | No message |
| “Your garden needs you!” | “Your garden is here when you're ready.” (only where useful) |

## 5. The five signature moments

### A. The invitation (0–10 seconds)
One promise, one clear instruction, and a **tappable pipe visible without scrolling** on compact phones. The world can peek through as context, but it cannot consume the first viewport. No modal onboarding, cinematic splash delay, account prompt or tutorial wall.

**Test:** at 320×568, 375×667 and 430×932 CSS px, first tile is visible and at least 68×68 CSS px in the browser research slice. This is a **browser gate**, not proof of native Unity/device accessibility. On device, measure physical target sizes and platform guidelines.

### B. The first answer (first tap)
Pipe turns with immediate visual feedback. Only truly spring-connected segments become blue. If reach grows, a restrained water ripple follows. If reach shrinks, the water retracts. The **shape of a channel** remains visible even without color. The player learns the rule by watching the world respond.

### C. The restoration
When the actual puzzle is complete, the stream reaches the village. Pause the interface's demand for attention for a short, skippable beat; avoid confetti, coins, noisy fireworks or arbitrary stars. The world is the reward.

### D. The ecological choice
Present three **specific, reversible-until-confirmed** possibilities: shade, crossing, flowers. Each previews what changes visually and in the next board's constraint. After commitment, show the changed place first; then a brief memory sentence. Never present three nearly identical cosmetic skins as consequential choices.

### E. The return
The garden loads exactly from durable saved history, without a forced reward collection. The journal is a **field notebook**, not an achievement inventory. Players can inspect their earlier decisions and see their effects. Save failure must be explicit, recoverable and never disguised as success.

## 6. Component anatomy

### Water tile
- Large stable hit target; shape communicates pipe orientation.
- Dry: earth-colored, unfilled channel; connected: visibly water-filled channel with distinct stroke/texture.
- Leak: small directional mismatch indicator, not a cluster of alarming red exclamation marks.
- Fixed ecological anchor: recognizable root/bridge/meadow form **plus** “fixed” semantics and non-rotating behavior.
- Focused/selected: thick visible outline, not only shadow.
- Disabled/completed: still legible; do not wash out the board.
- Undo restores both game state and matching visual water reach.

### Choice seed
A tactile, readable choice card with a small botanical emblem, action verb and one-sentence consequence. The preview must be true of implemented game state. No surprise costs, no hidden “best” answer.

### Memory leaf
A compact journal entry: **action → observed change → character response**. Use authored, persisted facts; do not fabricate emotional reactions or imply AI character consciousness.

### World vignette
A miniature, not a map full of icons. A consistent camera and spatial anchors help players notice differences. Keep the cottage, creek and landmarks in recognizable positions. Art must not obscure the actual state of the water.

### Gentle help
A visible, voluntary “A gentle hint” action. Progress from orientation toward one specific connection; never punish help use or imply the player is inadequate.

### Trust surfaces
A readable offline/save status when necessary, explicit destructive reset confirmation, no hidden analytics, and no dark-pattern monetization. If analytics are later introduced, consent, data minimization and documentation are required.

## 7. Motion, sound and touch: a living place, not a slot machine

**Motion budget:** immediate interaction response; short water travel after a valid connection; subtle rare environmental movement. Do not run perpetual particle effects just because the renderer can. Keep gameplay deterministic independently of animation timing.

**Timing hypotheses:** tap response visually apparent within ~100 ms; tile turn ~120–180 ms; connected-water response ~200–450 ms; restoration moment ~500–900 ms and skippable. Validate on low-end devices and with people; these are not measured performance claims.

**Reduced motion:** no camera drift, parallax, screen shake or nonessential animation. State changes must remain explicit and comprehensible when motion is disabled.

**Sound:** silent by default, optional quiet water/wood/bell sounds that reinforce *actual* changes. Honor system mute behavior after native verification. No reward jingles, audio-only instructions or autoplay music.

**Haptics:** optional, restrained, never necessary for comprehension; exclude from initial release until tested.

**Touch:** tap-to-rotate is primary; Undo and Hint are first-class. Do not make pinch, long-press or drag essential to basic play. Preserve predictable focus and reading order for switch control, keyboard and screen readers.

## 8. Layout and accessibility

- **Portrait-first, safe-area-aware, scrollable.** One clear task per moment. No bottom-sheet overlay blocking the board.
- **First action wins the fold.** Put optional world art, settings, language and research copy after the first puzzle on compact screens.
- **Minimum browser research tile:** 68×68 CSS px at tested viewport sizes; native touch targets must be checked separately on real devices.
- **Never encode state only by color, animation, audio or tiny symbols.** Announce row/column, orientation, reachability, leak/fixed status and action in accessible labels.
- **Visible focus, large type, sufficient contrast, readable captions, reduced-motion behavior and screen-reader order** are release gates, not post-launch polish.
- **Localization:** keep actions short, support expansion and RTL where applicable, verify glyph coverage. Existing 29 non-English catalogs are drafts pending native review.
- **Respect attention:** no notification permissions on first launch; no forced daily ritual.

## 9. Art production rules

1. **Silhouette before detail.** The app icon's golden M and moss/water contrast must read at ~60 px. Test grayscale and at small scale.
2. **Full-bleed square master.** No baked-in rounded border; keep important details within platform-safe icon area.
3. **Hand-authored ecological landmarks.** Three to five instantly recognizable objects outperform a screen of tiny generic assets.
4. **Materials:** damp stone, moss, soft wood, living leaf, moving water, warm window light. Use a consistent light direction.
5. **Lighting:** wonder belongs to the world, not to every button. Gold is rare and earned.
6. **No borrowed art, unlicensed typefaces or implied production ownership from a generated concept.** Check rights and asset provenance before shipping.
7. **Performance:** prefer a bounded fixed-camera miniature; measure texture memory, frame pacing, battery and startup on target phones before expanding art scope.

## 10. What we explicitly reject

- A generic cozy-game template: beige cards, green gradients and endless decorative leaves.
- A hyperrealistic garden sim that cannot support meaningful ecological consequences.
- An AI-chat gimmick posing as relationships or memory.
- XP, badges, streaks, loot boxes, timers, artificial scarcity or manipulative retention.
- A giant open world before the first small place feels alive.
- A long intro that hides the first playable action.
- Accessibility theater: passing a browser test while claiming iOS/Android screen-reader success.
- More speculative features before observing players and verifying native builds.

## 11. Design-to-engineering contract

**Current evidence (as of 2026-11-22):** the game has a Unity/C# prototype, deterministic puzzle/world rules, save journal, a browser first-minute research slice, automated Chromium viewport checks and C# assertions. These support specific implementation claims, **not** a production-quality native build, delight, retention or broad accessibility.

**Now implemented in source:** a shared Unity `MosseloraTheme` for paper/ink/leaf/water and tile-state precedence; branded English and 29 draft-locale titles; balanced ecological choice cards; a gentler open-circle leak marker; rounded pipe junctions; warmer cottage/world details in the original 2D illustration; a corresponding optional 2.5D palette; and a Mosselora-branded offline first-minute browser slice with automated viewport checks. These are **source changes and design hypotheses**, not proof of a native build or user preference. Current code and [DESIGN.md](DESIGN.md) remain the authority on what actually runs.

**Suggested implementation order:**
1. **Truth audit:** identify where the current browser and Unity UI diverge from this contract. Prioritize incorrect state, unreadable pipes, hidden first tap and inaccurate consequence copy.
2. **Prototype one visual slice:** first board → first connected water → full restoration → one ecological choice. Use existing game rules, not mocked behavior.
3. **Validate before expanding:** run automated browser screenshots, Unity licensed Editor compile, native device accessibility/performance, then uncoached human tests.
4. **Scale only proven elements:** apply the visual language to later chapters once the first-minute experience is understood and enjoyed.

## 12. Evidence and stopping rules

**Core questions:** Can players act without instruction? Do they understand why water moves? Do they notice the village change? Do they want to return voluntarily? Does the icon look distinctive at app-store size?

**First study:** 8–12 uncoached people, including players aged 45+, across real target phones when possible. Count *all starters*, abandoned attempts and restarts. Record time to first valid tap, cause/effect explanation in their own words, restoration recognition, choice comprehension, and spontaneous desire to continue. No leading prompts such as “Isn't the garden beautiful?”

**Practical decision thresholds (small-sample heuristics, not statistical proof):**
- With 10 starters, if **3+** cannot infer the goal or **3+** miss restoration, stop expanding visual scope and redesign.
- If **8+** act within 30 seconds and **7+** notice the consequence, proceed to licensed native M1/M2 QA, **not** to a launch claim.
- If art is liked but the puzzle is misunderstood, prioritize legibility over additional decoration.
- If the puzzle is understood but the garden feels inconsequential, deepen *real* cause/effect before commissioning more art.
- If return intent is merely polite praise, measure voluntary second-session behavior before asserting retention.

**Native release blockers:** Unity Editor compile, actual iOS/Android builds, safe areas, screen readers, text scaling, save durability/force-kill recovery, low-end frame pacing, license review and localization QA. Track these in the existing QA issues rather than checking boxes based on browser-only evidence.

## 13. North star

A child can understand what the water is doing. An adult can enjoy the quiet without being manipulated. Someone who returns after a week can see a place that genuinely remembers their choices.

**Make the smallest living place worth caring about. Then grow it.**
