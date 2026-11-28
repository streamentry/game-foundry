# Game upgrade brief (one game per agent)

Repo: `/Volumes/SANDISK-Data/game-foundry/.claude/worktrees/store-catalog` (a git worktree).
Touch ONLY `games/<slug>/` (except: never edit `export_presets.cfg`, `project.godot`,
`icon.png`, `icon_bg.png`, `icon_fg.png`). No git commands. Do not change puzzle rules,
levels, solutions or save format semantics.

Godot: `G=/Volumes/SSD/godot-4.7.2/Godot.app/Contents/MacOS/Godot`

## 0. Load skills first
Use the Skill tool to load, in order: `gamedev:game-feel`, `gamedev:godot-animation`,
`gamedev:godot-ui-control`, `gamedev:puzzle`. Apply their guidance.

## 1. Understand the game
Read `AGENTS.md` (adults 40+, accessibility, no infantilizing, no health claims), root
`DESIGN.md`, then `games/<slug>/README.md`, `DESIGN.md`, `scenes/*.tscn`, `scripts/*.gd`,
`tests/*.gd`. View `games/<slug>/icon.png` with the Read tool: the in-game palette must
harmonize with the icon (same family of hues, same mood).

## 2. Write `games/<slug>/DESIGN.md` (replace the file) with these sections, specific to
this game — no generic filler:
1. Positioning & emotional promise (2–3 sentences) · three adjectives · signature interaction
2. Color tokens — table: token, hex, use. Include `bg`, `surface`, `surface_alt`, `ink`,
   `ink_muted`, `accent`, `accent_ink`, `success`, `error`, `focus`. List WCAG contrast
   ratios for ink/surface, accent_ink/accent, error/surface (each ≥ 4.5:1; compute them).
   State is never color-only (always paired with text, icon, outline or shape).
3. Typography scale — title / heading / body / caption / cell-digit sizes and weights (px)
4. Spacing & layout — base unit, grid gaps, safe margins, min touch target 48 px
5. Components — board cell, given/clue cell, controls/keypad, primary & secondary buttons
   (normal/hover/pressed/focus/disabled), panels, status banner
6. States — idle, selected, related/peer highlight, conflict, completed group, solved
7. Motion system — table: name, trigger, duration ms, easing, property. Use the shared
   module timings: press 90, pop 180, shake 260, sweep 35 ms stagger/260, intro 16 ms
   stagger/220, slide 280, celebrate ≤ 1100. Describe the game-specific signature moment.
8. Reduced motion — every motion becomes instant; celebration omitted; no flashing.
9. Haptics intent (none shipped) · Accessibility checklist · Icon & screenshot guidance
10. QA checklist (5–8 items)

## 3. Implement
- Copy `tools/store_catalog/templates/motion.gd` to `games/<slug>/scripts/motion.gd`
  (do not rewrite it; you may only change constants if DESIGN.md says so).
- In `scripts/main.gd`: `const Motion = preload("res://scripts/motion.gd")`,
  `var motion := Motion.new()`. Call it from the EXISTING event points (do not restructure):
  - level load / restart → `motion.stagger_in(<board cell controls in reading order>)`
  - select/tap a cell → `motion.press(cell)`
  - place/toggle/rotate a value → `motion.pop(cell)`
  - conflict / invalid → `motion.shake(cell_or_board)` (keep the existing text cue)
  - a row/column/region/word/path becomes complete (only if the rules make this
    meaningful and it is cheap to detect with existing rule functions) →
    `motion.sweep(cells_of_that_group, <accent color>)`
  - solved → `motion.celebrate(self, <board global center>, [accent, success, surface])`
    plus `motion.sweep(all_cells)`
  - next/previous level → `motion.slide_swap(<board container>, ±1)` right after the new
    level is applied
  Game state must update immediately; animations only follow.
- Theme: apply the DESIGN.md tokens through `StyleBoxFlat` (rounded corners 10–16 px, a
  subtle shadow on raised surfaces via `shadow_size`/`shadow_color`, distinct
  hover/pressed/focus/disabled states) and font colors/sizes. Reuse the game's existing
  theming function if there is one; keep layout intact, keep text legible at 414×896.
- Reduce motion: add a visible toggle (a `CheckButton` or small `Button` labelled
  "Reduce motion") in the existing controls area. Persist it with the game's existing
  progress/save script if it has a settings field; otherwise in `user://settings.json`
  (`{"reduced_motion": bool}`). Set `motion.reduced_motion` on load and when toggled.

## 4. Test
- Add `games/<slug>/tests/test_motion.gd` (extends SceneTree, same style as the other
  tests): instance `res://scenes/main.tscn`, set reduced motion on, perform a select +
  place (use the methods the existing scene test uses), assert `Motion.running(cell) == 0`
  and offset_transform_scale is `Vector2.ONE`; then turn reduced motion off, place again,
  wait ~0.6 s of frames, assert the cell is back at rest. Print a summary and
  `quit(0 if failures == 0 else 1)`.
- Run:
  `$G --headless --path games/<slug> --editor --import --quit`
  then every `games/<slug>/tests/test_*.gd` with
  `$G --headless --path games/<slug> --script res://tests/<file>`.
  Each must exit 0 with 0 failures, and output must contain no `SCRIPT ERROR`,
  `Parse Error` or `ERROR:` lines. Fix until clean.

## 5. Look at it
`$G --path games/<slug> --windowed --resolution 200x200 --script /Volumes/SANDISK-Data/game-foundry/.claude/worktrees/store-catalog/tools/store_catalog/capture.gd -- /private/tmp/claude-501/-Volumes-SANDISK-Data-game-foundry/6044a27f-67e5-4233-8e31-3707e3298442/scratchpad/design-<slug> 1242 2688 414 896`
then Read `01.png` and `02.png`. Fix overlaps, clipped text, low contrast or a palette that
fights the icon. Iterate at most 3 times.

## 6. Report (≤ 15 lines)
Palette (4 key hexes), signature motion, files changed, test results per file
(checks/failures), screenshot path, anything not done.
