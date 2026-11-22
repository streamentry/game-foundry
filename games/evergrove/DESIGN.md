# Evergrove — distinct product design

**Positioning:** A place worth returning to because your small choices matter. Calm, legible, mobile-first and emotionally warm without artificial urgency. Not merely a reskin of Pipe Garden; the distinguishing gameplay loop is the *persistent, causal village* around the same control puzzle.

## Visual identity

- **Three adjectives:** tender · grounded · alive.
- **Typography scale:** 26px title, 17px counters, 15–16px instructions and outcomes, 12–14px hints. Use native fallback fonts with no external licensed assets.
- **Semantic color tokens:** paper `#F7F4E9`, evergreen ink `#23443C`, muted moss `#516B60`, active leaf `#276D54`, pale moss `#DFEBDD`, warm sand `#F3E5C9`, running-water blue `#2D7E8D`, white `#FFFFFF`.
- **Contrast:** text should meet 4.5:1 against its actual background wherever practical; inspect on phones and adjust tokens if any combination fails. Never encode correctness with color alone.
- **Layout/spacing:** 18px side margins, 12px vertical rhythm, 8px tile gaps, corner radii 13–17px, minimum 56px actionable buttons and 88px pipe tiles (at 414 logical px reference resolution).
- **Signature interaction:** rotate a tactile stream tile, see the whole stream complete, then choose one lasting change to the village.
- **Motion:** none required for comprehension. A future 150–250ms water-flow reveal may be added only after reduced-motion and low-end performance checks.
- **Sound/haptics:** silent by default; future optional soft water sound and gentle haptic only after explicit testing. No autoplay music.
- **Feedback:** display complete/blocked state in prose, not just hue; Undo and Restart are always available during a puzzle.
- **Icon brief:** independently drawn seed inside an imperfect circle; warm, simple, non-branded. No purchased artwork in this prototype.
- **App Store screenshots (not produced):** 1) 3×3 creek and one-tap instruction; 2) completed stream with exactly three consequential choices; 3) remembering characters and combined village consequences. Validate on real phones before public use.

## Accessible play

No countdown, score pressure, guilt-based streak, input timing challenge or mandatory account. Tile names contain positions, but Unity screen-reader semantics and focus order have **not yet been verified**. Test and repair before a public release. Scroll supports short screens. Test compact phones, large phones, font scaling, screen readers, landscape, iOS/Android safe areas, focus order and contrast with the intended participants before shipping.

## Differentiate with design evidence, not art promises

**Benchmark against** the same Pipe Garden board in puzzle-only mode. The question is not whether players like a cute garden aesthetic—it is whether remembered decisions and consistent NPC reactions improve return. If the player cannot predict why the world changed, the system has failed.

## Unity UI implementation note

The first playable C# slice programmatically constructs uGUI elements with large buttons and a scrollable portrait layout. This is a functional systems study rather than a final visual identity; verify hierarchy, content-sizing, touch semantics, contrast, device safe areas and reduced-motion preferences on actual phones before approval.
