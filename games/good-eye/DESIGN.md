# Good Eye — original product design (v0.1 playtest)

**Positioning:** a calm visual judgment challenge: split an irregular shape as evenly as possible, then see how close you came. For adults who enjoy small puzzles; do not label the game as a cognitive treatment or a "senior" product. **Three adjectives:** warm, precise, quietly playful.

## Design system

| Token | Color | Purpose |
| --- | --- | --- |
| Paper | `#F8F6EF` | Background |
| Ink | `#223D35` | Headings and cut-guide ink |
| Board | `#EFECE4` | Separate play surface |
| Muted | `#52675E` | Supporting text |
| Accent | `#24786D` | Main action and active mode |
| Shape | `#DFC68F` | Uncut ochre silhouette |
| Area A | `#6FA99C` | Revealed area |
| Area B | `#DCA36D` | Revealed area |

**Contrast:** dark ink on pale paper; white on deep-teal primary controls. All outcome information also appears as **numeric text** (A %, B %, error); never communicate correctness only through color. Final screen-reader and high-contrast verification must occur on actual phones.

**Type scale:** system-font fallback, title 30 px, score 24 px, instruction/buttons 16 px, supporting 12–14 px. **Spacing:** 8 px between actions, 12 px between sections, 18 px side padding; touch buttons at least 54 px high. Allow scroll on compact screens.

**Motion:** immediate guide adjustments, restrained 7 px visual separation after cut, no forced animation or streaks. **Sound/haptics:** none in this experiment; optional controls only if users request them. **Feedback:** actual clipped polygon area (shoelace formula), complementary percentages, percentage-point error and a 0–100 score. Hide the percentages until the user commits.

**Icon brief:** a unique ochre irregular shape bisected by one deliberate dark green cut; create original art rather than adapting a competitor icon. **Screenshot plan:** first cut, revealed split, daily five progress, controls on a compact phone. These are future assets, not existing screenshots.

## Accessible interaction / open gates

A player can drag the line on a large canvas, use four precision buttons, or use keyboard arrows and Enter. No timer, account, ads or required network. Touch targets meet the intended 48 logical-pixel baseline by height. Screen-reader descriptions are provided for controls, but nonvisual shape comprehension, VoiceOver/TalkBack, larger OS text, screen-size adaptation, accidental scrolling, safe areas and motor comfort **remain to be verified on physical phones**.

Product taste is judged on real devices with adults in target age bands. No reliance on unproven vision, memory or health claims; do not copy competitors' levels, iconography, identity or trade dress.
