extends Control
## Graph-first drawing and touch tracing. A visible dot, not a geometric
## intersection, is the only place a path can change direction.

signal node_selected(index: int)

const Rules = preload("res://scripts/trail_rules.gd")
const PAPER = Color("#F1E9DE")
const OUTLINE = Color("#B9B6AD")
const INK = Color("#3E3E40")
const DONE = Color("#D07651")
const WARM = Color("#E7B488")
const WHITE = Color("#FFFFFF")
const HINT = Color("#2F7A72")

var graph: Dictionary = {}
var path: Array = []
var _last_hover: int = -1


func show_graph(new_graph: Dictionary, new_path: Array) -> void:
    graph = new_graph
    path = new_path.duplicate()
    _last_hover = -1
    queue_redraw()


func update_path(new_path: Array) -> void:
    path = new_path.duplicate()
    queue_redraw()


func _side() -> float:
    return maxf(1.0, minf(size.x - 24.0, size.y - 24.0))


func _origin() -> Vector2:
    return (size - Vector2.ONE * _side()) * 0.5


func _point(point: Vector2) -> Vector2:
    return _origin() + point * _side()


func _edge_used(a: int, b: int) -> bool:
    for i in range(path.size() - 1):
        if (path[i] == a and path[i + 1] == b) or (path[i] == b and path[i + 1] == a):
            return true
    return false


func _draw() -> void:
    var panel := Rect2(_origin(), Vector2.ONE * _side())
    draw_rect(panel, PAPER, true)
    draw_rect(panel, OUTLINE, false, 2.0)
    if graph.is_empty():
        return

    # Plain crossings have no node and therefore cannot be used to turn.
    for edge in graph["edges"]:
        var used: bool = _edge_used(edge[0], edge[1])
        draw_line(_point(graph["nodes"][edge[0]]), _point(graph["nodes"][edge[1]]),
            DONE if used else OUTLINE, 7.0 if used else 4.0, true)

    var endpoints: Array = Rules.odd_vertices(graph)
    var current: int = int(path[path.size() - 1]) if not path.is_empty() else -1
    for i in range(graph["nodes"].size()):
        var center: Vector2 = _point(graph["nodes"][i])
        var is_endpoint: bool = endpoints.has(i)
        if is_endpoint and path.is_empty():
            draw_arc(center, 24.0, 0.0, TAU, 48, HINT, 4.0, true)
        var is_current: bool = i == current
        draw_circle(center, 22.0 if is_current else 20.0, INK)
        draw_circle(center, 17.0 if is_current else 16.0, DONE if is_current else WARM)
        var label: String = char(65 + i)
        var font: Font = ThemeDB.fallback_font
        var text_size: Vector2 = font.get_string_size(label, HORIZONTAL_ALIGNMENT_LEFT, -1, 19)
        draw_string(font, center + Vector2(-text_size.x / 2.0, text_size.y / 3.0), label,
            HORIZONTAL_ALIGNMENT_LEFT, -1, 19, WHITE if is_current else INK)


func _select_nearest(at: Vector2) -> void:
    if graph.is_empty():
        return
    var index: int = -1
    var distance: float = maxf(26.0, 0.060 * _side())
    for i in range(graph["nodes"].size()):
        var current_distance: float = at.distance_to(_point(graph["nodes"][i]))
        if current_distance <= distance:
            distance = current_distance
            index = i
    if index >= 0 and index != _last_hover:
        _last_hover = index
        node_selected.emit(index)
    if index == -1:
        _last_hover = -1


func _gui_input(event: InputEvent) -> void:
    if event is InputEventScreenTouch:
        var touch: InputEventScreenTouch = event
        if touch.pressed:
            _select_nearest(touch.position)
        else:
            _last_hover = -1
        accept_event()
    elif event is InputEventScreenDrag:
        _select_nearest(event.position)
        accept_event()
    elif event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT:
        if event.pressed:
            _select_nearest(event.position)
        else:
            _last_hover = -1
        accept_event()
    elif event is InputEventMouseMotion and event.button_mask & MOUSE_BUTTON_MASK_LEFT:
        _select_nearest(event.position)
        accept_event()
