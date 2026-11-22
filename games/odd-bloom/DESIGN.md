# Odd Bloom — independent original design (v0.1)

**Positioning:** one gentle invitation to notice what is different, with no countdown and no performance judgment. Not a memory-treatment or cognitive-health product.

**Three adjectives:** botanical · playful · reassuring.

| Token | Hex | Function |
| --- | --- | --- |
| Cream page | `#FCF5EC` | Background |
| Ink | `#40343B` | Typography / bloom detailing |
| Muted | `#62565D` | Explanations |
| Rose accent | `#A55263` | Primary next-round action |
| Petals | `#D58C90`, `#F5C9BB` | Flower drawings |
| Leaves | `#4D8578` | Leaf and stem shapes |
| Card background | `#FFFFFF` | High clarity play surface |
| Used choice | `#947B7B` | Marked incorrect card |
| Found choice | `#247965` | Verified answer cue |

**Typography:** system font, 29px headline, 16px instructions/actions, 15–16px progress and feedback, 12–13px captions. On each bloom card a small **numeric index** helps users refer to the choice.

**Spacing:** 18px outer margin, 12px rhythm, 7px grid gutters. 3-column cards nominally 103px; 4-column cards 76px. Buttons ≥54px. Scroll on compact screens.

**Contrast:** dark text on warm light background; rose buttons with white labels. Important results appear in text and a check, never in color only. Validate actual WCAG contrast on phones separately; no physical contrast testing has been performed.

**Motion:** static icons, immediate tap feedback, no timed highlights, no screen flashes, no confetti overload. **Sound/haptics:** none unless future players independently request opt-in cues.

**Puzzle feedback:** one correct card, a short "matches the others" response on wrong selection, visible recall marks, count of tries and replay without punishment.

**Icon brief:** a cluster of simple rose-colored flowers, one with a visibly different number of petals; use original vector art, no borrowed logos.

**Screenshot brief:** first six-card screen, nine-card variation, a marked incorrect answer, the different bloom highlighted and the final 16-card screen. Screenshots are not yet physically captured.

**Accessibility:** all choices are Button controls ≥76×76px, with screen-reader index labels and no required fast gestures. The actual **visual distinction is not nonvisually perceivable**; full accessibility requires additional design and physical VoiceOver/TalkBack testing. In trials, ask players to report eye strain or tiny petal/center details rather than assuming age implies one sensory profile.

**Originality:** all flowers are drawn using native Godot vector primitives, with deterministic original rounds. Existing odd-one-out apps are direct competitors; no claim of novelty or unmet search demand.
