extends Control
## One thermometer drawn as a single glass tube: a round bulb over its first cell,
## the glass running across its tip cell. Visual only; mouse input passes through to
## the cell buttons underneath.
const Palette = preload("res://scripts/palette.gd")
const BULB_RADIUS: float = 26.0
const GLASS_HALF: float = 14.0
const MERCURY_HALF: float = 8.0
const MERCURY_BULB: float = 19.0
const OUTLINE: float = 2.0
const CAP_STEPS: int = 12

## Index of the bulb cell (reading order for the intro stagger).
var bulb_cell: int = -1
## Fill in segments: 0 empty, 1 bulb segment, 2 both segments. Animated between values.
var fill_amount: float = 0.0
var _bulb_at: Vector2 = Vector2.ZERO
var _tip_at: Vector2 = Vector2.ZERO
var _fill_tween: Tween = null


func _init() -> void:
	mouse_filter = Control.MOUSE_FILTER_IGNORE


## Rects are in the parent board's coordinates.
func set_geometry(bulb_rect: Rect2, tip_rect: Rect2) -> void:
	var span: Rect2 = bulb_rect.merge(tip_rect).grow(4.0)
	position = span.position
	size = span.size
	_bulb_at = bulb_rect.get_center() - span.position
	_tip_at = tip_rect.get_center() - span.position
	queue_redraw()


## Rises or falls the mercury to `target`. Instant applies the value at once.
func animate_fill(target: float, seconds: float, instant: bool) -> void:
	if _fill_tween != null and _fill_tween.is_valid():
		_fill_tween.kill()
	_fill_tween = null
	if instant or not is_inside_tree() or is_equal_approx(fill_amount, target):
		_set_fill(target)
		return
	_fill_tween = create_tween()
	_fill_tween.tween_method(_set_fill, fill_amount, target, seconds) \
		.set_trans(Tween.TRANS_CUBIC).set_ease(Tween.EASE_OUT)


func _set_fill(value: float) -> void:
	fill_amount = value
	queue_redraw()


func _draw() -> void:
	var cell_length: float = _bulb_at.distance_to(_tip_at)
	if cell_length <= 0.0:
		return
	var axis: Vector2 = (_tip_at - _bulb_at) / cell_length
	var glass_end: Vector2 = _bulb_at + axis * (cell_length * 1.5 - GLASS_HALF)
	_capsule(_bulb_at, glass_end, GLASS_HALF + OUTLINE, Palette.INK)
	draw_circle(_bulb_at, BULB_RADIUS + OUTLINE, Palette.INK)
	_capsule(_bulb_at, glass_end, GLASS_HALF, Palette.SURFACE)
	draw_circle(_bulb_at, BULB_RADIUS, Palette.SURFACE)
	if fill_amount <= 0.001:
		return
	var top: float = _surface_distance(fill_amount, cell_length)
	_capsule(_bulb_at, _bulb_at + axis * top, MERCURY_HALF, Palette.ACCENT)
	draw_circle(_bulb_at, MERCURY_BULB, Palette.ACCENT)
	draw_circle(_bulb_at + Vector2(-7.0, -7.0), 3.5, Palette.HIGHLIGHT)


## Distance from the bulb centre to the liquid surface. Segment 1 ends at the
## bulb/tip boundary; segment 2 ends at the top of the glass.
func _surface_distance(amount: float, cell_length: float) -> float:
	var half: float = cell_length * 0.5
	var glass_top: float = cell_length * 1.5 - GLASS_HALF
	if amount <= 1.0:
		return half * clampf(amount, 0.0, 1.0)
	return half + (glass_top - half) * clampf(amount - 1.0, 0.0, 1.0)


func _capsule(a: Vector2, b: Vector2, radius: float, color: Color) -> void:
	var angle: float = (b - a).angle()
	var points := PackedVector2Array()
	for i in range(CAP_STEPS + 1):
		var t: float = angle + PI * 0.5 + PI * float(i) / CAP_STEPS
		points.append(a + Vector2(cos(t), sin(t)) * radius)
	for i in range(CAP_STEPS + 1):
		var t: float = angle - PI * 0.5 + PI * float(i) / CAP_STEPS
		points.append(b + Vector2(cos(t), sin(t)) * radius)
	draw_colored_polygon(points, color)
