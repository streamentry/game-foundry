extends Control
## Sand-gold capsules behind the letters of found words. Drawn from the cells'
## real rects, so it follows layout and stays aligned at any width. Each capsule
## grows from the first letter to the last in GROW_SECONDS; reduced motion shows
## the finished capsule at once. Visual only: game state never waits for it.
const FILL := Color("#F2C97D")
const GROW_SECONDS := 0.26
const RADIUS_RATIO := 0.44

var cells: Array = []
var reduced_motion := false
var _items: Array = []   # each: {"a": cell index, "b": cell index, "t": 0.0..1.0}
var _tweens: Array = []


func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_IGNORE
	resized.connect(queue_redraw)


func clear() -> void:
	for tw in _tweens:
		if (tw as Tween).is_valid():
			(tw as Tween).kill()
	_tweens.clear()
	_items.clear()
	queue_redraw()


## Adds a capsule from cell `a` to cell `b`. Returns its index.
func add_word(a: int, b: int, animate: bool) -> int:
	var index := _items.size()
	var instant := reduced_motion or not animate or not is_inside_tree()
	_items.append({"a": a, "b": b, "t": 1.0 if instant else 0.0})
	if instant:
		queue_redraw()
		return index
	var tw := create_tween()
	_tweens.append(tw)
	tw.tween_method(_set_progress.bind(index), 0.0, 1.0, GROW_SECONDS) \
		.set_trans(Tween.TRANS_CUBIC).set_ease(Tween.EASE_OUT)
	return index


func _set_progress(value: float, index: int) -> void:
	if index < _items.size():
		_items[index]["t"] = value
		queue_redraw()


func _center(index: int) -> Vector2:
	var cell: Control = cells[index]
	return get_global_transform().affine_inverse() * cell.get_global_rect().get_center()


func _radius() -> float:
	if cells.is_empty():
		return 24.0
	var cell_size: Vector2 = cells[0].size
	return minf(cell_size.x, cell_size.y) * RADIUS_RATIO


func _draw() -> void:
	if cells.is_empty():
		return
	var r := _radius()
	for item in _items:
		var a := _center(int(item["a"]))
		var b := _center(int(item["b"]))
		var tip := a.lerp(b, clampf(float(item["t"]), 0.0, 1.0))
		_draw_capsule(a, tip, r)


func _draw_capsule(a: Vector2, b: Vector2, r: float) -> void:
	draw_circle(a, r, FILL)
	var dir := b - a
	if dir.length() < 0.5:
		return
	draw_circle(b, r, FILL)
	var n := dir.normalized().orthogonal() * r
	draw_colored_polygon(PackedVector2Array([a + n, b + n, b - n, a - n]), FILL)
