# One Path — independent product design (v0.1 experiment)

**Positioning:** an elegant, untimed graph puzzle. Trace every connection exactly once. No brain-health, disease, or "senior" claims.

**Three adjectives:** tactile · thoughtful · warmly minimal.

## Visual design system

| Token | Color | Purpose |
| --- | --- | --- |
| Page | `#FBF4EB` | Warm paper backdrop |
| Board | `#F1E9DE` | Play surface |
| Ink | `#34363B` | Text and node outlines |
| Muted | `#605F60` | Instructions |
| Accent | `#A25138` | Primary action |
| Edges | `#B9B6AD` | Unused connections |
| Finished path | `#D07651` | Completed connections |
| Dots | `#E7B488` | Warm vertex fill |
| Start hint | `#2F7A72` | Permitted open-trail starts |

**Typography:** system fallback, title 29px, feedback 15–16px, buttons 16px, captions 12–13px. Labels use letters A–H for an alternate touch and screen-reader route.

**Spacing:** outer 18px, vertical rhythm 11px, actions 8px, board minimum 338px, interactive letter/buttons at least 54px tall. Scroll when the viewport is small.

**Contrast and feedback:** dark-on-cream labels, all route progress given as text ("X of Y lines"), not only stroke colors. Mark odd-degree endpoints with a colored ring, but instructions and letter buttons also communicate where to start.

**Motion:** no automatic or timed motion. A completed edge appears instantly; Undo reverses one. No streak or confetti pressure.

**Sound/haptics:** none in the prototype; only add optionally after evidence.

**Puzzle feedback:** legal route segments thicken, current vertex highlighted, invalid moves explain why, and dead ends prompt an undo instead of grading the user as "wrong."

**Icon brief:** a single terracotta route line tracing several linked paper-colored dots, with one subtle crossover that is not a junction. Must be original.

**Screenshot plan:** first trail board, branch with visible current vertex, explicit non-junction crossing, large letter selection grid. These are future device captures.

## Accessibility and constraints

Finger tap/drag on 40px visual dots uses a larger (52px+) input hit area. Eight **54px minimum letter buttons** provide a nonprecision alternative. Undo/Restart always remain independent from gesture accuracy. Screen-reader labels on controls are present but custom geometry descriptions and device voice navigation require physical testing; full blind navigation is **not** claimed yet.

**Originality:** graph layouts, nomenclature, visuals and UX are authored here; generic Euler trails are mathematical mechanics, not permission to copy existing level packs or trade dress.
