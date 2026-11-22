extends Control
## Original vector square-and-edge board; tap a square to select it.
signal cell_chosen(index: int)

const PAPER = Color("#FDF8F0")
const INK = Color("#4C414A")
const GUIDES = Color("#D7C9C2")
const HIGHLIGHT = Color("#F6E5C6")
const LOOP = Color("#A6534F")
const WHITE = Color("#FFFFFF")

var level: Dictionary = {}
var entries: Array = []
var selected: int = 0

func show_level(board: Dictionary, marks: Array, chosen: int) -> void:
    level = board
    entries = marks.duplicate()
    selected = chosen
    queue_redraw()

func _side() -> float:
    return maxf(1.0, minf(size.x - 22.0, size.y - 22.0))

func _origin() -> Vector2:
    return (size - Vector2.ONE * _side()) * 0.5

func _vertex(index: int) -> Vector2:
    return _origin() + Vector2(float(index % 4), float(int(index / 4))) * (_side() / 3.0)

func _draw() -> void:
    if level.is_empty():
        return
    draw_rect(Rect2(_origin(), Vector2.ONE * _side()), PAPER)
    var cell: float = _side() / 3.0
    var r: int = int(selected / 3)
    var c: int = selected % 3
    draw_rect(Rect2(_origin() + Vector2(c, r) * cell, Vector2.ONE * cell), HIGHLIGHT)
    var font: Font = ThemeDB.fallback_font
    for id in range(9):
        var clue: int = int(level["clues"][id])
        if clue < 0:
            continue
        var row: int = int(id / 3)
        var col: int = id % 3
        var center: Vector2 = _origin() + (Vector2(col, row) + Vector2(0.5, 0.5)) * cell
        var label: String = str(clue)
        var dimensions: Vector2 = font.get_string_size(label, HORIZONTAL_ALIGNMENT_LEFT, -1, 31)
        draw_string(font, center + Vector2(-dimensions.x * 0.5, dimensions.y * 0.35),
            label, HORIZONTAL_ALIGNMENT_LEFT, -1, 31, INK)
    for edge_id in range(entries.size()):
        var a: Array = _edge_endpoints(edge_id)
        draw_line(_vertex(int(a[0])), _vertex(int(a[1])),
            LOOP if int(entries[edge_id]) == 1 else GUIDES,
            7.0 if int(entries[edge_id]) == 1 else 2.0, true)
    for point in range(16):
        draw_circle(_vertex(point), 3.5, INK)

func _edge_endpoints(id: int) -> Array:
    var i: int = 0
    for row in range(4):
        for col in range(4):
            var v: int = row * 4 + col
            if col < 3:
                if i == id:
                    return [v, v + 1]
                i += 1
            if row < 3:
                if i == id:
                    return [v, v + 4]
                i += 1
    return []

func _select_at(position: Vector2) -> void:
    var p: Vector2 = (position - _origin()) / _side()
    if p.x >= 0.0 and p.x < 1.0 and p.y >= 0.0 and p.y < 1.0:
        cell_chosen.emit(clampi(int(p.y * 3.0), 0, 2) * 3 + clampi(int(p.x * 3.0), 0, 2))

func _gui_input(event: InputEvent) -> void:
    if event is InputEventScreenTouch:
        if event.pressed:
            _select_at(event.position)
        accept_event()
    elif event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT:
        if event.pressed:
            _select_at(event.position)
        accept_event()
