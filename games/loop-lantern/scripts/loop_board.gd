extends Control
## Original vector square-and-edge board; tap a square to select it.
signal cell_chosen(index: int)

## Palette harmonised with icon.png: plum ground, gold line, lavender guides.
const PAPER = Color("#3B1450")
const INK = Color("#F7EFDC")
const GUIDES = Color("#8A6FA3")
const LOOP = Color("#FDD241")
const DOTS = Color("#C9B3DE")
const CURSOR_FILL = Color(1.0, 0.82, 0.26, 0.22)
const CURSOR_EDGE = Color("#FDD241")

var level: Dictionary = {}
var entries: Array = []
var selected: int = 0
## Visual-only selection square; a child Control so motion.gd can press/pop it.
var cursor: Control

func _ready() -> void:
    cursor = Control.new()
    cursor.mouse_filter = Control.MOUSE_FILTER_IGNORE
    cursor.draw.connect(_draw_cursor)
    add_child(cursor)
    resized.connect(_place_cursor)

func show_level(board: Dictionary, marks: Array, chosen: int) -> void:
    level = board
    entries = marks.duplicate()
    selected = chosen
    queue_redraw()
    _place_cursor()

func _place_cursor() -> void:
    if cursor == null:
        return
    var cell: float = _side() / 3.0
    cursor.position = _origin() + Vector2(selected % 3, int(selected / 3)) * cell
    cursor.size = Vector2.ONE * cell
    cursor.visible = not level.is_empty()
    cursor.queue_redraw()

func _draw_cursor() -> void:
    cursor.draw_rect(Rect2(Vector2.ZERO, cursor.size), CURSOR_FILL)
    cursor.draw_rect(Rect2(Vector2.ZERO, cursor.size), CURSOR_EDGE, false, 3.0)

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
        draw_circle(_vertex(point), 3.5, DOTS)

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
