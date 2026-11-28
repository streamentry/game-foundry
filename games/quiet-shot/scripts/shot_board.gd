extends Control
## Custom vector board; precise aim can also be set by large textual controls.

signal angle_changed(degrees: float)

const Rules = preload("res://scripts/ray_rules.gd")
# Tokens from DESIGN.md section 2 (icon-derived playfield).
const SURFACE = Color("#2A3B63")      # playfield
const GRID = Color("#3A4D7A")         # playfield_grid
const INK = Color("#1C2A47")          # ink (target rings, launch outline)
const GUIDE = Color("#F2B45B")        # accent (launch guide)
const TARGET = Color("#F6EEDC")       # cream (unhit target disc)
const CLEARED = Color("#8FD9B6")      # mint (cleared target disc)
const MIRROR = Color("#F6EEDC")       # cream (mirror bars)
const TRAIL = Color("#F2B45B")        # trail (amber)
const CORAL = Color("#E8674F")        # coral (launch ball)
const WHITE = Color("#F6EEDC")        # cream, used for the launch-dot highlight

var level: Dictionary = {}
var angle: float = -45.0
var shot: Dictionary = {}


func set_board(new_level: Dictionary, new_angle: float) -> void:
    level = new_level
    angle = new_angle
    shot = {}
    queue_redraw()


func show_result(result: Dictionary) -> void:
    shot = result
    queue_redraw()


func reset_result() -> void:
    shot = {}
    queue_redraw()


func set_aim(degrees: float) -> void:
    if not shot.is_empty():
        return
    angle = clampf(degrees, -85.0, -10.0)
    queue_redraw()
    angle_changed.emit(angle)


func _square() -> float:
    return maxf(1.0, minf(size.x - 24.0, size.y - 24.0))


func _corner() -> Vector2:
    return (size - Vector2.ONE * _square()) * 0.5


func _point(v: Vector2) -> Vector2:
    return _corner() + v * _square()


func _world(p: Vector2) -> Vector2:
    return (p - _corner()) / _square()


func _draw() -> void:
    var bounds := Rect2(_corner(), Vector2.ONE * _square())
    draw_rect(bounds, SURFACE, true)
    draw_rect(bounds, GRID, false, 2.0)
    for step in range(1, 5):
        var x: float = float(step) * _square() / 5.0
        draw_line(bounds.position + Vector2(x, 0.0), bounds.position + Vector2(x, _square()), GRID, 1.0)
        draw_line(bounds.position + Vector2(0.0, x), bounds.position + Vector2(_square(), x), GRID, 1.0)
    if level.is_empty():
        return

    for mirror in level["mirrors"]:
        draw_line(_point(mirror["a"]), _point(mirror["b"]), MIRROR, 9.0, true)

    var hit: Array = shot.get("hit", [])
    var radius: float = float(level["radius"]) * _square()
    for i in range(level["targets"].size()):
        var center: Vector2 = _point(level["targets"][i])
        var succeeded: bool = i < hit.size() and bool(hit[i])
        draw_circle(center, radius + 3.0, INK)
        draw_circle(center, radius, CLEARED if succeeded else TARGET)
        if succeeded:
            draw_line(center + Vector2(-8, 0), center + Vector2(-2, 7), INK, 3.0, true)
            draw_line(center + Vector2(-2, 7), center + Vector2(10, -9), INK, 3.0, true)
        else:
            draw_arc(center, radius * 0.55, 0.0, TAU, 32, INK, 2.0, true)

    var launch: Vector2 = _point(Rules.START)
    if shot.is_empty():
        var direction: Vector2 = Rules.direction(angle)
        draw_line(launch, launch + direction * (_square() * 0.29), GUIDE, 4.0, true)
        draw_circle(launch + direction * (_square() * 0.29), 5.0, GUIDE)
    else:
        var path: PackedVector2Array = shot["path"]
        for i in range(path.size() - 1):
            draw_line(_point(path[i]), _point(path[i + 1]), TRAIL, 4.5, true)
        if path.size() > 1:
            draw_circle(_point(path[path.size() - 1]), 6.0, TRAIL)

    draw_circle(launch, 11.0, INK)
    draw_circle(launch, 9.0, CORAL)
    draw_circle(launch + Vector2(-3, -3), 2.5, WHITE)


func _from_pointer(pos: Vector2) -> void:
    if not shot.is_empty():
        return
    var delta: Vector2 = _world(pos) - Rules.START
    if delta.length_squared() < 0.001:
        return
    set_aim(rad_to_deg(atan2(delta.y, delta.x)))


func _gui_input(event: InputEvent) -> void:
    if not shot.is_empty():
        return
    if event is InputEventScreenTouch:
        var e: InputEventScreenTouch = event
        if e.pressed:
            _from_pointer(e.position)
        accept_event()
    elif event is InputEventScreenDrag:
        _from_pointer(event.position)
        accept_event()
    elif event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT:
        if event.pressed:
            _from_pointer(event.position)
        accept_event()
    elif event is InputEventMouseMotion and event.button_mask & MOUSE_BUTTON_MASK_LEFT:
        _from_pointer(event.position)
        accept_event()
