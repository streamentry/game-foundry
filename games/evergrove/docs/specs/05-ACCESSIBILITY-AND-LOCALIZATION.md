# 05 — Accessibility and localization release specification

**Principle:** readable, calm, low-friction play is core game quality. A selectable language or large-looking control is **not proof** it works on a phone. This spec adds release tests; it does not certify any device or locale.

## Interaction and low-vision gates

| Area | Required behavior | Test/acceptance |
| --- | --- | --- |
| Tap targets | One-tap rotation; frequent actions at least **44×44 pt iOS / 48×48 dp Android** target area, with forgiving spacing | Inspect actual interactive hit rects, not visual icon bounds, on compact devices |
| Input | Visible pressed/selected feedback, no precision swipe, no forced long press | Observe target cohort; median ≤2 accidental taps per 10-minute session is a **provisional** signal |
| Readability | Scalable text, clear type hierarchy, large board option, no clipped choices/journal | 320–430 logical-width phone matrix, large OS text and 1.25×/1.5× proposed board modes |
| Contrast | High-contrast option; flowing, leaking, connected, anchored states distinguishable by **shape/pattern/label**, not color alone | Complete representative boards in grayscale and color-filter conditions |
| Cognitive load | Plain-language prompts, one new rule at a time, progressive hints, reversible choices | First-minute unaided comprehension and observed stuck recovery |
| Motion | Reduced-motion toggle; no required camera drift, shimmer, butterflies or transition | Every consequence legible as still frames; 2.5D motion off by default |
| Audio | Optional sound, muted default, no audio-only instruction | Complete whole game muted; verify device ringer and volume settings |
| Haptics | Optional and subtle, never sole state signal | Unsupported devices and haptic-off mode behave identically |
| Screen readers | Descriptive button labels, tile row/column/ports, connection state, journal reading order, predictable focus | Device test of first three puzzles with VoiceOver/TalkBack **without sighted assistance**; do not claim support before passing |
| Safe areas and back | No hidden controls behind notch/home indicator; Android back predictable | Portrait phones, iPhone safe areas, OS scaling, app resume |

Official design references: [Apple accessibility HIG](https://developer.apple.com/design/human-interface-guidelines/accessibility), [Android accessibility principles](https://developer.android.com/guide/topics/ui/accessibility), [Material touch targets](https://m3.material.io/foundations/accessible-design/accessibility-basics). The touch sizes are platform guidance cited by the supplied research, **not a claim current UI passes**.

## Existing localization infrastructure

[LOCALIZATION.md](../../LOCALIZATION.md) is the canonical inventory and detection specification. Current code has **30 selectable development/research languages** (English plus 29 JSON catalogs), auto-detection, manual override, bundled offline resources and state-independent language switching. A source-level [consumer locale policy](../../Assets/Scripts/Core/LocaleShippingPolicy.cs) restricts future non-development candidates to English until specifically reviewed locales are added. All **82 compact player-facing keys** are present in the catalog inventory, but **translations are editorial drafts**. The richer original English narrative is not translated in full; `LocalizedVillage` supplies a concise cross-locale authored script.

**Available locale codes (30):** `en`, `zh-Hans`, `zh-Hant`, `es`, `pt-BR`, `fr`, `de`, `ja`, `ko`, `ar`, `hi`, `ru`, `id`, `vi`, `tr`, `th`, `it`, `nl`, `pl`, `uk`, `fa`, `he`, `ms`, `fil`, `bn`, `ur`, `ta`, `te`, `sw`, `sv`.

Current detection examples: `zh-TW`→`zh-Hant`; `zh-CN`→`zh-Hans`; `es-MX`→`es`; `pt-PT`→`pt-BR` (only supported Portuguese catalog). Unsupported languages fall back to English. Manual choice is stored separately from world progress. Verify this behavior on device, not just via .NET tests.

## Unicode and font rendering blocker (EG-006)

The current UI uses **legacy Unity uGUI Text** and best-effort OS fonts. Right alignment alone is **not** bidirectional text processing or glyph shaping. The production path should use **Unity UI Toolkit Advanced Text Generator** or an equivalent verified shaping engine, with correctly licensed font assets and a plan for fallback glyphs.

| Script/locale group | Must verify |
| --- | --- |
| Arabic `ar`, Persian `fa`, Urdu `ur`, Hebrew `he` | Bidirectional order, contextual shaping where applicable, numerals, punctuation, mixed Latin/RTL, controls and journal |
| Hindi `hi`, Bengali `bn`, Tamil `ta`, Telugu `te` | Conjuncts, ligatures, combining marks, line height, wrapping, fallback glyphs |
| Thai `th` | Word/line breaking without spaces, clipping, focus and long choice text |
| Chinese `zh-Hans/zh-Hant`, Japanese `ja`, Korean `ko` | Glyph coverage, punctuation, compact layout, consistent font fallback |
| Vietnamese `vi` and other Latin/extended Latin locales | Diacritics, line-height, truncation, long compound words and button widths |
| All 30 | Full 82-key coverage, placeholder parity, meaning, gender, cultural nuance, font license, screen reader, save independence |

Reference: [Unity Advanced Text Generator](https://docs.unity.com/en-us/engine/6000.0/manual/uitoolkits/uielements/uie-work-with-text/uie-advanced-text-generator) and [language direction](https://docs.unity.com/en-us/engine/6000.3/manual/uitoolkits/uielements/uie-work-with-text/language-direction). Do not distribute system fonts or assume OS availability.

## Test every platform and script family — not just catalog completeness

The 44×44 **points** iOS and 48×48 **density-independent pixels** Android guidelines are independent physical/UX checks, not interchangeable numeric units. Inspect actual UI hit rectangles rather than artwork bounds. System-wide text scaling **does not automatically enlarge Unity legacy uGUI Text**; an in-game text-size implementation is required before claiming it.

| Test viewport | Default and largest in-game text | High contrast / grayscale | Reduced motion | Screen reader | Script certification |
| --- | --- | --- | --- | --- | --- |
| Compact physical iPhone | Pending | Pending | Pending | VoiceOver pending | Each enabled locale pending |
| Large iPhone | Pending | Pending | Pending | VoiceOver pending | Each enabled locale pending |
| Compact physical Android | Pending | Pending | Pending | TalkBack pending | Each enabled locale pending |
| Larger Android / tablet layout | Pending | Pending | Pending | TalkBack pending | Each enabled locale pending |

**Pending** means no test evidence, not a known failure. Core device tests use `A11Y-001..005` and `LOC-002`. A screen reader cannot be certified merely because Unity `GameObject.name` contains coordinates; a semantic accessibility adapter needs an architecture spike (`EG-113`).

### Locale state machine

`Draft → Technical QA → Native QA → Store QA → Certified` (also `Rejected / Deferred`). Production builds must expose **only Certified locales**, not all 30 draft entries. The current safeguard exposes English as a **default candidate**, not as a certified locale; without English native/device/store approval, M5 remains blocked. A certified record needs reviewer identity outside public participant data, date, exact source/build hash, screenshot set, placeholders, glyph coverage, bidi/Indic shaping, line breaks, compact-layout and store-copy approval. Initially **no locale has such a record**.

Current first-stream markers in the code branch have been changed from text `S/G` to **blue waves (spring) and green sprout (garden)**; the first-stream copy has been updated in all 30 draft languages. This resolves Latin-letter dependence **in source only**. The art's contrast, recognizability and screen-reader equivalents still require device QA (`PUZ-007`, `A11Y-002`).

`pt-PT` → `pt-BR` is a **regional fallback**, not native European Portuguese. A Portugal launch must explicitly approve that fallback with a qualified reviewer, add a separate `pt-PT` catalog, or avoid claiming regional Portuguese support (`LOC-004`). Never silently equate catalog completeness with regional appropriateness.

### Complex text architecture decision

The current app still uses generated uGUI Text controls and best-effort OS fonts. **Correct direction/alignment alone is not shaping.** Evaluate Unity UI Toolkit Advanced Text Generator or a tested equivalent in an architecture spike: record shaping screenshot tests, fallback font licensing, binary/memory costs and screen-reader interaction. Native review follows technical shape/readability review. Until then, Arabic, Persian, Urdu, Hebrew and Indic scripts are **unverified release blockers if enabled**.

**Persistent save-warning source control:** the Unity warning now lives in a pinned, safe-area-aware banner outside the scrollable puzzle. When writing succeeds after an earlier failure, the banner is immediately removed rather than leaving a stale error; during failure, it stays visible while scrolling. This fixes a demonstrable UI state/scroll positioning risk in source, **not** VoiceOver/TalkBack or small-phone verification. M2 must test save-warning readability and that the reserved viewport does not clip puzzle controls.

**Additional source-only affordance:** `PipeRules.Inspect` now exposes per-tile leaking flags, and Unity UI draws a visible `!` on leaking water tiles. Verify grayscale, punctuation legibility at large text, actual touch targets and TalkBack/VoiceOver; this is **not** an accessibility certification. Five new draft help/save-warning messages bring catalogs to 81 canonical keys (82 with `ending_title`). The preview still uses legacy uGUI and does **not** implement complex-script shaping.

## Launch locale certification, not catalog count (EG-010)

A locale is **enabled for production** only after:

1. **Source parity:** all compact strings and interpolation placeholders complete, no untranslated fallback for key narrative moments.
2. **Native review:** named qualified native reviewer signs off meaning, tone, gender, numerals, culture and consistency with the game's choices.
3. **Renderer proof:** script shaping/bidi, fallback glyphs, line breaks, mixed-script examples and right-to-left focus order verified.
4. **Layout proof:** screenshots on compact Android and iPhone, normal and large text, language picker, first puzzle, all three choices, NPC dialogue, journal and ending.
5. **Behavior proof:** change language mid-puzzle, after a choice, on reload and at the ending; world events/solutions remain byte-for-byte logically equivalent.
6. **Commercial proof:** localized store title/description/screenshots reviewed, if advertised in that market.
7. **Record:** reviewer, date, device, build hash, failed screenshots and pass/fail outcome saved as evidence.

**If a locale fails, disable it in the shipping build.** The checked-in allowlist starts at English only; never broaden it without passing the corresponding native, font/layout, device and store review. Automatic locale detection and saved research preferences also pass through this policy, so they cannot bypass the shipping allowlist. Do not remove its draft catalog or claim that all 30 are production-ready. The first launch set should be based on reviewer access, market demand and device results, **not** a speculative list of "top languages."

### Example certification record (blank by design)

| Locale | Native reviewer/date | Android glyph/layout | iPhone glyph/layout | Large text | Store copy | Decision |
| --- | --- | --- | --- | --- | --- | --- |
| en | Pending | Not tested | Not tested | Not tested | Not reviewed | **Not certified** |
| ar | Pending | RTL/shaping unverified | RTL/shaping unverified | Not tested | Not reviewed | **Not certified** |
| hi | Pending | Conjunct shaping unverified | Conjunct shaping unverified | Not tested | Not reviewed | **Not certified** |
| All other locales | Pending | Not tested | Not tested | Not tested | Not reviewed | **Not certified** |

This is a **documentation baseline**, not evidence that even English has failed a test.

## Device/accessibility matrix

Minimum test set: one compact Android, one older/mid-tier Android, one notched iPhone, and a tablet or large-screen simulator for layout; **physical iPhone + Android are non-negotiable for release**. Test portrait safe areas, OS font enlargement, system high contrast/invert, screen readers, muted audio, reduced motion, offline/airplane mode, and forced app termination.

**Accessibility failure policy:** do not bury a reproducible blocker under "polish." Record exact device, OS, build, locale, steps, screenshot/video and impact; fix or narrow the claim before launch.

## Research grounding and limits

The supplied research cites older-adult usability findings about small controls, fast pacing, unclear rules and complex gestures, and official Apple/Android touch guidance. This supports **testing** a calm, large-target design with older players; it does not establish that all older adults share the same preferences or that Evergrove is accessible without device studies. See [evidence and assumptions](08-EVIDENCE-AND-DECISIONS.md).
