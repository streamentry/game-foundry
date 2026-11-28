extends Control
## Large original vector island-map view with repeatable exact positions.
signal island_pressed(index: int)

const WATER = Color("#E6F0EC")
const GUIDE = Color("#C9D6CF")
const INK = Color("#173B42")
const BRIDGE = Color("#1E6E76")
const GOLD = Color("#F2C46D")
const FOCUS = Color("#B5770D")

var level: Dictionary = {}
var bridges: Array = []
var selected: int = -1

func show_level(board: Dictionary, strengths: Array, selected_node: int) -> void:
    level = board
    bridges = strengths.duplicate()
    selected = selected_node
    queue_redraw()

func _origin() -> Vector2:
    return (size - Vector2.ONE * _span()) * 0.5

func _span() -> float:
    return maxf(1.0, minf(size.x - 16.0, size.y - 16.0))

func _point(index: int) -> Vector2:
    var pos: Array = level["nodes"][index]
    var inset: float = _span() * 0.10
    var cell: float = (_span() - inset * 2.0) / 4.0
    return _origin() + Vector2(inset + float(pos[1]) * cell,
        inset + float(pos[0]) * cell)

func _draw() -> void:
    if level.is_empty():
        return
    draw_rect(Rect2(_origin(), Vector2.ONE * _span()), WATER, true)
    draw_rect(Rect2(_origin(), Vector2.ONE * _span()), GUIDE, false, 2.0)
    for id in range(level["edges"].size()):
        var pair: Array = level["edges"][id]
        var a: Vector2 = _point(int(pair[0]))
        var b: Vector2 = _point(int(pair[1]))
        draw_line(a, b, GUIDE, 3.0, true)
        var strength: int = int(bridges[id])
        if strength == 1:
            draw_line(a, b, BRIDGE, 7.0, true)
        elif strength == 2:
            var normal: Vector2 = (b - a).normalized().orthogonal() * 5.0
            draw_line(a + normal, b + normal, BRIDGE, 5.0, true)
            draw_line(a - normal, b - normal, BRIDGE, 5.0, true)
    var font: Font = ThemeDB.fallback_font
    for id in range(level["nodes"].size()):
        var p: Vector2 = _point(id)
        draw_circle(p, 25.0 if id == selected else 23.0, FOCUS if id == selected else INK)
        draw_circle(p, 21.0 if id == selected else 19.0, GOLD)
        var text_value: String = str(level["clues"][id])
        var dimensions: Vector2 = font.get_string_size(text_value, HORIZONTAL_ALIGNMENT_LEFT, -1, 23)
        draw_string(font, p + Vector2(-dimensions.x * 0.5, dimensions.y * 0.35), text_value,
            HORIZONTAL_ALIGNMENT_LEFT, -1, 23, INK)
        draw_string(font, p + Vector2(-25.0, -27.0), char(65 + id),
            HORIZONTAL_ALIGNMENT_LEFT, -1, 13, INK)

func _hit_at(position: Vector2) -> int:
    if level.is_empty():
        return -1
    var best: int = -1
    var distance: float = 33.0
    for i in range(level["nodes"].size()):
        var d: float = position.distance_to(_point(i))
        if d < distance:
            best = i
            distance = d
    return best

func _gui_input(event: InputEvent) -> void:
    if event is InputEventScreenTouch:
        if event.pressed:
            var index: int = _hit_at(event.position)
            if index >= 0:
                island_pressed.emit(index)
        accept_event()
    elif event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT:
        if event.pressed:
            var index: int = _hit_at(event.position)
            if index >= 0:
                island_pressed.emit(index)
        accept_event()
