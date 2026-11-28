extends Control
## Draws Kropki dot markers in the gaps between board cells, using the cells' real rects.
## W (consecutive) = cream fill with an ink outline; B (double) = solid ink fill.
## Ring: success green when both cells are filled and the relation holds, error when
## violated, none while either cell is still empty. Visual only; mouse input is ignored.

const CREAM: Color = Color("#FFF8E7")
const INK: Color = Color("#242949")
const SUCCESS: Color = Color("#1E6B55")
const ERROR: Color = Color("#A3322E")
const DOT_SCALE: float = 0.2  # diameter as a fraction of the cell's shorter side

var cells: Array = []
var dots: Array = []
var entries: Array = []


func _draw() -> void:
	if cells.is_empty() or dots.is_empty() or entries.is_empty():
		return
	var origin: Vector2 = get_global_rect().position
	for dot in dots:
		var a: int = int(dot[0])
		var b: int = int(dot[1])
		var kind: String = str(dot[2])
		var ra: Rect2 = _local_rect(cells[a], origin)
		var rb: Rect2 = _local_rect(cells[b], origin)
		var center: Vector2
		if b == a + 1:
			center = Vector2((ra.end.x + rb.position.x) * 0.5, ra.get_center().y)
		else:
			center = Vector2(ra.get_center().x, (ra.end.y + rb.position.y) * 0.5)
		var radius: float = minf(ra.size.x, ra.size.y) * DOT_SCALE * 0.5
		var status: int = _status(a, b, kind)
		if status != 0:
			draw_arc(center, radius + 4.0, 0.0, TAU, 32, SUCCESS if status == 1 else ERROR, 3.0, true)
		if kind == "W":
			draw_circle(center, radius, CREAM)
			draw_arc(center, radius, 0.0, TAU, 32, INK, 2.0, true)
		else:
			draw_circle(center, radius, INK)


func _local_rect(cell: Control, origin: Vector2) -> Rect2:
	var r: Rect2 = cell.get_global_rect()
	return Rect2(r.position - origin, r.size)


## 1 = satisfied, -1 = violated, 0 = unknown (a cell is still empty).
func _status(a: int, b: int, kind: String) -> int:
	var va: int = int(entries[a])
	var vb: int = int(entries[b])
	if va == 0 or vb == 0:
		return 0
	var ok: bool
	if kind == "W":
		ok = absi(va - vb) == 1
	else:
		ok = maxi(va, vb) == 2 * mini(va, vb)
	return 1 if ok else -1
