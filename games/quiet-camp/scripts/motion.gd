## Motion system for Control-based puzzle boards (Godot 4.7).
##
## Visual-only feedback: every effect animates Control.offset_transform_* (which
## containers do not reset and which never moves layout or hit areas) or modulate,
## and always ends exactly at rest. Game state never waits for an animation.
## Tiers follow the game-feel guide: small (press/select), medium (place, conflict,
## completed group), large (solve). Set `reduced_motion = true` to make every call
## instant: final state applied synchronously, no tweens created.
##
## Usage (in main.gd):
##   const Motion = preload("res://scripts/motion.gd")
##   var motion := Motion.new()            # RefCounted; keep one per scene
##   motion.reduced_motion = settings.reduced_motion
##   motion.press(cell); motion.pop(cell); motion.shake(cell)
##   motion.stagger_in(cells); motion.sweep(row_cells, accent)
##   motion.celebrate(self, board.get_global_rect().get_center(), palette)
##   motion.slide_swap(board, direction)   # level transition
extends RefCounted

const PRESS_MS := 90
const POP_MS := 180
const SHAKE_MS := 260
const SWEEP_STEP_MS := 35
const SWEEP_MS := 260
const INTRO_STEP_MS := 16
const INTRO_MS := 220
const SLIDE_MS := 280
const CELEBRATE_MS := 1100

var reduced_motion := false
## Optional: a node to own tweens (needed in headless tests). Defaults to the target.
var owner: Node = null


static func _prep(c: Control) -> void:
	c.offset_transform_enabled = true
	c.offset_transform_pivot_ratio = Vector2(0.5, 0.5)


func _rest(c: Control) -> void:
	_prep(c)
	c.offset_transform_scale = Vector2.ONE
	c.offset_transform_rotation = 0.0
	c.offset_transform_position = Vector2.ZERO
	c.modulate.a = 1.0


func _tween_for(c: Control, key: StringName) -> Tween:
	var old: Variant = c.get_meta(key) if c.has_meta(key) else null
	if old is Tween and (old as Tween).is_valid():
		(old as Tween).kill()
	var host: Node = owner if owner != null else c
	var tw := host.create_tween()
	c.set_meta(key, tw)
	return tw


## Small tier: a quick dip that confirms a touch (select / tap).
func press(c: Control) -> void:
	if reduced_motion or not c.is_inside_tree():
		_rest(c)
		return
	_prep(c)
	var tw := _tween_for(c, &"_m_scale")
	c.offset_transform_scale = Vector2(0.92, 0.92)
	tw.tween_property(c, "offset_transform_scale", Vector2.ONE, PRESS_MS / 1000.0) \
		.set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)


## Medium tier: value placed. Squash-and-stretch overshoot that settles at 1.
func pop(c: Control, strength := 1.0) -> void:
	if reduced_motion or not c.is_inside_tree():
		_rest(c)
		return
	_prep(c)
	var tw := _tween_for(c, &"_m_scale")
	c.offset_transform_scale = Vector2(1.0 + 0.16 * strength, 1.0 - 0.10 * strength)
	tw.tween_property(c, "offset_transform_scale", Vector2.ONE, POP_MS / 1000.0) \
		.set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)


## Medium tier: rule conflict. Damped horizontal shake (visual only), ends at rest.
func shake(c: Control, amplitude := 7.0) -> void:
	if reduced_motion or not c.is_inside_tree():
		_rest(c)
		return
	_prep(c)
	var tw := _tween_for(c, &"_m_pos")
	var step := SHAKE_MS / 1000.0 / 6.0
	for i in 5:
		var a := amplitude * (1.0 - i / 5.0) * (1.0 if i % 2 == 0 else -1.0)
		tw.tween_property(c, "offset_transform_position", Vector2(a, 0), step) \
			.set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	tw.tween_property(c, "offset_transform_position", Vector2.ZERO, step)


## Soft attention pulse (hint / selected peer). Returns to rest.
func pulse(c: Control, amount := 0.06) -> void:
	if reduced_motion or not c.is_inside_tree():
		_rest(c)
		return
	_prep(c)
	var tw := _tween_for(c, &"_m_scale")
	tw.tween_property(c, "offset_transform_scale", Vector2.ONE * (1.0 + amount), 0.12) \
		.set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_OUT)
	tw.tween_property(c, "offset_transform_scale", Vector2.ONE, 0.16) \
		.set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)


## Board intro: cells rise and fade in with a short stagger (reading order).
func stagger_in(cells: Array) -> void:
	for i in cells.size():
		var c: Control = cells[i]
		if reduced_motion or not c.is_inside_tree():
			_rest(c)
			continue
		_prep(c)
		c.offset_transform_scale = Vector2(0.86, 0.86)
		c.offset_transform_position = Vector2(0, 10)
		c.modulate.a = 0.0
		var tw := _tween_for(c, &"_m_intro")
		tw.set_parallel(true)
		var delay := minf(i * INTRO_STEP_MS, 420) / 1000.0
		tw.tween_property(c, "offset_transform_scale", Vector2.ONE, INTRO_MS / 1000.0) \
			.set_delay(delay).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
		tw.tween_property(c, "offset_transform_position", Vector2.ZERO, INTRO_MS / 1000.0) \
			.set_delay(delay).set_trans(Tween.TRANS_CUBIC).set_ease(Tween.EASE_OUT)
		tw.tween_property(c, "modulate:a", 1.0, INTRO_MS / 1000.0 * 0.8).set_delay(delay)


## Medium tier: a completed row / region / word. A lift-and-glow wave across cells.
## `glow` tints self_modulate briefly and returns to white (state colors live in styles).
func sweep(cells: Array, glow := Color(1.0, 0.92, 0.6)) -> void:
	for i in cells.size():
		var c: Control = cells[i]
		if reduced_motion or not c.is_inside_tree():
			_rest(c)
			c.self_modulate = Color.WHITE
			continue
		_prep(c)
		var tw := _tween_for(c, &"_m_sweep")
		var delay := i * SWEEP_STEP_MS / 1000.0
		tw.tween_interval(delay)
		tw.set_parallel(true)
		tw.tween_property(c, "offset_transform_scale", Vector2(1.08, 1.08), SWEEP_MS / 2000.0) \
			.set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_OUT)
		tw.tween_property(c, "self_modulate", glow, SWEEP_MS / 2000.0)
		tw.set_parallel(false)
		tw.tween_property(c, "offset_transform_scale", Vector2.ONE, SWEEP_MS / 2000.0) \
			.set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
		tw.parallel().tween_property(c, "self_modulate", Color.WHITE, SWEEP_MS / 1000.0)


## Level transition: the board slides out, the new content is applied by the caller
## immediately (state never waits), then the board slides in from the other side.
func slide_swap(board: Control, direction := 1) -> void:
	if reduced_motion or not board.is_inside_tree():
		_rest(board)
		return
	_prep(board)
	var tw := _tween_for(board, &"_m_slide")
	var dx := 48.0 * direction
	board.offset_transform_position = Vector2(dx, 0)
	board.modulate.a = 0.0
	tw.set_parallel(true)
	tw.tween_property(board, "offset_transform_position", Vector2.ZERO, SLIDE_MS / 1000.0) \
		.set_trans(Tween.TRANS_CUBIC).set_ease(Tween.EASE_OUT)
	tw.tween_property(board, "modulate:a", 1.0, SLIDE_MS / 1000.0)


## Large tier: solved. A restrained burst of soft tiles from `center` (global coords)
## inside `layer` (any Control that covers the screen), plus nothing that flashes.
## ≤ 22 pieces, ≤ 1.1 s, all freed afterwards. Reduced motion: no pieces at all.
func celebrate(layer: Control, center: Vector2, palette: Array = []) -> void:
	if reduced_motion or not layer.is_inside_tree():
		return
	var colors: Array = palette if not palette.is_empty() else [Color("#F3C76B"), Color("#F28C6B"), Color("#FBF6EA")]
	var rng := RandomNumberGenerator.new()
	rng.seed = int(center.x * 31 + center.y * 17)
	var local := center - layer.get_global_rect().position
	for i in 22:
		var p := Panel.new()
		var sb := StyleBoxFlat.new()
		sb.bg_color = colors[i % colors.size()]
		sb.set_corner_radius_all(4 if i % 3 else 10)
		p.add_theme_stylebox_override("panel", sb)
		p.mouse_filter = Control.MOUSE_FILTER_IGNORE
		var s := rng.randf_range(8.0, 16.0)
		p.size = Vector2(s, s * (1.0 if i % 2 else 0.6))
		p.position = local - p.size / 2
		p.pivot_offset = p.size / 2
		layer.add_child(p)
		var ang := TAU * i / 22.0 + rng.randf_range(-0.2, 0.2)
		var dist := rng.randf_range(120.0, 230.0)
		var end := p.position + Vector2(cos(ang), sin(ang)) * dist + Vector2(0, 60)
		var host: Node = owner if owner != null else layer
		var tw := host.create_tween().set_parallel(true)
		var dur := CELEBRATE_MS / 1000.0 * rng.randf_range(0.75, 1.0)
		tw.tween_property(p, "position", end, dur).set_trans(Tween.TRANS_CUBIC).set_ease(Tween.EASE_OUT)
		tw.tween_property(p, "rotation", rng.randf_range(-3.0, 3.0), dur)
		tw.tween_property(p, "modulate:a", 0.0, dur * 0.5).set_delay(dur * 0.5)
		tw.chain().tween_callback(p.queue_free)


## Number of motion tweens currently running on `c` (for tests).
static func running(c: Control) -> int:
	var n := 0
	for key in [&"_m_scale", &"_m_pos", &"_m_intro", &"_m_sweep", &"_m_slide"]:
		var t: Variant = c.get_meta(key) if c.has_meta(key) else null
		if t is Tween and (t as Tween).is_valid() and (t as Tween).is_running():
			n += 1
	return n
